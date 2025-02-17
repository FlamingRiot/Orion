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

        private static Matrix4x4 _transform;

        /// <summary>Inits the Orion simulation robot.</summary>
        internal static void Init(float lat, float lon)
        {
            UpdateViewPoint(lat, lon);
            _transform = Raymath.MatrixTranslate(-10.1f, 1.7f, -0.16f);
            _transform *= Raymath.MatrixRotateZ(-40f / RAD2DEG);
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

        /// <summary>Draws the Orion robot simulation.</summary>
        internal static void Draw()
        {
            DrawMesh(Resources.Meshes["screen"], Resources.Materials["screen"], _transform); // Draw screen with shader

            // Draw arrow
            Vector3 target = Vector3.Normalize(Vector3.Subtract(EarthHologram.Satellite.RelativePosition, ViewerPosition));
            DrawLine3D(Vector3.UnitY * 12, target * 2 + Vector3.UnitY * 12, Color.Red);
        }
    }
}