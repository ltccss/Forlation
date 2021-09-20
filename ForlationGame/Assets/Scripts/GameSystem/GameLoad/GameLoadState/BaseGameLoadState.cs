using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameLoadState
{
    private GameLoadMgr _gameLoadMgr;

    public abstract GameLoadMgr.GameLoadState State
    {
        get;
    }

    public BaseGameLoadState(GameLoadMgr gameloadMgr)
    {
        this._gameLoadMgr = gameloadMgr;
    }

    public void Update()
    {
        this._OnUpdate();
    }

    protected abstract void _OnUpdate();

    public void GoToState(GameLoadMgr.GameLoadState state)
    {
        this._gameLoadMgr.EnterState(state);
    }

    public void NotifyEnterState()
    {
        this._OnEnterState();
    }

    protected abstract void _OnEnterState();

    protected string DisplayedStateName
    {
        get
        {
            return this._gameLoadMgr.GetStateDisplayName(this.State);
        }
    }
}
