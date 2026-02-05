using AK.Wwise;
using SparkBuffer;
using Lua;
using ManifestBinary;
using Beyond.Resource;

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
            else if (ext == ".bin")
            {
                // StringPathHash.bin, InitStringPathHash.bin
                StringPathHashBinary.InitMain(infile);
            }
        }
    }

}

