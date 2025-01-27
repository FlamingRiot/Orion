using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the 3D conceptor of the program.</summary>
    internal static class Conceptor3D
    {
        internal const int SKY_HEIGHT = 10;

        internal static Ray MouseRay;
        internal static RayCollision MouseRayCollision;

        internal static Camera3D Camera;
        internal static Vector3 ISSPoint;

        /// <summary>Initializes the 3D conceptor.</summary>
        internal static void Init()
        {
            // Create camera object.
            Camera = new Camera3D()
            {
                Position = new Vector3(25.0f, 15.0f, 25.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 45f,
                Projection = CameraProjection.Perspective
            };

            ISSPoint = new Vector3(SKY_HEIGHT, 0, 0);

            MouseRay = new Ray();
            MouseRayCollision = new RayCollision();

            UpdateISS();
        }

        /// <summary>Updates the ISS object by retrieving data from API.</summary>
        internal static async void UpdateISS()
        {
            Satellite iss = await OnlineRequests.GetCurrentISS();
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            BeginMode3D(Camera);

            // Draw sky box
            DrawSphereWires(Vector3.Zero, SKY_HEIGHT, 20, 20, Color.Gray);

            // Draw line
            DrawLine3D(Vector3.Zero, ISSPoint, Color.Red);

            // Draw ISS point
            DrawSphere(ISSPoint, 0.2f, Color.Yellow);

            // Draw cardinal points
            DrawReferentials();

            EndMode3D();
        }

        internal static void DrawReferentials()
        {
            DrawSphere(Vector3.UnitZ * SKY_HEIGHT * 1.5f, 0.2f, Color.Blue); // North point
            DrawSphere(Vector3.UnitX * SKY_HEIGHT * 1.5f, 0.2f, Color.Green); // West point
        }

        /// <summary>Updates the 3D conceptor.</summary>
        internal static void Update()
        {
            // Update environment camera
            //UpdateCamera(ref Camera, CameraMode.Orbital);

            // Update point
            UpdatePoint();
        }

        /// <summary>Updates the position of the ISS point on the sphere</summary>
        private static void UpdatePoint()
        {
            MouseRay = GetScreenToWorldRay(GetMousePosition(), Camera);
            MouseRayCollision = GetRayCollisionSphere(MouseRay, Vector3.Zero, SKY_HEIGHT);

            ISSPoint = MouseRayCollision.Point;
        }
    }
}