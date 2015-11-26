using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LibScreenInfo.Properties;

namespace LibScreenInfo
{
    public class ScreenInfo
    {
        private const int FirstTimingInfoAddress = 54;
        private const int WidthLsbAddress = 12;
        private const int HeightLsbAddress = 13;
        private const int WidthHeightMsbAddress = 14;
        private const double MillimeterPerInch = 25.4;

        private PixelsPerInch ppi;
        private ScreenSize dimensions;

        internal ScreenInfo(ScreenInfoProvider.PartialScreenInfo partialScreenInfo, string friendlyName, Screen managedInfo)
        {
            this.DevicePath = partialScreenInfo.DevicePath;
            this.EDID = partialScreenInfo.EDID;
            this.InstanceId = partialScreenInfo.InstanceId;
            this.FriendlyName = friendlyName;
            this.ManagedInfo = managedInfo;
        }

        public string DevicePath { get; internal set; }

        public string InstanceId { get; internal set; }

        /// <summary>
        /// As visible in Screen Resolution control panel applet
        /// </summary>
        public string FriendlyName { get; internal set; }

        public Screen ManagedInfo { get; private set; }

        /// <summary>
        /// Raw EDID blob, includes both EDID and EIA/CEA-861 extension block
        /// </summary>
        public byte[] EDID { get; private set; }

        /// <summary>
        /// Physical dimensions as indicated in EDID. In millimeters.
        /// </summary>
        public ScreenSize PhysicalDimensions
        {
            get { return this.dimensions ?? (this.dimensions = this.GetPhysicalDimensions()); }
        }

        /// <summary>
        /// Pixels per inch, calculated using <see cref="PhysicalDimensions"/> and current(!) resolution
        /// </summary>
        public PixelsPerInch PixelsPerInch
        {
            get { return this.ppi ?? (this.ppi = this.GetPpi()); }
        }

        private ScreenSize GetPhysicalDimensions()
        {
            if (this.EDID == null)
            {
                throw new InvalidOperationException(Resources.EdidNotAvailable);
            }

            if (!this.EDID.StartsWith(new byte[] {0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0}))
            {
                throw new InvalidDataException(Resources.EdidHeaderNotFound);
            }

            var basicEdidSum = this.EDID.Take(128).Sum(x => (int)x);
            if ((basicEdidSum % 256) != 0)
            {
                throw new InvalidDataException(Resources.EdidChecksumInvalid);
            }

            int width = this.EDID[FirstTimingInfoAddress + WidthLsbAddress];
            int height = this.EDID[FirstTimingInfoAddress + HeightLsbAddress];

            var combinedMsbs = this.EDID[FirstTimingInfoAddress + WidthHeightMsbAddress];

            width += (combinedMsbs & -16) << 4; // wanted: bits 7-4, thus 0x11110000
            height += (combinedMsbs & 15) << 8; // wanted: bits 3-0, thus 0x00001111

            return new ScreenSize(width, height);
        }

        private PixelsPerInch GetPpi()
        {
            if (this.ManagedInfo == null)
            {
                throw new InvalidOperationException(Resources.ManagedScreenInfoNotAvailable);
            }

            var horizontal = this.ManagedInfo.Bounds.Width / (this.PhysicalDimensions.Width / MillimeterPerInch);
            var vertical = this.ManagedInfo.Bounds.Height / (this.PhysicalDimensions.Height / MillimeterPerInch);

            return new PixelsPerInch(horizontal, vertical);
        }
    }
}