using System;
using System.IO;
using UnityEngine;

namespace CGK.Utils
{
    public static class FileHandling
    {
        public static bool CreateDirectoryIfDoesntExistAndWriteAllText(string path, string contents, out Exception exception)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                // Write gamedata to temp file first.
                // If anything goes south, we won't corrupt existing file at least.
                string tmpFileName = Path.Combine(directory, Guid.NewGuid().ToString());
                File.WriteAllText(tmpFileName, contents);
                // Now we're sure that there was enough space on disk and that our file was successfully written.
                // Replace old save with new.
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(tmpFileName, path);
                exception = null;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                exception = e;
                return false;
            }
        }

        public static void DeleteIfExists(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}