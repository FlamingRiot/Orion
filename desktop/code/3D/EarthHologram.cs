using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the static class of the earth hologram.</summary>
    internal static class EarthHologram
    {
        // Constants
        internal const float HOLOGRAM_RADIUS = 0.8f;
        internal const float EARTH_TILT = 23.44f;
        internal const float LERP_SPEED = 3f;

        internal static Matrix4x4 GlobeRotationMat; // Earth globe rotation matrix
        internal static Vector3 ORIGIN; // unmodified center, ever
        internal static Vector3 CENTER_TO_BE; // Used for interface interpolation
        internal static Vector3 CENTER; // current center
        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static Satellite Satellite; // Satellite object
        internal static bool InterfaceActive = false;

        internal static float IYaw, IPitch;
        internal static float PointLatitude, PointLongitude; // Simulation point coordinates

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
            Satellite.RelativePosition = CelestialMaths.ComputeECEFTilted(Satellite.Latitude, Satellite.Longitude, IYaw);
        }

        /// <summary>Draws the earth hologam.</summary>
        internal static void Draw()
        {
            // Update globe lerp
            CENTER = Raymath.Vector3Lerp(CENTER, CENTER_TO_BE, GetFrameTime() * LERP_SPEED);
            Shaders.Lights[0].Position = CENTER;
            Shaders.UpdateLight(Shaders.PBRLightingShader, Shaders.Lights[0]);
            UpdateTransform();

            // Update satellite
            UpdateSatellite();

            // Draw earth hologram box
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], GlobeRotationMat);

            // Draw satellite point
            DrawSphere(Satellite.RelativePosition * (HOLOGRAM_RADIUS + 0.1f) + CENTER, 0.02f, Color.Yellow);

            // Draw current position
            DrawSphere(OrionSim.ViewerPosition + CENTER, 0.02f, Color.Red);
#if DEBUG
            DrawLine3D(CENTER, Satellite.RelativePosition + CENTER, Color.Red);
#endif
        }

        /// <summary>Updates the earth hologram matrix</summary>
        internal static void UpdateTransform()
        {
            Matrix4x4 rm;
            if (!InterfaceActive) rm = Raymath.MatrixRotateXYZ(new Vector3(90, EARTH_TILT, 0) / RAD2DEG);
            else
            {
                rm = Raymath.MatrixRotateY((IYaw) / RAD2DEG);

                // Compute X/Z axis weights
                Vector3 cam = new Vector3(Conceptor3D.View.Camera.Position.X, 0, Conceptor3D.View.Camera.Position.Z) - 
                    new Vector3(Conceptor3D.View.Camera.Target.X, 0, Conceptor3D.View.Camera.Target.Z);

                float xWeight = Raymath.Vector3DotProduct(Vector3.UnitZ, Vector3.Normalize(cam));
                float zWeight = Raymath.Vector3DotProduct(Vector3.UnitX, Vector3.Normalize(cam));
                // Create weighted matrix
                rm *= Raymath.MatrixRotateXYZ(new Vector3(IPitch * xWeight + 90, EARTH_TILT, IPitch * zWeight) / RAD2DEG);
            }
            Matrix4x4 sm = Raymath.MatrixScale(1, 1, 1);
            Matrix4x4 pm = Raymath.MatrixTranslate(CENTER.X, CENTER.Y, CENTER.Z);
            // Multiply matrices
            GlobeRotationMat = pm * sm * rm;
        }

        /// <summary>Updates the interface of the hologram.</summary>
        internal static void UpdateInterface()
        {
            if (IsMouseButtonDown(MouseButton.Left)) // Drag
            {
                Vector2 mouse = GetMouseDelta() * 0.2f;
                IYaw += mouse.X;
                // Update view-point (relative to new rotation)
                OrionSim.UpdateViewPoint(PointLatitude, PointLongitude);
            }
            if (IsMouseButtonPressed(MouseButton.Left)) // Point click
            {
                RayCollision collision = GetRayCollisionSphere(GetMouseRay(GetMousePosition(), Conceptor3D.View.Camera), CENTER, HOLOGRAM_RADIUS);
                if (collision.Hit)
                {
                    (PointLatitude, PointLongitude) = CelestialMaths.ComputeECEFTiltedReverse((collision.Point - CENTER) / HOLOGRAM_RADIUS, IYaw);
                    OrionSim.UpdateViewPoint(PointLatitude, PointLongitude);
                }
            }
        }
    }
}