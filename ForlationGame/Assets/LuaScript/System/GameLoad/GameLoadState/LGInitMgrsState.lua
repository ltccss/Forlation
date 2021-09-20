---@class LGInitMgrsState : BaseLuaGameLoadState
LGInitMgrsState = DefineInheritedClass(LGInitMgrsState, BaseLuaGameLoadState)


function LGInitMgrsState:GetLoadState()
    return LuaGameLoadState.InitMgrs
end

function LGInitMgrsState:_OnEnterState()
    LuaCoroutineUtil.StartCoroutine(self, self._InitCo);
end

function LGInitMgrsState:_OnUpdate()

    
end

function LGInitMgrsState:_InitCo()

    math.randomseed(123456789 * math.sin(os.time()))

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.80)

    CS.AssetsManager.Init();

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.85)

    coroutine.yield(0)
    
    require("FF/FF")

    FF_InitCo()

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.93)

    coroutine.yield(0)

    -- CS.AssetsManager.Me:SetLongTermAliveBundleList({});

    CS.ForlationGameManager.Me:ReportLoadingProgress(self:GetDisplayedStateName(), 0.99)

    self:Complete()
end