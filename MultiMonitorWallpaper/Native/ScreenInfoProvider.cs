using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MultiMonitorWallpaper.Native
{
    public static class ScreenInfoProvider
    {
        public static IEnumerable<ScreenInfo> GetScreens()
        {
            var setupApiData = GetSetupApiData()
                .ToDictionary(x => x.DevicePath, StringComparer.InvariantCultureIgnoreCase);

            // We assume screen count hasn't changed in the meantime ;)
            var screenCount = setupApiData.Count;

            var displayDevice = new NativeMethods.DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);
            for (uint i = 0; i < screenCount; i++)
            {
                if (!NativeMethods.EnumDisplayDevices(null, i, ref displayDevice, 0))
                {
                    throw new IndexOutOfRangeException("Screen vanished while enumerating");
                }

                if (
                    !NativeMethods.EnumDisplayDevices(
                        displayDevice.DeviceName, 0, ref displayDevice, NativeMethods.EDD_GET_DEVICE_INTERFACE_NAME))
                {
                    throw new IndexOutOfRangeException("Screen vanished while enumerating");
                }

                var screenInfo = setupApiData[displayDevice.DeviceID];
                screenInfo.FriendlyName = displayDevice.DeviceString;

                // Format: \\.\DISPLAY0\Monitor0
                var gdiDeviceName = displayDevice.DeviceName;

                // Format: \\.\DISPLAY0
                var managedScreenInfo = Screen.AllScreens.Single(x => gdiDeviceName.StartsWith(x.DeviceName));
                screenInfo.ManagedInfo = managedScreenInfo;
            }

            return setupApiData.Values;
        }

        private static IEnumerable<ScreenInfo> GetSetupApiData()
        {
            var screens = new List<ScreenInfo>();
            var monitorGuid = NativeMethods.GUID_DEVINTERFACE_MONITOR;

            var devInfoSetHandle = IntPtr.Zero;
            try
            {
                devInfoSetHandle = NativeMethods.SetupDiGetClassDevs(
                    ref monitorGuid,
                    null,
                    IntPtr.Zero,
                    NativeMethods.DiGetClassFlags.DIGCF_PRESENT |
                        NativeMethods.DiGetClassFlags.DIGCF_DEVICEINTERFACE);
                if (devInfoSetHandle == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                uint i = 0;
                while (true)
                {
                    var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

                    if (!NativeMethods.SetupDiEnumDeviceInterfaces(
                        devInfoSetHandle, IntPtr.Zero, ref monitorGuid, i, ref deviceInterfaceData))
                    {
                        if (Marshal.GetLastWin32Error() == NativeMethods.ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }

                        throw new Win32Exception();
                    }

                    var deviceInterfaceDetail = GetDeviceInterfaceDetail(devInfoSetHandle, ref deviceInterfaceData);
                    var screenInfo = new ScreenInfo
                        {
                            DevicePath = deviceInterfaceDetail.DevicePath,
                            InstanceId = GetDeviceInstanceId(deviceInterfaceDetail.DevinfoData.DevInst),
                            EDID = GetMonitorEDID(devInfoSetHandle, deviceInterfaceDetail.DevinfoData)
                        };

                    screens.Add(screenInfo);

                    i++;
                }
            }
            finally
            {
                if (devInfoSetHandle != IntPtr.Zero)
                {
                    NativeMethods.SetupDiDestroyDeviceInfoList(devInfoSetHandle);
                }
            }

            return screens;
        }

        private static DeviceInterfaceDetail GetDeviceInterfaceDetail(IntPtr devInfoSetHandle, ref NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var devinfoData = new NativeMethods.SP_DEVINFO_DATA();
            devinfoData.cbSize = Marshal.SizeOf(devinfoData);

            uint requiredSize;
            if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(
                devInfoSetHandle,
                ref deviceInterfaceData,
                IntPtr.Zero,
                0,
                out requiredSize,
                ref devinfoData))
            {
                if (Marshal.GetLastWin32Error() != NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Win32Exception();
                }
            }

            var deviceInterfaceDetailDataBuffer = Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(uint)) +
                        Marshal.SystemDefaultCharSize +
                        (int)requiredSize + Marshal.SystemDefaultCharSize);

            string devicePath;
            try
            {
                // Potential danger: Writing int to uint
                Marshal.WriteInt32(
                    deviceInterfaceDetailDataBuffer,
                    NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA_SIZE);

                if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(
                    devInfoSetHandle,
                    ref deviceInterfaceData,
                    deviceInterfaceDetailDataBuffer,
                    requiredSize,
                    out requiredSize,
                    ref devinfoData))
                {
                    throw new Win32Exception();
                }

                devicePath = Marshal.PtrToStringAuto(
                    new IntPtr(deviceInterfaceDetailDataBuffer.ToInt32() + 4),
                    ((int)requiredSize - 6) / 2);
            }
            finally
            {
                Marshal.FreeHGlobal(deviceInterfaceDetailDataBuffer);
            }

            return new DeviceInterfaceDetail(devinfoData, devicePath);
        }

        private static string GetDeviceInstanceId(uint devInst)
        {
            uint requiredSize;
            var result = NativeMethods.CM_Get_Device_ID_Size(out requiredSize, devInst, 0);
            if (result != 0)
            {
                throw new Win32Exception(string.Format("Error 0x{0:X8} in CM_Get_Device_ID_Size", result));
            }

            var buffer = Marshal.AllocHGlobal((int)(requiredSize + 2));
            string deviceInstanceId;

            try
            {
                result = NativeMethods.CM_Get_Device_ID(devInst, buffer, requiredSize + 2, 0);
                if (result != 0)
                {
                    throw new Win32Exception(string.Format("Error 0x{0:X8} in CM_Get_Device_ID", result));
                }

                deviceInstanceId = Marshal.PtrToStringAuto(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return deviceInstanceId;
        }

        private static byte[] GetMonitorEDID(IntPtr pDevInfoSet, NativeMethods.SP_DEVINFO_DATA deviceInfoData)
        {
            var deviceRegistryKey = NativeMethods.SetupDiOpenDevRegKey(
                pDevInfoSet,
                ref deviceInfoData,
                NativeMethods.DICS_FLAG_GLOBAL,
                0,
                NativeMethods.DIREG_DEV,
                NativeMethods.KEY_QUERY_VALUE);

            if (deviceRegistryKey == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var ptrBuff = Marshal.AllocHGlobal(256);
            var edidBlock = new byte[256];
            try
            {
                var regKeyType = RegistryValueKind.Binary;
                uint length = 256;
                var result = NativeMethods.RegQueryValueEx(deviceRegistryKey, "EDID", IntPtr.Zero, ref regKeyType, ptrBuff, ref length);
                if (result != 0)
                {
                    throw new Win32Exception(result);
                }
                
                Marshal.Copy(ptrBuff, edidBlock, 0, 256);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrBuff);
                var result = NativeMethods.RegCloseKey(deviceRegistryKey);
                if (result != 0)
                {
                    throw new Win32Exception(result);
                }
            }
            
            return edidBlock;
        }
    }
}