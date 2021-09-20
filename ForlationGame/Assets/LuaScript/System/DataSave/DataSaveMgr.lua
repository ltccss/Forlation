---@class DataSaveMgr : BaseMgr
---@field _waitForSaveArray WaitForSaveInfo[]
---@field _hasPlaned bool
---@field _rootNode DataSaveNode
---@field _stop bool
DataSaveMgr = DefineInheritedClass(DataSaveMgr, BaseMgr)

function DataSaveMgr:_OnInit()
    self._waitForSaveArray = {}
    self._hasPlaned = false

    self._rootNode = CCC(DataSaveNode, "_r", nil)
end

---@param name string
---@return DataSaveNode
function DataSaveMgr:FetchSaveNode(name)
    return self._rootNode:FetchChild(name);
end

---@param path string
---@param data any
function DataSaveMgr:OnDataDirty(path, data)
    ---@type WaitForSaveInfo
    local newData = {};
    newData.path = path;
    newData.data = data;
    table.insert(self._waitForSaveArray, newData);

    if (not self._hasPlaned) then
        self:_PlanSaveData();
    end
end

function DataSaveMgr:_PlanSaveData()
    self._hasPlaned = true;
    TimeService.Delay(0.1, self, self._SaveData);
end

function DataSaveMgr:SaveImmediately()
    self:_SaveData();
end

function DataSaveMgr:_SaveData()
    if (self._stop) then
        return;
    end

    for i = 1, #self._waitForSaveArray do
        local info = self._waitForSaveArray[i];
        if (NotNull(info.data)) then
            LocalStorage.SetItem(info.path, JsonUtil.Encode(info.data));
        else
            LocalStorage.RemoveItem(info.path);
        end
    end

    self._waitForSaveArray = {};
    self._hasPlaned = false;
end

---@param path string
function DataSaveMgr:GetDataByPath(path)
    local result = LocalStorage.GetItem(path);
    if (NotNull(result)) then
        return JsonUtil.Decode(result);
    else
        return nil;
    end
end

function DataSaveMgr:ClearAll()
    LocalStorage.ClearAll();
end

function DataSaveMgr:Stop()
    self._stop = true;
end

---@class WaitForSaveInfo
---@field path string
---@field data any