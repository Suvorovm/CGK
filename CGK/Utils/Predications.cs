using System;
using UnityEngine;

namespace CGK.Utils
{
    public static class Predications
    {
        public static GameObject CheckNotNull(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null))
            {
                throw new ArgumentException("Argument can't be null");
            }

            return gameObject;
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