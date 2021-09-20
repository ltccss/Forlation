using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLVApplicationPauseLifeCycle : MonoBehaviour
{
    private LuaTable _luaTable;
    private LuaFunction _pauseFunc;

    public void Reset(LuaTable t, LuaFunction pauseFunc)
    {
        this._luaTable = t;
        this._pauseFunc = pauseFunc;
    }

    void OnApplicationPause(bool pause)
    {
        this._pauseFunc?.Call(this._luaTable, pause);
    }
}
