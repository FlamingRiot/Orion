﻿using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of a PBR material.</summary>
    internal unsafe struct PBRMaterial
    {
        internal Material Material;
        internal Dictionary<MaterialMapIndex, Texture2D> Maps;

        /// <summary>Sets one of the material's maps.</summary>
        /// <param name="map">Map type to use.</param>
        /// <param name="texture">Texture to use.</param>
        internal void SetMap(MaterialMapIndex map, Texture2D texture)
        {
            SetMaterialTexture(ref Material, map, texture);
            UnloadTexture(Maps[map]); // Unload unused map
            Maps[map] = texture; // Set new texture
        }

        /// <summary>Creates an instance of PBR material.</summary>
        /// <param name="mapFolder"></param>
        internal PBRMaterial(string mapFolder)
        {
            Material = LoadMaterialDefault();
            Material.Shader = Shaders.PBRLightingShader;
            Maps = new Dictionary<MaterialMapIndex, Texture2D>();
            string[] mapsPaths = Directory.GetFiles(mapFolder);
            for (int i = 0; i < mapsPaths.Length; i++)
            {
                string mapType = mapsPaths[i].Split('_').Last().Split('.')[0];
                if (Enum.IsDefined(typeof(MaterialMapIndex), mapType))
                {
                    MaterialMapIndex _type = (MaterialMapIndex)Enum.Parse(typeof(MaterialMapIndex), mapType);
                    Maps.Add(_type, LoadTexture(mapsPaths[i]));
                    SetMaterialTexture(ref Material, _type, Maps[_type]);
                } 
            }

            Material.Maps[(int)MaterialMapIndex.Albedo].Color = Color.White;
            Material.Maps[(int)MaterialMapIndex.Metalness].Value = 0;
            Material.Maps[(int)MaterialMapIndex.Roughness].Value = 0;
            Material.Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
            Material.Maps[(int)MaterialMapIndex.Emission].Color = Color.White;


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ORION: {mapFolder.Split('/').Last()} PBR Material loaded successfully");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    /// <summary>Defines the type of a light source.</summary>
    public enum PBRLightType
    {
        Directorional,
        Point,
        Spot
    }

    /// <summary>Represents an instance of light point in 3D world.</summary>
    public struct PBRLight
    {
        public PBRLightType Type;
        public bool Enabled;
        public Vector3 Position;
        public Vector3 Target;
        public Vector4 Color;
        public float Intensity;

        // Shader light parameters locations
        public int TypeLoc;
        public int EnabledLoc;
        public int PositionLoc;
        public int TargetLoc;
        public int ColorLoc;
        public int IntensityLoc;
    }

    /// <summary>Represents an instance of the static lights management class.</summary>
    public class PBRLights
    {
        public static PBRLight CreateLight(
            int lightsCount,
            PBRLightType type,
            Vector3 pos,
            Vector3 target,
            Color color,
            float intensity,
            Shader shader
        )
        {
            PBRLight light = new();

            light.Enabled = true;
            light.Type = type;
            light.Position = pos;
            light.Target = target;
            light.Color = new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            );
            light.Intensity = intensity;

            string enabledName = "lights[" + lightsCount + "].enabled";
            string typeName = "lights[" + lightsCount + "].type";
            string posName = "lights[" + lightsCount + "].position";
            string targetName = "lights[" + lightsCount + "].target";
            string colorName = "lights[" + lightsCount + "].color";
            string intensityName = "lights[" + lightsCount + "].intensity";

            light.EnabledLoc = GetShaderLocation(shader, enabledName);
            light.TypeLoc = GetShaderLocation(shader, typeName);
            light.PositionLoc = GetShaderLocation(shader, posName);
            light.TargetLoc = GetShaderLocation(shader, targetName);
            light.ColorLoc = GetShaderLocation(shader, colorName);
            light.IntensityLoc = GetShaderLocation(shader, intensityName);

            UpdateLightValues(shader, light);

            return light;
        }

        public static void UpdateLightValues(Shader shader, PBRLight light)
        {
            // Send to shader light enabled state and type
            SetShaderValue(
                shader,
                light.EnabledLoc,
                light.Enabled ? 1 : 0,
                ShaderUniformDataType.Int
            );
            SetShaderValue(shader, light.TypeLoc, (int)light.Type, ShaderUniformDataType.Int);

            // Send to shader light target position values
            SetShaderValue(shader, light.PositionLoc, light.Position, ShaderUniformDataType.Vec3);

            // Send to shader light target position values
            SetShaderValue(shader, light.TargetLoc, light.Target, ShaderUniformDataType.Vec3);

            // Send to shader light color values
            SetShaderValue(shader, light.ColorLoc, light.Color, ShaderUniformDataType.Vec4);

            // Send to shader light intensity values
            SetShaderValue(shader, light.IntensityLoc, light.Intensity, ShaderUniformDataType.Float);
        }
    }

    /// <summary>Represents an instance of the static shader class.</summary>
    internal static class Shaders
    {
        internal static Shader FixShader;
        internal static Shader PostProShader;
        internal static Shader PBRLightingShader;
        internal static Shader SkyboxShader;
        internal static Shader ScreenShader;
        internal static Shader ViewpointShader;
        internal static Shader ChromaticAberrationShader;
        private static Shader CubemapShader;

        private static Texture2D test;

        public static PBRLight[] Lights = new PBRLight[4];

        private static int EmissivePowerLoc;
        private static int EmissiveColorLoc;
        private static int TextureTilingLoc;
        private static int BackRenderLoc;
        private static int CompassRenderLoc;
        private static int TimeLocGlobe;
        private static int TimeLocScreen;
        private static int ViewPosLoc;
        private static int CloseUpIntensityLoc1;
        private static int CloseUpIntensityLoc2;
        private static int EarthPreviewLoc;
        private static int TimeViewpointLoc;
        private static int CloseUpIntensityLoc3;
        private static int TimeLocChromatic;

        private static readonly Mesh SKYBOX_MESH = GenMeshCube(1, 1, 1);

        internal static void Init()
        {
            LoadShaders();
        }

        /// <summary>Loads the shaders of the application.</summary>
        internal static unsafe void LoadShaders()
        {
            // UV coord fix shader
            FixShader = LoadShader("assets/shaders/default.vs", "assets/shaders/hologram.fs"); // Earth hologram shader
            TimeLocGlobe = GetShaderLocation(FixShader, "time");
            ViewPosLoc = GetShaderLocation(FixShader, "viewPos");
            CloseUpIntensityLoc1 = GetShaderLocation(FixShader, "closeUpIntensity");
            
            // Setup preview texture (Activate texture slot for the shader)
            EarthPreviewLoc = GetShaderLocation(FixShader, "preview");
            test = LoadTexture("assets/textures/earth.png");
            Rlgl.ActiveTextureSlot(10);
            Rlgl.EnableTexture(test.Id);
            Rlgl.ActiveTextureSlot(0);

            SetShaderValue(FixShader, EarthPreviewLoc, 10, ShaderUniformDataType.Int);

            ScreenShader = LoadShader("assets/shaders/default.vs", "assets/shaders/screen.fs");
            TimeLocScreen = GetShaderLocation(ScreenShader, "time");

            // Post-Processing shader
            PostProShader = LoadShader(null, "assets/shaders/postpro.fs"); // Post-Processing shader
            BackRenderLoc = GetShaderLocation(PostProShader, "bRender");
            CloseUpIntensityLoc2 = GetShaderLocation(PostProShader, "closeUpIntensity");

            // Chromatic aberration shader
            ChromaticAberrationShader = LoadShader(null, "assets/shaders/chromatic.fs");
            CompassRenderLoc = GetShaderLocation(ChromaticAberrationShader, "cRender");
            TimeLocChromatic = GetShaderLocation(ChromaticAberrationShader, "time");
            Vector2 size = new Vector2(GetScreenWidth(), GetScreenHeight());
            SetShaderValue(ChromaticAberrationShader, GetShaderLocation(ChromaticAberrationShader, "resolution"), size, ShaderUniformDataType.Vec2);

            // PBR lighting shader
            PBRLightingShader = LoadShader("assets/shaders/pbr.vs", "assets/shaders/pbr.fs");

            // Modify PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapAlbedo] = GetShaderLocation(PBRLightingShader, "albedoMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapMetalness] = GetShaderLocation(PBRLightingShader, "mraMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapNormal] = GetShaderLocation(PBRLightingShader, "normalMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapEmission] = GetShaderLocation(PBRLightingShader, "emissiveMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.ColorDiffuse] = GetShaderLocation(PBRLightingShader, "albedoColor");

            // Set PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(PBRLightingShader, "viewPos");
            int lightCountLoc = GetShaderLocation(PBRLightingShader, "numOfLights");
            int maxLightCount = 4;
            SetShaderValue(PBRLightingShader, lightCountLoc, &maxLightCount, ShaderUniformDataType.Int);

            // Setup ambient color and intensity parameters
            float ambientIntensity = 0.01f;
            Color ambientColor = Color.Black;
            //Color ambientColor = new Color(75, 75, 75, 255);
            Vector3 ambientColorNormalized = new Vector3(ambientColor.R / 255f, ambientColor.G / 255f, ambientColor.B / 255f);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambientColor"), &ambientColorNormalized, ShaderUniformDataType.Vec3);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambient"), &ambientIntensity, ShaderUniformDataType.Float);
       
            // Get real-time modification elligible uniforms
            EmissivePowerLoc = GetShaderLocation(PBRLightingShader, "emissivePower");
            EmissiveColorLoc = GetShaderLocation(PBRLightingShader, "emissiveColor");
            TextureTilingLoc = GetShaderLocation(PBRLightingShader, "tiling");
            // Create main light source
            Lights[0] = PBRLights.CreateLight(0, PBRLightType.Point, EarthHologram.GlobeCenter, Vector3.Zero, new Color(35, 110, 232, 255), 10, PBRLightingShader);
            Lights[1] = PBRLights.CreateLight(1, PBRLightType.Point, EarthHologram.GlobeCenter + Vector3.UnitY * 6, Vector3.Zero, Color.White, 50, PBRLightingShader);
            //Lights[2] = PbrLights.CreateLight(2, PbrLightType.Point, new Vector3(-7f, 3f, -0.1f), Vector3.Zero, new Color(8, 181, 255, 255), 50, PBRLightingShader);
            // Set PBR shader used maps
            int usage = 1;
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexAlbedo"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexNormal"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexMRA"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexEmissive"), &usage, ShaderUniformDataType.Int);

            // Skybox shader
            SkyboxShader = LoadShader("assets/shaders/skybox.vs", "assets/shaders/skybox.fs");
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "environmentMap"), (int)MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "doGamma"), 1, ShaderUniformDataType.Int);
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "vflipped"), 1, ShaderUniformDataType.Int);
            // Cubemap shader
            CubemapShader = LoadShader("assets/shaders/cubemap.vs", "assets/shaders/cubemap.fs");
            SetShaderValue(CubemapShader, GetShaderLocation(CubemapShader, "equirectangularMap"), 0, ShaderUniformDataType.Int);

            // Viewpoint shader
            ViewpointShader = LoadShader("assets/shaders/viewpoint.vs", "assets/shaders/viewpoint.fs");
            TimeViewpointLoc = GetShaderLocation(ViewpointShader, "time");
            CloseUpIntensityLoc3 = GetShaderLocation(ViewpointShader, "closeUpIntensity");
        }

        /// <summary>Updates the environement's PBR lighting.</summary>
        /// <param name="viewPos">Camera position of the currently rendered scene.</param>
        internal static unsafe void UpdatePBRLighting(Vector3 viewPos)
        {
            SetShaderValue(PBRLightingShader, PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView], viewPos, ShaderUniformDataType.Vec3);
            //UpdateLight(PBRLightingShader, GlobalLight);
            // Update tiling values
            SetShaderValue(PBRLightingShader, TextureTilingLoc, Vector2.One / 2, ShaderUniformDataType.Vec2);
            // Update intensity
            SetShaderValue(PBRLightingShader, EmissivePowerLoc, 0.5f, ShaderUniformDataType.Float);
            SetShaderValue(PBRLightingShader, EmissiveColorLoc, Vector4.One, ShaderUniformDataType.Vec4);
            SetShaderValue(FixShader, ViewPosLoc, viewPos, ShaderUniformDataType.Vec3);
        }

        /// <summary>Updates a single PBR light source.</summary>
        /// <param name="shader">PBR shader to update to.</param>
        /// <param name="light">PBR light to use.</param>
        internal static void UpdateLight(Shader shader, PBRLight light)
        {
            SetShaderValue(shader, light.EnabledLoc, light.Enabled, ShaderUniformDataType.Int);
            SetShaderValue(shader, light.TypeLoc, light.Type, ShaderUniformDataType.Int);

            // Send to shader light position values
            SetShaderValue(shader, light.PositionLoc, light.Position, ShaderUniformDataType.Vec3);

            // Send to shader light target position values
            SetShaderValue(shader, light.TargetLoc, light.Target, ShaderUniformDataType.Vec3);
            SetShaderValue(shader, light.ColorLoc, light.Color, ShaderUniformDataType.Vec4);
            SetShaderValue(shader, light.IntensityLoc, light.Intensity, ShaderUniformDataType.Float);
        }

        /// <summary>Draws and overlaps the hologram render with the previous one.</summary>
        internal static void OverlapHologramRender()
        {
            // Update hologram-shaders time uniform
            float time = (float)GetTime();
            SetShaderValue(FixShader, TimeLocGlobe, time, ShaderUniformDataType.Float);
            SetShaderValue(ChromaticAberrationShader, TimeLocChromatic, time % 10.0f, ShaderUniformDataType.Float);

            SetShaderValue(ViewpointShader, TimeViewpointLoc, time, ShaderUniformDataType.Float);

            // Define interpolation value in normalized scope
            if (Conceptor2D.OpenedInterface == Conceptor2D.Interface.Earth)
            {
                float l1 = ((OrionSim.ViewerPosition * 1.08f) + EarthHologram.GlobeCenter - Conceptor3D.View.Camera.Position).Length();
                float l2 = ((OrionSim.ViewerPosition * 1.08f) + EarthHologram.GlobeCenter - Interpolators.CameraPosition).Length();
                Interpolators.FocusInterp = 1.0f - l1 / l2;
                SetShaderValue(FixShader, CloseUpIntensityLoc1, Interpolators.FocusInterp, ShaderUniformDataType.Float);
                SetShaderValue(PostProShader, CloseUpIntensityLoc2, Interpolators.FocusInterp, ShaderUniformDataType.Float);
                SetShaderValue(ViewpointShader, CloseUpIntensityLoc3, Interpolators.FocusInterp, ShaderUniformDataType.Float);
            }

            SetShaderValue(ScreenShader, TimeLocScreen, time, ShaderUniformDataType.Float);

            // Start post-processing shader
            BeginShaderMode(PostProShader);

            // Update background render
            SetShaderValueTexture(PostProShader, BackRenderLoc, Program.Render.Texture);
            // Update compass render 
            SetShaderValueTexture(ChromaticAberrationShader, CompassRenderLoc, Conceptor2D.CompassRenderTexture.Texture);
            // Update Drawcall
            DrawTexturePro(Program.HologramRender.Texture, Program.SourceRender, Program.DestinationRender, Vector2.Zero, 0, Color.White);

            // Close post-processing shader
            EndShaderMode();
        }

        /// <summary>Draws a skybox object to the screen (draw before anything else in the scene).</summary>
        /// <param name="material">Skybox material to use.</param>
        internal static void DrawSkybox(Material material)
        {
            Rlgl.DisableBackfaceCulling();
            Rlgl.DisableDepthMask();
            DrawMesh(SKYBOX_MESH, material, Matrix4x4.Identity);
            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthMask();
        }

        /// <summary>Loads a skybox and configures its material.</summary>
        /// <param name="path">Path to .hdr file.</param>
        /// <returns>Configured skybox material.</returns>
        internal static Material LoadSkybox(string path)
        {
            Texture2D panorama = LoadTexture(path);
            Material mat = LoadMaterialDefault();
            mat.Shader = SkyboxShader;
            Texture2D cubemap = GenTextureCubemap(panorama, 3840, PixelFormat.UncompressedR8G8B8A8);
            SetMaterialTexture(ref mat, MaterialMapIndex.Cubemap, cubemap);
            UnloadTexture(panorama); // Unload unused texture;

            return mat;
        }

        /// <summary>Generates a render texture with a depth buffer attached to it.</summary>
        /// <returns></The configured render texture.returns>
        internal static RenderTexture2D GenRenderTexture(int width, int height)
        {
            return new RenderTexture2D();
        }

        /// <summary>Genreates a cubemap texture by processing data into a cubemap shader.</summary>
        /// <param name="panorama">2D texture to use for cubemap.</param>
        /// <param name="size">Pixel size of the cubemap.</param>
        /// <param name="format">Pixel format to use.</param>
        /// <returns>Configured cubemap texture.</returns>
        internal static unsafe Texture2D GenTextureCubemap(Texture2D panorama, int size, PixelFormat format)
        {
            Texture2D cubemap;

            // Disable Backface culling to render inside the cube
            Rlgl.DisableBackfaceCulling();

            // Setup frame buffer
            uint rbo = Rlgl.LoadTextureDepth(size, size, true);
            cubemap.Id = Rlgl.LoadTextureCubemap(null, size, format, 1);

            uint fbo = Rlgl.LoadFramebuffer();
            Rlgl.FramebufferAttach(fbo, rbo, FramebufferAttachType.Depth, FramebufferAttachTextureType.Renderbuffer, 0);
            Rlgl.FramebufferAttach(fbo, cubemap.Id, FramebufferAttachType.ColorChannel0, FramebufferAttachTextureType.CubemapPositiveY, 0);

            // Check if framebuffer is valid

            if (Rlgl.FramebufferComplete(fbo))
            {
                Console.WriteLine($"FBO: [ID {fbo}] Framebuffer object created successfully");
            }

            // Draw to framebuffer
            Rlgl.EnableShader(CubemapShader.Id);

            // Define projection matrix and send it to the shader
            Matrix4x4 matFboProjection = Raymath.MatrixPerspective(90.0f * DEG2RAD, 1.0f, Rlgl.CULL_DISTANCE_NEAR, Rlgl.CULL_DISTANCE_FAR);
            Rlgl.SetUniformMatrix(CubemapShader.Locs[(int)ShaderLocationIndex.MatrixProjection], matFboProjection);

            // Define view matrix for every side of the cube
            Matrix4x4[] fboViews = new Matrix4x4[]
            {
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3(-1.0f,  0.0f,  0.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 1.0f,  0.0f,  0.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  1.0f,  0.0f), new Vector3( 0.0f,  0.0f, -1.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f, -1.0f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f, -1.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f,  1.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
            };

            // Set viewport to current fbo dimensions
            Rlgl.Viewport(0, 0, size, size);

            // Activate and enable texture for drawing to cubemap faces
            Rlgl.ActiveTextureSlot(0);
            Rlgl.EnableTexture(panorama.Id);

            for (int i = 0; i < 6; i++)
            {
                // Set the view matrix for current face
                Rlgl.SetUniformMatrix(CubemapShader.Locs[(int)ShaderLocationIndex.MatrixView], fboViews[i]);

                // Select the current cubemap face attachment for the fbo
                Rlgl.FramebufferAttach(fbo, cubemap.Id, FramebufferAttachType.ColorChannel0, FramebufferAttachTextureType.CubemapPositiveX + i, 0);
                Rlgl.EnableFramebuffer(fbo);

                Rlgl.ClearScreenBuffers();
                Rlgl.LoadDrawCube();
            }

            // Unload framebuffer and reset state
            Rlgl.DisableShader();
            Rlgl.DisableTexture();
            Rlgl.DisableFramebuffer();

            Rlgl.UnloadFramebuffer(fbo);

            Rlgl.Viewport(0, 0, GetScreenWidth(), GetScreenHeight());
            Rlgl.EnableBackfaceCulling();

            cubemap.Width = size;
            cubemap.Height = size;
            cubemap.Mipmaps = 1;
            cubemap.Format = format;

            return cubemap;
        }
    }
}