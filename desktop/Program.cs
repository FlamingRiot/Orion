using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion
{
    /// <summary>Represents an instance of the running program.</summary>
    public class Program
    {
        // Constants
        public const string APP_NAME = "Orion";
        public const string APP_VERSION = "0.0.0-alpha";  

        /// <summary>Defines the entry point of the program.</summary>
        /// <param name="args">Arguments passed from outside the progam.</param>
        public static void Main(string[] args)
        {
            // Open base window
            InitWindow(800, 500, $"{APP_NAME} {APP_VERSION}");

            // Program loop
            while (!WindowShouldClose())
            {
                // Open drawing context
                BeginDrawing();

                // Define default background color
                ClearBackground(Color.Black);

                // Close drawing context
                EndDrawing();
            }

            // Close window and unload raylib default resources
            CloseWindow();
        }
    }
}