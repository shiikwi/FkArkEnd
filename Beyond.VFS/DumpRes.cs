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

        public void DumpChk(string inFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(inFile);
            var blcDir = Path.GetDirectoryName(inFile);
            var blcFile = Path.Combine(blcDir, Path.GetFileName(blcDir) + ".json");
            if (!File.Exists(blcFile))
            {
                Console.WriteLine($"dump blc first");
                return;
            }

            var json = File.ReadAllText(blcFile);
            var mainInfo = JsonConvert.DeserializeObject<VFBlockMainInfo>(json);
            foreach(var chunk in mainInfo.AllChunks)
            {
                if(chunk.Md5Name == fileName)
                {
                    foreach(var fvf in chunk.Files)
                    {
                        VFSFileReadStream.Read(fvf, blcDir);
                    }
                }
            }
        }

    }
}
