using MultiMonitorWallpaper.Native;

namespace MultiMonitorWallpaper
{
    internal class DeviceInterfaceDetail
    {
        public DeviceInterfaceDetail(NativeMethods.SP_DEVINFO_DATA devinfoData, string devicePath)
        {
            this.DevinfoData = devinfoData;
            this.DevicePath = devicePath;
        }

        public NativeMethods.SP_DEVINFO_DATA DevinfoData { get; private set; }
        public string DevicePath { get; private set; }
    }
}