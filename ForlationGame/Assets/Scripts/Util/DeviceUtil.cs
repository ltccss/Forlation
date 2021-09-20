using UnityEngine;
using System.Runtime.InteropServices;
public static class DeviceUtil
{
    public static readonly float DesignWidth = 1920;//开发时分辨率宽
    public static readonly float DesignHeight = 1080;//开发时分辨率高
    public static float DesignRatio = DesignWidth / DesignHeight;
    private static float ScreenAspect = DesignRatio;
    private static string _deviceID;

    static DeviceUtil()
    {
        ScreenAspect = GetScreenWidthHeightRatio();
    }

    public static float GetDesignWidthHeightRatio()
    {
        return DesignRatio;
    }

    public static float GetScreenWidthHeightRatio()
    {
        return (float)Screen.width / (float)Screen.height;
    }

    [XLua.LuaCallCSharp]
    public static string GetDeviceId()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }
}
