using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the static class of the earth hologram.</summary>
    internal static class EarthHologram
    {
        internal const float HOLOGRAM_RADIUS = 10;
        private static Matrix4x4 _globeCorrectionMat;
        private static int _posIndex = 0;

        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static Satellite Satellite;

        /// <summary>Inits the earth hologram.</summary>
        public static void Init()
        {
            // Create standby position
            SatellitePoints.Add(Vector3.UnitX * (HOLOGRAM_RADIUS + 1));
            Satellite = new Satellite();
            // Start by sending information request to the API
            OnlineRequests.StartConnexion();
            UpdateSatellite();
            // Create globe correction matrix
            _globeCorrectionMat = Raymath.MatrixRotateXYZ(new Vector3(90, 0, 0) / RAD2DEG);
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateSatellite()
        {
            await OnlineRequests.UpdateCurrentSatellite();
        }

        /// <summary>Draws the earth hologam.</summary>
        internal static void Draw()
        {
            // Update satellite
            UpdateSatellite();

            // Draw earth hologram box
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], _globeCorrectionMat);

            // Draw line
            DrawLine3D(Vector3.Zero, SatellitePoints.Last(), Color.Red);

            // Draw satellite point
            DrawSphere(SatellitePoints.Last(), 0.2f, Color.Yellow);
        }
    }
}