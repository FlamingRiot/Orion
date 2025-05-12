using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /*-----------------------------------------------------------------
     3D earth-globe class, functions and variables. 
     ------------------------------------------------------------------*/

    /// <summary>Represents the static class of the earth hologram.</summary>
    internal static class EarthHologram
    {
        // Constants
        internal const float HOLOGRAM_RADIUS = 0.8f; // Defined globe radius
        internal const float EARTH_TILT = 23.44f; // Incline angle of the earth
        internal const float EARTH_RADIUS = 6378; // In kilometers
        internal static readonly Vector3 GLOBE_ORIGIN = new Vector3(-3.5f, 2, 0.2f); // Position in world-space
        internal static readonly Vector3 GLOBE_NORTH = Raymath.Vector3RotateByAxisAngle(Vector3.UnitY, Vector3.UnitZ, EARTH_TILT * DEG2RAD); // Used for rotation and calculations

        // Earth globe attributes
        internal static Matrix4x4 ViewpointTransform; // Viewpoint ping transform matrix
        internal static Matrix4x4 GlobeTransform; // Earth globe transform matrix
        internal static Vector3 GlobeCenter; // current center
        internal static float Yaw, Pitch; // Rotation angles of the globe

        // Mini-Satellite attributes 
        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static SatelliteInfo Satellite = new SatelliteInfo(); // Satellite object

        // Planet attributes
        internal static PlanetCacheEntry CurrentPlanet = new PlanetCacheEntry();

        // Relative altitude used for precise calculations on the 3D pointing-arrow.
        internal static float RelativeSatelliteAltitude;

        // Defines whether the user is in zoom-mode or not
        internal static bool IsFocused = false;

        // Used for internal calculations on the interface
        private static double _holdTime;
        private static Vector2 _mouseOrigin;

        /// <summary>Initializes the earth hologram.</summary>
        public static void Init()
        {
            // Set world attributes
            GlobeCenter = GLOBE_ORIGIN;
            Pitch = 0;

            // Set interpolators
            Interpolators.EarthCenter = GlobeCenter;
            Interpolators.CameraUp = Vector3.UnitY;
            
            // Create standby position for the station (until some data is retrieved from the API)
            SatellitePoints.Add((GlobeCenter + Vector3.UnitX) * (HOLOGRAM_RADIUS + 0.1f));
            
            // Create globe correction matrix
            UpdateTransform();
            
            // Start by sending information request to the API
            OnlineRequests.StartConnexion();

            //OnlineRequests.DownloadTileset(4);
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateSatellite()
        {
            await OnlineRequests.UpdateCurrentSatellite();
            Satellite.RelativePosition = CelestialMaths.ComputeECEFTilted(Satellite.Latitude, Satellite.Longitude, Yaw);
        }

        /// <summary>Updates a planet object by retrieving data from API.</summary>
        internal static async void UpdatePlanet(AstralTarget target)
        {
            await OnlineRequests.UpdateCurrentPlanet(target);
            // !! The following lines only get executed when the potential API request is done (immedialty if none is send)
            // Find current info based on newly selected target
            OnlineRequests.PlanetCacheEntries.ForEach(entry => 
            {
                if (entry.Name == target) CurrentPlanet = entry;
            });
            Conceptor2D.UpdateUI(); // Update UI components with new information
        }

        /// <summary>Draws the earth hologam.</summary>
        internal static void Draw()
        {
            // Update globe lerp
            GlobeCenter = Raymath.Vector3Lerp(GlobeCenter, Interpolators.EarthCenter, GetFrameTime() * Conceptor3D.LERP_SPEED);
            Shaders.Lights[0].Position = GlobeCenter;
            Shaders.UpdateLight(Shaders.PBRLightingShader, Shaders.Lights[0]);
            Yaw = Raymath.Lerp(Yaw, Interpolators.EarthYaw, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();

            // Update satellite
            UpdateSatellite();

            // Draw earth hologram
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], GlobeTransform);

            // Draw satellite point
            DrawModel(Resources.Models["iss"], Satellite.RelativePosition * (HOLOGRAM_RADIUS + 0.2f) + GlobeCenter, 0.06f, Color.White);

            // Draw current position
            DrawMesh(Resources.Meshes["viewpoint"], Resources.Materials["viewpoint"], ViewpointTransform);
        }

        /// <summary>Computes the relative altitude of the ISS used for calculations.</summary>
        internal static void ComputeRelativeAltitude()
        {
            float alt = Satellite.Altitude;
            float ratio = alt / EARTH_RADIUS;
            RelativeSatelliteAltitude = HOLOGRAM_RADIUS * ratio;
        }

        /// <summary>Updates the interface of the hologram.</summary>
        internal static void UpdateInterface()
        {
            // Update camera lerp
            Conceptor3D.View.Camera.Up = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Up, Interpolators.CameraUp, GetFrameTime() * Conceptor3D.LERP_SPEED);

            if (IsFocused) 
            {
                Vector3 targetPosition = OrionSim.ViewerPosition * 1.15f + GlobeCenter;
                Conceptor3D.View.Camera.Position = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Position, targetPosition, GetFrameTime() * Conceptor3D.LERP_SPEED);
                Conceptor3D.View.Camera.Target = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Target, OrionSim.ViewerPosition + GlobeCenter, GetFrameTime() * Conceptor3D.LERP_SPEED);
            }
            else 
            { 
                Conceptor3D.View.Camera.Position = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Position, Interpolators.CameraPosition, GetFrameTime() * Conceptor3D.LERP_SPEED);
                Conceptor3D.View.Camera.Target = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Target, Interpolators.CameraTarget, GetFrameTime() * Conceptor3D.LERP_SPEED);
            }

            if (IsMouseButtonDown(MouseButton.Left) && !IsFocused) // Drag
            {
                Vector2 mouse = GetMouseDelta() * 0.2f;
                Yaw += mouse.X;
                Interpolators.EarthYaw = Yaw;
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
                    RayCollision collision = GetRayCollisionSphere(GetScreenToWorldRay(GetMousePosition(), Conceptor3D.View.Camera), GlobeCenter, HOLOGRAM_RADIUS);
                    if (collision.Hit)
                    {
                        (OrionSim.ViewerLatitude, OrionSim.ViewerLongitude) = CelestialMaths.ComputeECEFTiltedReverse((collision.Point - GlobeCenter) / HOLOGRAM_RADIUS, Yaw);
                        OrionSim.UpdateViewPoint();
                        // Update UI components
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLat"]).Text = OrionSim.ViewerLatitude.ToString();
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLon"]).Text = OrionSim.ViewerLongitude.ToString();
                        // Update state
                        IsFocused = true;
                        Interpolators.CameraPosition = Conceptor3D.View.PreviousPosition;
                        Interpolators.CameraUp = GLOBE_NORTH;
                    }
                } 
                _holdTime = 0;
            }
        }

        /// <summary>Updates the earth hologram matrix</summary>
        private static void UpdateTransform()
        {
            Matrix4x4 rm;
            if (!Conceptor2D.InterfaceActive) rm = Raymath.MatrixRotateXYZ(new Vector3(90, EARTH_TILT, Yaw) / RAD2DEG);
            else
            {
                //rm = Raymath.MatrixRotateY(IYaw / RAD2DEG);
                rm = Raymath.MatrixRotate(GLOBE_NORTH, Yaw / RAD2DEG);

                // Compute X/Z axis weights
                Vector3 cam = new Vector3(Conceptor3D.View.Camera.Position.X, 0, Conceptor3D.View.Camera.Position.Z) -
                    new Vector3(Conceptor3D.View.Camera.Target.X, 0, Conceptor3D.View.Camera.Target.Z);

                float xWeight = Raymath.Vector3DotProduct(Vector3.UnitZ, Vector3.Normalize(cam));
                float zWeight = Raymath.Vector3DotProduct(Vector3.UnitX, Vector3.Normalize(cam));
                // Create weighted matrix
                rm *= Raymath.MatrixRotateXYZ(new Vector3(Pitch * xWeight + 90, EARTH_TILT, Pitch * zWeight) * DEG2RAD);
            }
            Matrix4x4 sm = Raymath.MatrixScale(1, 1, 1);
            Matrix4x4 pm = Raymath.MatrixTranslate(GlobeCenter.X, GlobeCenter.Y, GlobeCenter.Z);
            // Multiply matrices
            GlobeTransform = pm * sm * rm;

            // Update ECEF position of the viewpoint
            OrionSim.UpdateViewPoint(); // Update un-rotated pos
        }
    }
}