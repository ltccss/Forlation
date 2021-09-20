using XLua;
using System;
using System.Reflection;
using UnityEngine.Events;
[LuaCallCSharp, ReflectionUse]
public static class UnityEventBaseEx
{
    public static void ReleaseUnusedListeners(this UnityEventBase unityEventBase)
    {
        BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = unityEventBase.GetType();
        MethodInfo method = type.GetMethod("PrepareInvoke", flag);
        method.Invoke(unityEventBase, null);
    }

    public static void RemoveAllListenersEx(this UnityEventBase unityEventBase)
    {
        unityEventBase.RemoveAllListeners();
        unityEventBase.ReleaseUnusedListeners();
    }
}