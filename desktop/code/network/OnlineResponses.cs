#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8601

using System.Numerics;
using Newtonsoft.Json.Linq;

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
        eclipsed
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

    internal class PlanetCacheEntry
    {
        public AstralTarget Name;
    }
}