using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SetupLuaGameLoadState : BaseGameLoadState
{
    private PackedLuaLoader _packedLuaLoader;

    public SetupLuaGameLoadState(GameLoadMgr gameLoadMgr) : base(gameLoadMgr)
    {

    }

    protected override void _OnEnterState()
    {
        // lua加载器
        if (!AppSetting.simulatePhoneMode || AppSetting.loadLuaDirectlyInSimulatePhoneMode)
        {
            LuaUtil.LuaEnv.AddLoader((ref string filepath) =>
            {

                string dir = Path.Combine(Application.dataPath, "LuaScript");
                string fileName = Path.Combine(dir, filepath) + ".lua";

                if (File.Exists(fileName))
                {
                    return File.ReadAllBytes(fileName);
                }
                else
                {
                    return null;
                }

            });
        }
        else
        {
            // 真机模式下实现下对应的加载器
            string luaPackPath = Path.Combine(AppSetting.GameCachePath, AppSetting.luaFileName);
            this._packedLuaLoader = new PackedLuaLoader(luaPackPath);
            LuaUtil.LuaEnv.AddLoader(this._packedLuaLoader.LoadLuaFile);
        }

        this.GoToState(GameLoadMgr.GameLoadState.Complete);
    }

    protected override void _OnUpdate()
    {
        ForlationGameManager.Me.ReportLoadingProgress(this.DisplayedStateName, 0.1f);
    }

    public override GameLoadMgr.GameLoadState State => GameLoadMgr.GameLoadState.SetupLua;
}
