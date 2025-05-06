using System.Numerics;

namespace Orion_Desktop
{
    /// <summary>Static class used for interpolators storrage.</summary>
    internal static class Interpolators
    {
        // Earth hologram interpolators
        internal static float EarthPitch;
        internal static float EarthYaw;
        internal static Vector3 EarthCenter;

        // Camera interpolators
        internal static Vector3 CameraPosition;
        internal static Vector3 CameraTarget;

        // Orion terminal interpolators
        internal static Vector3 TerminalCenter;
        internal static float TerminalPitch;
        internal static float TerminalYaw;  
    }
}