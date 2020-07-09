using System.Collections.Generic;
using System;

namespace UnityHelpers
{
    public static class EnumerableHelpers
    {
        /// <summary>
        /// Source: https://stackoverflow.com/questions/5215469/use-linq-to-break-up-listt-into-lots-of-listt-of-n-length
        /// Splits the given enumerable into multiple partitions with the given size
        /// </summary>
        /// <param name="sequence">The original sequence</param>
        /// <param name="size">The partition size</param>
        /// <typeparam name="T">The type of the enumerable</typeparam>
        /// <returns>An enumerable of enumerables with the given size</returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
        {
            List<T> partition = new List<T>(size);
            foreach(var item in sequence)
            {
                partition.Add(item);
                if (partition.Count == size)
                {
                    yield return partition;
                    partition = new List<T>(size);
                }
            }
            if (partition.Count > 0)
                yield return partition;
        }
        /// <summary>
        /// Enumerates through every pair of values within a sequence, does an operation on the pair, and puts the result in the final enumerable.
        /// The resulting enumerable should have a size equal to the original minus one.
        /// </summary>
        /// <param name="source">The source enumerable</param>
        /// <param name="combinator">The function to combine pairs</param>
        /// <typeparam name="TSource">The original type of the enumerable</typeparam>
        /// <typeparam name="TResult">The output type of the enumerable</typeparam>
        /// <returns>An enumerable where each pair was combined in a specified way</returns>
        public static IEnumerable<TResult> SelectEveryPair<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> combinator)
        {
            if (source == null) 
                throw new ArgumentNullException("source");
            
            bool firstIteration = true;
            using (var enumerator = source.GetEnumerator())
            {
                TSource firstItem = default;
                TSource secondItem = default;
                while (enumerator.MoveNext())
                {
                    firstItem = secondItem;
                    secondItem = enumerator.Current;
                    if (!firstIteration)
                        yield return combinator(firstItem, secondItem);
                    firstIteration = false;
                }
            }
        }
        /// <summary>
        /// Extends the value to become an enumerable starting with itself
        /// </summary>
        /// <param name="source">The starting value</param>
        /// <param name="extender">The method of extension</param>
        /// <param name="count">How far to extend the resulting enumerable</param>
        /// <typeparam name="TSource">The type operated on</typeparam>
        /// <returns>An enumerable</returns>
        public static IEnumerable<TSource> Extend<TSource>(this TSource source, Func<TSource, TSource> extender, int count)
        {
            TSource currentValue = source;
            yield return currentValue;
            for (int i = 0; i < count; i++)
            {
                currentValue = extender(currentValue);
                yield return currentValue;
            }
        }
    }
}