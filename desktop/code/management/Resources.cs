using Raylib_cs;
using Uniray;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the GPU-loaded resources dictionary of the program.</summary>
    internal static class Resources
    {
        // Internal resources
        internal static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        internal static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        internal static Dictionary<ShapeType, Mesh> ShapeMeshes = new Dictionary<ShapeType, Mesh>();

        // External resources
        internal static Dictionary<string, Model> Models = new Dictionary<string, Model>();
        internal static Dictionary<string, PBRMaterial> PBRMaterials = new Dictionary<string, PBRMaterial>();

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
            SetMaterialTexture(ref earthMat, MaterialMapIndex.Diffuse, LoadTexture("assets/textures/earth.png"));
            earthMat.Shader = Shaders.HologramShader;
            Materials.Add("earth", earthMat);

            // Load PBRs
            PBRMaterials.Add("rim", new PBRMaterial("assets/pbr/rim")); 
            PBRMaterials.Add("terminal", new PBRMaterial("assets/pbr/terminal")); 
        }

        /// <summary>Loads the application's textures.</summary>
        private static void LoadMeshes()
        {
            Meshes.Add("sphere", GenMeshSphere(EarthHologram.HOLOGRAM_RADIUS, 20, 20));

            // Load UShape Meshes
            ShapeMeshes = new Dictionary<ShapeType, Mesh>()
            {
                { ShapeType.Cube, GenMeshCube(1, 1, 1) },
                { ShapeType.Sphere, GenMeshSphere(0.5f, 20, 20) },
                { ShapeType.Cylinder, GenMeshCylinder(0.5f, 1, 20) },
                { ShapeType.Cone, GenMeshCone(0.5f, 1, 20) },
                { ShapeType.Torus, GenMeshTorus(0.5f, 1, 20, 20) },
                { ShapeType.HemiSphere, GenMeshHemiSphere(0.5f, 15, 15) },
                { ShapeType.Knot, GenMeshKnot(0.5f, 1, 20, 20) }
            };
        }
    }
}