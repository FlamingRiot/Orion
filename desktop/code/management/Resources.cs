﻿using Raylib_cs;
using System.Numerics;
using Uniray;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents the GPU-loaded resources dictionary of the program.</summary>
    internal static class Resources
    {
        internal static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        internal static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        internal static Dictionary<ShapeType, Mesh> ShapeMeshes = new Dictionary<ShapeType, Mesh>();
        internal static Model Arrow;

        internal static Texture2D TargetPreview;

        // External resources
        internal static Dictionary<string, Model> Models = new Dictionary<string, Model>();
        internal static Dictionary<string, PBRMaterial> PBRMaterials = new Dictionary<string, PBRMaterial>();

        /// <summary>Loads the resources of the application.</summary>
        internal static void Init()
        {
            LoadMaterials();
            LoadModels();
            LoadMeshes();
        }

        /// <summary>Unloads the GPU-loaded resources.</summary>
        internal static void Close()
        {
            // Unload materials
            foreach (KeyValuePair<string, Material> item in Materials)
            {
                UnloadMaterial(item.Value);
            }

            // Unload pbr materials
            foreach (KeyValuePair<string, PBRMaterial> pBRMaterial in PBRMaterials) 
            {
                UnloadMaterial(pBRMaterial.Value.Material);
            }

            // Unload meshes
            foreach (KeyValuePair <string, Mesh> mesh in Meshes)
            {
                UnloadMesh(mesh.Value);
            }

            // Unload meshes
            foreach (KeyValuePair<ShapeType, Mesh> mesh in ShapeMeshes)
            {
                UnloadMesh(mesh.Value);
            }

            // Unload models
            foreach (KeyValuePair<string, Model> models in Models)
            {
                UnloadModel(models.Value);
            }
        }

        /// <summary>Loads the application's materials and configures them.</summary>
        private static void LoadMaterials()
        {
            Texture2D earthTex = LoadTexture("assets/textures/earth_preview.png");

            // Earth mat (texture-hologram)
            Material earthMat = LoadMaterialDefault();
            SetMaterialTexture(ref earthMat, MaterialMapIndex.Diffuse, earthTex);
            earthMat.Shader = Shaders.FixShader;
            Materials.Add("earth", earthMat);
            // Viewpoint material
            Material def = LoadMaterialDefault();
            def.Shader = Shaders.ViewpointShader;
            Materials.Add("viewpoint", def);

            // Load PBRs
            PBRMaterials.Add("rim", new PBRMaterial("assets/pbr/rim"));
            PBRMaterials.Add("terminal", new PBRMaterial("assets/pbr/terminal")); 
            PBRMaterials.Add("bench", new PBRMaterial("assets/pbr/bench")); 
            PBRMaterials.Add("ceiling", new PBRMaterial("assets/pbr/ceiling")); 
        }

        /// <summary>Loads the application's shape meshes.</summary>
        private static void LoadMeshes()
        {
            Meshes.Add("sphere", GenMeshSphere(EarthHologram.HOLOGRAM_RADIUS, 20, 20));
            float widthRatio = (float)GetScreenHeight() / Conceptor2D.SCREEN_RATIO;
            float heightRatio = (float)GetScreenWidth() / Conceptor2D.SCREEN_RATIO;
            Meshes.Add("screen", GenMeshPlane(widthRatio, heightRatio, 1 , 1));

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

            Meshes.Add("viewpoint", GenMeshPlane(0.2f, 0.2f, 100, 100));
        }

        /// <summary>Loads the application's custom 3D models from .m3d files.</summary>
        private static void LoadModels()
        {
            Model iss = LoadModel("assets/iss.m3d");
            SetMaterialShader(ref iss, 0, ref Shaders.FixShader);
            Models.Add("iss", iss);

            Arrow = LoadModel("assets/arrow.m3d");
            Texture2D mat = LoadTexture("assets/textures/arrow_Image_0.png");
            SetMaterialTexture(ref Arrow, 0, MaterialMapIndex.Diffuse, ref mat);
        }

        /// <summary>Sets the transform matrix for the arrow model.</summary>
        /// <param name="transform">Transform to set.</param>
        internal static void SetArrowTransform(Matrix4x4 transform)
        {
            Arrow.Transform = transform;
        }
    }
}