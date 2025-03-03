using static Raylib_cs.Raylib;
using Raylib_cs;
using RayGUI_cs;
using static RayGUI_cs.RayGUI;
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

        internal static List<Component> Components = new List<Component>();

        /// <summary>Opens the 2D conceptor and loads its parameters.</summary>
        internal static void Init()
        {
            // Get window size
            Width = GetScreenWidth();
            Height = GetScreenHeight();
            Size = new Vector2(Width, Height);

            // GUI
            InitGUI(new Color(0, 225, 255), new Color(0, 135, 153), LoadFont("assets/textures/SpaceMono-Bold.ttf"));
            ConstructUI();
    
            OpenedInterface = Interface.None; // Defines the opened interface

            // Load font
            Font = LoadFont("assets/textures/Ubuntu-Bold.ttf");

            ActionRec = new Rectangle(Width / 2 - ACTION_REC_SIZE, Height / 2 - ACTION_REC_SIZE, ACTION_REC_SIZE, ACTION_REC_SIZE);
        }

        internal static Vector2 RetargetMousePosition(Rectangle source, Rectangle destination)
        {
            Vector2 mouse = GetMousePosition();
            float newX = destination.X + (mouse.X - source.X) * destination.Width / source.Width;
            float newY = destination.Y - (mouse.Y - source.Y) * destination.Height / source.Height;
            return new Vector2(newX, newY);
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
                        Vector2 mouse = RetargetMousePosition(Program.SourceRender, OrionSim.ScreenRelatedRender);
                        DrawRectangle((int)mouse.X, (int)mouse.Y, 7, 7, Color.White);
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
                            DisableCursor();
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

        /// <summary>Constructs the UI of the Orion Terminal.</summary>
        internal static void ConstructUI()
        {
            SetDefaultFontSize(64);
            
            // Static fields
            Button leftButton = new Button("<", 800, 800, 50, 50);
            leftButton.Event = OrionSim.SwitchTargetLeft;
            Components.Add(leftButton);
            Button rightButton = new Button(">", 1300, 800, 50, 50);
            rightButton.Event = OrionSim.SwitchTargetRight;
            Components.Add(rightButton);
            Textbox nameTxb = new Textbox(855, 800, 440, 50, $"{OrionSim.Target}");
            nameTxb.OnEntry = OrionSim.VerifiyTargetEntry;
            Components.Add(nameTxb);
            Label lblTerminal = new Label(780, 50, "Orion Terminal");
            Components.Add(lblTerminal);
            Label lblTarget = new Label(205, 170, "Current Target:");
            Components.Add(lblTarget);

            // Non-static fields
            Label lblName = new Label(800, 170, $"Name : {OrionSim.Target}");
            Components.Add(lblName);
            Label lblLat = new Label(800, 220, $"Latitude : {EarthHologram.Satellite.Latitude}");
            Components.Add(lblLat);
            Label lblLong = new Label(800, 270, $"Longitude : {EarthHologram.Satellite.Longitude}");
            Components.Add(lblLong);
            Label lblDistance = new Label(800, 320, $"Distance from Earth : {EarthHologram.Satellite.Altitude} km");
            Components.Add(lblDistance);
        }

        internal static void UpdateUI()
        {
            ((Label)Components[6]).Text = $"Latitude : {EarthHologram.Satellite.Latitude}";
            ((Label)Components[7]).Text = $"Longitude : {EarthHologram.Satellite.Longitude}";
            ((Label)Components[8]).Text = $"Distance from Earth : {EarthHologram.Satellite.Altitude} km";
        }
    }
}
