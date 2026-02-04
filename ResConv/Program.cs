using AK.Wwise;
using SparkBuffer;
using Lua;
using ManifestBinary;

namespace ResConv
{
    class Program
    {
        static void Main(string[] args)
        {
            var infile = args[0];
            var ext = Path.GetExtension(infile);
            if (ext == ".pck")
            {
                AkSoundEngine.LoadPCKPack(infile);
            }
            else if (ext == ".bytes")
            {
                SparkBufferManagement.Deserialize(infile);
            }
            else if (ext == ".lua")
            {
                LuaCypher.DecryptLua(infile);
            }
            else if (ext == ".hgmmap")
            {
                Manifest.ManifestRead(infile);
            }
        }
    }

}

