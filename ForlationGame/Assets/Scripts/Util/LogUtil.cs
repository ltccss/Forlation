using System.Collections;
using System.Collections.Generic;
using XLua;
[LuaCallCSharp]
public static class LogUtil
{
#if DEBUG
    public static bool enableLog = true;
#else
    public static bool enableLog = false;
#endif
    public static bool enableWarn = true;
    public static bool enableError = true;

    [LuaCallCSharp]
    public static void Log(string str)
    {
        if (enableLog)
        {
            UnityEngine.Debug.Log(str);
        }
    }
    [LuaCallCSharp]
    public static void LogFormat(string str, params object[] args)
    {
        if (enableLog)
        {
            UnityEngine.Debug.LogFormat(str, args);
        }
    }
    [LuaCallCSharp]
    public static void LogObject(string str, UnityEngine.Object obj)
    {
        if (enableLog)
        {
            UnityEngine.Debug.Log(str, obj);
        }
    }
    [LuaCallCSharp]
    public static void Warn(string str)
    {
        if (enableWarn)
        {
            UnityEngine.Debug.LogWarning(str);
        }
    }
    [LuaCallCSharp]
    public static void WarnFormat(string str, params object[] args)
    {
        if (enableWarn)
        {
            UnityEngine.Debug.LogWarningFormat(str, args);
        }
    }
    [LuaCallCSharp]
    public static void WarnObject(string str, UnityEngine.Object obj)
    {
        if (enableWarn)
        {
            UnityEngine.Debug.LogWarning(str, obj);
        }
    }
    [LuaCallCSharp]
    public static void Error(string str)
    {
        if (enableError)
        {
            UnityEngine.Debug.LogError(str);
        }
    }
    [LuaCallCSharp]
    public static void ErrorFormat(string str, params object[] args)
    {
        if (enableError)
        {
            UnityEngine.Debug.LogErrorFormat(str, args);
        }
    }
    [LuaCallCSharp]
    public static void ErrorObject(string str, UnityEngine.Object obj)
    {
        if (enableError)
        {
            UnityEngine.Debug.LogError(str, obj);
        }
    }
    [LuaCallCSharp]
    public static void Assert(bool cond, string log)
    {
        if (cond == false)
        {
            UnityEngine.Debug.LogError(log);
            UnityEngine.Debug.LogError(UnityEngine.StackTraceUtility.ExtractStackTrace());
        }
    }
    [LuaCallCSharp]
    public static string CombineColor(string str, UnityEngine.Color color)
    {
        return string.Format("<color=#{0}>{1}</color>", UnityEngine.ColorUtility.ToHtmlStringRGB(color), str);
    }
}
