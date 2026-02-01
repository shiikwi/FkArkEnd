using System;
using System.Collections.Generic;
using System.Text;

namespace ResConv
{
    class Program
    {
        static void Main(string[] args)
        {
            var infile = "ActivityTable.bytes";
            var ext = Path.GetExtension(infile);
            if (ext == ".pck")
            {
                AkSoundEngine.LoadPCKPack(infile);
            }
            else if (ext == ".bytes")
            {
                SparkBuffer.Deserialize(infile);
            }
        }
    }

}

