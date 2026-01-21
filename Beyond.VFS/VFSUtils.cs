using System;
using System.Collections.Generic;
using System.Text;
using CSChaCha20;


namespace Beyond.VFS
{
    public static class VFSUtils
    {
        public static VFBlockMainInfo DecryptCreateBlockGroupInfo(byte[] data)
        {
            var vfs = new VirtualFileSystem();
            var key = vfs.GetCommonChachaKeyBs();

            var iv = new byte[12];
            Array.Copy(data, 0, iv, 0, iv.Length);
            var cipherBytes = new byte[data.Length - VFSDefine.BLOCK_HEAD_LEN];
            Array.Copy(data, 12, cipherBytes, 0, cipherBytes.Length);

            byte[] plainBytes = new byte[cipherBytes.Length];
            using(var chacha = new ChaCha20(key, iv, 1))
            {
                chacha.DecryptBytes(plainBytes, cipherBytes);
            }

            var TotalResData = new byte[data.Length];
            Array.Copy(data, 0, TotalResData, 0, VFSDefine.BLOCK_HEAD_LEN);
            Array.Copy(plainBytes, 0, TotalResData, VFSDefine.BLOCK_HEAD_LEN, plainBytes.Length);
            return VFBlockMainInfo.ReadFromByteBuf(TotalResData);
        }
    }
}
