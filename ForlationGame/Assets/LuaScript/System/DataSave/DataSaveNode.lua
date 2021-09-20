---@class DataSaveNode : XObject
---@field _parent DataSaveNode
---@field _fullPath string
---@field _data any
---@field _childTable table<string, DataSaveNode>
DataSaveNode = DefineClass(DataSaveNode)

---@param fullPath string
---@param parent DataSaveNode
function DataSaveNode:_Ctor(fullPath, parent)
    self._fullPath = fullPath;
    self._parent = parent;

    self._childTable = {};

    self._data = FF.DataSaveMgr:GetDataByPath(self._fullPath);
    if (IsNull(self._data)) then
        self._data = {};
    end
end

---@return bool 是否是空的
function DataSaveNode:IsEmpty()
    for k in pairs(self._data) do
        return false;
    end
    return true;
end

---@return string 这个节点的全路径
function DataSaveNode:GetPath()
    return self._fullPath;
end

---@param childKey string
---@return DataSaveNode
--- 如果孩子节点本身存有数据，就会返回一个带着数据的孩子节点，不然就新建一个
function DataSaveNode:FetchChild(childKey)
    if (NotNull(self._childTable[childKey])) then
        return self._childTable[childKey];
    end
    local childPath = self:_MakeChildPath(childKey);
    local child = CCC(DataSaveNode, childPath, self);
    self._childTable[childKey] = child;
    return child;
end

---@param childKey string
---@return bool
--- 查看是否有无这个孩子节点的数据
function DataSaveNode:HasChildData(childKey)
    local childPath = self._MakeChildPath(childKey);
    return NotNull(FF.DataSaveMgr:GetDataByPath(childPath));
end

function DataSaveNode:_MakeChildPath(childKey)
    return self._fullPath ..  "." .. childKey
end

---@param key string
function DataSaveNode:SetValue(key, value)
    self._data[key] = value;
    -- 通知管理类，这个数据更新了
    FF.DataSaveMgr:OnDataDirty(self._fullPath, self._data);
end

---@param key string
---@param defaultValue any
---@return any
function DataSaveNode:GetValue(key, defaultValue)
    local v = self._data[key];
    if (IsNull(v)) then
        return defaultValue;
    else
        return v;
    end
end

--- 清除该节点的数据
function DataSaveNode:CleatData()
    self._data = {};
    FF.DataSaveMgr:OnDataDirty(self._fullPath, nil);
end



