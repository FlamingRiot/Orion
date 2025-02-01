﻿using System.Numerics;
using Orion_Desktop;
using Raylib_cs;

namespace Uniray
{
    /// <summary>Represents an instance of <see cref="UModel"/>, inheriting <see cref="GameObject3D"/>.</summary>
    public unsafe class UModel : GameObject3D, IRotation, IScalable
    {
        private float _pitch;
        private float _yaw;
        private float _roll;

        private float _width = 1;
        private float _height = 1;
        private float _depth = 1;

        // -----------------------------------------------------------
        // Public attributes
        // -----------------------------------------------------------

        /// <summary>Meshes of the object.</summary>
        public Mesh[] Meshes = new Mesh[0];
        /// <summary>Materials of the object.</summary>
        public Material[] Materials = new Material[0];

        /// <summary>X Axis rotation.</summary>
        public float Pitch { get { return _pitch; } set
            {
                _pitch = value;
                UpdateTransform();
            } 
        }
        /// <summary>Y Axis rotation</summary>
        public float Yaw
        {
            get { return _yaw; }
            set
            {
                _yaw = value;
                UpdateTransform();
            }
        }
        /// <summary>Z Axis rotation.</summary>
        public float Roll
        {
            get { return _roll; }
            set
            {
                _roll = value;
                UpdateTransform();
            }
        }

        /// <summary>X Axis Scale.</summary>
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                UpdateTransform();
            }
        }
        /// <summary>Y Axis Scale</summary>
        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdateTransform();
            }
        }
        /// <summary>Z Axis Scale.</summary>
        public float Depth
        {
            get { return _depth; }
            set
            {
                _depth = value;
                UpdateTransform();
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return new Vector3(Pitch, Yaw, Roll);
            }
            set
            {
                UpdateTransform();
            }
        }

        /// <summary>Model ID in the dictionary.</summary>
        public string ModelID;

        // -----------------------------------------------------------
        // Public properties
        // -----------------------------------------------------------
        /// <summary>3-Dimensional position of the object.</summary>
        public override Vector3 Position 
        {
            get 
            {
                return new Vector3(X, Y, Z);
            } 
            set 
            {
                X = value.X; 
                Y = value.Y; 
                Z = value.Z;
            } 
        }

        /// <summary>Gets the amount of meshes the object has.</summary>
        public int MeshCount {  get { return Meshes.Length; } }
        /// <summary>Gets the amount of materials the object has.</summary>
        public int MaterialCount {  get { return Materials.Length; } }

        /// <summary>Creates an empty instance of <see cref="UModel"/>.</summary>
        public UModel() : base()
        {
            ModelID = "";
        }

        /// <summary>Creates an instance of <see cref="UModel"/>.</summary>
        /// <param name="name">Object name</param>
        /// <param name="position">Object position</param>
        public UModel(string name, Vector3 position, string modelID) : base(name, position)
        {
            // Apply Model
            ModelID = modelID;
            LoadMeshes();
            LoadMaterials();

            // Position
            Transform = Matrix4x4.Identity;
            Position = position;
        }

        /// <summary>Draws the model's meshes to the screen.</summary>
        public override void Draw()
        {
            for (int j = 0; j < MeshCount; j++)
            {
                Raylib.DrawMesh(Meshes[j], Materials[j + 1], Transform);
            }
        }

        /// <summary>Loads meshes and materials from registered keys.</summary>
        internal void LoadMeshes()
        {
            if (ModelID != "")
            {
                try
                {
                    int meshCount = Resources.Models[ModelID].MeshCount;
                    Meshes = new Mesh[meshCount];
                    Mesh* meshes = Resources.Models[ModelID].Meshes;
                    for (int i = 0; i < meshCount; i++) Meshes[i] = meshes[i];
                }
                catch
                {
#if !DEBUG
                    ErrorHandler.Send(new Error(3, $"No model found at registered location -> {ModelID}"));
#elif DEBUG
                    Raylib.TraceLog(TraceLogLevel.Warning, $"No model found at registered location -> {ModelID}");
#endif
                }
            }
        }

        /// <summary>Loads personal materials (used for applying different shaders and effects).</summary>
        internal void LoadMaterials()
        {
            int materialCount = Resources.Models[ModelID].MaterialCount;
            Materials = new Material[materialCount];
            Material* materials = Resources.Models[ModelID].Materials;
            for (int i = 0; i < materialCount; i++) Materials[i] = materials[i];
        }

        /// <summary>Returns informations about the current instance.</summary>
        /// <returns>Informations as a <see langword="string"/>.</returns>
        public override string ToString()
        {
            return "Name: " + Name + " Position: " + Position + " Rotation: < " + Yaw + "; " + Pitch + "; " + Roll + " >";  
        }

        /// <summary>Updates the entire transform matrix of the object.</summary>

        private void UpdateTransform()
        {
            // Calculate matrix rotation
            Matrix4x4 rm = Raymath.MatrixRotateXYZ(new Vector3(_pitch / Raylib.RAD2DEG, _yaw / Raylib.RAD2DEG, _roll / Raylib.RAD2DEG));
            Matrix4x4 sm = Raymath.MatrixScale(_width, _height, _depth);
            Matrix4x4 pm = Raymath.MatrixTranslate(X, Y, Z);
            // Multiply matrices
            Transform = pm * sm * rm;
        }
    }
}