using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using CGK.Encryption.Descriptor;

namespace CGK.Encryption.Editor
{
    public class BuildProcess
    {
        private const string BYTES_EXTENSION = ".bytes";

        private readonly EncryptionConfig _config;

        public BuildProcess(EncryptionConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public List<string> Run(string backupPath)
        {
            List<string> createdEncryptedFiles = new List<string>();
            EncryptConfigs(backupPath, createdEncryptedFiles);
            GenerateKeyClass();
            return createdEncryptedFiles;
        }

        private void EncryptConfigs(string backupPath, List<string> createdEncryptedFiles)
        {
            string[] files = Directory.GetFiles(backupPath, "*.xml");
            if (files.Length == 0)
            {
                Debug.LogWarning($"[Encryption] No XML files found in backup {backupPath}");
                return;
            }

            Debug.Log($"[Encryption] Found {files.Length} XML files in {backupPath} for encryption");

            foreach (string file in files)
            {
                try
                {
                    byte[] plainBytes = File.ReadAllBytes(file);

                    byte[] keyHash;
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(_config.EncryptionKey));
                    }

                    byte[] encrypted = Encrypt(plainBytes, keyHash);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    string outPath = Path.Combine(_config.FolderPath, fileNameWithoutExtension + BYTES_EXTENSION);
                    File.WriteAllBytes(outPath, encrypted);
                    createdEncryptedFiles.Add(outPath);
                    Debug.Log($"[Encryption] Encrypted {Path.GetFileName(file)} from backup to {Path.GetFileName(outPath)}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Encryption] Failed to encrypt {file}: {ex.Message}");
                    throw;
                }
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

            string code = $@"using CGK.Encryption.Abstraction;

namespace RuntimeSecurity
{{
    internal partial class EncryptionKeyHolder : IEncryptionKeyProvider
        {{
            public static readonly byte[] Key = new byte[] {{ {byteArrayString} }};

            public byte[] GetKey() => Key;
        }}
}}";

            Directory.CreateDirectory(_config.GeneratedKeyPath);
            string path = Path.Combine(_config.GeneratedKeyPath, "EncryptionKeyHolder.cs");
            File.WriteAllText(path, code, Encoding.UTF8);
            Debug.Log($"[Encryption] Generated {path}");
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