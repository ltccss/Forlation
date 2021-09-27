---@class LuaGameManager
---@field _gameLoadMgr LuaGameLoadMgr
---@field Event_AnyKeyPressed       LuaDelegate
LuaGameManager = DefineClass(LuaGameManager)

local _reload = false;
local _waitReload = false;

function LuaGameManager:Init()
    TimeUtil.Init()
    TimeService.Init()

    -- 运行期间禁止自动熄屏
    CS.UnityEngine.Screen.sleepTimeout = -1;

    self._gameLoadMgr = CCC(LuaGameLoadMgr)

    self._gameLoadMgr:Init(function()
        DlgMgr.FetchDlg(TestDlg);
        
        if (CS.AppSetting.runningMode == "debug") then
            DlgMgr.FetchDlg(DevMenuDlg)
        end
    end)

    self.Event_AnyKeyPressed = CCC(LuaDelegate);
end

function LuaGameManager.Me()
    return luaGameManager
end

function LuaGameManager:Update()

    if (not self._gameLoadMgr:IsComplete()) then
        self._gameLoadMgr:Update()
    end

    if (not _reload) then
        TimeService.Update()
    end

    if (_waitReload) then
        _waitReload = false;
        ForceUnlinkAllAliveRefs();
        CS.CommonUtil.GC();
        CS.ForlationGameManager.Me:ReturnLaunchScene();
    end
end

function LuaGameManager:LateUpdate()
    if (not _reload) then
        TimeService.LateUpdate()
    end
end

---@param pause bool @true=暂停，false=恢复
function LuaGameManager:OnGamePause(pause)
    print("pause : " .. tostring(pause))
    FF.DataSaveMgr:SaveImmediately();
end

function LuaGameManager:OnGameQuit()
    print("game quit")
    FF.DataSaveMgr:SaveImmediately();
end

function LuaGameManager:ReloadGame()
    LogUtil.Warn("========== Reload Game ==========")
    FF.DataSaveMgr:SaveImmediately();
    DlgMgr.Destroy();
    BaseViewWrapper.ForceReleaseWrappers();
    FF_Destroy();
    _reload = true;
    _waitReload = true;
end

function LuaGameManager:OnAnyKeyPressed()

    self.Event_AnyKeyPressed:Execute();
end