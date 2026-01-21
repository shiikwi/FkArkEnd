using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Beyond.Security;

namespace Beyond.VFS
{
    public class VirtualFileSystem
    {
        private string v13Str = "Ks6k3zhrV5g";
        private string v14Str = "uOVtMpqHxFv";
        private string v15Str = "gi4BZzU9xUY";
        private string v16Str = "CnBfZVqAgL";
        private string v17Str = "SjpNhdKK89V";
        private string v18Str = "qzl3BC/08Da";
        private string v19Str = "oXafvEwR54";
        private string v20Str = "4ZzYokf5I7Z";
        private static string[] CHACHA_KEYS;
        private static string Padding = "=";

        public VirtualFileSystem()
        {
            CHACHA_KEYS = new string[]
            {
                v13Str, v14Str, v15Str, v16Str, v17Str, v18Str, v19Str, v20Str
            };

        }

        public byte[] GetCommonChachaKeyBs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CHACHA_KEYS[0]);
            sb.Append(CHACHA_KEYS[3]);
            sb.Append(CHACHA_KEYS[5]);
            sb.Append(CHACHA_KEYS[2]);
            sb.Append(Padding);

            string masterKey = "Assets/Beyond/DynamicAssets/";
            var key = new byte[32];
            IOOO000iiI.IooiIIO(sb.ToString(), key, masterKey);
            return key;
        }

    }
}
