using static Raylib_cs.Raylib;
using Raylib_cs;
using RayGUI_cs;
using static RayGUI_cs.RayGUI;
using System.Numerics;
using System.Security.AccessControl;

namespace Orion_Desktop
{
    /// <summary>Represents the 2D conceptor of the game.</summary>
    internal static partial class Conceptor2D
    {
        /// <summary>Defines the type of opened interface.</summary>
        internal enum Interface
        {
            None,
            Earth,
            Terminal
        }

        /*
         Directions: 
         -X : South
         +X : North
         -Z : West
         +Z : East
         */

        internal const int ACTION_REC_SIZE = 30;
        internal const int SCREEN_RATIO = 1000;

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

        // Gui menu
        internal static GuiContainer TerminalGui = new GuiContainer();

        // Compass Render-textures and rectangles
        internal static RenderTexture2D CompassRenderTexture;
        private static Rectangle CompassDestinationRectangle;

        /// <summary>Opens the 2D conceptor and loads its parameters.</summary>
        internal static void Init()
        {
            // Get window size
            Width = GetScreenWidth();
            Height = GetScreenHeight();
            Size = new Vector2(Width, Height);

            // Load GUi
            TerminalGui = new GuiContainer(new Color(0, 225, 255), new Color(0, 190, 190));
            LoadGUI(LoadFont("assets/textures/SpaceMono-Bold.ttf"));
            ConstructUI();

            // Load compass render-texure and render-rectangles
            CompassDestinationRectangle = new Rectangle(0, 0, Width, Height);
            CompassRenderTexture = LoadRenderTexture((int)CompassDestinationRectangle.Width, (int)CompassDestinationRectangle.Height);

            OpenedInterface = Interface.None; // Defines the opened interface

            // Load font
            Font = LoadFont("assets/textures/Ubuntu-Bold.ttf");

            ActionRec = new Rectangle(Width / 2 - ACTION_REC_SIZE, Height / 2 - ACTION_REC_SIZE, ACTION_REC_SIZE, ACTION_REC_SIZE);

            DeactivateGui(TerminalGui); // Deactive cause not in the interface right off the bat
        }

        internal static Vector2 RetargetMousePosition(Rectangle source, Rectangle destination)
        {
            Vector2 mouse = GetMousePosition();
            float newX = destination.X + (mouse.X - source.X) * destination.Width / source.Width;
            float newY = destination.Y - (mouse.Y - source.Y) * destination.Height / source.Height;
            return new Vector2(newX, newY);
        }

        /// <summary>Draws the compass at the top of the screen. (Context: Drawing or no Drawing).</summary>
        private static void DrawCompass()
        {
            BeginTextureMode(CompassRenderTexture);

            ClearBackground(Color.Black);

            // Measure text
            string txt = $"45  |  60  |  75  |  E  |  105  |  120  |  135  |  150  |  165  |  S  |  195  |  210  |  225  |  240  |  255  |  W  " +
                $"|  285  |  300  |  315  |  330  |  345  |  N  |  15  |  30  |  45  |  60  |  75  |  E  |  105  |  120  |";
            Vector2 txtSize = MeasureTextEx(Font, txt, 30, 1); // The width is equivalent to 360° as a rotation angle
            float angle = Conceptor3D.View.Yaw * RAD2DEG * -1; // [0° to 360°]
            float ratio = (txtSize.X + 128) / 360f; // Constant offset of 128 (Thankfully does not rely on display size)
            float center = Width / 2 - 300; // Constant offset of -300 (Thankfully does not rely on display size)
            // Draw text
            DrawTextPro(RayGUI.Font, txt, new Vector2(center - angle * ratio, 20), Vector2.Zero, 0, 30, 1, Color.White);

            EndTextureMode();
        }

        /// <summary>Displays 2D information to the screen.</summary>
        internal static void Draw()
        {
            Update();

            // Draw compass at the top of the screen
            DrawCompass();

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
        }

        /// <summary>Updates most of the actions of the 2D conceptor.</summary>
        private static void Update()
        {
            // Update action sounds
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                TerminalGui.ForEach(x =>
                {
                    if (Hover(x)) AudioCenter.PlaySound("button_click");
                });
            }

            if (IsKeyPressed(KeyboardKey.E))
            {
                if (InteractiveEnabled && !InterfaceActive)
                {
                    AudioCenter.PlaySound("button_click");
                    // Defines which interface to open
                    switch (OpenedInterface)
                    {
                        case Interface.Earth:
                            InterfaceActive = true;
                            // Define 3D post based current camera pos
                            Ray dir = GetScreenToWorldRay(new Vector2(Width / 3f, Height / 2), Conceptor3D.View.Camera);
                            Vector3 pos = dir.Position + dir.Direction * 2.5f;
                            EarthHologram.GlobeCenterToBe = pos;
                            EarthHologram.BackupCameraPosition = Conceptor3D.View.Camera.Position;
                            EarthHologram.BackupCameraTarget = Conceptor3D.View.Camera.Target;
                            EnableCursor();
                            break;
                        case Interface.Terminal:

                            ActivateGui(TerminalGui);

                            InterfaceActive = true;
                            Ray center = GetScreenToWorldRay(Size / 2, Conceptor3D.View.Camera);
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
                    // Check if no textbox's opened
                    bool focus = false;
                    TerminalGui.ForEach(x =>
                    {
                        if (x is Textbox textbox) if (textbox.Focus) focus = true;
                    });
                    // Close only if no textbox's opened
                    if (!focus)
                    {
                        if (!EarthHologram.IsFocused)
                        {
                            if (Raymath.Vector3Length(Conceptor3D.View.Camera.Position - EarthHologram.BackupCameraPosition) < 0.1f)
                            {
                                InterfaceActive = false;
                                if (OpenedInterface == Interface.Earth)
                                {
                                    EarthHologram.GlobeCenterToBe = EarthHologram.GlobeOrigin;
                                    EarthHologram.IPitchToBe = 0;
                                    EarthHologram.IYawToBe = 0;
                                }
                                OpenedInterface = Interface.None;
                                DisableCursor();
                            }
                        }
                        if (EarthHologram.IsFocused)
                        {
                            EarthHologram.IsFocused = false;
                        }
                        if (OpenedInterface == Interface.Terminal)
                        {
                            DeactivateGui(TerminalGui);

                            InterfaceActive = false;
                            OrionSim.PositionToBe = OrionSim.OriginPosition;
                            OrionSim.IYawToBe = OrionSim.INCLINE_YAW;
                            OrionSim.IPitchToBe = 0;
                            DisableCursor();
                            OpenedInterface = Interface.None;
                        }
                    }
                }
            }
        }

        /// <summary>Constructs the UI of the Orion Terminal.</summary>
        internal static void ConstructUI()
        {
            TerminalGui.SetDefaultFontSize(64);

            // Static fields
            Button leftButton = new Button(800, 800, 50, 50, "<");
            leftButton.Event = SwitchTargetLeft;
            TerminalGui.Add("leftButton", leftButton);
            Button rightButton = new Button(1300, 800, 50, 50, ">");
            rightButton.Event = SwitchTargetRight;
            TerminalGui.Add("rightButton", rightButton);
            Button submitButton = new Button(800, 730, 550, 50, "Submit");
            TerminalGui.Add("submitButton", submitButton);
            Textbox nameTxb = new Textbox(855, 800, 440, 50, $"{OrionSim.Target}");
            nameTxb.OnEntry = OrionSim.VerifiyTargetEntry;
            TerminalGui.Add("nameTxb", nameTxb);
            Label lblTerminal = new Label(780, 50, "Orion Terminal");
            TerminalGui.Add("lblTerminal", lblTerminal);
            Label lblTarget = new Label(205, 170, "Current Target:");
            TerminalGui.Add("lblTarget", lblTarget);

            // Non-static fields
            Label lblName = new Label(700, 170, $"Name : {OrionSim.Target}");
            TerminalGui.Add("lblName", lblName);
            Label lblLat = new Label(700, 220, $"Latitude : {EarthHologram.Satellite.Latitude}");
            TerminalGui.Add("lblLat", lblLat);
            Label lblLong = new Label(700, 270, $"Longitude : {EarthHologram.Satellite.Longitude}");
            TerminalGui.Add("lblLong", lblLong);
            Label lblDistance = new Label(700, 320, $"Distance from Earth : {EarthHologram.Satellite.Altitude} km");
            TerminalGui.Add("lblDistance", lblDistance);

            Label lblViewPoint = new Label(700, 390, $"Current Earth viewpoint :");
            TerminalGui.Add("lblViewPoint", lblViewPoint);
            Label lblCurrentLat = new Label(700, 450, $"Latitude (North)");
            TerminalGui.Add("lblCurrentLat", lblCurrentLat);
            Label lblCurrentLong = new Label(700, 510, $"Longitude (East)");
            TerminalGui.Add("lblCurrentLong", lblCurrentLong);

            Textbox txbCurrentLat = new Textbox(1160, 430, 360, 45, $"{OrionSim.ViewerLatitude}");
            txbCurrentLat.OnEntry = UpdateLatitude;
            TerminalGui.Add("txbCurrentLat", txbCurrentLat);
            Textbox txbCurrentLon = new Textbox(1160, 490, 360, 45, $"{OrionSim.ViewerLongitude}");
            txbCurrentLon.OnEntry = UpdateLongitude;
            TerminalGui.Add("txbCurrentLon", txbCurrentLon);

            TerminalGui.SetDefaultRoundness(0.25f);
        }

        /// <summary>Updates the UI components based on real-time values from the retrievement point.</summary>
        internal static void UpdateUI()
        {
            if (OrionSim.Target == AstralTarget.ISS)
            {
                ((Label)TerminalGui["lblLat"]).Text = $"Latitude : {EarthHologram.Satellite.Latitude}";
                ((Label)TerminalGui["lblLong"]).Text = $"Longitude : {EarthHologram.Satellite.Longitude}";
                ((Label)TerminalGui["lblDistance"]).Text = $"Distance from Earth : {EarthHologram.Satellite.Altitude} Km";
            }
            else
            {
                ((Label)TerminalGui["lblLat"]).Text = $"Altitude : {EarthHologram.CurrentPlanet.Altitude}";
                ((Label)TerminalGui["lblLong"]).Text = $"Azimuth : {EarthHologram.CurrentPlanet.Azimuth}";
                ((Label)TerminalGui["lblDistance"]).Text = $"Distance from Earth : {EarthHologram.CurrentPlanet.Distance} AU";
            }
        }
    }
}
