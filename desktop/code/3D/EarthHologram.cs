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

        internal static Matrix4x4 GlobeRotationMat; // Earth globe rotation matrix
        internal static Vector3 ORIGIN; // unmodified center, ever
        internal static Vector3 CENTER_TO_BE; // Used for interface interpolation
        internal static Vector3 CENTER; // current center
        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static Satellite Satellite; // Satellite object

        internal static float IYaw, IPitch, IYawToBe, IPitchToBe;
        internal static bool IsFocused;

        internal static Vector3 BackupCameraPosition, BackupCameraTarget;

        private static double _holdTime;
        private static Vector2 _mouseOrigin;

        /// <summary>Inits the earth hologram.</summary>
        public static void Init()
        {
            CENTER = new Vector3(-3.5f, 2, 0.2f);
            ORIGIN = CENTER;
            CENTER_TO_BE = CENTER;
            IPitch = 0;
            // Create standby position
            SatellitePoints.Add((CENTER + Vector3.UnitX) * (HOLOGRAM_RADIUS + 0.1f));
            Satellite = new Satellite();
            // Start by sending information request to the API
            OnlineRequests.StartConnexion();
            UpdateSatellite();
            // Create globe correction matrix
            UpdateTransform();
            IsFocused = false;
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateSatellite()
        {
            await OnlineRequests.UpdateCurrentSatellite();
            Satellite.RelativePosition = CelestialMaths.ComputeECEFTilted(Satellite.Latitude, Satellite.Longitude, IYaw);
        }

        internal static async void UpdatePlanet()
        {
            await OnlineRequests.GetCurrentPlanet(AstralTarget.Mars);
        }

        /// <summary>Draws the earth hologam.</summary>
        internal static void Draw()
        {
            // Update globe lerp
            CENTER = Raymath.Vector3Lerp(CENTER, CENTER_TO_BE, GetFrameTime() * Conceptor3D.LERP_SPEED);
            Shaders.Lights[0].Position = CENTER;
            Shaders.UpdateLight(Shaders.PBRLightingShader, Shaders.Lights[0]);
            IYaw = Raymath.Lerp(IYaw, IYawToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();

            // Update satellite
            UpdateSatellite();

            // Draw earth hologram box
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], GlobeRotationMat);

            // Draw satellite point
            DrawModel(Resources.Models["iss"], Satellite.RelativePosition * (HOLOGRAM_RADIUS + 0.2f) + CENTER, 0.06f, Color.White);

            // Draw current position
            //DrawSphere(OrionSim.ViewerPosition + CENTER, 0.02f, Color.Red);
#if DEBUG
            //DrawLine3D(CENTER, Satellite.RelativePosition + CENTER, Color.SkyBlue);
#endif
        }

        /// <summary>Updates the earth hologram matrix</summary>
        internal static void UpdateTransform()
        {
            Matrix4x4 rm;
            if (!Conceptor2D.InterfaceActive) rm = Raymath.MatrixRotateXYZ(new Vector3(90, EARTH_TILT, IYaw) / RAD2DEG);
            else
            {
                rm = Raymath.MatrixRotateY(IYaw / RAD2DEG);

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
                
            // Update ECEF position of the viewpoint
            OrionSim.UpdateViewPoint(); // Update un-rotated pos
        }

        /// <summary>Updates the interface of the hologram.</summary>
        internal static void UpdateInterface()
        {
            // Update camera lerp
            if (IsFocused) 
            { 
                Conceptor3D.View.Camera.Position = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Position, (OrionSim.ViewerPosition * 1.25f) + CENTER, GetFrameTime() * Conceptor3D.LERP_SPEED);
                Conceptor3D.View.Camera.Target = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Target, OrionSim.ViewerPosition + CENTER, GetFrameTime() * Conceptor3D.LERP_SPEED);
            }
            else 
            { 
                Conceptor3D.View.Camera.Position = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Position, BackupCameraPosition, GetFrameTime() * Conceptor3D.LERP_SPEED);
                Conceptor3D.View.Camera.Target = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Target, BackupCameraTarget, GetFrameTime() * Conceptor3D.LERP_SPEED);
            }

            if (IsMouseButtonDown(MouseButton.Left) && !IsFocused) // Drag
            {
                Vector2 mouse = GetMouseDelta() * 0.2f;
                IYaw += mouse.X;
                IYawToBe = IYaw;
                // Start hold time
                if (_holdTime == 0)
                {
                    _holdTime = GetTime();
                    _mouseOrigin = GetMousePosition();
                }
            }
            if (IsMouseButtonReleased(MouseButton.Left)) // Point click
            {
                // Check for hold-time
                if ((GetMousePosition() - _mouseOrigin).Length() == 0)
                {
                    RayCollision collision = GetRayCollisionSphere(GetScreenToWorldRay(GetMousePosition(), Conceptor3D.View.Camera), CENTER, HOLOGRAM_RADIUS);
                    if (collision.Hit)
                    {
                        (OrionSim.ViewerLatitude, OrionSim.ViewerLongitude) = CelestialMaths.ComputeECEFTiltedReverse((collision.Point - CENTER) / HOLOGRAM_RADIUS, IYaw);
                        OrionSim.UpdateViewPoint();
                        // Update UI components
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLat"]).Text = OrionSim.ViewerLatitude.ToString();
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLon"]).Text = OrionSim.ViewerLongitude.ToString();
                        // Update state
                        IsFocused = true;
                        BackupCameraPosition = Conceptor3D.View.PreviousPosition;
                    }
                } 
                _holdTime = 0;
            }
        }
    }
}