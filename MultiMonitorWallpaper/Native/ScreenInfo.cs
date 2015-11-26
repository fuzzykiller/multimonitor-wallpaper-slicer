using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MultiMonitorWallpaper.Native
{
    public class ScreenInfo
    {
        private const int FirstTimingInfoAddress = 54;
        private const int WidthLsbAddress = 12;
        private const int HeightLsbAddress = 13;
        private const int WidthHeightMsbAddress = 14;
        private const double MillimeterPerInch = 25.4;

        public string DevicePath { get; set; }

        public string InstanceId { get; set; }

        public string FriendlyName { get; set; }

        public Screen ManagedInfo { get; set; }

        public byte[] EDID { get; set; }

        public Size PhysicalDimensions
        {
            get { return this.GetPhysicalDimensions(); }
        }

        public double PixelsPerInch
        {
            get { return this.GetPpi(); }
        }

        private Size GetPhysicalDimensions()
        {
            if (this.EDID == null)
            {
                throw new InvalidOperationException("No EDID available.");
            }

            if (!this.EDID.StartsWith(new byte[] {0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0}))
            {
                throw new InvalidDataException("Didn't find EDID header.");
            }

            if ((this.EDID.Sum(x => (int)x) % 256) != 0)
            {
                throw new InvalidDataException("EDID checksum invalid.");
            }

            int width = this.EDID[FirstTimingInfoAddress + WidthLsbAddress];
            int height = this.EDID[FirstTimingInfoAddress + HeightLsbAddress];

            var combinedMsbs = this.EDID[FirstTimingInfoAddress + WidthHeightMsbAddress];

            width += (combinedMsbs & -16) << 4; // wanted: bits 7-4, thus 0x11110000
            height += (combinedMsbs & 15) << 8; // wanted: bits 3-0, thus 0x00001111

            return new Size(width, height);
        }

        private double GetPpi()
        {
            if (this.ManagedInfo == null)
            {
                throw new InvalidOperationException("Managed info not available.");
            }

            var dimensions = this.GetPhysicalDimensions();
            var xDpi = this.ManagedInfo.Bounds.Width / (dimensions.Width / MillimeterPerInch);
            var yDpi = this.ManagedInfo.Bounds.Height / (dimensions.Height / MillimeterPerInch);

            return xDpi;
        }
    }
}