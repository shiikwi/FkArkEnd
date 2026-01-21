using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Beyond.VFS
{
    public class DumpRes
    {
        public void DumpBlc(string inFile, string outFile)
        {
            var indata = File.ReadAllBytes(inFile);
            var info = VFSUtils.DecryptCreateBlockGroupInfo(indata);
            var json = JsonConvert.SerializeObject(info, Formatting.Indented);

            File.WriteAllText(outFile, json);
            Console.WriteLine($"Dump {Path.GetFileName(inFile)} Success");
        }
    }
}
