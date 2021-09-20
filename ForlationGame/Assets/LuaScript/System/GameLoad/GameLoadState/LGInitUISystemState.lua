---@class LGInitUISystemState : BaseLuaGameLoadState
LGInitUISystemState = DefineInheritedClass(LGInitUISystemState, BaseLuaGameLoadState)

function LGInitUISystemState:GetLoadState()
    return LuaGameLoadState.InitUISystem
end

function LGInitUISystemState:_OnEnterState()

end

function LGInitUISystemState:_OnUpdate()
    -- todo
    DlgMgr.Init()

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.11)

    self:GoToState(LuaGameLoadState.LoadConfig)
end