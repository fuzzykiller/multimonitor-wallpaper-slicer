using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;

namespace MultiMonitorWallpaper
{
    public static class Extensions
    {
        [Pure]
        public static RectangleF Transform(this Rectangle rectangle, float factor)
        {
            return new RectangleF(rectangle.Left * factor, rectangle.Top * factor, rectangle.Width * factor,
                rectangle.Height * factor);
        }

        public static void DrawRectangle(this Graphics g, Pen pen, RectangleF rectangle)
        {
            g.DrawRectangle(pen, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        public static bool StartsWith<T>(this IEnumerable<T> target, IEnumerable<T> startSequence)
        {
            return StartsWith(target, startSequence, EqualityComparer<T>.Default);
        }

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