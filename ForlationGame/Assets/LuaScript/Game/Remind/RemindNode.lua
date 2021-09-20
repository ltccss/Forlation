---@class RemindNode : XObject
---@field _selfVisible boolean
---@field _finalVisible boolean
---@field _remindType RemindType
---@field _childArray RemindNode[]
---@field _parent RemindNode
---@field _showingObject CS.UnityEngine.GameObject
---@field _refreshFuncCallee any
---@field _refreshFunc fun(visible:boolean)
RemindNode = DefineClass(RemindNode)

---@param remindtype RemindType
function RemindNode:_Ctor(remindtype)
    self._remindType = remindtype;
    self._childArray = {};
    self._selfVisible = false;
    self._finalVisible = false;
end

---@return RemindType
function RemindNode:GetRemindType()
    return self._remindType
end

---@param remindNode RemindNode
function RemindNode:AddChild(remindNode)
    -- 懒得查重了
    table.insert(self._childArray, remindNode);
    remindNode:SetParent(self);
    
    self:UpdateFinalVisible();
end

---@param parentRemindNode RemindeNode
function RemindNode:SetParent(parentRemindNode)
    self._parent = parentRemindNode;
end

function RemindNode:UpdateFinalVisible()
    local visible = self._selfVisible;
    for i = 1, #self._childArray do
        visible = visible or self._childArray[i]:GetFinalVisible()
    end

    self._finalVisible = visible;

    if (NotNull(self._parent)) then
        self._parent:UpdateFinalVisible();
    end

    self:_Refresh();
end

---@param visible boolean
function RemindNode:SetSelfVisible(visible)
    self._selfVisible = visible;
    self:UpdateFinalVisible();
end

---@return boolean
function RemindNode:GetSelfVisible()
    return self._selfVisible;
end

---@return boolean
function RemindNode:GetFinalVisible()
    return self._finalVisible;
end

function RemindNode:_Refresh()
    if (NotNull(self._showingObject)) then
        self._showingObject:SetActive(self._finalVisible);
    end

    if (NotNull(self._refreshFunc)) then
        FuncCall(self._refreshFuncCallee, self._refreshFunc, self._finalVisible)
    end
end

---@param obj CS.UnityEngine.GameObject
function RemindNode:SetShowingObject(obj)
    self._showingObject = obj;

    self:_Refresh();
end

---@param callee any
---@param func fun(visible:boolean)
function RemindNode:SetRefreshFunc(callee, func)
    self._refreshFuncCallee = callee;
    self._refreshFunc = func;

    self:_Refresh();
end