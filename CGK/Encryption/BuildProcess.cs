using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CGK.Encryption.Descriptor;

namespace CGK.Encryption
{
    public class BuildProcess
    {
        private readonly EncryptionConfig _config;

        public BuildProcess(EncryptionConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Encrypts configs and generates EncryptionKeyHolder.cs (with an already hashed key).
        /// </summary>
        public void Run()
        {
            EncryptConfigs();
            GenerateKeyClass();
        }

        private void EncryptConfigs()
        {
            string[] files = Directory.GetFiles(_config.FolderPath, "*.xml");
            foreach (string file in files)
            {
                byte[] plainBytes = File.ReadAllBytes(file);

                byte[] keyHash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(_config.EncryptionKey));
                }

                byte[] encrypted = Encrypt(plainBytes, keyHash);
                string outPath = file + ".enc";
                File.WriteAllBytes(outPath, encrypted);
            }
        }

        private void GenerateKeyClass()
        {
            byte[] keyHash;
            using (SHA256 sha256 = SHA256.Create())
            {
                keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(_config.EncryptionKey));
            }

            string byteArrayString = string.Join(", ", keyHash);

            string code = $@"using EncryptionTool;

namespace RuntimeSecurity
{{
    /// <summary>
    /// The generated key provider (sewn in at the build stage).
    /// Stores the hash (SHA-256) Depending on the original key string, this is the AES encryption key itself.
    /// </summary>
    internal sealed class EncryptionKeyHolder : IEncryptionKeyProvider
    {{
        public static readonly byte[] Key = new byte[] {{ {byteArrayString} }};

        public byte[] GetKey() => Key;
    }}
}}";
            string projectRoot = Directory.GetCurrentDirectory(); 
            string targetFolder = Path.Combine(projectRoot, "Assets", "Scripts");
            Directory.CreateDirectory(targetFolder); 
            
            string path = Path.Combine(targetFolder, "EncryptionKeyHolder.cs");
            File.WriteAllText(path, code, Encoding.UTF8);
        }

        private static byte[] Encrypt(byte[] data, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = new byte[16];

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();

        }
    }
}