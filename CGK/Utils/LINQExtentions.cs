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
        
        /// <summary>
        /// Performs an action on each element of a collection.
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source) action(element);
            return source;
        }

        /// <summary>
        /// Performs a function on each element of a collection.
        /// </summary>
        public static IEnumerable<T> ForEach<T, R>(this IEnumerable<T> source, Func<T, R> func)
        {
            foreach (T element in source) func(element);
            return source;
        }

        /// <summary>
        /// Performs an action on each element of a collection with its index
        /// passed along.
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (T element in source) { action(element, index); ++index; }
            return source;
        }

        /// <summary>
        /// Performs an action on each element of a collection with its index
        /// passed along.
        /// </summary>
        public static IEnumerable<T> ForEach<T, R>(this IEnumerable<T> source, Func<T, int, R> func)
        {
            int index = 0;
            foreach (T element in source) { func(element, index); ++index; }
            return source;
        }

    }
}