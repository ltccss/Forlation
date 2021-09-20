using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;

public class UpdateLuaFileGameLoadState : BaseGameLoadState
{
    public UpdateLuaFileGameLoadState(GameLoadMgr gameLoadMgr) : base(gameLoadMgr)
    {

    }

    protected override void _OnEnterState()
    {
        // TODO
        this.GoToState(GameLoadMgr.GameLoadState.SetupLua);
    }

    protected override void _OnUpdate()
    {
        ForlationGameManager.Me.ReportLoadingProgress(this.DisplayedStateName, 0.06f);
    }

    public override GameLoadMgr.GameLoadState State => GameLoadMgr.GameLoadState.UpdateLuaFile;
}
