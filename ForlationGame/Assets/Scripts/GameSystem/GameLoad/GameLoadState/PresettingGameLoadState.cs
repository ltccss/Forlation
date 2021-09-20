using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PresettingGameLoadState : BaseGameLoadState
{

    private GameLoadMgr _gameLoadMgr;

    public PresettingGameLoadState(GameLoadMgr gameLoadMgr) : base(gameLoadMgr)
    {
        this._gameLoadMgr = gameLoadMgr;
    }

    protected override void _OnEnterState()
    {
        // 一段时间内没操作（并且不在自动spin）的时候可以考虑切30fps？
        Application.targetFrameRate = 60;

        // 工具类初始化
        CoroutineUtil.Init(ForlationGameManager.Me.Root);

        // sdk相关初始化

        // Platform.Me;

        if (this._gameLoadMgr.IsFirstInit)
        {
            // Platform.Me;
        }

        CoroutineUtil.StartCoroutine(_Co());
    }


    private IEnumerator _Co()
    {
        if (LuaUtil.LuaEnv != null)
        {
            LuaUtil.LuaEnv.GC();
        }
        System.GC.Collect();

        yield return Resources.UnloadUnusedAssets();

        // lua初始化
        LuaUtil.Init(true);
        // lua扩展库
        LuaUtil.LuaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
        LuaUtil.LuaEnv.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
        LuaUtil.LuaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);
        LuaUtil.LuaEnv.AddBuildin("ffi", XLua.LuaDLL.Lua.LoadFFI);

        // 确保GameCache存在
        if (!Directory.Exists(AppSetting.GameCachePath))
        {
            Directory.CreateDirectory(AppSetting.GameCachePath);
        }


        if (!AppSetting.simulatePhoneMode)
        {
            this.GoToState(GameLoadMgr.GameLoadState.SetupLua);
        }
        else
        {
            this.GoToState(GameLoadMgr.GameLoadState.GetPackageInfo);
        }
    }

    protected override void _OnUpdate()
    {
        ForlationGameManager.Me.ReportLoadingProgress(this.DisplayedStateName, 0.01f);
    }

    public override GameLoadMgr.GameLoadState State => GameLoadMgr.GameLoadState.Presetting;
}
