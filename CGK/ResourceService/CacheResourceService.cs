using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CGK.ResourceService
{
    public class CacheResourceService : IDisposable
    {
        private readonly Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();

        public T Load<T>(string path)
            where T : Object
        {
            if (_resourceCache.ContainsKey(path))
            {
                return (T)_resourceCache[path];
            }

            T resource = Resources.Load<T>(path);
            if (resource == default)
            {
                return default;
            }

            _resourceCache.Add(path, resource);
            return resource;
        }

        public async UniTask<T> LoadAsync<T>(string path)
            where T : Object

        {
            if (_resourceCache.ContainsKey(path))
            {
                return (T)_resourceCache[path];
            }

            Object resource = await Resources.LoadAsync(path).ToUniTask();
            T loadedResource = resource as T;
            if (loadedResource == default)
            {
                return default;
            }
            _resourceCache.Add(path, loadedResource);
            return loadedResource;
        }

        public void Dispose()
        {
            _resourceCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}