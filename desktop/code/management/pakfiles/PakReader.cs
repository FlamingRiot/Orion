using Raylib_cs;
using System.IO.Compression;
namespace Orion_Desktop
{
    /// <summary>Represents an instance of <see cref="PakReader"/>.</summary>
    internal unsafe class PakReader
    {
        // -----------------------------------------------------------
        // Private and internal instances
        // -----------------------------------------------------------

        public const string DEFAULT_TMP_FILE = "TEMP_PAK_FILE"; // Temporary file for not supported Load*FromMemory functions

        private readonly Dictionary<string, PakFileEntry> entries = new Dictionary<string, PakFileEntry>();
        private readonly string _pakFilePath;
        private readonly long _tableOffset;
        private readonly int _fileCount;

        /// <summary>Creates a connection to .pak file.</summary>
        /// <param name="pakFilePath"></param>
        public PakReader(string pakFilePath)
        {
            _pakFilePath = pakFilePath;

            // Open and read pak file
            FileStream pakFile = new FileStream(_pakFilePath, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(pakFile))
            {
                // Read entries count
                _fileCount = reader.ReadInt32(); // 4 bytes entry
                // Read table offset
                _tableOffset = reader.ReadInt64(); // 8 bytes entry
                // Move to table offset
                pakFile.Seek(_tableOffset, SeekOrigin.Begin);
                // Loop over different file entries
                for (int i = 0; i < _fileCount; i++)
                {
                    // Read entry informations
                    string fileName = reader.ReadString();
                    string fileType = reader.ReadString();
                    long fileOriginalSize = reader.ReadInt64();
                    long fileCompressedSize = reader.ReadInt64();
                    long offset = reader.ReadInt64();

                    entries.Add(fileName, new PakFileEntry(fileName, fileType, fileOriginalSize, fileCompressedSize, offset));
                }
            }
        }

        /// <summary>Loads a file from a .pak file from file name.</summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns><see langword="byte[]"/> containing the file's bytes.</returns>
        /// <exception cref="Exception">File not found.</exception>
        public byte[] LoadRawFile(string fileName)
        {
            // Check if file exists in archive entries
            if (!entries.ContainsKey(fileName))
                throw new Exception($"File {fileName}not found in .pak archive");
            // Get pak entry
            PakFileEntry entry = entries[fileName];

            using (FileStream pakFile = new FileStream(_pakFilePath, FileMode.Open))
            {
                // Move to file index
                pakFile.Seek(entry.Index, SeekOrigin.Begin);
                // Read compressed data from pak file
                byte[] compressedBuffer = new byte[entry.CompressedSize];
                pakFile.ReadExactly(compressedBuffer);
                // Decompress data
                byte[] buffer = Decompress(compressedBuffer);
                // Return loaded data
                return buffer;
            }
        }

        /// <summary>Loads texture from .pak file from file name.</summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns>Loaded <see cref="Texture2D"/> of the specified file.</returns>
        public Texture2D LoadTextureFromPak(string fileName)
        {
            if (entries[fileName].FileType == "png")
            {
                // Read data of the file
                byte[] data = LoadRawFile(fileName);
                // Load image from data
                Image image = Raylib.LoadImageFromMemory(".png", data);
                // Convert to Texture2D
                Texture2D texture = Raylib.LoadTextureFromImage(image);
                // Unload image
                Raylib.UnloadImage(image);
                return texture;
            }
            // Return blank texture if no png file was found within the archive
            Raylib.TraceLog(TraceLogLevel.Warning, "Failed to load texture from .pak archive");
            return new Texture2D();
        }

        /// <summary>Loads model from .pak file from file name.</summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns><see cref="Model"/> of the specified file.</returns>
        public Model LoadModelFromPak(string fileName)
        {
            if (entries[fileName].FileType == "m3d")
            {
                // Read data of the file
                byte[] data = LoadRawFile(fileName);
                // Write data to temporary m3d file
                File.WriteAllBytes(DEFAULT_TMP_FILE + ".m3d", data);
                // Load model from tmp created file and fix material issue
                Console.WriteLine($"ORION: Preparing to extract {fileName} from PAK archive");
                Model model = Raylib.LoadModel(DEFAULT_TMP_FILE + ".m3d");
                for (int i = 0; i < model.MeshCount; i++)
                {
                    Raylib.GenMeshTangents(ref model.Meshes[i]);
                }
                // Delete tmp file
                File.Delete(DEFAULT_TMP_FILE + ".m3d");
                return model;
            }
            // Return blank model if no m3d file was found within the archive
            Raylib.TraceLog(TraceLogLevel.Warning, "Failed to load model from .pak archive");
            return new Model();
        }


        /// <summary>Decompresses an array of bytes with the GZIP algorithm</summary>
        /// <param name="data">Data to decompress.</param>
        /// <returns>Decompressed data.</returns>
        private static byte[] Decompress(byte[] data)
        {
            MemoryStream memoryStream = new MemoryStream(data);
            GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using (MemoryStream resultStream = new MemoryStream())
            {
                gzipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}