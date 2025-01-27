using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the 3D conceptor of the program.</summary>
    internal static class Conceptor3D
    {
        internal const int SKY_HEIGHT = 10;

        internal static Camera3D Camera;
        internal static Matrix4x4 globeCorrectionMat;
        internal static Satellite ISS;

        /// <summary>Initializes the 3D conceptor.</summary>
        internal static void Init()
        {
            // Start by sending information request to the API
            UpdateISS();

            // Create camera object.
            Camera = new Camera3D()
            {
                Position = new Vector3(25.0f, 15.0f, 25.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 45f,
                Projection = CameraProjection.Perspective
            };
            globeCorrectionMat = Raymath.MatrixRotateXYZ(new Vector3(90, 0, 0) / RAD2DEG);

            ISS = new Satellite();

            Shaders.Init();
            Resources.Init(); // Load GPU resources (e.g. meshes, textures, shaders, etc.)
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateISS()
        {
            ISS = await OnlineRequests.GetCurrentISS();
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            Vector3 issPoint = Vector3.Normalize(CelestialMaths.ComputeECEF(ISS.Latitude, ISS.Longitude)) * (SKY_HEIGHT + 1f);
            
            BeginMode3D(Camera);

            // Draw sky box
            DrawMesh(Resources.Meshes["sphere"], Resources.Materials["earth"], globeCorrectionMat);

            // Draw line
            DrawLine3D(Vector3.Zero, issPoint, Color.Red);

            // Draw ISS point
            DrawSphere(issPoint, 0.2f, Color.Yellow);

            EndMode3D();
        }

        /// <summary>Updates the 3D conceptor.</summary>
        internal static void Update()
        {
            // Update environment camera
            UpdateCamera(ref Camera, CameraMode.Orbital);
        }
    }
}