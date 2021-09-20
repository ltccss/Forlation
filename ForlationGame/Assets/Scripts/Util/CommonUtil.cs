using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtil
{
    public static float ASPECT_RATIO_THRESHOLD = 2.1f;
    public static float IphoneXTopOffset = 100f;

    public static bool IsUnityObjectNull(UnityEngine.Object obj)
    {
        return obj == null;
    }

    public static bool IsUnityObject(System.Object obj)
    {
        return obj is UnityEngine.Object;
    }

    public static void GC()
    {
        System.GC.Collect();
        LuaUtil.LuaEnv.FullGc();
        LuaUtil.LuaEnv.GC();

        // 不要问为什么gc姿势那么奇怪，就是这样
        LuaUtil.LuaEnv.FullGc();

        Resources.UnloadUnusedAssets();
    }

    public static float GetExtraPadding()
    {
        float deviceRatio = 1.0f * Screen.width / Screen.height;
        if (deviceRatio > ASPECT_RATIO_THRESHOLD)
            return IphoneXTopOffset;
        else
            return 0;
    }
}
