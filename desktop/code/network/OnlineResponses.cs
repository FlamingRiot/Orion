#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace Orion_Desktop
{
    /*-----------------------------------------------------------------
               Satellite response information based on API. 
    -------------------------------------------------------------------*/

    /// <summary>Defines the visibility of a satellite.</summary>
    internal enum SatelliteVisibility
    {
        daylight,
        nighttime,
        eclipsed,
        visible
    }

    /// <summary>Defines the different units of satellite velocity measurement.</summary>
    internal enum SatelliteUnits
    {
        miles,
        kilometers
    }

    /// <summary>Represents an instance of a <see cref="Satellite"/> object.</summary>
    internal class SatelliteInfo
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
        public Vector3 RelativePosition;

        public SatelliteInfo() { Name = ""; }

        /// <summary>Updates the values of the satellite based on a new json object.</summary>
        /// <param name="json">Json object to use.</param>
        public void UpdateSatellite(JObject json)
        {
            Name = (string)json["name"];
            Latitude = (float)json["latitude"];
            Longitude = (float)json["longitude"];
            Altitude = (float)json["altitude"];
            Velocity = (float)json["velocity"];
            Visibility = (SatelliteVisibility)Enum.Parse(typeof(SatelliteVisibility), (string)json["visibility"]);
            Units = (SatelliteUnits)Enum.Parse(typeof(SatelliteUnits), (string)json["units"]);
            Footprint = (float)json["footprint"];
            Timestamp = (long)json["timestamp"];
        }
    }

    /*-----------------------------------------------------------------
               Planet response information based on API. 
    -------------------------------------------------------------------*/

    /// <summary>Represents an entry of planet information.</summary>
    internal class PlanetCacheEntry
    {
        internal const string PLANET_CACHE_FILE = $"{OnlineRequests.CACHE_DIRECTORY}/Orion-Desktop.planets.json";
        internal const char SEPARATOR = '#';

        internal AstralTarget Name;
        internal readonly string Raw;

        internal float Altitude;
        internal float Azimuth;
        internal float Distance;
        internal string Date;

        internal Vector3 NormalizedPosition;

        /// <summary>Creates an instance of <see cref="PlanetCacheEntry"/> by deserializing a <see cref="JObject"/>.</summary>
        /// <param name="json">JSON Object to parse.</param>
        /// <param name="write">Defines whether or not the newly created planet entry should be written in cache.</param>
        internal PlanetCacheEntry(JObject json, bool write)
        {
            // Store raw
            Raw = json.ToString();

            // Get Type name
            Name = (AstralTarget)Enum.Parse(typeof(AstralTarget), (string)json["data"]["table"]["rows"][0]["entry"]["name"]);
            // Get degrees for Alt/Az
            Altitude = (float)json["data"]["table"]["rows"][0]["cells"][0]["position"]["horizontal"]["altitude"]["degrees"];
            Azimuth = (float)json["data"]["table"]["rows"][0]["cells"][0]["position"]["horizontal"]["azimuth"]["degrees"];
            Distance = (float)json["data"]["table"]["rows"][0]["cells"][0]["distance"]["fromEarth"]["au"]; // As astronomical units
            Date = (string)json["data"]["table"]["rows"][0]["cells"][0]["date"];
            Date = Date.Split(' ')[0];
            Date ??= ""; // F*ck Microsoft

            // Compute data
            NormalizedPosition = CelestialMaths.ComputeHorizontalCoordinates(Azimuth, Altitude);

            // Add or replace cache entry in already existing ones
            if (write)
            {
                string text = "";
                // Update data in RAM
                foreach (PlanetCacheEntry entry in OnlineRequests.PlanetCacheEntries)
                {
                    if (entry.Name == this.Name) continue; // Skip old one
                    else text += entry.Raw + SEPARATOR;
                }
                text += this.Raw;

                // Write data to cache
                StreamWriter stream = new StreamWriter(PLANET_CACHE_FILE, false);
                stream.Write(text);
                stream.Close();
            }
        }

        /// <summary>Creates an empty instance of <see cref="PlanetCacheEntry"/>.</summary>
        internal PlanetCacheEntry()
        {
            Raw = "";
            Date = "";
        }
    }
}