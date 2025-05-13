using Uniray;

namespace Orion_Desktop
{
    /// <summary>Represents an instance of the scene loading class.</summary>
    internal static class UnirayLoader
    {
        internal static byte[] ENCRYPTION_KEY = new byte[0];
        internal static byte[] SYMMETRICAL_VECTOR = new byte[0];

        /// <summary>Loads scene informations and assigns model files from .PAK file.</summary>
        /// <returns>Usable list of <see cref="GameObject3D"/>.</returns>
        internal static List<GameObject3D> LoadScene()
        {
            // Retrieve encryption keys
            FileStream cryptoStream = new FileStream("assets/crypto.env", FileMode.Open);
            using BinaryReader reader = new BinaryReader(cryptoStream);
            ENCRYPTION_KEY = reader.ReadBytes(DatDecoder.AES_KEY_LENGTH);
            SYMMETRICAL_VECTOR = reader.ReadBytes(DatDecoder.AES_IV_LENGTH);

            List<GameObject3D> objects = DatDecoder.DecodeScene("assets/Hub.DAT", ENCRYPTION_KEY, SYMMETRICAL_VECTOR);
            PakReader pakReader = new PakReader("assets/Hub.pak");
            // Assign/load models
            objects.Where(x => x is UModel).ToList().ForEach(x =>
            {
                string id = ((UModel)x).ModelID;
                string name = x.Name;
                if (!Resources.Models.ContainsKey(id)) // Load model if not already loaded
                {
                    Resources.Models.Add(((UModel)x).ModelID, pakReader.LoadModelFromPak(id));
                }
                // Bind resources to object
                ((UModel)x).LoadMeshes();
                ((UModel)x).LoadMaterials(Resources.PBRMaterials[x.Name]);
            });

            return objects;
        }
    }
}