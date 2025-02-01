using System.Numerics;
using Orion_Desktop;
using Raylib_cs;

namespace Uniray
{
    /// <summary>Defines the type of the <see cref="UShape"/> object.</summary>
    public enum ShapeType
    {
        Cube,
        Sphere,
        Cylinder,
        Cone,
        HemiSphere,
        Knot,
        Torus
    }

    /// <summary>Represents a <see cref="UShape"/> game object.</summary>
    public class UShape : GameObject3D, IRotation, IScalable
    {
        // -----------------------------------------------------------
        // Private attributes
        // -----------------------------------------------------------
        private ShapeType shapeType;
        private float _pitch;
        private float _yaw;
        private float _roll;

        private float _width = 1;
        private float _height = 1;
        private float _depth = 1;

        // -----------------------------------------------------------
        // Public attributes
        // -----------------------------------------------------------
        public Mesh Mesh;
        public Material Material;

        // -----------------------------------------------------------
        // Public properties
        // -----------------------------------------------------------
        /// <summary>3-Dimensional position of the object.</summary>
        public override Vector3 Position
        {
            get
            {
                return new Vector3(Transform.M14, Transform.M24, Transform.M34);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>X Axis rotation.</summary>
        public float Pitch
        {
            get { return _pitch; }
            set
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

        /// <summary>Type of the shape object.</summary>
        public ShapeType Type { get { return shapeType; } set { shapeType = value; } }

        /// <summary>Creates an empty <see cref="UShape"/> instance.</summary>
        public UShape() : base()
        {
            // Empty shape object
            Type = ShapeType.Cube; // Default
            Mesh = Resources.ShapeMeshes[Type];
            Material = Raylib.LoadMaterialDefault();
        }

        /// <summary>Creates an instance of <see cref="UShape"/>.</summary>
        /// <param name="name">Name of shape</param>
        /// <param name="position">Position of the shape.</param>
        /// <param name="type">Type of the shape object.</param>
        public UShape(string name, Vector3 position, ShapeType type) : base(name, position)
        {
            Type = type;
            Mesh = Resources.ShapeMeshes[Type];
            Material = Raylib.LoadMaterialDefault();
        }

        /// <summary>Draws a shape object to the screen.</summary>
        public override void Draw()
        {
            Raylib.DrawMesh(Mesh, Material, Transform);
        }

        /// <summary>Updates the mesh used for rendering using the current type.</summary>
        internal void UpdateMesh()
        {
            Mesh = Resources.ShapeMeshes[Type];
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