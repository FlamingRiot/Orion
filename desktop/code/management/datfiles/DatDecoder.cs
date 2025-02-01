using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Uniray;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the static DAT file decoder.</summary>
    internal class DatDecoder
    {
        // -----------------------------------------------------------
        // Private and constant instances
        // -----------------------------------------------------------

        internal const int AES_KEY_LENGTH = 32;
        internal const int AES_IV_LENGTH = 16;

        private const string MODELS_SECTION = "MODEL";
        private const string SHAPES_SECTION = "SHAPE";

        private static List<DatFileEntry> _entries = new List<DatFileEntry>();

        /// <summary>Reads a .DAT file and decodes the scene informations from it.</summary>
        /// <param name="path">Path to the .DAT file.</param>
        /// <returns>Uniray corresponding Scene.</returns>
        /// <exception cref="Exception">No file found exception.</exception>
        public static List<GameObject3D> DecodeScene(string path, byte[] key, byte[] iv)
        {
            if (!Path.Exists(path)) throw new Exception("No .DAT file was found at the given location");
            // Open file stream
            FileStream datFile = new FileStream(path, FileMode.Open);
            using BinaryReader reader = new BinaryReader(datFile);
            // Read file header data
            int _entryCount = reader.ReadInt32(); // Read object count
            int _tableOffset = reader.ReadInt32(); // Read entry table offset
            string skyboxId = reader.ReadString(); // Read skybox resource id
            // Move to table offset
            datFile.Seek(_tableOffset, SeekOrigin.Begin);
            // Loop over different file entries
            for (int i = 0; i < _entryCount; i++)
            { 
                // Read entry data
                string entryName = reader.ReadString();
                int index = reader.ReadInt32();
                int size = reader.ReadInt32();
                _entries.Add(new DatFileEntry(entryName, index, size));
            }
            List<GameObject3D> objects = new List<GameObject3D>();
            // Read entries data
            foreach (DatFileEntry entry in _entries) 
            { 
                // Move to entry index
                datFile.Seek(entry.Index, SeekOrigin.Begin);
                // Read encrypted data from the file
                byte[] encryptedData = new byte[entry.Size];
                datFile.ReadExactly(encryptedData);
                // Decrypt data
                string text = Decrypt(encryptedData, key, iv);
                switch (entry.Name)
                {
                    case MODELS_SECTION:
                        List<UModel>? models = JsonConvert.DeserializeObject<List<UModel>>(text);
                        if (models is not null && models.Count != 0) objects.AddRange(models);
                        break;
                    case SHAPES_SECTION:
                        List<UShape>? shapes = JsonConvert.DeserializeObject<List<UShape>>(text);
                        if (shapes is not null && shapes.Count != 0)
                        {
                            // Update shapes mesh reference
                            shapes.ForEach(x => x.UpdateMesh());
                            objects.AddRange(shapes);
                        }
                        break;
                }
            }
            // Reset entries list
            _entries.Clear();
            // Return objects
            return objects;
        }

        /// <summary>Decrypts data with the AES algorithm.</summary>
        /// <param name="data">Data to decrypt.</param>
        /// <param name="key">Decryption key.</param>
        /// <param name="iv">Decryption IV.</param>
        /// <returns>Decrypted data under the form of a string.</returns>
        /// <exception cref="ArgumentNullException">Null data exception.</exception>
        private static string Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            // Null exceptions
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            // Decrypted data
            string text;
            // Start Aes algorithm
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            // Create encryptor based on the key and IV
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream myDecrypt = new MemoryStream(data);
            using CryptoStream csDecrypt = new CryptoStream(myDecrypt, decryptor, CryptoStreamMode.Read);
            using BinaryReader stream = new BinaryReader(csDecrypt, Encoding.UTF8);
            // Write data to stream
            text = stream.ReadString();
            return text;
        }
    }
}