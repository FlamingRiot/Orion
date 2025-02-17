using static Raylib_cs.Raylib;
using Raylib_cs;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the 2D conceptor of the game.</summary>
    internal class Conceptor2D
    {
        internal const int ACTION_REC_SIZE = 30;

        // Defines if player is approaching interactible object
        internal static bool InteractiveEnabled;
        internal static Rectangle ActionRec;
        internal static Font Font;
        internal static RenderTexture2D OrionTerminalRender;

        // (Should be monitor size.)
        internal static int Width;
        internal static int Height;
        internal static Vector2 Size;

        /// <summary>Opens the 2D conceptor and loads its parameters.</summary>
        internal static void Init()
        {
            // Get window size
            Width = GetScreenWidth();
            Height = GetScreenHeight();
            Size = new Vector2(Width, Height);

            OrionTerminalRender = LoadRenderTexture(Width, Height); // Load orion terminal render

            // Load font
            Font = LoadFont("assets/textures/Ubuntu-Bold.ttf");

            ActionRec = new Rectangle(Width / 2 - ACTION_REC_SIZE, Height / 2 - ACTION_REC_SIZE, ACTION_REC_SIZE, ACTION_REC_SIZE);
        }

        /// <summary>Displays 2D information to the screen.</summary>
        internal static void Draw()
        {
            // Draw E hint
            if (InteractiveEnabled) 
            {
                if (!EarthHologram.InterfaceActive)
                {
                    DrawRectangleRounded(ActionRec, 0.5f, 5, new Color(222, 222, 222, 255));
                    DrawTextPro(Font, "E", ActionRec.Position, new Vector2(-10, -6), 0, 20, 1, new Color(66, 66, 66, 255));
                }
            }

            // Update earth hologram interface
            if (EarthHologram.InterfaceActive)
            {
                EarthHologram.UpdateInterface();
            }

            if (IsKeyPressed(KeyboardKey.E))
            {
                if (!EarthHologram.InterfaceActive && InteractiveEnabled)
                {
                    // Activate background blur
                    EarthHologram.InterfaceActive = true;
                    // Define 3D post based current camera pos
                    Ray dir = GetMouseRay(new Vector2(Width / 3f, Height / 2), Conceptor3D.View.Camera);
                    Vector3 pos = dir.Position + dir.Direction * 2.5f;
                    EarthHologram.CENTER_TO_BE = pos;
                    EnableCursor();
                }
                else
                {
                    EarthHologram.InterfaceActive = false;
                    EarthHologram.CENTER_TO_BE = EarthHologram.ORIGIN;
                    EarthHologram.IPitchToBe = 0;
                    EarthHologram.IYawToBe = 0;
                    DisableCursor();
                }
            }
        }
    }
}
