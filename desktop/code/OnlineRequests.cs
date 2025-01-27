#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604

using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    internal static class OnlineRequests
    {
        internal static async Task<Satellite> GetCurrentISS()
        {
            using (HttpClient client = new HttpClient()) 
            {
                string url = "http://api.open-notify.org/iss-now.json";
                // Try fetching, abort otherwise and go offline mode
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Abort if no response, thus offline

                    // Read response
                    string body = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(body);

                    // Extract data
                    long timestamp = (long)json["timestamp"];
                    float latitude = (float)json["iss_position"]["latitude"];
                    float longitude = (float)json["iss_position"]["longitude"];

                    Satellite satellite = new Satellite()
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                    };

                    Console.WriteLine($"Timestamp: {timestamp}");
                    Console.WriteLine($"Latitude: {satellite.Latitude}, Longitude: {satellite.Longitude}");

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