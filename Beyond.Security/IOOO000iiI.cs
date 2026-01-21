using System.Text;

namespace Beyond.Security
{
    public class IOOO000iiI
    {
        public static void IooiIIO(string encryptText, byte[] result, string key = null)
        {
            if(string.IsNullOrEmpty(key))
            {
                key = "Assets/Beyond/DynamicAssets/Gameplay/UI/Fonts/";
            }

            var base64Decoded = Convert.FromBase64String(encryptText);
            IIiiIIIoO(base64Decoded, key, result);
        }

        public static void IIiiIIIoO(byte[] data, string key, byte[] result)
        {
            byte[] masterBytes = Encoding.UTF8.GetBytes(key);
            for(int i = 0; i< data.Length;i++)
            {
                int cipherByte = data[i];
                int masterByte = masterBytes[i % key.Length];
                result[i] = (byte)((cipherByte - masterByte + 256) % 256);
            }
        }
    }
}
