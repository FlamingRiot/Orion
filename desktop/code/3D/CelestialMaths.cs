#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Numerics;
using Raylib_cs;

namespace Orion_Desktop
{
    /// <summary>Represents the static class for celestial maths.</summary>
    internal class CelestialMaths
    {
        public const float EARTH_RADIUS = 6371; // Km
        public const float ISS_ALTITUDE = 420; // Km

        // temp
        public const float POSITION_LATITUDE = 40 + 6.94f;
        public const float POSITION_LONGITUDE = 46.99f - 40;

        /// <summary>Computes an object's 3D position based on its latitude/longitude.</summary>
        /// <param name="latitude">Latitude of the object.</param>
        /// <param name="longitude">Longitude of the object.</param>
        /// <param name="globeYaw">Yaw angle of the globe (used for interface mode.)</param>
        /// <returns>3D position of the object on a normalized basis.</returns>
        internal static Vector3 ComputeECEFTilted(float latitude, float longitude, float globeYaw)
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
           
            Matrix4x4 rotation = Raymath.MatrixRotateZ(-globeYaw / Raylib.RAD2DEG);
            rotation *= Raymath.MatrixRotateY(-EarthHologram.EARTH_TILT / Raylib.RAD2DEG);

            float newX = rotation.M11 * x + rotation.M12 * y + rotation.M13 * z;
            float newY = rotation.M21 * x + rotation.M22 * y + rotation.M23 * z;
            float newZ = rotation.M31 * x + rotation.M32 * y + rotation.M33 * z;

            return new Vector3(newX, newZ, newY);
        }

        /// <summary>Computes an object's 3D position based on its latitude/longitude. (used for robot information computing.)</summary>
        /// <param name="latitude">Latitude of the object.</param>
        /// <param name="longitude">Longitude of the object.</param>
        /// <returns>3D position of the object on a normalized basis.</returns>
        internal static Vector3 ComputeECEF(float latitude, float longitude)
        {
            // Fix negative/positive longitude
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

        /// <summary>Computes the latitude/longitude of a 3D position around a sphere.</summary>
        /// <param name="position">Position around the sphere.</param>
        /// <returns>Calculated Latitude & Longitude.</returns>
        internal static (float, float) ComputeECEFeverse(Vector3 position)
        {
            // Reverse latitude and longitude as radians (from ECEF equations)
            float latRad = MathF.Asin(position.Y);
            float longRad = MathF.Asin(position.Z / MathF.Cos(latRad));
        
            // Convert from radians to degrees
            float latitude = latRad * Raylib.RAD2DEG;
            float longitude = longRad * Raylib.RAD2DEG;

            // Recover negative/positive longitude fix
            if (longitude - 180 > 0)
            {
                longitude -= 180;
                longitude -= longitude * 2;
            }
            else
            {
                longitude = 180 - longitude;
            }

            return (latitude, longitude);
        }
    }
}