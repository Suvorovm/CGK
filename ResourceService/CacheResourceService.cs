using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Core.ResourceService
{
    public class CacheResourceService : IDisposable
    {
        private readonly Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();

        public T Load<T>(string path)
            where T : Object
        {
            if (_resourceCache.ContainsKey(path))
            {
                return (T) _resourceCache[path];
            }

            T resource = Resources.Load<T>(path);
            if(resource == default)
            {
                return default;
            }
            _resourceCache.Add(path, resource);
            return resource;
        }
        

        public void Dispose()
        {
            _resourceCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}