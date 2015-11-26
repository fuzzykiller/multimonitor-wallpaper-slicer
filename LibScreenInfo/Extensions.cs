using System.Collections.Generic;

namespace LibScreenInfo
{
    internal static class Extensions
    {
        /// <summary>
        /// Test whether <paramref name="target"/> starts with <paramref name="startSequence"/>. Uses <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if all elements from <paramref name="startSequence"/> are present at start of <paramref name="target"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool StartsWith<T>(this IEnumerable<T> target, IEnumerable<T> startSequence)
        {
            return StartsWith(target, startSequence, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Test whether <paramref name="target"/> starts with <paramref name="startSequence"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if all elements from <paramref name="startSequence"/> are present at start of <paramref name="target"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool StartsWith<T>(this IEnumerable<T> target, IEnumerable<T> startSequence, IEqualityComparer<T> equalityComparer)
        {
            using (var targetEnumerator = target.GetEnumerator())
            using (var startEnumerator = startSequence.GetEnumerator())
            {
                while (startEnumerator.MoveNext())
                {
                    if (!targetEnumerator.MoveNext())
                    {
                        return false;
                    }

                    if (!equalityComparer.Equals(startEnumerator.Current, targetEnumerator.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
