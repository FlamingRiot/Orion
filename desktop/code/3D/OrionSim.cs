using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the Orion robot simulation.</summary>
    internal static class OrionSim
    {
        internal static float ViewerLatitude;
        internal static float ViewerLongitude;

        internal static Vector3 ViewerPosition;
        internal static Vector3 OriginPosition = new Vector3(-10.1f, 1.7f, -0.16f);
        internal static Vector3 TerminalPosition;
        internal static Vector3 PositionToBe;

        internal static Matrix4x4 Transform;
        private static RenderTexture2D TerminalScreen;
        private static Material TerminalScreenMat;

        /// <summary>Inits the Orion simulation robot.</summary>
        internal static void Init(float lat, float lon)
        {
            UpdateViewPoint(lat, lon);
            Transform = Raymath.MatrixTranslate(TerminalPosition.X, TerminalPosition.Y, TerminalPosition.Z);
            Transform *= Raymath.MatrixRotateZ(-40f / RAD2DEG);
            // Load screen render texture
            TerminalScreen = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
            TerminalScreenMat = LoadMaterialDefault();

            TerminalPosition = OriginPosition;
            PositionToBe = OriginPosition;
        }

        /// <summary>Updates the viewer's position.</summary>
        /// <param name="lat">New latitude.</param>
        /// <param name="lon">New longitude.</param>
        internal static void UpdateViewPoint(float lat, float lon)
        {
            ViewerLatitude = lat;
            ViewerLongitude = lon;
            ViewerPosition = CelestialMaths.ComputeECEFTilted(lat, lon, EarthHologram.IYaw) * EarthHologram.HOLOGRAM_RADIUS;
        }

        internal static void Draw()
        {
            TerminalPosition = Raymath.Vector3Lerp(TerminalPosition, PositionToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();

            DrawMesh(Resources.Meshes["screen"], TerminalScreenMat, Transform); // Draw screen with shader

            // Draw arrow
            Vector3 target = Vector3.Normalize(Vector3.Subtract(EarthHologram.Satellite.RelativePosition, ViewerPosition));
            DrawLine3D(Vector3.UnitY * 12, target * 2 + Vector3.UnitY * 12, Color.Red);
        }

        /// <summary>Draws the orion terminal screen to a render texture and applies it to a material.</summary>
        internal static void DrawTerminalScreen()
        {
            // Open texture-mode
            BeginTextureMode(TerminalScreen);

            ClearBackground(Color.SkyBlue);



            // Close texture-mode
            EndTextureMode();

            // Set material texture
            SetMaterialTexture(ref TerminalScreenMat, MaterialMapIndex.Diffuse, TerminalScreen.Texture);
        }

        /// <summary>Updates the transform of the hologram screen.</summary>
        internal static void UpdateTransform()
        {
            Transform = Raymath.MatrixTranslate(TerminalPosition.X, TerminalPosition.Y, TerminalPosition.Z);
            Transform *= Raymath.MatrixRotateZ(-40f / RAD2DEG);
        }
    }
}