using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using SimpleJson;

public class GetPackageInfoGameLoadState : BaseGameLoadState
{
    public GetPackageInfoGameLoadState(GameLoadMgr gameLoadMgr) : base(gameLoadMgr)
    {

    }

    protected override void _OnEnterState()
    {
        // TODO
        this.GoToState(GameLoadMgr.GameLoadState.CheckVersion);
    }

    protected override void _OnUpdate()
    {
        ForlationGameManager.Me.ReportLoadingProgress(this.DisplayedStateName, 0.02f);
    }


    public override GameLoadMgr.GameLoadState State => GameLoadMgr.GameLoadState.GetPackageInfo;
}
