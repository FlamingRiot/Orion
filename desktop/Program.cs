#pragma warning disable CS4014

using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the running program.</summary>
    public class Program
    {
        // Constants
        public const string APP_NAME = "Orion";
        public const string APP_VERSION = "beta-0.2.0";

        // Different render 
        internal static RenderTexture2D Render;
        internal static RenderTexture2D HologramRender;
        internal static Rectangle SourceRender;
        internal static Rectangle DestinationRender;
        internal static RenderTexture2D FinalRender;

        public static int Width;
        public static int Height;

        /// <summary>Defines the entry point of the program.</summary>
        /// <param name="args">Arguments passed from outside the progam.</param>
        public static void Main(string[] args)
        {
            // Open base window
            InitWindow(GetMonitorWidth(0), GetMonitorHeight(0), $"{APP_NAME} {APP_VERSION}");
            SetWindowState(ConfigFlags.ResizableWindow);

            SetConfigFlags(ConfigFlags.VSyncHint);
            SetConfigFlags(ConfigFlags.Msaa4xHint);

            SetWindowIcon(LoadImage("assets/logo.png"));

            ToggleFullscreen();

            Width = GetScreenWidth();
            Height = GetScreenHeight();

            // Init connexion to the WebSocket
            WebsocketRequests.InitializeConnexion();

#if DEBUG
            WebsocketRequests.SHOW_RESPONSE_POOL = true;
#endif

            // Open different services (Call-order matters here)
            AudioCenter.Init();
            Conceptor3D.Init();
            Conceptor2D.Init();

            // Load render texture
            LoadRender();

#if !DEBUG
            SetTargetFPS(60);
#endif
            SetExitKey(KeyboardKey.Null);
            DisableCursor();
            while (!WindowShouldClose())
            {
                // Update WebSocket 
                WebsocketRequests.UpdateWebSocket();

                // Ambient sound
                AudioCenter.UpdateMusic("ambient");

                // Update functions
                Conceptor3D.Update();

                // Start rendering to texture
                BeginTextureMode(Render);

                // Define default background color
                ClearBackground(Color.White);

                // Draw 3D environement
                Conceptor3D.Draw();

                // Close rendering to texture
                EndTextureMode();

                // Start rendering to texture
                BeginTextureMode(HologramRender);

                ClearBackground(Color.Black);

                // Re-open 3d mode
                BeginMode3D(Conceptor3D.View.Camera);

                // Earth hologram drawing
                EarthHologram.Draw();

                // Orion terminal drawing
                OrionSim.Draw();

                // Close rendering to texture
                EndTextureMode();

                // End 3D mode
                EndMode3D();

                // Draw Orion Terminal screen (render to texture for next render pass.)
                OrionSim.DrawTerminalScreen();

                // Begin screen rendering
                BeginTextureMode(FinalRender);

                // Draws the holographic elements of the scene and overlaps them with the current render
                Shaders.OverlapHologramRender();

                // Close final render pass
                EndTextureMode();

                // Begin Rendering to the screen
                BeginDrawing();

                // Apply final post-pro
                BeginShaderMode(Shaders.ChromaticAberrationShader);

                // Draw final render
                DrawTexturePro(FinalRender.Texture, SourceRender, DestinationRender, Vector2.Zero, 0, Color.White);

                EndShaderMode();

                // Draw 2D information
                Conceptor2D.Draw();

                // Close drawing context
                EndDrawing();
            }

            // Close window and unload raylib default resources
            CloseWindow();

            // Unloads the resources
            Resources.Close();
        }

        /// <summary>Loads the <see cref="RenderTexture2D"/> for post-processing effects.</summary>
        internal static void LoadRender()
        {
            Render = LoadRenderTexture(Width, Height);
            // Configure render
            FinalRender = LoadRenderTexture(Width, Height);
            SetTextureWrap(FinalRender.Texture, TextureWrap.Clamp);

            HologramRender = LoadRenderTexture(Width, Height);
            SourceRender = new Rectangle(0, 0, Width, -Height);
            DestinationRender = new Rectangle(0, 0, Width, Height);
        }
    }
}