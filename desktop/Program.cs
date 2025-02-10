using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the running program.</summary>
    public class Program
    {
        // Constants
        public const string APP_NAME = "Orion";
        public const string APP_VERSION = "0.1.0-alpha";

        internal static RenderTexture2D Render;
        internal static Rectangle SourceRender;
        internal static Rectangle DestinationRender;

        /// <summary>Defines the entry point of the program.</summary>
        /// <param name="args">Arguments passed from outside the progam.</param>
        public static void Main(string[] args)
        {
            // Open base window
            InitWindow(1600, 1000, $"{APP_NAME} {APP_VERSION}");
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);

            // Open different services
            Conceptor3D.Init();
            Conceptor2D.Init();

            // Load render texture
            LoadRender();

            // Program loop
            //SetTargetFPS(60);
            DisableCursor();
            while (!WindowShouldClose())
            {
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

                // Begin screen rendering
                BeginDrawing();

                // Start post-processing shader
                BeginShaderMode(Shaders.PostProShader);

                // Update render texture used for motion blur
                Shaders.UpdateRenderTexture(Render);

                // Draw Render Texture
                DrawTexturePro(Render.Texture, SourceRender, DestinationRender, Vector2.Zero, 0, Color.White);

                // End post-processing shader
                EndShaderMode();

                // Draw 2D information
                Conceptor2D.Draw();

                // Draw debug framerate
                DrawFPS(10, 10);    

                // Close drawing context
                EndDrawing();
            }

            // Close window and unload raylib default resources
            CloseWindow();
        }

        /// <summary>Loads the <see cref="RenderTexture2D"/> for post-processing effects.</summary>
        internal static void LoadRender()
        {
            int width = GetScreenWidth();
            int height = GetScreenHeight(); 

            Render = LoadRenderTexture(width, height);
            SourceRender = new Rectangle(0, 0, width, -height);
            DestinationRender = new Rectangle(0, 0, width, height);
        }
    }
}