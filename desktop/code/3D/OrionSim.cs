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

        private static Model _hemisphere;

        /// <summary>Inits the Orion simulation robot.</summary>
        internal static void Init(float lat, float lon)
        {
            _hemisphere = LoadModelFromMesh(GenMeshHemiSphere(2, 15, 15));
            ViewerLatitude = lat;
            ViewerLongitude = lon;
            ViewerPosition = CelestialMaths.ComputeECEFTilted(CelestialMaths.POSITION_LATITUDE, CelestialMaths.POSITION_LONGITUDE) * (EarthHologram.HOLOGRAM_RADIUS + 1);
        }

        /// <summary>Draws the Orion robot simulation.</summary>
        internal static void Draw()
        {
            DrawModelWires(_hemisphere, Vector3.UnitY * 10, 1, Color.Gray);

            // Draw arrow
            Vector3 target = Vector3.Normalize(Vector3.Subtract(EarthHologram.Satellite.RelativePosition, ViewerPosition));
            DrawLine3D(Vector3.UnitY * 12, target * 2 + Vector3.UnitY * 12, Color.Red);
        }
    }
}