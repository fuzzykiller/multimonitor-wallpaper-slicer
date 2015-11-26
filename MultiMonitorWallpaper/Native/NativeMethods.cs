using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace MultiMonitorWallpaper.Native
{
    internal static class NativeMethods
    {
        // ReSharper disable InconsistentNaming

        public static Guid GUID_DEVINTERFACE_MONITOR = new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7");
        public const int ERROR_NO_MORE_ITEMS = 0x103;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        public const int DICS_FLAG_GLOBAL = 0x1;
        public const int DIREG_DEV = 0x1;
        public const int KEY_QUERY_VALUE = 0x1;
        public const int EDD_GET_DEVICE_INTERFACE_NAME = 0x1;

        public static readonly int SP_DEVICE_INTERFACE_DETAIL_DATA_SIZE =
            Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA));

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] [Optional] string Enumerator,
            [Optional] IntPtr hwndParent,
            DiGetClassFlags Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,
            IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid,
            uint MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,
            [In] ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            [Optional] IntPtr DeviceInterfaceDetailData,
            uint DeviceInterfaceDetailDataSize,
            out uint RequiredSize,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiOpenDevRegKey(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            int Scope,
            int HwProfile,
            int KeyType,
            int samDesired);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern uint CM_Get_Device_ID_Size(
            out uint pulLen,
            uint dnDevInst,
            uint ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern uint CM_Get_Device_ID(
            uint dnDevInst,
            IntPtr Buffer,
            uint BufferLen,
            uint ulFlags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern int RegQueryValueEx(
            IntPtr hkey,
            string lpValueName,
            IntPtr lpReserved,
            ref RegistryValueKind lpType,
            IntPtr lpData,
            ref uint lpcbData);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(
            IntPtr hkey);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(
            string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [Flags]
        public enum DiGetClassFlags : uint
        {
            DIGCF_DEFAULT = 1, // only valid with DIGCF_DEVICEINTERFACE
            DIGCF_PRESENT = 2,
            DIGCF_ALLCLASSES = 4,
            DIGCF_PROFILE = 8,
            DIGCF_DEVICEINTERFACE = 16,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            private IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            private IntPtr Reserved;
        }

        [UsedImplicitly]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        private struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            [UsedImplicitly]
            public uint cbSize;

            [UsedImplicitly]
            public char devicePath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags]
        public enum DisplayDeviceStateFlags
        {
            /// <summary>Display is currently used to display part of the desktop.</summary>
            Active = 0x1,

            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        // ReSharper restore InconsistentNaming
    }
}