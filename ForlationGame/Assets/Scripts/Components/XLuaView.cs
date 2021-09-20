using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XLua;

[System.Serializable]
public class XLVInt
{
    public string name;
    public int value;
}
[System.Serializable]
public class XLVLong
{
    public string name;
    public long value;
}
[System.Serializable]
public class XLVDouble
{
    public string name;
    public double value;
}
[System.Serializable]
public class XLVString
{
    public string name;
    public string value;
}
[System.Serializable]
public class XLVBool
{
    public string name;
    public bool value;
}
[System.Serializable]
public class XLVVector3
{
    public string name;
    public Vector3 value;
}
[System.Serializable]
public class XLVColor
{
    public string name;
    public Color value;
}
[System.Serializable]
public class XLVSprite
{
    public string name;
    public Sprite value;
}
public class XLVTestData
{
    public string funcName;
    public XLua.LuaFunction func;
}
public class XLuaView : MonoBehaviour
{
    public GameObject[] gameObjects;

    public XLVInt[] ints;
    public XLVLong[] longs;
    public XLVBool[] bools;
    public XLVDouble[] doubles;
    public XLVString[] strings;
    public XLVVector3[] vector3s;
    public XLVColor[] colors;
    public XLVSprite[] sprites;

    [System.NonSerialized]
    public List<XLVTestData> testFuncList;

    public string luaFilePath;
    [System.NonSerialized]
    public string luaFilePathRuntime;

    private LuaTable _luaTable;
    private LuaFunction _awakeFunc;
    private LuaFunction _startFunc;
    private LuaFunction _enableFunc;
    private LuaFunction _disableFunc;
    private LuaFunction _destroyFunc;

    public void SetRuntimeLuaPath(string relativeLuaPath)
    {
        this.luaFilePathRuntime = Path.Combine("Assets", "LuaScript", relativeLuaPath) + ".lua";
    }

    public void AddTestFunc(string name, XLua.LuaFunction func)
    {
        if (this.testFuncList == null)
        {
            this.testFuncList = new List<XLVTestData>();
        }
        var data = new XLVTestData();
        data.funcName = name;
        data.func = func;
        this.testFuncList.Add(data);
    }

    public void SetCommonLifeCallback(LuaTable t, LuaFunction awakeFunc, LuaFunction startFunc, LuaFunction enableFunc, LuaFunction disableFunc, LuaFunction destroyFunc)
    {
        this._luaTable = t;
        this._awakeFunc = awakeFunc;
        this._startFunc = startFunc;
        this._enableFunc = enableFunc;
        this._disableFunc = disableFunc;
        this._destroyFunc = destroyFunc;
    }

    private void Awake()
    {
        this._awakeFunc?.Call(this._luaTable);
    }

    private void Start()
    {
        this._startFunc?.Call(this._luaTable);
    }

    private void OnEnable()
    {
        this._enableFunc?.Call(this._luaTable);
    }

    private void OnDisable()
    {
        this._disableFunc?.Call(this._luaTable);
    }

    private void OnDestroy()
    {
        this._destroyFunc?.Call(this._luaTable);
    }
}
