---@class DevDlg : BaseDlg
---@field _scrollView CS.UnityEngine.UI.ScrollRect
---@field _contentTrans CS.UnityEngine.RectTransform
---@field _categoryScrollView CS.UnityEngine.UI.ScrollRect
---@field _categoryContentTrans CS.UnityEngine.RectTransform
---@field _recycle CS.UnityEngine.Transform
---@field _devButtonPrefab CS.UnityEngine.GameObject
---@field _devButtonExPrefab CS.UnityEngine.GameObject
---@field _devCategoryPrefab CS.UnityEngine.GameObject
---@field _devButtonItemArray (DevButtonItemWrapper | DevButtonExItemWrapper)[]
---@field _devButtonCacheArray DevButtonItemWrapper[]
---@field _devButtonExCacheArray DevButtonExItemWrapper[]
---@field _itemTable table<string, DevButtonInfo[]>
---@field _categoryWrapperArray GMCategoryItemWrapper[]
DevDlg = DefineInheritedClass(DevDlg, BaseDlg)

local _defaultCategoryName = "默认"

local _lastCategoryName = _defaultCategoryName;

---@return string
function DevDlg:GetPrefabPath()
    return "Assets/Assets_UI/Dev/Prefabs/DevDlg.prefab"
end

---@return string
function DevDlg:GetLayerName()
    return DlgLayer.Top
end

function DevDlg:_OnInit()
    self._scrollView = self:GetUIScrollRect("ScrollView_Content");
    self._contentTrans = self._scrollView.content;

    self._categoryScrollView = self:GetUIScrollRect("ScrollView_Category")
    self._categoryContentTrans = self._categoryScrollView.content;

    self._recycle = self:GetTransform("_recycle");

    self._devButtonPrefab = LuaAssetsManager.LoadAssetWithClass("Assets/Assets_UI/Dev/Prefabs/DevButtonItem.prefab", CS.UnityEngine.GameObject)
    self._devButtonExPrefab = LuaAssetsManager.LoadAssetWithClass("Assets/Assets_UI/Dev/Prefabs/DevButtonExItem.prefab", CS.UnityEngine.GameObject);

    self._devCategoryPrefab = LuaAssetsManager.LoadAssetWithClass("Assets/Assets_UI/Dev/Prefabs/DevCategoryItem.prefab", CS.UnityEngine.GameObject)

    self._itemTable = {};
    self._devButtonItemArray = {};

    self._devButtonCacheArray = {};
    self._devButtonExCacheArray = {};

    self._categoryWrapperArray = {};

    self:_InitDevItems();

    self:_RefreshCategory();

    self:SwitchToCategory(_lastCategoryName);

    self:HoldFullScreen(true, 1)
end

function DevDlg:_OnVisible(visible)
    -- todo
end

function DevDlg:_OnDestroy()
    for i = 1, #self._devButtonItemArray do
        self._devButtonItemArray[i]:DestroyWithoutView();
    end

    for i = 1, #self._devButtonCacheArray do
        self._devButtonCacheArray[i]:DestroyWithoutView()
    end

    for i = 1, #self._devButtonExCacheArray do
        self._devButtonExCacheArray[i]:DestroyWithoutView()
    end

    for i = 1, #self._categoryWrapperArray do
        self._categoryWrapperArray[i]:DestroyWithoutView()
    end
end

---@param category string
---@param name string
---@param callback fun()
function DevDlg:_AddDevButton(category, name, callback)
    if (IsStringEmptyOrNull(category)) then
        category = _defaultCategoryName;
    end

    local array = self._itemTable[category];
    if (IsNull(array)) then
        array = {};
        self._itemTable[category] = array;
    end

    ---@type DevButtonInfo
    local info = {};
    info.name = name;
    info.func = callback;
    info.type = DevButtonType.normal;

    table.insert(array, info)
end

---@param name string
---@param callback fun()
---@param index number
---@return DevButtonItemWrapper
function DevDlg:_FetchDevButton(name, callback, index)
    ---@type DevButtonItemWrapper
    local wrapper;
    if (#self._devButtonCacheArray > 0) then
        wrapper = self._devButtonCacheArray[#self._devButtonCacheArray];
        table.remove(self._devButtonCacheArray, #self._devButtonCacheArray);
        wrapper:GetXRoot().transform:SetParent(self._contentTrans);
    else
        local newGo = CS.UnityEngine.GameObject.Instantiate(self._devButtonPrefab, self._contentTrans);
        wrapper = CCC(DevButtonItemWrapper);
        wrapper:Init(newGo);
    end

    wrapper:Reset(name, callback, index);
    table.insert(self._devButtonItemArray, wrapper);

    return wrapper;
end

---@param category string
---@param name string
---@param callback fun(param:string)
function DevDlg:_AddDevButtonEx(category, name, callback)
    if (IsStringEmptyOrNull(category)) then
        category = _defaultCategoryName;
    end

    local array = self._itemTable[category];
    if (IsNull(array)) then
        array = {};
        self._itemTable[category] = array;
    end

    ---@type DevButtonInfo
    local info = {};
    info.name = name;
    info.func = callback;
    info.type = DevButtonType.cmdInput;

    table.insert(array, info)
end

---@param name string
---@param callback fun(param:string)
---@param index number
---@return DevButtonExItemWrapper
function DevDlg:_FetchDevButtonEx(name, callback, index)
    ---@type DevButtonExItemWrapper
    local wrapper;
    if (#self._devButtonExCacheArray > 0) then
        wrapper = self._devButtonExCacheArray[#self._devButtonExCacheArray];
        table.remove(self._devButtonExCacheArray, #self._devButtonExCacheArray);
        wrapper:GetXRoot().transform:SetParent(self._contentTrans);
    else
        local newGo = CS.UnityEngine.GameObject.Instantiate(self._devButtonExPrefab, self._contentTrans);
        wrapper = CCC(DevButtonExItemWrapper);
        wrapper:Init(newGo);
    end

    wrapper:Reset(name, callback, index);
    table.insert(self._devButtonItemArray, wrapper);

    return wrapper;
end

function DevDlg:_InitDevItems()

    self:_AddDevButton("", "热重启", function()
        luaGameManager:ReloadGame();
    end);

    self:_AddDevButton("", "全屏遮蔽测试", function()
        self:Hide()
        self:Delay(3, function() 
            self:Show()
        end)
    end);

    self:_AddDevButton("诊断", "显示泄露的Wrapper", function()
        BaseViewWrapper.DebugShowLeakedWrappers();
    end)

    self:_AddDevButton("诊断", "显示所有非Class的全局定义", function()
        DebugShowNonClassGlobalDefines();
    end)

    self:_AddDevButton("诊断", "统计当前类实例数", function()
        DebugShowClassInstanceStats()
    end)

    self:_AddDevButtonEx("诊断", "显示指定类的实例堆栈", function(param)
        local className = param
        local class = GetClassByName(className)
        if (class) then
            DebugShowClassInstanceTrace(class)
        else
            LogUtil.Error("输入的类名无效")
        end
    end)

end

---@param categoryName string
function DevDlg:SwitchToCategory(categoryName)
    -- 回收已经存在的item
    for i = 1, #self._devButtonItemArray do
        local wrapper = self._devButtonItemArray[i];
        if (IsInheritedFrom(wrapper, DevButtonItemWrapper)) then
            table.insert(self._devButtonCacheArray, wrapper);
        else
            table.insert(self._devButtonExCacheArray, wrapper);
        end
        wrapper:GetXRoot().transform:SetParent(self._recycle);
    end

    self._devButtonItemArray = {};

    local array = self._itemTable[categoryName];

    for i = 1, #array do
        local info = array[i];
        if info.type == DevButtonType.normal then
            self:_FetchDevButton(info.name, info.func, i)
        else
            self:_FetchDevButtonEx(info.name, info.func, i);
        end
    end

    for i = 1, #self._categoryWrapperArray do
        local wrapperCategoryName = self._categoryWrapperArray[i]:GetCategoryName();
        self._categoryWrapperArray[i]:SetHighlight(wrapperCategoryName == categoryName);
    end

    _lastCategoryName = categoryName;
end

function DevDlg:_RefreshCategory()
    local categoryArray = {};
    for name, v in pairs(self._itemTable) do
        table.insert(categoryArray, name);
    end

    -- 将默认目录置顶
    table.sort(categoryArray, function(a, b)
        if (a == _defaultCategoryName) then
            return true
        end
        if (b == _defaultCategoryName) then
            return false
        end
        return CS.System.String.Compare(a, b) < 0
    end)


    for i = 1, #categoryArray do
        local go = CS.UnityEngine.GameObject.Instantiate(self._devCategoryPrefab, self._categoryContentTrans)
        local wrapper = CCC(DevCategoryItemWrapper);
        wrapper:Init(go);
        wrapper:ResetCategoryName(categoryArray[i])
        table.insert(self._categoryWrapperArray, wrapper)
    end
end


DevButtonType = {
    normal = 0,
    cmdInput = 1,
}

---@class DevButtonInfo
---@field name string
---@field func function
---@field type DevButtonType
