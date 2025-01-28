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
                Position = new Vector3(25.0f, 15.0f, 25.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 45f,
                Projection = CameraProjection.Perspective
            };

            EarthHologram.Init(); // Connect to earth hologram
            Shaders.Init(); // Load program shaders
            Resources.Init(); // Load GPU resources (e.g. meshes, textures, shaders, etc.)
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            BeginMode3D(Camera);

            EarthHologram.Draw();

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