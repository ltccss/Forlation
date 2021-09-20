---@class LGLoadConfigState : BaseLuaGameLoadState
LGLoadConfigState = DefineInheritedClass(LGLoadConfigState, BaseLuaGameLoadState)

function LGLoadConfigState:GetLoadState()
    return LuaGameLoadState.LoadConfig
end

function LGLoadConfigState:_OnEnterState()

end

function LGLoadConfigState:_OnUpdate()

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.72)

    self:GoToState(LuaGameLoadState.InitMgrs)
end