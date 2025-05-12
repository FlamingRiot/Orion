#pragma warning disable CS8602

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the <see cref="WebsocketRequests"/> class.</summary>
    internal static class WebsocketRequests
    {
        // Constants
        internal const string WEBSOCKET_ADDRESS = "ws://130.130.130.1:81";
        internal const string WATCHDOG_MESSAGE = "ping";
        internal const int WATCHDOG_TIMER = 10000;

        // Public attributes
        internal static bool SHOW_RESPONSE_POOL = false; // Not showing by default

        // Private attributes
        private static string? CLIENT_ID = "";
        private static byte[] WATCHDOG_BYTES = { };
        private static readonly byte[] RESPONSE_BUFFER = new byte[1024];

        // Private functional attributes
        private static Stopwatch? timer;
        private static ClientWebSocket WebSocketClient = new ClientWebSocket();
        private static int _timeLastCheck = -1;

        internal static async Task InitializeConnexion()
        {
            // Start watchdog-handler timer
            if (timer is null)
            {
                timer = new Stopwatch();
                timer.Start();
            }

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
            if (CLIENT_ID == null) SendWSLog("Unable to register client, already exists", "ERROR");
            else
            {
                SendWSLog($"Client {CLIENT_ID} registered successfully", "INFO");

                // Connect to the Websocket
                WebSocketClient = new ClientWebSocket();
                WebSocketClient.Options.SetRequestHeader("Cookie", $"ManagerId={CLIENT_ID}");
                await WebSocketClient.ConnectAsync(new Uri($"{WEBSOCKET_ADDRESS}/?watchdog={WATCHDOG_TIMER}"), CancellationToken.None);
                if (WebSocketClient.HttpStatusCode != 0) SendWSLog("Unable to connect to WebSocket, check your network", "ERROR");
                else
                {
                    // Register watchdog message
                    WATCHDOG_BYTES = Encoding.UTF8.GetBytes(WATCHDOG_MESSAGE);

                    // Establish first watchdog contact 
                    await WebSocketClient.SendAsync(new ArraySegment<byte>(WATCHDOG_BYTES), WebSocketMessageType.Text, true, CancellationToken.None);
                    if (SHOW_RESPONSE_POOL)
                    {
                        string response = await ReceiveResponse();
                        SendWSLog(response, "POOL");
                    }

                    SendWSLog("Configuration executed successfully", ConsoleColor.Green, "INFO");
                }
            }
        }

        internal static async Task UpdateWebSocket()
        {
            await SendWatchdogPing();
        }

        private static async Task SendWatchdogPing()
        {
            int time = (int)timer.Elapsed.TotalSeconds;
            if (_timeLastCheck < time)
            {
                _timeLastCheck = time;
                // Send "ping" every 5 seconds
                if (time % (WATCHDOG_TIMER / 2000) == 0)
                {
                    await WebSocketClient.SendAsync(new ArraySegment<byte>(WATCHDOG_BYTES), WebSocketMessageType.Text, true, CancellationToken.None);
                    SendWSLog(WATCHDOG_MESSAGE, ConsoleColor.Gray, "POOL");
                    if (SHOW_RESPONSE_POOL)
                    {
                        string response = await ReceiveResponse();
                        SendWSLog(response, ConsoleColor.Gray, "POOL");
                    }
                }
            }
        }

        private static async Task<string> ReceiveResponse()
        {
            WebSocketReceiveResult result = await WebSocketClient.ReceiveAsync(new ArraySegment<byte>(RESPONSE_BUFFER), CancellationToken.None);
            string message = Encoding.UTF8.GetString(RESPONSE_BUFFER, 0, result.Count);
            return message;
        }

        private static void SendWSLog(string msg, ConsoleColor color, string logLevel)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{logLevel}: WEBSOCKET: {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void SendWSLog(string msg, string logLevel)
        {
            Console.WriteLine($"{logLevel}: WEBSOCKET: {msg}");
        }
    }
}