using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiMonitorWallpaper
{
    public partial class Form1 : Form
    {
        private readonly ManualResetEvent redrawMutex = new ManualResetEvent(true);

        public Form1()
        {
            this.InitializeComponent();

            this.pictureBox1.SizeChanged += this.VisualizeScreenConfiguration;
            this.Load += this.VisualizeScreenConfiguration;
        }

        private async void VisualizeScreenConfiguration(object sender, EventArgs eventArgs)
        {
            await Task.Factory.StartNew(() => redrawMutex.WaitOne());

            var screens = Screen.AllScreens;
            var leftX = screens.Min(x => x.Bounds.Left);
            var rightX = screens.Max(x => x.Bounds.Right);
            var topY = screens.Min(x => x.Bounds.Top);
            var bottomY = screens.Max(x => x.Bounds.Bottom);

            var scaleFactor = Math.Min(this.pictureBox1.Width * 1f / (rightX - leftX),
                                       this.pictureBox1.Height * 1f / (bottomY - topY));

            var thickRedPen = new Pen(Color.Red, 2);
            var textPen = Brushes.Red;
            var textFont = new Font(FontFamily.GenericSansSerif, 12);
            var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

            var bitmap = new Bitmap((int)((rightX - leftX) * scaleFactor), (int)((bottomY - topY) * scaleFactor));
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var screen in screens)
                {
                    var rect = screen.Bounds;
                    rect.Offset(leftX, topY);
                    var visualRect = rect.Transform(scaleFactor);

                    g.DrawRectangle(thickRedPen, visualRect);

                    var screenLabel = screen.DeviceName;
                    g.DrawString(screenLabel, textFont, textPen, visualRect, textFormat);
                }
            }

            var oldBitmap = this.pictureBox1.Image;
            this.pictureBox1.Image = bitmap;

            if (oldBitmap != null)
            {
                oldBitmap.Dispose();
            }

            redrawMutex.Set();
        }
    }
}
