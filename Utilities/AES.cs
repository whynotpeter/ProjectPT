using System.IO;
using System.Security.Cryptography;

namespace Utilities
{
    public class AES
    {
        public static ByteArray Encrypt(ByteArray data, ByteArray key, ref ByteArray iv)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var aes = new AesManaged { Key = key.ByteData, IV = iv.ByteData, Padding = PaddingMode.None, Mode = CipherMode.CBC })
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data.ByteData, 0, data.Length);
                    }
                }
                ByteArray result = new ByteArray(memoryStream.ToArray());
                iv = new ByteArray(result.Extract(result.Length - 16, 16).ByteData);
                return result;
            }
        }

        public static ByteArray Decrypt(ByteArray encrypted, ByteArray key, ref ByteArray iv)
        {
            using (var memoryStream = new MemoryStream(encrypted.ByteData))
            {
                using (var aes = new AesManaged { Key = key.ByteData, IV = iv.ByteData, Padding = PaddingMode.None, Mode = CipherMode.CBC })
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] decrypt = new byte[encrypted.Length];
                        cryptoStream.Read(decrypt, 0, decrypt.Length);
                        ByteArray result = new ByteArray(decrypt);
                        iv = new ByteArray(encrypted.Extract(encrypted.Length - 16, 16).ByteData);
                        return result;
                    }
                }
            }
        }
    }
}
