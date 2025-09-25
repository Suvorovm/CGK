using System.IO;
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
        private const string GENERATED_KEY_PATH = "Assets/Scripts/Encryption/generated";

        private EncryptionConfig _config;
        private BuildProcess _buildProcess;
        private string _keyFilePath;
        private string _backupKeyContent;
        private bool _skipEncryption;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[Encryption] Preprocess build started");

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
            using (StreamReader reader = new StreamReader(gameConfigPath))
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

            _config = new DescriptorFileLoader().LoadDescriptorFromString<EncryptionConfig>(File.ReadAllText(configPath));
            _buildProcess = new BuildProcess(_config);

            _keyFilePath = Path.Combine(GENERATED_KEY_PATH, "EncryptionKeyHolder.cs");
            Directory.CreateDirectory(GENERATED_KEY_PATH);

            if (File.Exists(_keyFilePath))
            {
                _backupKeyContent = File.ReadAllText(_keyFilePath);
            }
            else
            {
                _backupKeyContent = GenerateStubContent();
                File.WriteAllText(_keyFilePath, _backupKeyContent);
                Debug.Log("[Encryption] Created stub EncryptionKeyHolder.cs");
            }

            _buildProcess.Run();
            encryptionInto.IsEncrypted = true;
            AssetDatabase.ImportAsset(RelativePath(_keyFilePath));
            AssetDatabase.Refresh();
            SaveEncryptFlag(encryptionInto, Path.Combine(PATH_TO_GAME_CONFIG, "encrypt.json"));

            Debug.Log("[Encryption] Preprocess build finished");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (_skipEncryption)
            {
                Debug.Log("[Encryption] Postprocess: Encryption disabled.");
                return;
            }

            Debug.Log("[Encryption] Postprocess build started");

            foreach (string encFile in Directory.GetFiles(_config.FolderPath, "*.enc"))
            {
                File.Delete(encFile);
            }

            File.WriteAllText(_keyFilePath, GenerateStubContent());

            AssetDatabase.ImportAsset(RelativePath(_keyFilePath));
            AssetDatabase.Refresh();

            Debug.Log("[Encryption] Postprocess build finished");
        }

        private void SaveEncryptFlag(EncryptionInto encryptionInto, string path)
        {
            string json = JsonUtility.ToJson(encryptionInto, true);
            File.WriteAllText(path, json);
            string relativePath = RelativePath(path);
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Build data saved to JSON.");
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
namespace RuntimeSecurity
{
    internal static class EncryptionKeyHolder
    {
        public static readonly byte[] Key = new byte[0];
    }
}";
        }
    }
}