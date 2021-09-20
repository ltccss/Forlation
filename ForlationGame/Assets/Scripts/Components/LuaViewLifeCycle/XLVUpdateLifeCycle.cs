using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLVUpdateLifeCycle : MonoBehaviour
{
    private LuaTable _luaTable;
    private LuaFunction _updateFunc;

    public void Reset(LuaTable t, LuaFunction updateFunc)
    {
        this._luaTable = t;
        this._updateFunc = updateFunc;
    }

    private void Update()
    {
        this._updateFunc?.Call(this._luaTable);
    }
}
