---@class BaseMgr : XObject
BaseMgr = DefineClass(BaseMgr)

function BaseMgr:Init()
    self:_OnInit()
end

function BaseMgr:_OnInit()
end

function BaseMgr:Destroy()
    self:_OnDestroy();
end

function BaseMgr:_OnDestroy()
end