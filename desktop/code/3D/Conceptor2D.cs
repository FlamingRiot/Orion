using static Raylib_cs.Raylib;
using Raylib_cs;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the 2D conceptor of the game.</summary>
    internal class Conceptor2D
    {
        /// <summary>Defines the type of opened interface.</summary>
        internal enum Interface
        {
            None,
            Earth,
            Terminal
        }

        internal const int ACTION_REC_SIZE = 30;
        internal const int SCREEN_RATIO = 700;

        // Defines if player is approaching interactible object
        internal static bool InteractiveEnabled;
        internal static Rectangle ActionRec;
        internal static Font Font;
        internal static Interface OpenedInterface;
        internal static bool InterfaceActive = false;

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

            OpenedInterface = Interface.None; // Defines the opened interface

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
                if (!InterfaceActive)
                {
                    DrawRectangleRounded(ActionRec, 0.5f, 5, new Color(222, 222, 222, 255));
                    DrawTextPro(Font, "E", ActionRec.Position, new Vector2(-10, -6), 0, 20, 1, new Color(66, 66, 66, 255));
                }
            }

            // Update earth hologram interface
            if (InterfaceActive)
            {
                switch (OpenedInterface)
                {
                    case Interface.Earth:
                        EarthHologram.UpdateInterface();
                        break;
                    case Interface.Terminal:
                        break;
                }
            }

            if (IsKeyPressed(KeyboardKey.E))
            {
                if (InteractiveEnabled && !InterfaceActive)
                {
                    // Defines which interface to open
                    switch (OpenedInterface)
                    {
                        case Interface.Earth:
                            InterfaceActive = true;
                            // Define 3D post based current camera pos
                            Ray dir = GetMouseRay(new Vector2(Width / 3f, Height / 2), Conceptor3D.View.Camera);
                            Vector3 pos = dir.Position + dir.Direction * 2.5f;
                            EarthHologram.CENTER_TO_BE = pos;
                            EnableCursor();
                            break;
                        case Interface.Terminal:
                            InterfaceActive = true;
                            Ray center = GetMouseRay(Size / 2, Conceptor3D.View.Camera);
                            pos = center.Position + center.Direction;
                            OrionSim.PositionToBe = pos;

                            // Define orientation angles
                            OrionSim.IYawToBe = (Conceptor3D.View.Pitch * RAD2DEG) + 90;
                            OrionSim.IPitchToBe = (Conceptor3D.View.Yaw * RAD2DEG) + 90;
                            EnableCursor();
                            break;
                    }
                }
                else
                {
                    InterfaceActive = false;
                    if (OpenedInterface == Interface.Earth)
                    {
                        EarthHologram.CENTER_TO_BE = EarthHologram.ORIGIN;
                        EarthHologram.IPitchToBe = 0;
                        EarthHologram.IYawToBe = 0;
                    }
                    else if (OpenedInterface == Interface.Terminal) 
                    {
                        OrionSim.PositionToBe = OrionSim.OriginPosition;
                        OrionSim.IYawToBe = OrionSim.INCLINE_YAW;
                        OrionSim.IPitchToBe = 0;
                    }
                    DisableCursor();
                }
            }
        }
    }
}
