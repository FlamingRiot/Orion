using Raylib_cs;
using static Raylib_cs.Raylib;
using Orion_Desktop;

namespace Orion
{
    /// <summary>Represents an instance of the running program.</summary>
    public class Program
    {
        // Constants
        public const string APP_NAME = "Orion";
        public const string APP_VERSION = "0.1.0-alpha";  

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

            // Program loop
            //SetTargetFPS(60);
            DisableCursor();
            while (!WindowShouldClose())
            {
                // Update functions
                Conceptor3D.Update();
                
                // Open drawing context
                BeginDrawing();
                
                // Define default background color
                ClearBackground(Color.White);

                // Draw functions
                Conceptor3D.Draw();

                // Draw debug framerate
                DrawFPS(10, 10);    

                // Close drawing context
                EndDrawing();
            }

            // Close window and unload raylib default resources
            CloseWindow();
        }
    }
}