using System.Numerics;
using Raylib_cs;

namespace Orion_Desktop
{
    internal struct Satellite
    {
        public float Latitude;
        public float Longitude;
        public Vector3 RelativePosition;

        public Satellite() { }
    }

    internal class CelestialMaths
    {
        public const float EARTH_RADIUS = 6371; // Km
        public const float ISS_ALTITUDE = 420; // Km

        internal static Vector3 ComputeECEF(float latitude, float longitude)
        {
            // Fix negative latitude
            if (longitude < 0) longitude = 180 + Math.Abs(longitude);
            // Convert data to radians
            float latRad = latitude * Raylib.DEG2RAD;
            float longRad = longitude * Raylib.DEG2RAD;

            // Compute coordinates based on ECEF equations
            float x = MathF.Cos(latRad) * MathF.Cos(longRad);
            float y = MathF.Cos(latRad) * MathF.Sin(longRad);
            float z = MathF.Sin(latRad);

            return new Vector3(x, z, y);
        }
    }
}