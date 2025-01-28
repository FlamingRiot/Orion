#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    internal static class OnlineRequests
    {
        public const int REQUEST_INTERVAL = 5;

        private static Stopwatch? timer;
        private static int _timeLastCheck;
        private static int counter;

        /// <summary>Starts connexion timer.</summary>
        internal static void StartConnexion()
        {
            if (timer is null)
            {
                timer = new Stopwatch();
                timer.Start();
            }
        }

        internal static async Task GetCurrentSatellite()
        {
            using (HttpClient client = new HttpClient()) 
            {
                string url = "https://api.wheretheiss.at/v1/satellites/25544";
                // Try fetching, abort otherwise and go offline mode
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Abort if no response, thus offline

                    // Read response
                    string body = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(body);

                    // JSON Template from API
                    //{
                    //    "name": "iss",
                    //    "id": 25544,
                    //    "latitude": 50.11496269845,
                    //    "longitude": 118.07900427317,
                    //    "altitude": 408.05526028199,
                    //    "velocity": 27635.971970874,
                    //    "visibility": "daylight",
                    //    "footprint": 4446.1877699772,
                    //    "timestamp": 1364069476,
                    //    "daynum": 2456375.3411574,
                    //    "solar_lat": 1.3327003598631,
                    //    "solar_lon": 238.78610691196,
                    //    "units": "kilometers"
                    //}

                    EarthHologram.Satellite.UpdateSatellite(json);
                    EarthHologram.SatellitePoints.Add(CelestialMaths.ComputeECEF(EarthHologram.Satellite.Latitude, EarthHologram.Satellite.Longitude) * (EarthHologram.HOLOGRAM_RADIUS + 1)); // Compute XYZ coord.


                    Console.WriteLine($"ORION: Succesfully retrieved {(string)json["name"]} informations");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>Updates the current satellite based on API.</summary>
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
    }
}