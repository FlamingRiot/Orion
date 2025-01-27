using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Represents the 3D conceptor of the program.</summary>
    internal static class Conceptor3D
    {
        internal static Camera3D Camera;

        /// <summary>Initializes the 3D conceptor.</summary>
        internal static void Init()
        {
            // Create camera object.
            Camera = new Camera3D()
            {
                Position = new Vector3(20.0f, 10.0f, 20.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 45f,
                Projection = CameraProjection.Perspective
            };
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            BeginMode3D(Camera);

            DrawGrid(5, 5);

            DrawLine3D(Vector3.Zero, new Vector3(5, 5, 5), Color.Red);

            EndMode3D();
        }

        /// <summary>Updates the 3D conceptor.</summary>
        internal static void Update()
        {
            UpdateCamera(ref Camera, CameraMode.Orbital);
        }
    }
}