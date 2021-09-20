---@class BaseLuaGameLoadState : XObject
---@field protected _gameLoadMgr LuaGameLoadMgr
BaseLuaGameLoadState = DefineClass(BaseLuaGameLoadState)

---@param gameLoadMgr LuaGameLoadMgr
function BaseLuaGameLoadState:_Ctor(gameLoadMgr)
    self._gameLoadMgr = gameLoadMgr
end

function BaseLuaGameLoadState:Update()
    self:_OnUpdate()
end

function BaseLuaGameLoadState:_OnUpdate()
end

---@param stateName string @LuaGameLoadState
function BaseLuaGameLoadState:GoToState(stateName)
    self._gameLoadMgr:EnterState(stateName)
end

function BaseLuaGameLoadState:Complete()
    self._gameLoadMgr:Complete()
end

function BaseLuaGameLoadState:NotifyEnterState()
    self:_OnEnterState()
end

function BaseLuaGameLoadState:_OnEnterState()
end

---@return string @返回LuaGameLoadState枚举类型
function BaseLuaGameLoadState:GetLoadState()
end

---@return string
function BaseLuaGameLoadState:GetDisplayedStateName()
    return self._gameLoadMgr:GetDisplayedStateName(self:GetLoadState())
end