using System.Numerics;
using Raylib_cs;

namespace Orion_Desktop
{
    /// <summary>Defines the visibility of a satellite.</summary>
    internal enum SatelliteVisibility
    {
        daylight,
        nighttime
    }

    /// <summary>Defines the different units of satellite velocity measurement.</summary>
    internal enum SatelliteUnits
    {
        miles,
        kilometers
    }

    /// <summary>Represents an instance of a <see cref="Satellite"/> object.</summary>
    internal struct Satellite
    {
        // API informations
        public string Name;
        public float Latitude;
        public float Longitude;
        public float Altitude;
        public float Velocity;
        public SatelliteVisibility Visibility;
        public float Footprint;
        public long Timestamp;
        public SatelliteUnits Units;

        // Program informations
        public Vector3 RelativePosition;

        public Satellite() { }
    }

    /// <summary>Represents the static class for celestial maths.</summary>
    internal class CelestialMaths
    {
        public const float EARTH_RADIUS = 6371; // Km
        public const float ISS_ALTITUDE = 420; // Km

        internal static Vector3 ComputeECEF(float latitude, float longitude)
        {
            // Fix negative/positive latitude
            if (longitude < 0) longitude = 180 + Math.Abs(longitude);
            else longitude = 180 - longitude;
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