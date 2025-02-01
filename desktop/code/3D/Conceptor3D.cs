using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using Uniray;

namespace Orion_Desktop
{
    /// <summary>Represents the 3D conceptor of the program.</summary>
    internal static class Conceptor3D
    {
        internal static Camera3D Camera;
        internal static List<GameObject3D> objects = new List<GameObject3D>();

        /// <summary>Initializes the 3D conceptor.</summary>
        internal static void Init()
        {
            // Create camera object.
            Camera = new Camera3D()
            {
                Position = new Vector3(5.0f, 5.0f, 5.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 60f,
                Projection = CameraProjection.Perspective
            };

            // Init center tables
            EarthHologram.Init(); // Connect to earth hologram
            Camera.Target = EarthHologram.CENTER;
            OrionSim.Init(CelestialMaths.POSITION_LATITUDE, CelestialMaths.POSITION_LONGITUDE); // Start Orion robot simulation

            Shaders.Init(); // Load program shaders
            Resources.Init(); // Load GPU resources (e.g. meshes, textures, shaders, etc.)

            objects = RLoading.LoadScene();
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            BeginMode3D(Camera);

            // Draw earth hologram
            EarthHologram.Draw();
            // Draw Orion robot simulation
            //OrionSim.Draw();

            // Draw scene
            objects.ForEach(x => x.Draw());

            DrawSphereWires(Shaders.Lights[2].Position, 0.2f, 10, 10, Color.White);

            EndMode3D();
        }

        /// <summary>Updates the 3D conceptor.</summary>
        internal static void Update()
        {
            // Update environment camera
            UpdateCamera(ref Camera, CameraMode.Free);

            // Update PBR lighting
            Shaders.UpdatePBRLighting(Camera.Position);
        }
    }
}