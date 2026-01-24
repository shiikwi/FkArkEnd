using System;
using System.Collections.Generic;
using System.Text;

namespace ResConv
{
    class Program
    {
        static void Main(string[] args)
        {
            var infile = args[0];
            AkSoundEngine.LoadPCKPack(infile);
        }
    }

}

