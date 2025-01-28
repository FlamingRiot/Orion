using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the GPU-loaded resources dictionary of the program.</summary>
    internal static class Resources
    {
        internal static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        internal static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();

        /// <summary>Loads the resources of the application.</summary>
        internal static void Init()
        {
            LoadMaterials();
            LoadMeshes();
        }

        /// <summary>Loads the application's textures.</summary>
        private static void LoadMaterials()
        {
            Material earthMat = LoadMaterialDefault();
            SetMaterialTexture(ref earthMat, MaterialMapIndex.Diffuse, LoadTexture("assets/earth.png"));
            earthMat.Shader = Shaders.HologramShader;
            Materials.Add("earth", earthMat);
        }

        /// <summary>Loads the application's textures.</summary>
        private static void LoadMeshes()
        {
            Meshes.Add("sphere", GenMeshSphere(EarthHologram.HOLOGRAM_RADIUS, 20, 20));
        }
    }
}