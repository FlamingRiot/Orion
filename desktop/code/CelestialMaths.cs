using System.Numerics;
using Raylib_cs;

namespace Orion_Desktop
{
    internal struct Satellite
    {
        public float Latitude;
        public float Longitude;

        public Satellite() { }
    }

    internal class CelestialMaths
    {
        public const float EARTH_RADIUS = 6371; // Km
        public const float ISS_ALTITUDE = 420; // Km

        internal static Vector3 ComputeECEF(float latitude, float longitude)
        {
            float radius = EARTH_RADIUS + ISS_ALTITUDE;

            // Convert data to radians
            float latRad = latitude * Raylib.DEG2RAD;
            float longRad = longitude * Raylib.DEG2RAD;

            // Compute coordinates based on ECEF equations
            float x = radius * MathF.Cos(latRad) * MathF.Cos(longRad);
            float z = radius * MathF.Cos(latRad) * MathF.Sin(longRad);
            float y = radius * MathF.Sin(latRad);

            return new Vector3(x, y, z);
        }
    }
}