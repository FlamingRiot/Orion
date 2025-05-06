using Raylib_cs;
using static Raylib_cs.Raylib;
using RayGUI_cs;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Defines the currently targeted astral object. (With ids corresponding to the API's)</summary>
    internal enum AstralTarget
    {
        ISS,
        Mercury,
        Venus,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Pluto,
        Sun,
        Moon
    }

    /// <summary>Represents the Orion robot simulation.</summary>
    internal static class OrionSim
    {
        // Constants
        internal const float INCLINE_YAW = 40f; // Represents the terminal-screen's orientation (vertically)
        internal static readonly Vector3 TERMINAL_ORIGIN = new Vector3(-10.1f, 1.7f, -0.16f);
        internal static readonly Vector3 ARROW_SOURCE = EarthHologram.GlobeCenter + Vector3.UnitY * 2; // Defines the origin of the 3D pointing-arrow

        // Simulation attributes, such as view-positin and pointing-arrow information
        internal static float ViewerLatitude, ViewerLongitude;
        internal static Vector3 ViewerPosition; // Current simulation viewpoint
        internal static Vector3 ArrowTarget; // Defines the orientation of the arrow, relative to the horizontal plane
        internal static AstralTarget Target;

        // Orion terminal attributes
        internal static Vector3 TerminalPosition; // Current position
        internal static float Yaw, Pitch; // Rotation angles of the terminal

        // Orion Robot Angles
        internal static float RobotYaw, RobotPitch;

        // Render-related attributes
        internal static Rectangle ScreenRelatedRender;
        internal static Matrix4x4 Transform;
        private static RenderTexture2D TerminalScreen;
        private static Material TerminalScreenMat;

        /// <summary>Inits the Orion simulation robot.</summary>
        internal static void Init(float lat, float lon)
        {
            // Initialize view point
            ViewerLongitude = lon;
            ViewerLatitude = lat;
            UpdateViewPoint();

            // Set world attributes
            Transform = Raymath.MatrixTranslate(TerminalPosition.X, TerminalPosition.Y, TerminalPosition.Z);
            Transform *= Raymath.MatrixRotateZ(INCLINE_YAW / RAD2DEG);
            Yaw = INCLINE_YAW;
            TerminalPosition = TERMINAL_ORIGIN;

            // Set interpolators
            Interpolators.TerminalYaw = INCLINE_YAW;
            Interpolators.TerminalCenter = TERMINAL_ORIGIN;

            // Load screen render texture
            TerminalScreen = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
            TerminalScreenMat = LoadMaterialDefault();
            TerminalScreenMat.Shader = Shaders.ScreenShader;

            // Load default image preview
            string? targetName = Enum.GetName(Target);
            Resources.TargetPreview = LoadTexture($"assets/textures/previews/{targetName}.png");

            // Define sim-to-screen ratio
            float x = GetScreenWidth() / 17;
            float y = GetScreenHeight() / 17;
            ScreenRelatedRender = new Rectangle(x, y, GetScreenWidth() - x * 2, GetScreenHeight() - y * 2);

            // Compute arrow direction for stand-by position
            ComputeArrowDirection();
        }

        /// <summary>Updates the viewer's position.</summary>
        internal static void UpdateViewPoint()
        {
            ViewerPosition = CelestialMaths.ComputeECEFTilted(ViewerLatitude, ViewerLongitude, EarthHologram.Yaw) * EarthHologram.HOLOGRAM_RADIUS;
            ComputeArrowDirection();
        }

        /// <summary>Draws the terminal screen along with the 3D pointing arrow.</summary>
        internal static void Draw()
        {
            TerminalPosition = Raymath.Vector3Lerp(TerminalPosition, Interpolators.TerminalCenter, GetFrameTime() * Conceptor3D.LERP_SPEED);
            Yaw = Raymath.Lerp(Yaw, Interpolators.TerminalYaw, GetFrameTime() * Conceptor3D.LERP_SPEED);
            Pitch = Raymath.Lerp(Pitch, Interpolators.TerminalPitch, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();
            
            DrawMesh(Resources.Meshes["screen"], TerminalScreenMat, Transform); // Draw screen with shader

            DrawModel(Resources.Arrow, Vector3.Zero, 1, Color.White);
        }

        /// <summary>Draws the orion terminal screen to a render-texture and applies it to a material.</summary>
        internal static void DrawTerminalScreen()
        {
            // Open texture-mode
            BeginTextureMode(TerminalScreen);

            // Blue background for better hologram integration
            ClearBackground(Color.Blue);

            // Earth texture on the left
            DrawTexture(Resources.TargetPreview, 100, 250, Color.White);

            // Draw GUI
            Conceptor2D.TerminalGui.Draw();

            // Close texture-mode
            EndTextureMode();

            // Set material texture
            SetMaterialTexture(ref TerminalScreenMat, MaterialMapIndex.Diffuse, TerminalScreen.Texture);
        }

        /// <summary>Computes the direction of the 3D pointing arrow.</summary>
        internal static void ComputeArrowDirection()
        {
            // Direction vector based on visual appearance
            if (Target == AstralTarget.ISS) 
            {
                // This right here is one of the most mind-fucking thing i've ever done in programming - but hey, it works
                Vector3 west = Vector3.Normalize(Raymath.Vector3CrossProduct(EarthHologram.GLOBE_NORTH, ViewerPosition));
                Vector3 north = Raymath.Vector3CrossProduct(ViewerPosition, west);

                // Calculate raw direction
                Vector3 direction = Raymath.Vector3Subtract(EarthHologram.Satellite.RelativePosition * (EarthHologram.HOLOGRAM_RADIUS + EarthHologram.RelativeSatelliteAltitude), ViewerPosition);

                // Project on local base
                float westComponent = Raymath.Vector3DotProduct(direction, west);
                float northComponent = Raymath.Vector3DotProduct(direction, north);
                float upComponent = Raymath.Vector3DotProduct(direction, ViewerPosition);

                Vector3 local = new Vector3(northComponent, upComponent, westComponent);
                ArrowTarget = Raymath.Vector3Normalize(local);

                // Calculate angles used for physical and virtual display
                Vector3 arrow = Vector3.Normalize(ArrowTarget);

                RobotYaw = 90 - MathF.Asin(arrow.Y) * RAD2DEG;
                RobotPitch = MathF.Atan2(arrow.X, arrow.Z) * RAD2DEG;
                // Tweak pitch
                RobotPitch = (RobotPitch + 360 + 180) % 360; // 180° correction to correspond the chosen system for the app
            }
            else
            {
                ArrowTarget = EarthHologram.CurrentPlanet.NormalizedPosition;
                RobotPitch = 270 - EarthHologram.CurrentPlanet.Azimuth; // 270° correction to correspond the chosen system for the app
                RobotYaw = 90 - EarthHologram.CurrentPlanet.Altitude; // Invert the vertical system 
            }

            // Adapt arrow
            Matrix4x4 rotation = Raymath.MatrixRotateY(RobotPitch * DEG2RAD);
            rotation *= Raymath.MatrixRotateX(-RobotYaw * DEG2RAD);
            Matrix4x4 position = Raymath.MatrixTranslate(ARROW_SOURCE.X, ARROW_SOURCE.Y, ARROW_SOURCE.Z);
            Matrix4x4 scale = Raymath.MatrixScale(3f, 3f, 3f);
            Resources.SetArrowTransform(position * scale * rotation);
        }

        /// <summary>Moves the targeted astral object to the right or left.</summary>
        /// <param name="delta">Index delta (-1 or 1).</param>
        internal static void SwitchTarget(int delta)
        {
            int current = (int)Target;
            Target = (AstralTarget)Enum.ToObject(typeof(AstralTarget), current + delta);

            string? targetName = Enum.GetName(Target);
            if (targetName == null) // Check for null
            {
                if (delta == -1) Target = Enum.GetValues<AstralTarget>().Cast<AstralTarget>().Last();
                else Target = (AstralTarget)Enum.ToObject(typeof(AstralTarget), 0);
            }
            targetName = Enum.GetName 
               (Target);
            // Unload previous
            UnloadTexture(Resources.TargetPreview);            
            // Load new
            Resources.TargetPreview = LoadTexture($"assets/textures/previews/{targetName}.png");
            // Udate components
            Conceptor2D.TerminalGui.Clear();
            Conceptor2D.ConstructUI();
            // Set textbox text
            ((Textbox)Conceptor2D.TerminalGui["nameTxb"]).Text = $"{targetName}";
            // Update API target
            EarthHologram.UpdatePlanet(Target);
        }

        /// <summary>Parses a value to potentially update the orion target (resets the textbox if no matching found).</summary>
        /// <param name="args">Arguments passed from the textbox.</param>
        /// <param name="value">Textbox value.</param>
        internal static void VerifiyTargetEntry(string[] args, string value)
        {
            if (Enum.IsDefined(typeof(AstralTarget), value))
            {
                Target = (AstralTarget)Enum.Parse(typeof(AstralTarget), value);
                UnloadTexture(Resources.TargetPreview);
                // Load new
                Resources.TargetPreview = LoadTexture($"assets/textures/previews/{value}.png");
                // Udate components
                Conceptor2D.TerminalGui.Clear();
                Conceptor2D.ConstructUI();
                // Update API target
                EarthHologram.UpdatePlanet(Target);
            }
            else
            {
                ((Textbox)Conceptor2D.TerminalGui["nameTxb"]).Text = $"{Enum.GetName(typeof(AstralTarget), Target)}";
            }
        }

        /// <summary>Updates the transform of the hologram screen.</summary>
        private static void UpdateTransform()
        {
            Transform = Raymath.MatrixTranslate(TerminalPosition.X, TerminalPosition.Y, TerminalPosition.Z);
            Transform *= Raymath.MatrixRotateY(Pitch / RAD2DEG);
            Transform *= Raymath.MatrixRotateZ(-Yaw / RAD2DEG);
        }
    }
}