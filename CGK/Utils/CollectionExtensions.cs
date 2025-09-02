using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace CGK.Utils
{
    [PublicAPI]
    public static class CollectionExtensions
    {
        private static readonly IRandomProvider RandomProvider = new UnityRandomProvider();

        private interface IRandomProvider
        {
            int GetRandomIndex(int minInclusive, int maxExclusive);
        }

        private class UnityRandomProvider : IRandomProvider
        {
            public int GetRandomIndex(int minInclusive, int maxExclusive) => Random.Range(minInclusive, maxExclusive);
        }

        #region Insert and Remove

        public static T[] InsertAt<T>(this T[] sourceArray, int index)
        {
            if (sourceArray == null)
            {
                Debug.LogError("Source array is null.");
                return sourceArray;
            }

            if (index < 0 || index > sourceArray.Length)
            {
                Debug.LogError($"Index {index} is out of bounds for array of length {sourceArray.Length}.");
                return sourceArray;
            }

            T[] newArray = new T[sourceArray.Length + 1];
            Array.Copy(sourceArray, 0, newArray, 0, index);
            Array.Copy(sourceArray, index, newArray, index + 1, sourceArray.Length - index);
            return newArray;
        }

        public static T[] RemoveAt<T>(this T[] sourceArray, int index)
        {
            if (sourceArray == null)
            {
                Debug.LogError("Source array is null.");
                return sourceArray;
            }

            if (index < 0 || index >= sourceArray.Length)
            {
                Debug.LogError($"Index {index} is out of bounds for array of length {sourceArray.Length}.");
                return sourceArray;
            }

            T[] newArray = new T[sourceArray.Length - 1];
            Array.Copy(sourceArray, 0, newArray, 0, index);
            Array.Copy(sourceArray, index + 1, newArray, index, sourceArray.Length - index - 1);
            return newArray;
        }

        #endregion

        #region Random Selection

        public static T GetRandom<T>(this T[] collection)
        {
            if (collection.IsNullOrEmpty())
            {
                Debug.LogError("Collection is null or empty.");
                return default;
            }
            return collection[RandomProvider.GetRandomIndex(0, collection.Length)];
        }

        public static T GetRandom<T>(this IList<T> collection)
        {
            if (collection.IsNullOrEmpty())
            {
                Debug.LogError("Collection is null or empty.");
                return default;
            }
            return collection[RandomProvider.GetRandomIndex(0, collection.Count)];
        }

        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            if (collection.IsNullOrEmpty())
            {
                Debug.LogError("Collection is null or empty.");
                return default;
            }
            int count = collection.Count();
            return collection.ElementAt(RandomProvider.GetRandomIndex(0, count));
        }

        #endregion

        #region Null or Empty Checks

        public static bool IsNullOrEmpty<T>(this T[] collection) => collection == null || collection.Length == 0;

        public static bool IsNullOrEmpty<T>(this IList<T> collection) => collection == null || collection.Count == 0;

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();

        public static bool NotNullOrEmpty<T>(this T[] collection) => !collection.IsNullOrEmpty();

        public static bool NotNullOrEmpty<T>(this IList<T> collection) => !collection.IsNullOrEmpty();

        public static bool NotNullOrEmpty<T>(this IEnumerable<T> collection) => !collection.IsNullOrEmpty();

        #endregion

        public static int NextIndexInCircle<T>(this T[] array, int desiredPosition)
        {
            if (array.IsNullOrEmpty())
            {
                Debug.LogError("Array is null or empty.");
                return -1;
            }

            int length = array.Length;
            return length == 1 ? 0 : (desiredPosition % length + length) % length;
        }

        public static int IndexOfItem<T>(this IEnumerable<T> collection, T item)
        {
            if (collection == null)
            {
                Debug.LogError("Collection is null.");
                return -1;
            }

            int index = 0;
            foreach (T element in collection)
            {
                if (Equals(element, item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static bool ContentsMatch<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first.IsNullOrEmpty() && second.IsNullOrEmpty())
            {
                return true;
            }
            if (first.IsNullOrEmpty() || second.IsNullOrEmpty())
            {
                return false;
            }

            int firstCount = first.Count();
            if (firstCount != second.Count())
            {
                return false;
            }

            return first.All(item => second.Contains(item));
        }

        public static bool ContentsMatchKeys<T1, T2>(this IDictionary<T1, T2> source, IEnumerable<T1> check)
        {
            return source.IsNullOrEmpty() && check.IsNullOrEmpty() || source.NotNullOrEmpty() && check.NotNullOrEmpty() && source.Keys.ContentsMatch(check);
        }

        public static bool ContentsMatchValues<T1, T2>(this IDictionary<T1, T2> source, IEnumerable<T2> check)
        {
            return source.IsNullOrEmpty() && check.IsNullOrEmpty() || source.NotNullOrEmpty() && check.NotNullOrEmpty() && source.Values.ContentsMatch(check);
        }

        public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) where TValue : new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.ContainsKey(key))
            {
                source[key] = new TValue();
            }
            return source[key];
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.ContainsKey(key))
            {
                source[key] = value;
            }
            return source[key];
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> valueFactory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.ContainsKey(key))
            {
                source[key] = valueFactory();
            }
            return source[key];
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.ContainsKey(key))
            {
                source[key] = valueFactory(key);
            }
            return source[key];
        }

        public static TValue GetOrAdd<TKey, TValue, TArg>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.ContainsKey(key))
            {
                source[key] = valueFactory(key, factoryArgument);
            }
            return source[key];
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foreach (T element in source)
            {
                action(element);
            }
            return source;
        }

        public static IEnumerable<T> ForEach<T, R>(this IEnumerable<T> source, Func<T, R> func)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foreach (T element in source)
            {
                func(element);
            }
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            int index = 0;
            foreach (T element in source)
            {
                action(element, index++);
            }
            return source;
        }

        public static IEnumerable<T> ForEach<T, R>(this IEnumerable<T> source, Func<T, int, R> func)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            int index = 0;
            foreach (T element in source)
            {
                func(element, index++);
            }
            return source;
        }

        public static T MaxBy<T, S>(this IEnumerable<T> source, Func<T, S> selector) where S : IComparable<S>
        {
            if (source.IsNullOrEmpty())
            {
                Debug.LogError("Source collection is null or empty.");
                return default;
            }
            T maxElement = source.First();
            S maxValue = selector(maxElement);
            foreach (T element in source.Skip(1))
            {
                S value = selector(element);
                if (value.CompareTo(maxValue) > 0)
                {
                    maxElement = element;
                    maxValue = value;
                }
            }
            return maxElement;
        }

        public static T MinBy<T, S>(this IEnumerable<T> source, Func<T, S> selector) where S : IComparable<S>
        {
            if (source.IsNullOrEmpty())
            {
                Debug.LogError("Source collection is null or empty.");
                return default;
            }
            T minElement = source.First();
            S minValue = selector(minElement);
            foreach (T element in source.Skip(1))
            {
                S value = selector(element);
                if (value.CompareTo(minValue) < 0)
                {
                    minElement = element;
                    minValue = value;
                }
            }
            return minElement;
        }

        public static IEnumerable<T> SingleToEnumerable<T>(this T source) => new[] { source };

        public static int FirstIndex<T>(this IList<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                Debug.LogError("Source collection is null.");
                return -1;
            }
            for (int index = 0; index < source.Count; index++)
            {
                if (predicate(source[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int FirstIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                Debug.LogError("Source collection is null.");
                return -1;
            }
            int index = 0;
            foreach (T element in source)
            {
                if (predicate(element))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static int LastIndex<T>(this IList<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                Debug.LogError("Source collection is null.");
                return -1;
            }
            for (int index = source.Count - 1; index >= 0; index--)
            {
                if (predicate(source[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        public static IList<T> FillBy<T>(this IList<T> source, Func<int, T> valueFactory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            for (int index = 0; index < source.Count; index++)
            {
                source[index] = valueFactory(index);
            }
            return source;
        }

        public static T[] FillBy<T>(this T[] source, Func<int, T> valueFactory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            for (int index = 0; index < source.Length; index++)
            {
                source[index] = valueFactory(index);
            }
            return source;
        }

        public static IList<T> SwapInPlace<T>(this IList<T> source, int index1, int index2)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (index1 < 0 || index1 >= source.Count || index2 < 0 || index2 >= source.Count)
            {
                Debug.LogError($"Invalid indices: {index1}, {index2} for collection of size {source.Count}.");
                return source;
            }
            (source[index1], source[index2]) = (source[index2], source[index1]);
            return source;
        }

        public static IList<T> Shuffle<T>(this IList<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            for (int index = 0; index < source.Count - 1; index++)
            {
                int randomIndex = RandomProvider.GetRandomIndex(index, source.Count);
                source.SwapInPlace(index, randomIndex);
            }
            return source;
        }
    }
}