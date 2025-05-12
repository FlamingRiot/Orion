#pragma warning disable CS8602

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orion_Desktop
{
    /// <summary>Defines the motor ID of the robot.</summary>
    internal enum StepMotorID
    {
        M2,
        M3
    }

    /// <summary>Represents an instance of the <see cref="WebsocketRequests"/> class.</summary>
    internal static class WebsocketRequests
    {
        // Constants
        internal const float MOTOR_STEPS = 1000;

        internal const string WEBSOCKET_ADDRESS = "ws://130.130.130.1:81";
        internal const string WATCHDOG_MESSAGE = "ping";
        internal const string WEBSOCKET_CONFIG_SUCCESSFULL_MESSAGE = "{\"Command\":\"event\",\"Data\":{\"event\":\"ContentActivated\",\"eventArgs\":{\"contentId\":\"PageContent\",\"visuId\":\"firstvisu\"},\"source\":{\"type\":\"session.Event\"}},\"Resource\":\"publisher\"}";
        internal const int WATCHDOG_TIMER = 10000;

        // Public attributes
        internal static bool SHOW_RESPONSE_POOL = false; // Not showing by default
        internal static bool WEBSOCKET_READY = false;

        // Private attributes
        private static string? CLIENT_ID = "";
        private static byte[] WATCHDOG_BYTES = { };
        private static readonly byte[] RESPONSE_BUFFER = new byte[1024];

        // Private functional attributes
        private static Stopwatch? timer;
        private static ClientWebSocket WebSocketClient = new ClientWebSocket();
        private static int _timeLastCheck = -1;

        /// <summary>Initializes the connexion with the robot's WebSocket.</summary>
        /// <returns>Await statement.</returns>
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
                    ShowPoolAction(WATCHDOG_MESSAGE);

                    // Activate visu
                    string activateVisuUrl = $"http://130.130.130.1:81/services/client/activateVisu?visuId=firstvisu&t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    string visuActivationInfo = await client.GetStringAsync(activateVisuUrl);
                    JObject visuActivationInfoJson = JObject.Parse(visuActivationInfo);

                    // Activate Navigation (required but not used)
                    string navigationUrl = "{\"Command\":\"activatecontent\",\"Parameter\":{\"contentId\":\"Navigation\",\"visuId\":\"firstvisu\"},\"Resource\":\"services/client\"}";
                    byte[] navigationBytes = Encoding.UTF8.GetBytes(navigationUrl);
                    await WebSocketClient.SendAsync(new ArraySegment<byte>(navigationBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    ShowPoolAction(navigationUrl);

                    // Activate PageContent (required but not used)
                    string pageContentUrl = "{\"Command\":\"activatecontent\",\"Parameter\":{\"contentId\":\"PageContent\",\"visuId\":\"firstvisu\"},\"Resource\":\"services/client\"}";
                    byte[] pageContentBytes = Encoding.UTF8.GetBytes(pageContentUrl);
                    await WebSocketClient.SendAsync(new ArraySegment<byte>(pageContentBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    ShowPoolAction(pageContentUrl);

                    SendWSLog("Configuration executed successfully", ConsoleColor.Green, "INFO");
                }
            }
        }

        /// <summary>Sends a specified motor the instruction to move.</summary>
        /// <param name="id">Motor ID (clamped if not valid.)</param>
        /// <param name="steps">Number of steps to move.</param>
        /// <returns>Await statement.</returns>
        internal static async Task SendMotorInstruction(StepMotorID id, float angle)
        {
            // Define steps
            int steps = (int)(MOTOR_STEPS / 360f * angle);
            if (id == StepMotorID.M3) steps *= -1;

            for (int i = 0; i < 15; i++) // The loop guarantees the success of the POST request (for some reason it only works every random times)
            {
                // Update steps value
                string updateMessage = @$"{{""Command"":""update"",""Data"":[{{""event"":""PropertyValueChanged"",""eventArgs"":[{{""refId"":""PageContent_NumericInput{(int)id+2}"",""data"":[{{""attribute"":""node"",""value"":{{""value"":{steps},""id"":""::AsGlobalPV:HMINbStepMot{(int)id + 2}"",""unit"":null,""minValue"":-3.40282347e+38,""maxValue"":3.40282347e+38,""time"":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}}}}]}}]}}],""Resource"":""services/client""}}";
                byte[] updateMessageBytes = Encoding.UTF8.GetBytes(updateMessage);
                await WebSocketClient.SendAsync(new ArraySegment<byte>(updateMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                ShowPoolAction(updateMessage);

                // Enable clicked state (starts movement)
                string postMessage = @$"{{""Command"":""update"",""Data"":[{{""event"":""PropertyValueChanged"",""eventArgs"":[{{""refId"":""PageContent_MomentaryPushButton{(int)id+2}"",""data"":[{{""attribute"":""value"",""value"":1}}]}}]}}],""Resource"":""services/client""}}";
                byte[] postMessageBytes = Encoding.UTF8.GetBytes(postMessage);
                await WebSocketClient.SendAsync(new ArraySegment<byte>(postMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                ShowPoolAction(postMessage);

                Task.WaitAll();

                // Disable clicked state (Prevents from looping on movement)
                postMessage = @$"{{""Command"":""update"",""Data"":[{{""event"":""PropertyValueChanged"",""eventArgs"":[{{""refId"":""PageContent_MomentaryPushButton{(int)id+2}"",""data"":[{{""attribute"":""value"",""value"":0}}]}}]}}],""Resource"":""services/client""}}";
                postMessageBytes = Encoding.UTF8.GetBytes(postMessage);
                await WebSocketClient.SendAsync(new ArraySegment<byte>(postMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                ShowPoolAction(postMessage);

                Task.WaitAll();
            }
        }

        /// <summary>Updates the different WebSocket interactions.</summary>
        /// <returns>Await statement.</returns>
        internal static async Task UpdateWebSocket()
        {
            await SendWatchdogPing();
        }

        /// <summary>Sends a ping message to the watchdog every 5 seconds.</summary>
        /// <returns>Await statement.</returns>
        private static async Task SendWatchdogPing()
        {
            int time = (int)timer.Elapsed.TotalSeconds;
            if (_timeLastCheck < time)
            {
                _timeLastCheck = time;
                // Send "ping" every 5 seconds
                if (time % (WATCHDOG_TIMER / 5000) == 0)
                {
                    await WebSocketClient.SendAsync(new ArraySegment<byte>(WATCHDOG_BYTES), WebSocketMessageType.Text, true, CancellationToken.None);
                    // Manage ready-state
                    string response = await ReceiveResponse();
                    if (response == WEBSOCKET_CONFIG_SUCCESSFULL_MESSAGE) WEBSOCKET_READY = true;
                    if (SHOW_RESPONSE_POOL)
                    {
                        SendWSLog(WATCHDOG_MESSAGE, ConsoleColor.Green, "POOL");
                        SendWSLog(response, ConsoleColor.Red, "POOL");
                    }
                }
            }
        }

        /// <summary>Receives the latest response from the WebSocket.</summary>
        /// <returns>Await statement.</returns>
        private static async Task<string> ReceiveResponse()
        {
            WebSocketReceiveResult result = await WebSocketClient.ReceiveAsync(new ArraySegment<byte>(RESPONSE_BUFFER), CancellationToken.None);
            string message = Encoding.UTF8.GetString(RESPONSE_BUFFER, 0, result.Count);
            return message;
        }

        /// <summary>Shows the pool for a return action on the websocket.</summary>
        /// <param name="msg">Sent message.</param>
        private static async void ShowPoolAction(string msg)
        {
            if (SHOW_RESPONSE_POOL)
            {
                SendWSLog(msg, ConsoleColor.Green, "POOL");
                string response = await ReceiveResponse();
                SendWSLog(response, ConsoleColor.Red, "POOL");
            }
        }

        /// <summary>Sends a debug log to the console.</summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="color">Color to use.</param>
        /// <param name="logLevel">Level or distinction of the log.</param>
        private static void SendWSLog(string msg, ConsoleColor color, string logLevel)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{logLevel}: WEBSOCKET: {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>Sends a debug log to the console.</summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="logLevel">Level or distinction of the log.</param>
        private static void SendWSLog(string msg, string logLevel)
        {
            Console.WriteLine($"{logLevel}: WEBSOCKET: {msg}");
        }
    }
}