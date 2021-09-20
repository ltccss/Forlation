using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
using System;

public enum GameState
{
    Prepare,
    Loading,
    LuaLoading,
}
public class ForlationGameManager
{
    private static ForlationGameManager _me;

    public static ForlationGameManager Me
    {
        get
        {
            if (_me == null)
            {
                _me = new ForlationGameManager();
            }
            return _me;
        }
    }

    private GameObject _root;

    private GameState _state = GameState.Prepare;

    private LuaTable _luaGameMgr;
    private LuaFunction _luaUpdate;
    private LuaFunction _luaLateUpdate;

    private LoadingView _loadingView;

    private bool _returnLaunch = false;

    public GameObject Root
    {
        get
        {
            return this._root;
        }
    }
    
    public void Init(GameObject root)
    {
        this._root = root;

        this._loadingView = GameObject.FindObjectOfType<LoadingView>();
        this._state = GameState.Prepare;
    }

    /// <summary>
    /// 重新加载游戏
    /// </summary>
    public void ReturnLaunchScene()
    {
        this._returnLaunch = true;
    }

    private void _ReturnLaunchScene()
    {
        this._luaGameMgr = null;
        this._luaUpdate = null;
        this._luaLateUpdate = null;
        SceneManager.LoadScene("GameLaunch");
    }

    public void Update()
    {
        if (Input.anyKey)
        {
            OnAnyKeyPressed();
        }

        switch (this._state)
        {
            case GameState.Prepare:
                {
                    GameLoadMgr.Me.StartGameLoad();
                    this._state = GameState.Loading;

                    break;
                }

            case GameState.Loading:
                {
                    GameLoadMgr.Me.Update();
                    if (GameLoadMgr.Me.IsComplete())
                    {
                        this._state = GameState.LuaLoading;

                        // 开始加载lua
                        // 这里开始把控制权交给lua了
                        LuaUtil.DoString("require(\"LuaGameEntry\")");
                    }
                    break;
                }
                
        }

        if (this._luaUpdate != null)
        {
            _luaUpdate.Action<LuaTable>(_luaGameMgr);
        }
        
        if (this._returnLaunch)
        {
            this._returnLaunch = false;
            this._ReturnLaunchScene();
        }
    }

    public void LateUpdate()
    {
        if (this._luaLateUpdate != null)
        {
            _luaLateUpdate.Action<LuaTable>(_luaGameMgr);
        }
    }

    public void OnGamePause(bool pause)
    {
        if (this._luaGameMgr != null)
        {
            var luaFunc = this._luaGameMgr.Get<LuaFunction>("OnGamePause");
            luaFunc.Call(this._luaGameMgr, pause);
        }
         
    }

    public void OnGameQuit()
    {
        if (this._luaGameMgr != null)
        {
            var luaFunc = this._luaGameMgr.Get<LuaFunction>("OnGameQuit");
            luaFunc.Call(this._luaGameMgr);
        }
    }

    public void EnterState(GameState newState)
    {
        this._state = newState;
    }

    public void SetLuaMgrUpdate(LuaTable callee, LuaFunction updateFunc)
    {
        this._luaGameMgr = callee;
        this._luaUpdate = updateFunc;
    }

    public void SetLuaLateUpdate(LuaFunction lateUpdateFunc)
    {
        this._luaLateUpdate = lateUpdateFunc;
    }

    public void ReportLoadingProgress(string info, float progress)
    {
        if (this._loadingView != null)
        {
            this._loadingView.RefreshLoadingText(info);
            this._loadingView.RefreshProgress(progress);
        }
    }

    public void ShowloadingProgress(bool on)
    {
        if (this._loadingView != null)
        {
            this._loadingView.ShowProgress(on);
        }
    }

    public void ShowVersionInfo(string version)
    {
        if (this._loadingView != null)
        {
            this._loadingView.RefreshVersion(version);
        }
    }

    public void OnAnyKeyPressed()
    {
        if (this._luaGameMgr != null)
        {
            var luaFunc = this._luaGameMgr.Get<LuaFunction>("OnAnyKeyPressed");
            luaFunc.Call(this._luaGameMgr);
        }
    }
}
