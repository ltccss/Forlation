using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using XLua;
using SimpleJson;
using System;


/**
 * 游戏加载步骤：
 * c#层：
 * 1 - GameLoadState.Presetting             0.00 - 0.01
 * 2 - GameLoadState.GetPackageInfo         0.01 - 0.03
 * 3 - GameLoadState.CheckVersion           0.03 - 0.06
 * 4 - GameLoadState.UpdateLuaFile          0.06 - 0.10
 * 5 - GameLoadState.SetupLua               0.10 - 0.11
 * 
 * lua层：
 * 1 - LuaGameLoadState.InitUISystem        0.11 - 0.13
 * 2 - LuaGameLoadState.VersionCheck        0.13 - 0.15
 * 3 - LuaGameLoadState.ExtractPackageFiles 0.15 - 0.30
 * 4 - LuaGameLoadState.DownloadFiles       0.30 - 0.72
 * 5 - LuaGameLoadState.LoadConfig          0.72 - 0.80
 * 6 - LuaGameLoadState.InitMgrs            0.80 - 0.90
 * 7 - LuaGameLoadState.SyncPlayerData      0.90 - 1.00
 **/

public class GameLoadMgr
{
    private static GameLoadMgr _me;
    public static GameLoadMgr Me
    {
        get
        {
            if (_me == null)
            {
                _me = new GameLoadMgr();
            }
            return _me;
        }
    }

    public enum GameLoadState
    {
        /// <summary>
        /// 杂项设置，包括一些sdk初始化
        /// </summary>
        Presetting,

        /// <summary>
        /// 获取包(资源)信息
        /// </summary>
        GetPackageInfo,

        /// <summary>
        /// 向服务器查询文件版本
        /// </summary>
        CheckVersion,

        /// <summary>
        /// 如果有最新的lua文件，下载下来
        /// </summary>
        UpdateLuaFile,

        /// <summary>
        /// lua加载设置
        /// </summary>
        SetupLua,

        Complete,
    }

    private GameLoadState _state = GameLoadState.Presetting;
    private BaseGameLoadState _loadState;

    private PackedLuaLoader _packedLuaLoader;

    private bool _firstInit = true;
    private int _initCount = 0;
    
    public void StartGameLoad()
    {
        _initCount += 1;
        if (_initCount > 1)
        {
            _firstInit = false;
        }
        else
        {
            _firstInit = true;
        }
        
        this.EnterState(GameLoadState.Presetting);
    }

    public void EnterState(GameLoadState state)
    {
        switch (state)
        {
            case GameLoadState.Presetting:
                this._loadState = new PresettingGameLoadState(this);
                break;
            case GameLoadState.GetPackageInfo:
                this._loadState = new GetPackageInfoGameLoadState(this);
                break;
            case GameLoadState.CheckVersion:
                this._loadState = new CheckVersionGameLoadState(this);
                break;
            case GameLoadState.UpdateLuaFile:
                this._loadState = new UpdateLuaFileGameLoadState(this);
                break;
            case GameLoadState.SetupLua:
                this._loadState = new SetupLuaGameLoadState(this);
                break;
            default:
                this._loadState = null;
                break;
        }
        this._state = state;

        LogUtil.Log("switch to state : " + state.ToString());

        if (this._loadState != null)
        {
            this._loadState.NotifyEnterState();
        }
        
    }

    public void Update()
    {
        if (this._loadState != null)
        {
            this._loadState.Update();
        }
    }

    public bool IsComplete()
    {
        return this._state == GameLoadState.Complete;
    }

    public string GetStateDisplayName(GameLoadState state)
    {
        switch (state)
        {
            case GameLoadState.CheckVersion:
                return "CheckVersion...";
            case GameLoadState.GetPackageInfo:
                return "GetPackageInfo...";
            case GameLoadState.Presetting:
                return "Presetting...";
            case GameLoadState.SetupLua:
                return "SetupLua...";
            case GameLoadState.UpdateLuaFile:
                return "UpdateLuaFile...";
        }
        return "...";
    }

    /// <summary>
    /// 是否是首次初始化, 
    /// 还是从游戏重回加载了
    /// </summary>
    public bool IsFirstInit
    {
        get
        {
            return this._firstInit;
        }         
    }
}
