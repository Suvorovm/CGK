using System;
using System.Collections.Generic;
using System.Linq;

namespace CGK.Utils
{
    public static class LINQExtentions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            var r = new Random();  
            var list = enumerable as IList<T> ?? enumerable.ToList(); 
            return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
        }
        
        public static int RandomIndex<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            var r = new Random();  
            var list = enumerable as IList<T> ?? enumerable.ToList(); 
            return list.Count == 0 ? 0 : r.Next(0, list.Count);
        }
    }
}