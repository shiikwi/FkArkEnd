using System;
using System.Collections.Generic;
using System.Text;
using CSChaCha20;

namespace Beyond.VFS
{
    public class VFSFileReadStream
    {
        public static void Read(FVFBlockFileInfo fvf, string outDir)
        {
            var vfs = new VirtualFileSystem();
            var key = vfs.GetCommonChachaKeyBs();

            using (var fs = File.OpenRead(fvf.FileChunkMD5Name + ".chk"))
            {
                fs.Seek(fvf.Offset, SeekOrigin.Begin);
                var buffer = new byte[fvf.Len];
                fs.Read(buffer, 0, (int)fvf.Len);

                if(fvf.BUseEncrypt)
                {
                    byte[] iv = new byte[12];
                    Array.Copy(BitConverter.GetBytes(VFSDefine.VFS_PROTO_VERSION), 0, iv, 0, 4);
                    Array.Copy(BitConverter.GetBytes(fvf.IvSeed), 0, iv, 4, 8);

                    using(var chacha = new ChaCha20(key, iv, 1))
                    {
                        var decData = new byte[buffer.Length];
                        chacha.DecryptBytes(decData, buffer);
                        buffer = decData;
                    }
                }

                string outPath = Path.Combine(Path.GetDirectoryName(outDir)!, fvf.FileName);
                string dir = Path.GetDirectoryName(outPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllBytes(outPath, buffer);
            }
            Console.WriteLine($"Extracted: {fvf.FileName}");
        }

    }

}
