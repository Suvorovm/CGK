using UnityEditor;
using UnityEngine;

namespace CGK.Encryption.Editor
{
    public class EncryptionBuildIncluder : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".enc") && assetPath.Contains("Config"))
                {
                    var importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.assetBundleName = null;
                        importer.userData = "EncryptedConfig";
                        importer.SaveAndReimport();
                        Debug.Log($"[Encryption] Configured {assetPath} as TextAsset for build");
                    }
                }
            }
        }
    }
}