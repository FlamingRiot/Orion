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
            Material.Maps[(int)MaterialMapIndex.Metalness].Value = 1;
            Material.Maps[(int)MaterialMapIndex.Roughness].Value = 1;
            Material.Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
            Material.Maps[(int)MaterialMapIndex.Emission].Color = Color.Black;


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ORION: PBR Material loaded successfully");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    /// <summary>Defines the type of a light source.</summary>
    internal enum LightType
    {
        LIGHT_DIRECTIONAL,
        LIGHT_POINT,
        LIGHT_SPOT
    }

    /// <summary>Represents an instance of light point in 3D world.</summary>
    internal struct Light
    {
        internal int TypeLoc;
        internal int EnabledLoc;
        internal int PositionLoc;
        internal int TargetLoc;
        internal int ColorLoc;
        internal int IntensityLoc;

        internal LightType Type;
        internal bool Enabled;
        internal Vector3 Position;
        internal Vector3 Target;
        internal Color Color;
        internal float Intensity;
    }

    /// <summary>Represents an instance of the static lights management class.</summary>
    internal static class Rlights
    {
        internal const int MAX_LIGHTS = 4;

        /// <summary>Creates a light source and links its shader attributes.</summary>
        /// <param name="lightsIndex">Index within total lights.</param>
        /// <param name="type">Light type.</param>
        /// <param name="pos">Light source position.</param>
        /// <param name="target">Light source target.</param>
        /// <param name="color">Light source color.</param>
        /// <param name="shader">Shader to use for light rendering.</param>
        /// <returns>Correctly configured light source.</returns>
        internal static Light CreateLight(
            int lightsIndex,
            LightType type,
            Vector3 pos,
            Vector3 target,
            Color color,
            float intensity,
            Shader shader
        )
        {
            Light light = new();

            light.Enabled = true;
            light.Type = type;
            light.Position = pos;
            light.Target = target;
            light.Color = color;
            light.Intensity = intensity;

            string enabledName = "lights[" + lightsIndex + "].enabled";
            string typeName = "lights[" + lightsIndex + "].type";
            string posName = "lights[" + lightsIndex + "].position";
            string targetName = "lights[" + lightsIndex + "].target";
            string colorName = "lights[" + lightsIndex + "].color";
            string intensityName = "lights[" + lightsIndex + "].intensity";

            light.EnabledLoc = GetShaderLocation(shader, enabledName);
            light.TypeLoc = GetShaderLocation(shader, typeName);
            light.PositionLoc = GetShaderLocation(shader, posName);
            light.TargetLoc = GetShaderLocation(shader, targetName);
            light.ColorLoc = GetShaderLocation(shader, colorName);
            light.IntensityLoc = GetShaderLocation(shader, intensityName);

            UpdateLightValues(shader, light);

            return light;
        }

        /// <summary>Updates the values of a light source.</summary>
        /// <param name="shader">Shader to update values to.</param>
        /// <param name="light">Light source to update.</param>
        internal static void UpdateLightValues(Shader shader, Light light)
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
            float[] color = new[]
            {
                light.Color.R / (float)255,
                light.Color.G / (float)255,
                light.Color.B / (float)255,
                light.Color.A / (float)255
            };
            SetShaderValue(shader, light.ColorLoc, color, ShaderUniformDataType.Vec4);
            SetShaderValue(shader, light.IntensityLoc, light.Intensity, ShaderUniformDataType.Float);
        }
    }

    /// <summary>Represents an instance of the static shader class.</summary>
    internal static class Shaders
    {
        internal static Shader HologramShader;
        internal static Shader PBRLightingShader;

        internal static Light GlobalLight;

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
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "numOfLights"), Rlights.MAX_LIGHTS, ShaderUniformDataType.Int);
            // Set PBR shader ambient color and intensity parameters
            float ambientIntensity = 0.02f;
            Color ambientColor = new Color(0, 0, 0, 255);
            Vector3 ambientColorNormalized = new Vector3(ambientColor.R, ambientColor.G, ambientColor.B) / 255;
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambientColor"), ambientColorNormalized, ShaderUniformDataType.Vec3);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambient"), ambientIntensity, ShaderUniformDataType.Float);
            // Get real-time modification elligible uniforms
            EmissivePowerLoc = GetShaderLocation(PBRLightingShader, "emissivePower");
            EmissiveColorLoc = GetShaderLocation(PBRLightingShader, "emissiveColor");
            TextureTilingLoc = GetShaderLocation(PBRLightingShader, "tiling");
            // Create main light source
            GlobalLight = Rlights.CreateLight(0, LightType.LIGHT_POINT, new Vector3(-3.7f, 5, 0f), Vector3.Zero, new Color(253, 255, 184, 255), 5, PBRLightingShader);
            // Set PBR shader used maps
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexAlbedo"), 1, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexNormal"), 1, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexMRA"), 1, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexEmissive"), 1, ShaderUniformDataType.Int);
        }

        internal static unsafe void UpdatePBRLighting(Vector3 viewPos)
        {
            SetShaderValue(PBRLightingShader, PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView], viewPos, ShaderUniformDataType.Vec3);
            // Update tiling values
            SetShaderValue(PBRLightingShader, TextureTilingLoc, Vector2.One / 2, ShaderUniformDataType.Vec2);
            // Update intensity
            SetShaderValue(PBRLightingShader, EmissivePowerLoc, 0.01f, ShaderUniformDataType.Float);
            SetShaderValue(PBRLightingShader, EmissiveColorLoc, Vector4.One, ShaderUniformDataType.Vec4);
            Rlights.UpdateLightValues(PBRLightingShader, GlobalLight);
        }

        /// <summary>Loads the shader materials of the application.</summary>
        internal static void LoadMaterials()
        {

        }
    }
}