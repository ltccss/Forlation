-- 检查更新，把资源更新到指定版本

-- 初始化UI系统

-- 初始化各种mgr类，统一加载配置

-- 拉取玩家数据

-- 切到正式场景

-- 打开主界面

LuaGameLoadState = 
{
    --- 初始化UI系统
    InitUISystem = "InitUISystem",
    --- 加载配置
    LoadConfig = "LoadConfig",
    --- 初始化各种mgr类
    InitMgrs = "InitMgrs",
    --- 拉取玩家数据
    SyncPlayerData = "SyncPlayerData",

}

---@class LuaGameLoadMgr
---@field _state BaseLuaGameLoadState
---@field _stateTable table<string, BaseLuaGameLoadState>
---@field _finishCallback function
---@field _isFinish bool
---@field _displayedStateNameTable table<string, string>
---@field _pause bool
LuaGameLoadMgr = DefineClass(LuaGameLoadMgr)

---@param finishCallback function
function LuaGameLoadMgr:Init(finishCallback)

    self._pause = false;

    self._finishCallback = finishCallback
    self._isFinish = false

    self._stateTable = {}

    self._stateTable[LuaGameLoadState.InitUISystem] = CCC(LGInitUISystemState, self)
    self._stateTable[LuaGameLoadState.LoadConfig] = CCC(LGLoadConfigState, self)
    self._stateTable[LuaGameLoadState.InitMgrs] = CCC(LGInitMgrsState, self)
    self._stateTable[LuaGameLoadState.SyncPlayerData] = CCC(LGSyncPlayerDataState, self)

    self._displayedStateNameTable = {}
    self._displayedStateNameTable[LuaGameLoadState.InitUISystem] = "Initing UI system..."
    self._displayedStateNameTable[LuaGameLoadState.LoadConfig] = "Loading configs..."
    self._displayedStateNameTable[LuaGameLoadState.InitMgrs] = "Initing game system..."
    self._displayedStateNameTable[LuaGameLoadState.SyncPlayerData] = "Fetching user data..."

    -- Reporter插件
    local reportGo = CS.UnityEngine.GameObject.Find("Reporter")
    if (CS.AppSetting.runningMode == "debug" and CS.AppSetting.platform ~= "editor") then
        if (IsNull(reportGo)) then
            ---@type CS.UnityEngine.GameObject
            local reportPrefab = CS.UnityEngine.Resources.Load("Prefabs/Reporter");
            reportGo = CS.UnityEngine.GameObject.Instantiate(reportPrefab);
            reportGo.name = "Reporter";
            CS.UnityEngine.GameObject.DontDestroyOnLoad(reportGo);
        end
        reportGo:SetActive(true)
    else
        if (NotNull(reportGo)) then
            reportGo:SetActive(false)
        end
    end

    self:EnterState(LuaGameLoadState.InitUISystem)
end

function LuaGameLoadMgr:Update()
    if (self._state and not self._pause) then
        self._state:Update()
    end
end

function LuaGameLoadMgr:PauseUpdate()
    self._pause = true;
end

function LuaGameLoadMgr:IsComplete()
    return self._isFinish
end

function LuaGameLoadMgr:Complete()
    self._isFinish = true
    self._state = nil
    CS.ForlationGameManager.Me:ShowloadingProgress(false);
    if (self._finishCallback) then
        FuncCall(nil, self._finishCallback)
        self._finishCallback = nil
    end
end

---@param stateName string @LuaGameLoadState
function LuaGameLoadMgr:EnterState(stateName)
    if (self._stateTable[stateName]) then
        self._state = self._stateTable[stateName]
        LogUtil.Log('lua go to load state: ' .. stateName);
        self._state:NotifyEnterState()
    else
        LogUtil.Error('loading state not found: ' .. stateName)
    end
end


---@param state string @LuaGameLoadState
function LuaGameLoadMgr:GetDisplayedStateName(state)
    local s = self._displayedStateNameTable[state]
    return s
end
