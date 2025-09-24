using System.IO;
using System.Security.Cryptography;
using CGK.Encryption.Abstraction;

namespace CGK.Encryption
{
    public static class RuntimeDecryptor
    {
        private static IEncryptionKeyProvider _keyProvider;

        public static void SetKeyProvider(IEncryptionKeyProvider provider)
        {
            _keyProvider = provider;
        }

        public static byte[] Decrypt(byte[] encryptedData)
        {
            if (_keyProvider == null)
            {
                throw new System.InvalidOperationException("EncryptionKeyProvider is not set.");
            }

            using Aes aes = Aes.Create();
            aes.Key = _keyProvider.GetKey();
            aes.IV = new byte[16];

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}