using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLVLateUpdateLifeCycle : MonoBehaviour
{
    private LuaTable _luaTable;
    private LuaFunction _lateUpdateFunc;

    public void Reset(LuaTable t, LuaFunction lateUpdateFunc)
    {
        this._luaTable = t;
        this._lateUpdateFunc = lateUpdateFunc;
    }

    private void LateUpdate()
    {
        this._lateUpdateFunc?.Call(this._luaTable);
    }
}
