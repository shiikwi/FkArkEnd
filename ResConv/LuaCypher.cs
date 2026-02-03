using System;
using System.Collections.Generic;
using System.Text;
using Beyond.Security;
using Xxtea;

namespace ResConv
{
    public class LuaCypher
    {
        private const string KEY = "==";
        private static readonly string[] KEYS =
        {
            "cynb5", "paeky", "xmF5og", "ud35+e", "72iUy", "azWk3", "901lU", "dDfl2"
        };
        private const string INITIAL_ASSETS_ROOT_PATH = "Assets/Beyond/InitialAssets/";


        public static void DecryptLua(string infile)
        {
            var Base64String = File.ReadAllText(infile);

            if (Base64String.Length < 100 && Base64String.StartsWith("return") && !Base64String.Contains("function"))
            {
                Console.WriteLine("Plain text");
                return;
            }

            byte[] cipherBytes = Convert.FromBase64String(Base64String);
            var xxKey = GetKey();
            byte[] plainData = XXTEA.Decrypt(cipherBytes, xxKey);

            var outfile = Path.ChangeExtension(infile, ".dec.lua");
            File.WriteAllBytes(outfile, plainData);
        }

        private static string GetKey()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(KEYS[1]);
            sb.Append(KEYS[5]);
            sb.Append(KEYS[3]);
            sb.Append(KEYS[2]);
            sb.Append(KEY);

            var key = new byte[16];
            IOOO000iiI.IooiIIO(sb.ToString(), key, INITIAL_ASSETS_ROOT_PATH);
            return Encoding.UTF8.GetString(key);
        }

    }
}
