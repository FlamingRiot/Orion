using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the static class of the earth hologram.</summary>
    internal static class EarthHologram
    {
        internal const float HOLOGRAM_RADIUS = 0.8f;
        private static Matrix4x4 _globeCorrectionMat;

        internal static Vector3 ORIGIN; // no modification
        internal static Vector3 CENTER_TO_BE; // Used for interface interpolation
        internal static Vector3 CENTER;
        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static Satellite Satellite;
        internal static bool InterfaceActive = false;

        /// <summary>Inits the earth hologram.</summary>
        public static void Init()
        {
            CENTER = new Vector3(-3.5f, 2, 0.2f);
            ORIGIN = CENTER;
            CENTER_TO_BE = CENTER;
            // Create standby position
            SatellitePoints.Add((CENTER + Vector3.UnitX) * (HOLOGRAM_RADIUS + 0.1f));
            Satellite = new Satellite();
            // Start by sending information request to the API
            OnlineRequests.StartConnexion();
            UpdateSatellite();
            // Create globe correction matrix
            UpdateTransform();
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateSatellite()
        {
            await OnlineRequests.UpdateCurrentSatellite();
            Satellite.RelativePosition = Raymath.Vector3Lerp(Satellite.RelativePosition, SatellitePoints.Last(), (float)GetFrameTime());
        }

        /// <summary>Draws the earth hologam.</summary>
        internal static void Draw()
        {
            // Update lerp
            CENTER = Raymath.Vector3Lerp(CENTER, CENTER_TO_BE, GetFrameTime() * 3);
            UpdateTransform();

            // Update satellite
            UpdateSatellite();

            // Draw earth hologram box
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], _globeCorrectionMat);

            // Draw satellite point
            DrawSphere(Satellite.RelativePosition + CENTER, 0.02f, Color.Yellow);

            // Draw current position
            DrawSphere(CelestialMaths.ComputeECEFTilted(CelestialMaths.POSITION_LATITUDE, CelestialMaths.POSITION_LONGITUDE) * (HOLOGRAM_RADIUS + 0.1f) + CENTER, 0.02f, Color.Green);

            DrawLine3D(CENTER, Satellite.RelativePosition + CENTER, Color.Red);
        }

        /// <summary>Updates the earth hologram matrix</summary>
        internal static void UpdateTransform()
        {
            // Calculate matrix rotation
            Matrix4x4 rm = Raymath.MatrixRotateXYZ(new Vector3(90, 23.44f, 0) / RAD2DEG);
            Matrix4x4 sm = Raymath.MatrixScale(1, 1, 1);
            Matrix4x4 pm = Raymath.MatrixTranslate(CENTER.X, CENTER.Y, CENTER.Z);
            // Multiply matrices
            _globeCorrectionMat = pm * sm * rm;
        }
    }
}