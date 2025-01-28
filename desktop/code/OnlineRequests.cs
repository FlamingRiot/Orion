#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    internal static class OnlineRequests
    {
        internal static async Task<Satellite> GetCurrentISS()
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

                    // Extract data to satellite object
                    Satellite satellite = new Satellite()
                    {
                        Name = (string)json["name"],
                        Latitude = (float)json["latitude"],
                        Longitude = (float)json["longitude"],
                        Altitude = (float)json["altitude"],
                        Velocity = (float)json["velocity"],
                        Visibility = (SatelliteVisibility)Enum.Parse(typeof(SatelliteVisibility), (string)json["visibility"]),
                        Units = (SatelliteUnits)Enum.Parse(typeof(SatelliteUnits), (string)json["units"]),
                        Footprint = (float)json["footprint"],
                        Timestamp = (long)json["timestamp"]
                    };

                    Console.WriteLine($"Latitude: {satellite.Latitude}, Longitude: {satellite.Longitude}, Altitude: {satellite.Altitude}");

                    return satellite;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return new Satellite();
                }
            }
        }
    }
}