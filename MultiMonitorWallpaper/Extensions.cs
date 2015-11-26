using System.Diagnostics.Contracts;
using System.Drawing;

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
    }
}