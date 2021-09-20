using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJson;
using System;
using System.IO;

public class CheckVersionGameLoadState : BaseGameLoadState
{

    public CheckVersionGameLoadState(GameLoadMgr gameLoadMgr) : base(gameLoadMgr)
    {
        
    }

    protected override void _OnEnterState()
    {
        ForlationGameManager.Me.ReportLoadingProgress(this.DisplayedStateName, 0.03f);

        // TODO
        this.GoToState(GameLoadMgr.GameLoadState.UpdateLuaFile);
    }

    protected override void _OnUpdate()
    {
        
    }


    public override GameLoadMgr.GameLoadState State => GameLoadMgr.GameLoadState.CheckVersion;
}
