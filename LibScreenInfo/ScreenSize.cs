namespace LibScreenInfo
{
    public class ScreenSize
    {
        public ScreenSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public int Width { get; internal set; }

        public int Height { get; private set; }
    }
}
