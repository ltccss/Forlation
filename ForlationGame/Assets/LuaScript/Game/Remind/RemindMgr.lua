---@class RemindMgr : BaseMgr
---@field _remindTable table<RemindType, RemindNode>
RemindMgr = DefineInheritedClass(RemindMgr, BaseMgr)


function RemindMgr:_OnInit()
    self._remindTable = {}

    self:_BuildRemindTree();
end

function RemindMgr:_BuildRemindTree()
    self:_AddRemind(nil, RemindType.TestPlayer)
    self:_AddRemind(RemindType.TestPlayer, RemindType.TestPlayerSub)
end

---@param parentRemindType RemindType
---@param remindType RemindType
function RemindMgr:_AddRemind(parentRemindType, remindType)
    if (IsNull(self._remindTable[remindType])) then
        local remindNode = CCC(RemindNode);
        self._remindTable[remindType] = remindNode

        if (NotNull(parentRemindType)) then
            local parentRemindNode = self._remindTable[parentRemindType];
            if (IsNull(parentRemindNode)) then
                LogUtil.Error("需要先创建父节点")
            end

            parentRemindNode:AddChild(remindNode);
        end
    end
end

---@param remindType RemindType
---@param visible boolean
function RemindMgr:ToggleRemind(remindType, visible)
    local node = self._remindTable[remindType];
    if (IsNull(node)) then
        LogUtil.Error("remindType not found: " .. remindType)
    end
    node:SetSelfVisible(visible);
end

---@param remindType RemindType
---@param obj CS.UnityEngine.GameObject
function RemindMgr:RegisterRemindObject(remindType, obj)
    local node = self._remindTable[remindType];
    if (IsNull(node)) then
        LogUtil.Error("remindType not found: " .. remindType)
    end
    node:SetShowingObject(obj);
end

---@param remindType RemindType
function RemindMgr:ClearRemindObject(remindType)
    local node = self._remindTable[remindType];
    if (NotNull(node)) then
        node:SetShowingObject(nil);
    end
    
end

---@param remindType RemindType
---@param callee any
---@param func fun(visible:boolean)
function RemindMgr:RegisterRefreshFunc(remindType, callee, func)
    local node = self._remindTable[remindType];
    if (IsNull(node)) then
        LogUtil.Error("remindType not found: " .. remindType)
    end
    node:SetRefreshFunc(callee, func);
end

---@param remindType RemindType
function RemindMgr:ClearRefreshFunc(remindType)
    local node = self._remindTable[remindType];
    if (NotNull(node)) then
        node:SetRefreshFunc(nil, nil);
    end
end