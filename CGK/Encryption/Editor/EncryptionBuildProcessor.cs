using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using CGK.Descriptor.Service;
using CGK.Settings;
using CGK.Encryption.Descriptor;

namespace CGK.Encryption.Editor
{
    public class EncryptionBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string PATH_TO_ROOT_CONFIG = "Assets/Config";
        private const string PATH_TO_GAME_CONFIG = "Assets/Resources/Config";
        private const string BYTES_EXTENSION = ".bytes";

        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private EncryptionConfig _config;
        private BuildProcess _buildProcess;
        private string _keyFilePath;
        private string _backupKeyContent;
        private bool _skipEncryption;
        private List<string> _createdEncryptedFiles;

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[Encryption] Preprocess build started");

            _createdEncryptedFiles = new List<string>();

            string gameConfigPath = Path.Combine(PATH_TO_GAME_CONFIG, "GameConfig.xml");
            if (!File.Exists(gameConfigPath))
            {
                Debug.Log("[Encryption] GameConfig.xml not found. No encryption.");
                _skipEncryption = true;
                return;
            }

            EncryptionInto encryptionInto = new EncryptionInto { IsEncrypted = false };
            DescriptorFileLoader loader = new DescriptorFileLoader();
            GameConfig gameConfig;
            
            using (StreamReader reader = new StreamReader(gameConfigPath, Utf8NoBom))
            {
                string xml = reader.ReadToEnd();
                gameConfig = loader.LoadDescriptorFromString<GameConfig>(xml);
            }

            if (gameConfig?.BuildSettings == null || !gameConfig.BuildSettings.Encrypt)
            {
                Debug.Log("[Encryption] Encryption disabled in GameConfig.xml.");
                _skipEncryption = true;
                SaveEncryptFlag(encryptionInto, Path.Combine(PATH_TO_GAME_CONFIG, "encrypt.json"));
                return;
            }

            string configPath = Path.Combine(PATH_TO_ROOT_CONFIG, "EncryptionConfig.xml");
            if (!File.Exists(configPath))
            {
                Debug.LogError("[Encryption] EncryptionConfig.xml not found!");
                _skipEncryption = true;
                SaveEncryptFlag(encryptionInto, Path.Combine(PATH_TO_GAME_CONFIG, "encrypt.json"));
                return;
            }

            _config = new DescriptorFileLoader().LoadDescriptorFromString<EncryptionConfig>(
                File.ReadAllText(configPath, Utf8NoBom));

            _buildProcess = new BuildProcess(_config);

            _keyFilePath = Path.Combine(_config.GeneratedKeyPath, "EncryptionKeyHolder.cs");
            Directory.CreateDirectory(_config.GeneratedKeyPath);

            if (File.Exists(_keyFilePath))
            {
                _backupKeyContent = File.ReadAllText(_keyFilePath, Utf8NoBom);
            }
            else
            {
                _backupKeyContent = GenerateStubContent();
                File.WriteAllText(_keyFilePath, _backupKeyContent, Utf8NoBom);
                Debug.Log("[Encryption] Created stub EncryptionKeyHolder.cs");
            }

            // Create backup and delete original XML files
            string backupPath = Path.Combine(Directory.GetCurrentDirectory(), _config.TempBackupPath);
            Directory.CreateDirectory(backupPath);
            foreach (string file in Directory.GetFiles(_config.FolderPath, "*.xml"))
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string backupFilePath = Path.Combine(backupPath, fileName);
                    File.Copy(file, backupFilePath, true);
                    File.Delete(file);
                    string relativePath = RelativePath(file);
                    AssetDatabase.DeleteAsset(relativePath);
                    Debug.Log($"[Encryption] Backed up {fileName} to {backupFilePath} and deleted original");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Encryption] Failed to backup or delete {file}: {ex.Message}");
                    throw;
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Encrypt files and track created .bytes files
            _createdEncryptedFiles = _buildProcess.Run(backupPath);
            encryptionInto.IsEncrypted = true;

            if (_createdEncryptedFiles.Count == 0)
            {
                Debug.LogError($"[Encryption] No encrypted files created in {_config.FolderPath}. Build may fail!");
            }

            // Import and configure encrypted files
            foreach (string encFile in _createdEncryptedFiles)
            {
                if (!File.Exists(encFile))
                {
                    Debug.LogError($"[Encryption] Encrypted file {encFile} does not exist!");
                    continue;
                }

                string relativePath = RelativePath(encFile);
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(relativePath);
                if (importer != null)
                {
                    importer.assetBundleName = null;
                    importer.userData = "EncryptedConfig";
                    Debug.Log($"[Encryption] Configured {relativePath} as TextAsset");
                }
                else
                {
                    Debug.LogError($"[Encryption] Failed to get AssetImporter for {relativePath}");
                }
            }

            AssetDatabase.ImportAsset(RelativePath(_keyFilePath), ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            SaveEncryptFlag(encryptionInto, Path.Combine(PATH_TO_GAME_CONFIG, "encrypt.json"));

            Debug.Log($"[Encryption] Preprocess build finished. Created {_createdEncryptedFiles.Count} encrypted files.");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (_skipEncryption)
            {
                Debug.Log("[Encryption] Postprocess: Encryption disabled.");
                return;
            }

            Debug.Log("[Encryption] Postprocess build started");

            // Delete encrypted files
            foreach (string encFile in _createdEncryptedFiles)
            {
                if (File.Exists(encFile))
                {
                    string relativePath = RelativePath(encFile);
                    File.Delete(encFile);
                    AssetDatabase.DeleteAsset(relativePath);
                    Debug.Log($"[Encryption] Deleted encrypted file: {relativePath}");
                }
            }

            // Restore original XML files from backup
            string backupPath = Path.Combine(Directory.GetCurrentDirectory(), _config.TempBackupPath);
            if (Directory.Exists(backupPath))
            {
                foreach (string backupFile in Directory.GetFiles(backupPath, "*.xml"))
                {
                    try
                    {
                        string fileName = Path.GetFileName(backupFile);
                        string originalPath = Path.Combine(_config.FolderPath, fileName);
                        File.Copy(backupFile, originalPath, true);
                        string relativePath = RelativePath(originalPath);
                        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"[Encryption] Restored {fileName} to {relativePath}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Encryption] Failed to restore {backupFile}: {ex.Message}");
                        throw;
                    }
                }
                Directory.Delete(backupPath, true);
                Debug.Log($"[Encryption] Deleted backup folder {backupPath}");
            }
            else
            {
                Debug.LogError($"[Encryption] Backup folder {backupPath} not found. XML files not restored!");
            }

            File.WriteAllText(_keyFilePath, GenerateStubContent(), Utf8NoBom);
            AssetDatabase.ImportAsset(RelativePath(_keyFilePath), ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log("[Encryption] Postprocess build finished");
        }

        private void SaveEncryptFlag(EncryptionInto encryptionInto, string path)
        {
            string json = JsonUtility.ToJson(encryptionInto, true);
            File.WriteAllText(path, json, Utf8NoBom);
            string relativePath = RelativePath(path);
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Encryption] Encryption flag saved to: {relativePath}");
        }

        private static string RelativePath(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");
            string projectPath = Application.dataPath.Replace("Assets", "");
            return fullPath.Replace(projectPath, "");
        }

        private static string GenerateStubContent()
        {
            return @"
using CGK.Encryption.Abstraction;

namespace RuntimeSecurity
{
    internal partial class EncryptionKeyHolder : IEncryptionKeyProvider
        {
            public static readonly byte[] Key = new byte[0];

            public byte[] GetKey() => Key;
        }
}";
        }
    }
}
