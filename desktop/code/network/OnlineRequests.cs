#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Diagnostics;
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    /// <summary>The static class used for sendind HTTP requests.</summary>
    internal static class OnlineRequests
    {
        public const int REQUEST_INTERVAL = 2;

        private static Stopwatch? timer;
        private static int _timeLastCheck = -1;

        /// <summary>Starts connexion timer.</summary>
        internal static void StartConnexion()
        {
            if (timer is null)
            {
                timer = new Stopwatch();
                timer.Start();
            }
        }

        /// <summary>Retrieves satellite informations from the API.</summary>
        /// <returns>Async Task</returns>
        internal static async Task GetCurrentSatellite()
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
                    EarthHologram.SatellitePoints.Add(CelestialMaths.ComputeECEFTilted(EarthHologram.Satellite.Latitude, EarthHologram.Satellite.Longitude, EarthHologram.IYaw) * (EarthHologram.HOLOGRAM_RADIUS + 0.1f)); // Compute XYZ coord.

                    Conceptor2D.UpdateUI();
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

        internal async static Task<Vector3> GetCurrentPlanet(AstralTarget target)
        {
            //string url = $"https://ssd.jpl.nasa.gov/api/horizons.api?" +
            //    $"format=text&COMMAND='499'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&" +
            //    $"EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&" +
            //    $"STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'";
            //string url = $"https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='499'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='coord@399'&SITE_COORD='6.1,46.2,0.375'&START_TIME='2025-03-18'&STOP_TIME='2025-03-19'&STEP_SIZE='1h'&QUANTITIES='4'";
            string url = $"https://ssd.jpl.nasa.gov/api/horizons.api?format=json&COMMAND='499'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='coord@399'&SITE_COORD='6.1,46.2,0.375'&START_TIME='2025-03-18'&STOP_TIME='2025-03-19'&STEP_SIZE='1h'&QUANTITIES='1,4,20,31'";

            // Try fetching, abort otherwise and debug error
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Abort if no response

                    // Read response
                    string body = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(body);
                    Console.WriteLine(json.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return Vector3.Zero;
        }
    }
}