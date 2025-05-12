using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    internal static class WebsocketRequests
    {
        internal static string? CLIENT_ID = "";

        internal static async Task InitializeConnexion()
        {
            // Register client
            using HttpClient client = new HttpClient();
            string clientUrl = $"http://130.130.130.1:81/services/client/registerclient?visuId=firstvisu&t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            
            // Send GET request
            HttpResponseMessage clientResponseMessage = await client.GetAsync(clientUrl);
            clientResponseMessage.EnsureSuccessStatusCode();

            // Exctract returned data
            string clientInfo = await client.GetStringAsync(clientUrl);
            JObject json = JObject.Parse(clientInfo);
            CLIENT_ID = (string?)json["ClientId"];
            if (CLIENT_ID != null) SendWSLog($"Client {CLIENT_ID} registered successfully", "INFO");
            else SendWSLog("Unable to register client, already exists", "ERROR");
        }

        private static void SendWSLog(string msg, ConsoleColor color, string logLevel)
        {
            Console.ForegroundColor = color;
            Console.Write($"{logLevel}: WEBSOCKET: {msg}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void SendWSLog(string msg, string logLevel)
        {
            Console.Write($"{logLevel}: WEBSOCKET: {msg}");
        }
    }
}