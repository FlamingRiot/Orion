using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Orion_Desktop
{
    internal static class Shaders
    {
        internal static Shader HologramShader;

        internal static void Init()
        {
            LoadShaders();
            LoadMaterials();
        }

        /// <summary>Loads the shaders of the application.</summary>
        internal static void LoadShaders()
        {
            HologramShader = LoadShader("assets/shaders/shader.vs", "assets/shaders/shader.fs");
        }

        /// <summary>Loads the shader materials of the application.</summary>
        internal static void LoadMaterials()
        {

        }
    }
}