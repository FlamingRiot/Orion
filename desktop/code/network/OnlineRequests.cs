#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Diagnostics;
using System.Numerics;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    /// <summary>The static class used for sendind HTTP requests.</summary>
    internal static class OnlineRequests
    {
        internal const int REQUEST_INTERVAL = 2;
        internal const int MAX_SIMULTANEOUS_TILE_DOWNLOADS = 4;
        internal const string CACHE_DIRECTORY = "cache/";

        internal static List<PlanetCacheEntry> PlanetCacheEntries = new List<PlanetCacheEntry>();
        private static Stopwatch? timer;
        private static int _timeLastCheck = -1;

        // AstronomyAPI credentials
        private const string ASTRONOMY_API_ID = "c15d51ed-bd48-4af8-94a8-5eccb4953332";
        private const string ASTRONOMY_API_SECRET = "a6df73d2e471d7761be374b76640fc22fa" +
            "6b86ce14215166a9a3a6456e318f51ccf2a6f6b242799f504d76af0a8ff62f39338770a5a7" +
            "e15233a0fac4d30e4385ce39a8b140cd73363ba9e2ab90c71a66492e9e8a4b78807ab2c6be" +
            "f12a4a74198e04d9904b6c965a22a2667f3f1cccb1";

        /// <summary>Starts connexion timer (used for API max-request and caching files).</summary>
        internal static void StartConnexion()
        {
            // Start max-request timer
            if (timer is null)
            {
                timer = new Stopwatch();
                timer.Start();
            }
            // Create caching directory if not already exists
            if (!Directory.Exists(CACHE_DIRECTORY)) Directory.CreateDirectory(CACHE_DIRECTORY);
            
            // Create planet caching file if not already exists
            if (!File.Exists(PlanetCacheEntry.PLANET_CACHE_FILE))
            {
                FileStream stream = File.Create(PlanetCacheEntry.PLANET_CACHE_FILE);
                // Close stream
                stream.Close();
            }

            // Retrieve cached data for planets
            StreamReader cacheStream = new StreamReader(PlanetCacheEntry.PLANET_CACHE_FILE);
            string cache = cacheStream.ReadToEnd();
            cacheStream.Close();
            if (cache != "")
            {
                string[] jsons = cache.Split(PlanetCacheEntry.SEPARATOR);
                for (int i = 0; i < jsons.Length; i++)
                {
                    PlanetCacheEntries.Add(new PlanetCacheEntry(JObject.Parse(jsons[i]), false));
                }
            }

            // Send debug
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ORION: {PlanetCacheEntries.Count} planet info cached");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /*-----------------------------------------------------------------
        Satellite and planets data-retrieving functions
        ------------------------------------------------------------------*/

        /// <summary>Retrieves satellite informations from the API and computes them.</summary>
        /// <returns>Async Task</returns>
        private static async Task GetCurrentSatellite()
        {
            using (HttpClient client = new HttpClient()) 
            {
                string url = "https://api.wheretheiss.at/v1/satellites/25544";
                // Try fetching, abort otherwise and debug error
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Abort if no response, thus offline

                    // Read response
                    string body = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(body);

                    EarthHologram.Satellite.UpdateSatellite(json);
                    EarthHologram.SatellitePoints.Add(CelestialMaths.ComputeECEFTilted(EarthHologram.Satellite.Latitude, EarthHologram.Satellite.Longitude, EarthHologram.IYaw) * (EarthHologram.HOLOGRAM_RADIUS + 0.1f)); // Compute XYZ coord.
                    EarthHologram.ComputeRelativeAltitude(); // Compute what the distance from the earth-globe should be (used for accuracy in calculations)

                    OrionSim.ComputeArrowDirection(); // Computes the direction used for the 3D pointing-arrow

                    Conceptor2D.UpdateUI();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>Updates the current satellite based on API and computes its data.</summary>
        /// <returns>dunno.</returns>
        internal async static Task UpdateCurrentSatellite()
        {
            int time = (int)timer.Elapsed.TotalSeconds;
            if (_timeLastCheck < time) 
            {
                _timeLastCheck = time; 
                if (time % REQUEST_INTERVAL == 0) await GetCurrentSatellite();
            }
        }

        /// <summary>Updates data for a given planet.</summary>
        /// <param name="target">Given target planet.</param>
        /// <returns>Async task.</returns>
        internal async static Task UpdateCurrentPlanet(AstralTarget target)
        {
            // Check if not already cached (and up to date), prevents from sending too much API request
            bool exists = false;
            // Get current date (for data renewal)
            DateTime now = DateTime.Now;
            string date = $"{now.Month.ToString().PadLeft(2, '0')}/{now.Day.ToString().PadLeft(2, '0')}/{now.Year}";

            // Check for existence
            PlanetCacheEntries.ForEach(entry =>
            {
                if (entry.Name == target) 
                {
                    // Check for date
                    if (entry.Date == date)
                    {
                        exists = true; 
                    }
                }
            });

            if (target != AstralTarget.ISS && !exists) // Ignore for ISS and if not already exists in cache
            {
                string response = "";
                string id = Enum.GetName(target);
                // Send API request for given astral
                try
                {
                    HttpClient client = new HttpClient();
                    string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ASTRONOMY_API_ID}:{ASTRONOMY_API_SECRET}"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

                    // For now, constant view altitude
                    string url = $"https://api.astronomyapi.com/api/v2/bodies/positions/{id}?latitude={OrionSim.ViewerLatitude}&longitude={OrionSim.ViewerLongitude}&elevation=400&from_date=2025-04-10&to_date=2025-04-10&time=12:00:00";
                    HttpResponseMessage msg = await client.GetAsync(url);
                    msg.EnsureSuccessStatusCode(); // Abort if no response, thus offline
                    response = await msg.Content.ReadAsStringAsync();

                    // Parse data
                    JObject json = JObject.Parse(response);
                    PlanetCacheEntry entry = new PlanetCacheEntry(json, true); // This will retrieve the correct data
                    PlanetCacheEntries.Add(entry);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /*-----------------------------------------------------------------
          Map-Tiles data-retrieving functions
        ------------------------------------------------------------------*/

        /// <summary>Retrieves data from a Map-tiling endpoint, for a selected tile config.</summary>
        /// <param name="row">Tile row.</param>
        /// <param name="column">Tile column.</param>
        /// <param name="zoom">Zoom level.</param>
        /// <returns>Whatever <see cref="Task"/> is.</returns>
        internal static async Task DownloadTileset(int zoom)
        {
            int _downloadWithCount = 0;
            int _downloadHeightCount = 0;
            bool _isLoadingDone = false;

            int _widthMax = 0, _heightMax = 0;

            // Create directory if not already exists
            string dirPath = $"{CACHE_DIRECTORY}{TilingManager.MAP_CONFIG}_{zoom}/";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            // Update loading
            while (!_isLoadingDone)
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string imgName = $"{TilingManager.MAP_CONFIG}_{zoom}_{_downloadHeightCount}_{_downloadWithCount}.png";
                        if (!File.Exists($"{dirPath}{imgName}"))
                        {
                            string url = $"https://gibs.earthdata.nasa.gov/wmts/epsg4326/best/{TilingManager.MAP_CONFIG}/default/2013-07-09/250m/{zoom}/{_downloadHeightCount}/{_downloadWithCount}.jpg";
                            HttpResponseMessage response = await client.GetAsync(url);
                            response.EnsureSuccessStatusCode();
                            byte[] data = await response.Content.ReadAsByteArrayAsync(); // Read stream
                            File.WriteAllBytes($"{dirPath}{imgName}", data); // Write img to cache

                            _downloadWithCount++;
                        }
                    }
                    catch
                    {
                        if (_downloadWithCount == 0)
                        {
                            _isLoadingDone = true;
                            TilingManager.Configs[zoom] = new Vector2(_widthMax - 1, _heightMax - 1);
                            TilingManager.ConvertCoordinatesToTiles(OrionSim.ViewerLatitude, OrionSim.ViewerLongitude, zoom);
                        }

                        _downloadHeightCount++;
                        _heightMax++;
                        _widthMax = _downloadWithCount;
                        _downloadWithCount = 0;
                    }
                }
            }
        }
        //internal static async Task DownloadTileset(int zoom)
        //{
        //    int maxConcurrentDownloads = 4;
        //    using SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads);

        //    int y = 0;
        //    bool isLoadingDone = false;
        //    int widthMax = 0;
        //    int heightMax = 0;

        //    string dirPath = $"{TilingManager.CACHE_DIRECTORY}{TilingManager.MAP_CONFIG}_{zoom}/";
        //    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        //    while (!isLoadingDone)
        //    {
        //        List<Task<bool>> tasks = new List<Task<bool>>();
        //        int x = 0;
        //        bool atLeastOneSuccess = false;

        //        while (true)
        //        {
        //            await semaphore.WaitAsync();
        //            int tileX = x;
        //            int tileY = y;

        //            string imgName = $"{TilingManager.MAP_CONFIG}_{zoom}_{tileY}_{tileX}.png";
        //            string filePath = $"{dirPath}{imgName}";

        //            if (File.Exists(filePath))
        //            {
        //                x++;
        //                semaphore.Release();
        //                continue;
        //            }

        //            tasks.Add(Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    using HttpClient client = new HttpClient();
        //                    string url = $"https://gibs.earthdata.nasa.gov/wmts/epsg4326/best/{TilingManager.MAP_CONFIG}/default/2013-07-09/250m/{zoom}/{tileY}/{tileX}.jpg";

        //                    HttpResponseMessage response = await client.GetAsync(url);
        //                    response.EnsureSuccessStatusCode();
        //                    byte[] data = await response.Content.ReadAsByteArrayAsync();
        //                    await File.WriteAllBytesAsync(filePath, data);

        //                    return true;
        //                }
        //                catch
        //                {
        //                    return false;
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                }
        //            }));

        //            x++;

        //            // Pour éviter de lancer trop de tâches (genre 10'000 tiles d’un coup)
        //            if (tasks.Count >= 32) break;
        //        }

        //        bool[] results = await Task.WhenAll(tasks);
        //        atLeastOneSuccess = results.Any(success => success);

        //        if (!atLeastOneSuccess)
        //        {
        //            isLoadingDone = true;
        //            TilingManager.Configs[zoom] = new Vector2(widthMax - 1, heightMax - 1);
        //            TilingManager.ConvertCoordinatesToTiles(OrionSim.ViewerLatitude, OrionSim.ViewerLongitude, zoom);
        //        }
        //        else
        //        {
        //            widthMax = Math.Max(widthMax, x);
        //            heightMax++;
        //            y++;
        //        }
        //    }
        //}
    }
}