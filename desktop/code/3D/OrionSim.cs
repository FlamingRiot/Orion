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
        Pluto
    }

    /// <summary>Represents the Orion robot simulation.</summary>
    internal static class OrionSim
    {
        internal const float INCLINE_YAW = 40f; // Represents the terminal-screen's orientation (vertically)

        internal static readonly Vector3 ArrowSource = EarthHologram.GlobeCenter + Vector3.UnitY * 2; // Defines the origin of the 3D pointing-arrow

        internal static float ViewerLatitude;
        internal static float ViewerLongitude;
        internal static Vector3 ViewerPosition; // Current simulation viewpoint
        internal static Vector3 ArrowTarget; // Defines the orientation of the arrow, relative to the horizontal plane
        internal static AstralTarget Target;

        internal static Vector3 OriginPosition = new Vector3(-10.1f, 1.7f, -0.16f); // Original position (never to be changed)
        internal static Vector3 TerminalPosition; // Current position
        internal static Vector3 PositionToBe; // Used for interpolation
        internal static float IYaw, IPitch, IYawToBe, IPitchToBe; // Used for interpolation

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

            Transform = Raymath.MatrixTranslate(TerminalPosition.X, TerminalPosition.Y, TerminalPosition.Z);
            Transform *= Raymath.MatrixRotateZ(INCLINE_YAW / RAD2DEG);
            IYaw = INCLINE_YAW;
            IYawToBe = INCLINE_YAW;
            // Load screen render texture
            TerminalScreen = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
            TerminalScreenMat = LoadMaterialDefault();
            TerminalScreenMat.Shader = Shaders.ScreenShader;

            TerminalPosition = OriginPosition;
            PositionToBe = OriginPosition;

            string? targetName = Enum.GetName(Target);
            Resources.TargetPreview = LoadTexture($"assets/textures/previews/{targetName}.png");

            // Define sim-to-screen ration
            float x = GetScreenWidth() / 17;
            float y = GetScreenHeight() / 17;
            ScreenRelatedRender = new Rectangle(x, y, GetScreenWidth() - x * 2, GetScreenHeight() - y * 2);

            ComputeArrowDirection();
        }

        /// <summary>Updates the viewer's position.</summary>
        /// <param name="lat">New latitude.</param>
        /// <param name="lon">New longitude.</param>
        internal static void UpdateViewPoint()
        {
            ViewerPosition = CelestialMaths.ComputeECEFTilted(ViewerLatitude, ViewerLongitude, EarthHologram.IYaw) * EarthHologram.HOLOGRAM_RADIUS;
        }

        /// <summary>Draws the terminal screen along with the 3D pointing arrow.</summary>
        internal static void Draw()
        {
            TerminalPosition = Raymath.Vector3Lerp(TerminalPosition, PositionToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            IYaw = Raymath.Lerp(IYaw, IYawToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            IPitch = Raymath.Lerp(IPitch, IPitchToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();
            
            DrawMesh(Resources.Meshes["screen"], TerminalScreenMat, Transform); // Draw screen with shader
            // Draw Pointing-Arrow
            DrawSphere(ArrowSource, 0.03f, Color.White);
            DrawLine3D(ArrowSource, ArrowTarget + ArrowSource, new Color(0, 177, 252));
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
            Vector3 direction = Raymath.Vector3Subtract(EarthHologram.Satellite.RelativePosition * (EarthHologram.HOLOGRAM_RADIUS + EarthHologram.RelativeSatelliteAltitude) + EarthHologram.GlobeCenter, ViewerPosition + EarthHologram.GlobeCenter);
            direction = Raymath.Vector3RotateByAxisAngle(direction, new Vector3(-ViewerPosition.Z, 0, ViewerPosition.X), EarthHologram.VerticalAngle * DEG2RAD);
            ArrowTarget = Raymath.Vector3Normalize(direction);
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
            Transform *= Raymath.MatrixRotateY(IPitch / RAD2DEG);
            Transform *= Raymath.MatrixRotateZ(-IYaw / RAD2DEG);
        }
    }
}