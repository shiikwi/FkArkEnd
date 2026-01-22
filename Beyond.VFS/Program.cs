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
            var ext = Path.GetExtension(infile);
            var dump = new DumpRes();
            if (ext == ".blc")
            {
                dump.DumpBlc(infile, outfile);

            }
            else if (ext == ".chk")
            {
                dump.DumpChk(infile);
            }

        }
    }
}

