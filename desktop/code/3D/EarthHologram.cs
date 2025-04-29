using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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
        internal const float HOLOGRAM_RADIUS = 0.8f;
        internal const float EARTH_TILT = 23.44f;
        internal const float EARTH_RADIUS = 6378; // kilometers

        // Earth globe attributes
        internal static Matrix4x4 GlobeRotationMat; // Earth globe rotation matrix
        internal static Vector3 GlobeOrigin; // unmodified center, ever
        internal static Vector3 GlobeCenterToBe; // Used for interface interpolation
        internal static Vector3 GlobeCenter; // current center
        internal static float IYaw, IPitch, IYawToBe, IPitchToBe;

        // Mini-Satellite attributes 
        internal static List<Vector3> SatellitePoints = new List<Vector3>();
        internal static SatelliteInfo Satellite = new SatelliteInfo(); // Satellite object

        // Planet attributes
        internal static PlanetCacheEntry CurrentPlanet = new PlanetCacheEntry();

        // Variables used for precise calculations on the 3D pointing-arrow.
        internal static float RelativeSatelliteAltitude;
        internal static float VerticalAngle;

        // Defines whether the user is in zoom-mode or not
        internal static bool IsFocused;

        // Backup position and target used for interface zoom & interpolations
        internal static Vector3 BackupCameraPosition, BackupCameraTarget;

        // Used for internal calculations on the interface
        private static double _holdTime;
        private static Vector2 _mouseOrigin;

        /// <summary>Inits the earth hologram.</summary>
        public static void Init()
        {
            GlobeCenter = new Vector3(-3.5f, 2, 0.2f);
            GlobeOrigin = GlobeCenter;
            GlobeCenterToBe = GlobeCenter;
            IPitch = 0;
            // Create standby position
            SatellitePoints.Add((GlobeCenter + Vector3.UnitX) * (HOLOGRAM_RADIUS + 0.1f));
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
            GlobeCenter = Raymath.Vector3Lerp(GlobeCenter, GlobeCenterToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            Shaders.Lights[0].Position = GlobeCenter;
            Shaders.UpdateLight(Shaders.PBRLightingShader, Shaders.Lights[0]);
            IYaw = Raymath.Lerp(IYaw, IYawToBe, GetFrameTime() * Conceptor3D.LERP_SPEED);
            UpdateTransform();

            // Update satellite
            UpdateSatellite();

            // Draw earth hologram
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], GlobeRotationMat);

            // Draw satellite point
            DrawModel(Resources.Models["iss"], Satellite.RelativePosition * (HOLOGRAM_RADIUS + 0.2f) + GlobeCenter, 0.06f, Color.White);

            // Draw current position
            DrawSphere(OrionSim.ViewerPosition + GlobeCenter, 0.01f, Color.Red);
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
            TilingManager.DrawMapManager();
            // Update camera lerp
            if (IsFocused) 
            {
                Vector3 targetPosition = OrionSim.ViewerPosition * 1.25f + GlobeCenter;
                Conceptor3D.View.Camera.Position = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Position, targetPosition, GetFrameTime() * Conceptor3D.LERP_SPEED);
                Conceptor3D.View.Camera.Target = Raymath.Vector3Lerp(Conceptor3D.View.Camera.Target, OrionSim.ViewerPosition + GlobeCenter, GetFrameTime() * Conceptor3D.LERP_SPEED);
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
                    RayCollision collision = GetRayCollisionSphere(GetScreenToWorldRay(GetMousePosition(), Conceptor3D.View.Camera), GlobeCenter, HOLOGRAM_RADIUS);
                    if (collision.Hit)
                    {
                        (OrionSim.ViewerLatitude, OrionSim.ViewerLongitude) = CelestialMaths.ComputeECEFTiltedReverse((collision.Point - GlobeCenter) / HOLOGRAM_RADIUS, IYaw);
                        OrionSim.UpdateViewPoint();
                        // Update UI components
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLat"]).Text = OrionSim.ViewerLatitude.ToString();
                        ((RayGUI_cs.Textbox)Conceptor2D.TerminalGui["txbCurrentLon"]).Text = OrionSim.ViewerLongitude.ToString();
                        // Update state
                        IsFocused = true;
                        BackupCameraPosition = Conceptor3D.View.PreviousPosition;

                        // Compute vertical-axis angle
                        ComputeViewPointOffsetAngle();

                        TilingManager.ConvertCoordinatesToTiles(OrionSim.ViewerLatitude, OrionSim.ViewerLongitude, 3);
                    }
                } 
                _holdTime = 0;
            }
        }

        /// <summary>Computes the vertical-axis angle according to the observer's point.</summary>
        internal static void ComputeViewPointOffsetAngle()
        {
            VerticalAngle = MathF.Acos(Raymath.Vector3DotProduct(Vector3.UnitY, OrionSim.ViewerPosition) / OrionSim.ViewerPosition.Length()) * RAD2DEG;
        }

        /// <summary>Updates the earth hologram matrix</summary>
        private static void UpdateTransform()
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
            Matrix4x4 pm = Raymath.MatrixTranslate(GlobeCenter.X, GlobeCenter.Y, GlobeCenter.Z);
            // Multiply matrices
            GlobeRotationMat = pm * sm * rm;

            // Update ECEF position of the viewpoint
            OrionSim.UpdateViewPoint(); // Update un-rotated pos
        }
    }

    /*-----------------------------------------------------------------
     Map-Tiling classes, functions and variables. 
     ------------------------------------------------------------------*/

    /// <summary>Represents the 2D map-tiling managing class.</summary>
    internal static class TilingManager
    {
        // Constants
        internal const string MAP_CONFIG = $"MODIS_Terra_CorrectedReflectance_TrueColor";

        // Tile Levels to be updated during background loading
        internal static Dictionary<int, Vector2> Configs = new Dictionary<int, Vector2>();

        private static List<MapTile> Tiles = new List<MapTile>();

        /// <summary>Draws the Map-tiling (Required context: 2D).</summary>
        internal static void DrawMapManager()
        {
            Tiles.ForEach(t => t.Draw());
        }

        internal static Vector2 ConvertCoordinatesToTiles(float lat, float lon, int zoomLevel)
        {
            // Define maximums
            int latMax = 180;
            int lonMax = 360;

            // Adapt
            lat += latMax / 2;
            lon += lonMax / 2;

            // Retrieve zoom-level values
            Vector2 position = new Vector2(0);
            Configs.TryGetValue(zoomLevel, out position);

            position.X = lat / latMax * position.X;
            position.Y = lon / lonMax * position.Y;

            return position;
        }
    }

    /// <summary>Represents a single map-tile used for 2D rendering in <see cref="TilingManager"/>.</summary>
    internal class MapTile
    {
        // Constants
        internal const int TILE_SIZE = 128;

        // Rectangles used for tile rendering
        private static Rectangle _source = new Rectangle(0, 0, 512, 512);
        private Rectangle _target;

        // Attributes
        public int Row;
        public int Column;
        public Texture2D Texture;

        /// <summary>Creates an instance of <see cref="MapTile"/>.</summary>
        /// <param name="row">Map row.</param>
        /// <param name="column">Map column.</param>
        internal MapTile(int row, int column)
        {
            Row = row;
            Column = column;
            _target = new Rectangle(TILE_SIZE * column, TILE_SIZE * row, TILE_SIZE, TILE_SIZE);
        }

        /// <summary>Draws a single map-tile according to its relative position.</summary>
        internal void Draw()
        {
            DrawTexturePro(Texture, _source, _target, Vector2.Zero, 0, Color.White);
        }
    }
}