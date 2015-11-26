namespace LibScreenInfo
{
    public class PixelsPerInch
    {
        public PixelsPerInch(double horizontal, double vertical)
        {
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }

        public double Horizontal { get; internal set; }

        public double Vertical { get; internal set; }
    }
}