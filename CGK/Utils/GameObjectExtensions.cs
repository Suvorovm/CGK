using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace CGK.Utils
{
    public static class GameObjectExtensions
    {
        [CanBeNull]
        public static GameObject GetChildByName(this GameObject gameObject, string name)
        {
            List<GameObject> allChild = gameObject.GetAllChildren();
            return allChild.FirstOrDefault(g => g.name.Equals(name));
        }
        
        /// <summary>
        /// Returns all children of GO. May be expensive if GameObject has a lot of children. Invoke iteration by Transform
        /// </summary>
        /// <param name="gameObject"> Source object</param>
        /// <returns></returns>
        public static List<GameObject> GetAllChildren(this GameObject gameObject)
        {
            List<GameObject> allChild = new List<GameObject>();
            foreach (Transform childTransform in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                if (gameObject != childTransform.gameObject)
                {
                    allChild.Add(childTransform.gameObject);
                }
            }
            return allChild;
        }

        
        public static void RemoveAllChild(this GameObject gameObject)
        {
            List<GameObject> allChild = gameObject.GetAllChildren();
            for (var i = 0; i < allChild.Count; i++)
            {
                Object.Destroy(allChild[i]);
            }
        }

        public static T GetOrCreateComponent<T>(this GameObject gameObject)
            where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                return gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}