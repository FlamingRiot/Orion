using System.Numerics;

namespace Uniray
{
    /// <summary>Defines the necessity to be drawable.</summary>
    public interface IDrawable 
    { 
        void Draw(); // Draw function
    }

    /// <summary>Defines the ability of an object to rotate.</summary>
    public interface IRotation
    {
        float Pitch { get; set; }
        float Yaw { get; set; }
        float Roll { get; set; }
    }

    /// <summary>Defines the ability of an object to be scaled.</summary>
    public interface IScalable
    {
        float Width { get; set; }
        float Height { get; set; }
        float Depth { get; set; }
    }

    /// <summary>Reprsents an instance of <see cref="GameObject3D"/>.</summary>
    public abstract class GameObject3D : IDrawable
    {
        private string _scene;
        // -----------------------------------------------------------
        // Public attributes
        // -----------------------------------------------------------
        /// <summary>Transformative matrix of the object.</summary>
        public Matrix4x4 Transform;
        /// <summary>Name of the object.</summary>
        private string _name;

        // -----------------------------------------------------------
        // Public properties
        // -----------------------------------------------------------
        /// <summary>3-Dimensional object position.</summary>
        abstract public Vector3 Position { get; set; }
        /// <summary>X Position of the object.</summary>
        public virtual float X { get { return Transform.M14; } set { Transform.M14 = value; } }
        /// <summary>X Position of the object.</summary>
        public virtual float Y { get { return Transform.M24; } set { Transform.M24 = value; } }
        /// <summary>X Position of the object.</summary>
        public virtual float Z { get { return Transform.M34; } set { Transform.M34 = value; } }
        /// <summary>X Position of the object.</summary>
        public string Name { get { return _name; } set { _name = value; } }
        /// <summary>Scene containing this game object.</summary>
        public string Scene { get { return _scene; } set { _scene = value; } }

        /// <summary>Creates an instance of <see cref="GameObject3D"/>.</summary>
        public GameObject3D() 
        {
            _name = "";
            _scene = "";
            Transform = Matrix4x4.Identity; // Get identity (default) matrix
        }

        /// <summary>Creates an instance of <see cref="GameObject3D"/>.</summary>
        /// <param name="name">Object name</param>
        /// <param name="position">Object position</param>
        public GameObject3D(string name, Vector3 position)
        {
            _name = name;
            _scene = "";
            Transform = Matrix4x4.Identity; // Get identity (default) matrix
            Position = position;
        }

        /// <summary>Draws the Game Object to the screen.</summary>
        public abstract void Draw();

        /// <summary>Returns informations about the current instance.</summary>
        /// <returns>Informations as a <see langword="string"/>.</returns>
        public override string ToString()
        {
            return "Name : " + Name + " Position : ";
        }
    }
}