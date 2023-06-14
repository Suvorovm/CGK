using JetBrains.Annotations;
using UnityEngine;

namespace CGK.Repository
{
    /// <summary>
    /// Repository to save any data-model to PlayerPrefs.
    /// Use the JsonUtility to Serialize/DeSerialize data.
    /// T model must be Serializeble
    /// </summary>
    /// <typeparam name="T">T is model. All variables in T-Model must have set-accessory </typeparam>
    public class PlayerPrefsRepository<T> : MonoBehaviour, IRepository<T>
        where T : class
    {
        /// <summary>
        /// Try get data from PlayerPrefs by generic type's name 
        /// </summary>
        /// <returns>Saved model in PlayerPrefs or null</returns>
        [CanBeNull]
        public T Get()
        {
            string jsonData = PlayerPrefs.GetString(typeof(T).FullName);
            if (string.IsNullOrEmpty(jsonData))
            {
                return null;
            }

            T savedModel = JsonUtility.FromJson<T>(jsonData);
            return savedModel;
        }

        /// <summary>
        /// Save T-model to PlayerPrefs. 
        /// </summary>
        /// <param name="model">Model with data you want to save</param>
        public void Save(T model)
        {
            string jsonData = JsonUtility.ToJson(model);
            PlayerPrefs.SetString(typeof(T).FullName, jsonData);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Try remove the T-Model from PLayerPrefs. If the Model does not exist, Remove has no impact.
        /// </summary>
        public void Clear()
        {
            PlayerPrefs.DeleteKey(typeof(T).FullName);
        }
    }
}