﻿using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using Uniray;

namespace Orion_Desktop
{
    /// <summary>Represents the 3D conceptor of the program.</summary>
    internal static class Conceptor3D
    {
        internal const int HUB_RADIUS = 8;
        internal const float LERP_SPEED = 3f;

        // General 3D environement objets
        internal static View3D View;
        internal static List<GameObject3D> objects = new List<GameObject3D>();
        internal static Material SkyboxMat;

        /// <summary>Initializes the 3D conceptor.</summary>
        internal static void Init()
        {
            // Create camera object.
            Camera3D cam = new Camera3D()
            {
                Position = new Vector3(3.0f, 2.5f, 3.0f),
                Target = Vector3.Zero,
                Up = Vector3.UnitY,
                FovY = 60f,
                Projection = CameraProjection.Perspective
            };
            // Create View 3D object.
            View = new View3D()
            {
                Camera = cam,
            };

            // Init center tables
            EarthHologram.Init(); // Connect to earth hologram

            Shaders.Init(); // Load program shaders
            OrionSim.Init(0, 0); // Start Orion robot simulation
            Resources.Init(); // Load GPU resources (e.g. meshes, textures, shaders, etc.)

            objects = UnirayLoader.LoadScene();

            // Load skybox and apply hdr texture
            SkyboxMat = Shaders.LoadSkybox("assets/textures/space.png");
        }

        /// <summary>Draws the components of the 3D conceptor to an opened render buffer.</summary>
        internal static void Draw()
        {
            BeginMode3D(View.Camera);

            // Draw skybox
            Shaders.DrawSkybox(SkyboxMat);

            // Draw scene
            objects.ForEach(x => x.Draw());

            EndMode3D();
        }

        /// <summary>Updates the 3D conceptor.</summary>
        internal static void Update()
        {
            // Update environment camera
            if (!Conceptor2D.InterfaceActive) UpdateCamera();

            // Update PBR lighting
            Shaders.UpdatePBRLighting(View.Camera.Position);

            // Update action-ray detection
            Ray mouse = GetScreenToWorldRay(Conceptor2D.Size / 2, View.Camera);
            Conceptor2D.InteractiveEnabled = GetRayCollisionSphere(mouse, EarthHologram.GlobeCenter, EarthHologram.HOLOGRAM_RADIUS).Hit;
            if (Conceptor2D.InteractiveEnabled && IsKeyPressed(KeyboardKey.E)) Conceptor2D.OpenedInterface = Conceptor2D.Interface.Earth;
            if (!Conceptor2D.InteractiveEnabled)
            {
                Conceptor2D.InteractiveEnabled = GetRayCollisionMesh(mouse, Resources.Meshes["screen"], OrionSim.Transform).Hit;
                if (Conceptor2D.InteractiveEnabled && IsKeyPressed(KeyboardKey.E)) Conceptor2D.OpenedInterface = Conceptor2D.Interface.Terminal;
            }
        }

        /// <summary>Updates the camera movement.</summary>
        internal static void UpdateCamera()
        {
            // Camera position movement
            Vector3 movement = new Vector3();
            if (IsKeyDown(KeyboardKey.W))
            {
                movement -= Raymath.Vector3CrossProduct(GetCameraRight(ref View.Camera), Vector3.UnitY);
            }
            if (IsKeyDown(KeyboardKey.S))
            {
                movement += Raymath.Vector3CrossProduct(GetCameraRight(ref View.Camera), Vector3.UnitY);
            }
            if (IsKeyDown(KeyboardKey.D))
            {
                movement += GetCameraRight(ref View.Camera);
            }
            if (IsKeyDown(KeyboardKey.A))
            {
                movement -= GetCameraRight(ref View.Camera);
            }

            // Update movement
            if (movement != Vector3.Zero)
            {
                View.Camera.Position += Vector3.Normalize(movement) * GetFrameTime() * View3D.SPEED;
                View.Camera.Target = Vector3.Add(View.Camera.Target, View.Camera.Position);
                View.UpdateCircularConstraint(); // Compute motion constraint of the 3D view
            }

            // Camera target movement
            Vector2 mouse = GetMouseDelta();
            float targetYawSpeed = -mouse.X * View3D.SENSITIVITY / 1000;
            float targetPitchSpeed = -mouse.Y * View3D.SENSITIVITY / 1000;

            View._yawSpeed = Raymath.Lerp(View._yawSpeed, targetYawSpeed, GetFrameTime() * View3D.SMOOTH_FACTOR);
            View._pitchSpeed = Raymath.Lerp(View._pitchSpeed, targetPitchSpeed, GetFrameTime() * View3D.SMOOTH_FACTOR);

            View.Yaw += View._yawSpeed;
            View.Pitch += View._pitchSpeed;

            // Correct angles to stay in desired values
            View.Yaw = CelestialMaths.ClampAngleRadianNegative(View.Yaw);

            View.Pitch = Math.Clamp(View.Pitch, -1.5f, 1.5f);
        }
    }

    /// <summary>Represents an enhanced 3D camera object.</summary>
    internal struct View3D
    {
        // Constants
        internal const float SMOOTH_FACTOR = 15.0f;
        internal const float SENSITIVITY = 1.6f;
        internal const float SPEED = 8f;

        // Rotation angless
        private float _yaw;
        private float _pitch;

        // Rotation angles speed (lerp.)
        internal float _yawSpeed;
        internal float _pitchSpeed;

        internal Camera3D Camera;
        internal Vector3 PreviousPosition; // used for circular collision detection

        /// <summary>Yaw angle of the camera.</summary>
        internal float Yaw { get { return _yaw; } set { _yaw = value; UpdateView(); } }
        /// <summary>Pitch angle of the camera.</summary>
        internal float Pitch { get { return _pitch; } set { _pitch = value; UpdateView(); } }

        /// <summary>Creates an instance of <see cref="View3D"/>.</summary>
        /// <param name="camera">3D camera object to use.</param>
        public View3D(Camera3D camera)
        {
            Camera = camera;
        }

        /// <summary>Updates the circular constraint for the current 3D view.</summary>
        internal void UpdateCircularConstraint()
        {
            float distance = (Camera.Position - EarthHologram.GLOBE_ORIGIN).Length();
            if (distance >= Conceptor3D.HUB_RADIUS)
            {
                Camera.Position = PreviousPosition;
            }
            PreviousPosition = Camera.Position;
        }

        /// <summary>Updates the view (target) of the camera.</summary>
        private void UpdateView()
        {
            Camera.Target.X = MathF.Cos(Pitch) * MathF.Sin(Yaw);
            Camera.Target.Y = MathF.Sin(Pitch);
            Camera.Target.Z = MathF.Cos(Pitch) * MathF.Cos(Yaw);
            Camera.Target += Camera.Position;
        }
    }
}