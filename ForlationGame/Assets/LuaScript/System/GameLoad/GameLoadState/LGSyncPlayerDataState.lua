---@class LGSyncPlayerDataState : BaseLuaGameLoadState
LGSyncPlayerDataState = DefineInheritedClass(LGSyncPlayerDataState, BaseLuaGameLoadState)

function LGSyncPlayerDataState:GetLoadState()
    return LuaGameLoadState.SyncPlayerData
end

function LGSyncPlayerDataState:_OnEnterState()

end

function LGSyncPlayerDataState:_OnUpdate()
    -- todo
    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.9)
end