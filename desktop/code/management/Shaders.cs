using Raylib_cs;
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
            Console.WriteLine($"ORION: PBR Material loaded successfully");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    /// <summary>Defines the type of a light source.</summary>
    public enum PbrLightType
    {
        Directorional,
        Point,
        Spot
    }

    /// <summary>Represents an instance of light point in 3D world.</summary>
    public struct PbrLight
    {
        public PbrLightType Type;
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
    public class PbrLights
    {
        public static PbrLight CreateLight(
            int lightsCount,
            PbrLightType type,
            Vector3 pos,
            Vector3 target,
            Color color,
            float intensity,
            Shader shader
        )
        {
            PbrLight light = new();

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

        public static void UpdateLightValues(Shader shader, PbrLight light)
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
        internal static Shader HologramShader;
        internal static Shader PBRLightingShader;

        internal static PbrLight[] Lights = new PbrLight[4];

        internal static int EmissivePowerLoc;
        internal static int EmissiveColorLoc;
        internal static int TextureTilingLoc;

        internal static void Init()
        {
            LoadShaders();
            LoadMaterials();
        }

        /// <summary>Loads the shaders of the application.</summary>
        internal static unsafe void LoadShaders()
        {
            HologramShader = LoadShader("assets/shaders/shader.vs", "assets/shaders/shader.fs");
            PBRLightingShader = LoadShader("assets/shaders/pbr.vs", "assets/shaders/pbr.fs");

            // Modify PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapAlbedo] = GetShaderLocation(PBRLightingShader, "albedoMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapMetalness] = GetShaderLocation(PBRLightingShader, "mraMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapNormal] = GetShaderLocation(PBRLightingShader, "normalMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapEmission] = GetShaderLocation(PBRLightingShader, "emissiveMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.ColorDiffuse] = GetShaderLocation(PBRLightingShader, "albedoColor");

            // Set PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(PBRLightingShader, "viewPos");
            var lightCountLoc = GetShaderLocation(PBRLightingShader, "numOfLights");
            var maxLightCount = 4;
            SetShaderValue(PBRLightingShader, lightCountLoc, &maxLightCount, ShaderUniformDataType.Int);            // Set PBR shader ambient color and intensity parameters

            // Setup ambient color and intensity parameters
            float ambientIntensity = 0.01f;
            Color ambientColor = new Color(0, 0, 0, 255);
            Vector3 ambientColorNormalized = new Vector3(ambientColor.R / 255f, ambientColor.G / 255f, ambientColor.B / 255f);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambientColor"), &ambientColorNormalized, ShaderUniformDataType.Vec3);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambient"), &ambientIntensity, ShaderUniformDataType.Float);
       
            // Get real-time modification elligible uniforms
            EmissivePowerLoc = GetShaderLocation(PBRLightingShader, "emissivePower");
            EmissiveColorLoc = GetShaderLocation(PBRLightingShader, "emissiveColor");
            TextureTilingLoc = GetShaderLocation(PBRLightingShader, "tiling");
            // Create main light source
            Lights[0] = PbrLights.CreateLight(0, PbrLightType.Point, EarthHologram.CENTER, Vector3.Zero, new Color(8, 181, 255, 255), 10, PBRLightingShader);
            Lights[1] = PbrLights.CreateLight(1, PbrLightType.Point, EarthHologram.CENTER + Vector3.UnitY * 6, Vector3.Zero, Color.White, 50, PBRLightingShader);
            Lights[2] = PbrLights.CreateLight(2, PbrLightType.Point, new Vector3(-10f, 2f, -0.1f), Vector3.Zero, new Color(8, 181, 255, 255), 50, PBRLightingShader);
            //GlobalLight = PbrLights.CreateLight(0, PbrLightType.Point, new Vector3(-3.7f, 6, 0f), Vector3.Zero, new Color(253, 255, 184, 255), 50, PBRLightingShader);
            // Set PBR shader used maps
            int usage = 1;
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexAlbedo"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexNormal"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexMRA"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexEmissive"), &usage, ShaderUniformDataType.Int);
        }

        internal static unsafe void UpdatePBRLighting(Vector3 viewPos)
        {
            SetShaderValue(PBRLightingShader, PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView], viewPos, ShaderUniformDataType.Vec3);
            //UpdateLight(PBRLightingShader, GlobalLight);
            // Update tiling values
            SetShaderValue(PBRLightingShader, TextureTilingLoc, Vector2.One / 2, ShaderUniformDataType.Vec2);
            // Update intensity
            SetShaderValue(PBRLightingShader, EmissivePowerLoc, 0.5f, ShaderUniformDataType.Float);
            SetShaderValue(PBRLightingShader, EmissiveColorLoc, Vector4.One, ShaderUniformDataType.Vec4);
        }

        internal static void UpdateLight(Shader shader, PbrLight light)
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

        /// <summary>Loads the shader materials of the application.</summary>
        internal static void LoadMaterials()
        {

        }
    }
}