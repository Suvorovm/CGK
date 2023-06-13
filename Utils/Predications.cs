using System;
using UnityEngine;

namespace CommonGameKit.com.suvorov.common_e419b53c39.Utils
{
    public static class Predications
    {
        public static void CheckNotNull(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null))
            {
                throw new ArgumentException("Argument can't be null");
            }
        }
        
        public static void CheckNotNull(MonoBehaviour monoBeh)
        {
            if (ReferenceEquals(monoBeh, null))
            {
                throw new ArgumentException("Argument can't be null");
            }
        }
    }
}