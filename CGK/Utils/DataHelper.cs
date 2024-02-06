using System.IO;
using UnityEngine;

namespace CGK.Utils
{
    public static class DataHelper
    {
        public static void PlayerPrefsDeleteAll() => PlayerPrefs.DeleteAll();
        
        public static void DeleteGameDataInPersistentDataPath()
        {
            var pathData = Application.persistentDataPath;
            var files = Directory.GetFiles(pathData);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                fileInfo.Delete();
            }
        }
        
        public static void DeleteAll()
        {
            PlayerPrefsDeleteAll();
            DeleteGameDataInPersistentDataPath();
        }
    }
}