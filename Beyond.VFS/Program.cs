using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.VFS
{
    class Program
    {
        static void Main(string[] args)
        {
            var infile = args[0];
            var outfile = Path.ChangeExtension(infile, ".json");
            var dump = new DumpRes();
            dump.DumpBlc(infile, outfile);
        }
    }
}

