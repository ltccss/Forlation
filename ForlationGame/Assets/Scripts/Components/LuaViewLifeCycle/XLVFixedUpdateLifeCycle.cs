using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLVFixedUpdateLifeCycle : MonoBehaviour
{
    private LuaTable _luaTable;
    private LuaFunction _fixedUpdateFunc;

    public void Reset(LuaTable t, LuaFunction fixedUpdateFunc)
    {
        this._luaTable = t;
        this._fixedUpdateFunc = fixedUpdateFunc;
    }

    private void FixedUpdate()
    {
        this._fixedUpdateFunc?.Call(this._luaTable);
    }
}
