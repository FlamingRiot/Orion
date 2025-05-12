using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace Orion_Desktop
{
    /*-----------------------------------------------------------------
     Map-Tiling classes, functions and variables. 
     ------------------------------------------------------------------*/
    /// <summary>Represents the 2D map-tiling managing class.</summary>
    internal static class TilingManager
    {
        // Constants
        internal const string MAP_CONFIG = $"satellite";
        internal const int BASE_ZOOM = 2;

        /*
            The following 'configs' dictionary is meant to hold the dimensions (as tiles) for a defined
            zoom level of tiling. 
        */
        internal static Dictionary<int, Vector2> Configs = new Dictionary<int, Vector2>();

        // Attributes
        internal static int CurrentZoom = BASE_ZOOM;

        // Active tiles
        private static List<MapTile> Tiles = new List<MapTile>();

        /// <summary>Draws the Map-tiling (Required context: 2D).</summary>
        internal static void DrawMapManager()
        {
            Tiles.ForEach(t => t.Draw());
        }

        /// <summary>Converts geographic coordinates to tile coordinates.</summary>
        /// <param name="lat">Geographic latitude.</param>
        /// <param name="lon">Geographics longitude.</param>
        /// <param name="zoomLevel">Tile zoom level.</param>
        /// <returns></returns>
        internal static Vector2 ConvertCoordinatesToTiles(float lat, float lon, int zoomLevel)
        {
            // Define bounds
            int latMax = 180;
            int lonMax = 360;

            // Adapt
            lat += latMax / 2;
            lon += lonMax / 2;

            // Retrieve zoom-level values
            Vector2 position = new Vector2(0);
            Configs.TryGetValue(zoomLevel, out position);

            position.X = lat / latMax * position.X;
            position.Y = lon / lonMax * position.Y;

            return position;
        }
    }

    /// <summary>Represents a single map-tile used for 2D rendering in <see cref="TilingManager"/>.</summary>
    internal struct MapTile
    {
        // Constants
        internal const int TILE_SIZE = 256;

        // Rectangles used for tile rendering
        private static Rectangle _source = new Rectangle(0, 0, TILE_SIZE, TILE_SIZE);
        private Rectangle _target;

        // Attributes
        public int Row;
        public int Column;
        public Texture2D Texture;

        /// <summary>Creates an instance of <see cref="MapTile"/>.</summary>
        /// <param name="row">Map row.</param>
        /// <param name="column">Map column.</param>
        internal MapTile(int row, int column)
        {
            Row = row;
            Column = column;
            _target = new Rectangle(TILE_SIZE * column, TILE_SIZE * row, TILE_SIZE, TILE_SIZE);
            Texture = new Texture2D();
        }

        /// <summary>Draws a single map-tile according to its relative position.</summary>
        internal readonly void Draw()
        {
            DrawTexturePro(Texture, _source, _target, Vector2.Zero, 0, Color.White);
        }

        /// <summary>Defines whether a <see cref="MapTile"/> instance and a <see cref="Vector2"/> are equal or not.</summary>
        /// <param name="mpa">MapTile to test.</param>
        /// <param name="position">Position to test.</param>
        /// <returns><see langword="true"/> if the two are equal. <see langword="false"/> otherwise.</returns>
        public static bool operator ==(MapTile mpa, Vector2 position) => new Vector2(mpa.Column, mpa.Row) == position;

        /// <summary>Defines whether a <see cref="MapTile"/> instance and a <see cref="Vector2"/> are equal or not.</summary>
        /// <param name="mpa">MapTile to test.</param>
        /// <param name="position">Position to test.</param>
        /// <returns><see langword="true"/> if the two are not equal. <see langword="false"/> otherwise.</returns>
        public static bool operator !=(MapTile mpa, Vector2 position) => new Vector2(mpa.Column, mpa.Row) != position;

        /// <summary>Defines the wanted Equals function for this structure.</summary>
        /// <param name="obj">Object to test.</param>
        /// <returns><see langword="true"/> if the two are not equal. <see langword="false"/> otherwise.</returns>
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        /// <summary>Defines the wanted GetHashCode function for this structure.</summary>
        /// <returns>Hashcode as an <see langword="int"/>.</returns>
        public override readonly int GetHashCode() => base.GetHashCode();
    }
}