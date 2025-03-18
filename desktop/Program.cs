using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the running program.</summary>
    public class Program
    {
        // Constants
        public const string APP_NAME = "Orion";
        public const string APP_VERSION = "0.2.0-alpha";

        internal static RenderTexture2D Render;
        internal static RenderTexture2D HologramRender;
        internal static Rectangle SourceRender;
        internal static Rectangle DestinationRender;

        public static int Width;
        public static int Height;

        /// <summary>Defines the entry point of the program.</summary>
        /// <param name="args">Arguments passed from outside the progam.</param>
        public static void Main(string[] args)
        {
            // Open base window
            InitWindow(GetMonitorWidth(0), GetMonitorHeight(0), $"{APP_NAME} {APP_VERSION}");
            SetWindowState(ConfigFlags.ResizableWindow);

            MaximizeWindow();

            Width = GetScreenWidth();
            Height = GetScreenHeight();

            // Open different services
            AudioCenter.Init();
            Conceptor3D.Init();
            Conceptor2D.Init();

            // Load render texture
            LoadRender();

            //EarthHologram.UpdatePlanet();

            // Program loop
            //SetTargetFPS(60);
            SetExitKey(KeyboardKey.Null);
            DisableCursor();
            while (!WindowShouldClose())
            {
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

                // re open 3d mode
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
                BeginDrawing();

                // Draws the holographic elements of the scene and overlaps them with the current render
                Shaders.OverlapHologramRender();

                // Draw 2D information
                Conceptor2D.Draw();
#if DEBUG
                // Draw debug framerate
                DrawFPS(10, 10);
#endif
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
            HologramRender = LoadRenderTexture(Width, Height);
            SourceRender = new Rectangle(0, 0, Width, -Height);
            DestinationRender = new Rectangle(0, 0, Width, Height);
        }
    }
}