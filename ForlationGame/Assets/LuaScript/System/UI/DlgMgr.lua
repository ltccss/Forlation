---@class DlgLayer
DlgLayer = 
{
    Bottomest = "Bottomest",
    Bottom = "Bottom",
    Middle = "Middle",
    Top = "Top",
    Toppest = "Toppest"
}

---@class DlgLayerOrder
DlgLayerOrder = 
{
    Bottomest = 1,
    Bottom = 2,
    Middle = 3,
    Top = 4,
    Toppest = 5
}

---@class DlgMgr
---@field Event_DlgClose LuaDelegate @fun(dlg:BaseDlg)
---@field Event_DlgWillClose LuaDelegate @fun(dlg:BaseDlg)
DlgMgr = DefineClass(DlgMgr)

DlgMgr.Event_DlgClose = CCC(LuaDelegate);
DlgMgr.Event_DlgWillClose = CCC(LuaDelegate);
--- 页面系统全屏状态发生了变更，既所有dlg都非全屏，或者有dlg处于全屏状态， 参数：是否全屏
DlgMgr.Event_FullScreenChanged = CCC(LuaDelegate);

-- Dlg之间的sortingOrder间隔,也就是说，某个Dlg可以占用的sortingOrder为20
local _SortingOrderInterval = 20;

---@type table<string, CS.UnityEngine.RectTransform>
local _layerDict = {}

---@type CS.UnityEngine.RectTransform
local _root = nil;

local _uiEnable = false;

---@type table<DlgClass, BaseDlg[]>
local _existedDlgDict = {}

--- 逻辑上显示的页面数组
---@type BaseDlg[]
local _showedDlgArray = {}

---@type TimerHandle
local _sortTimer = nil;

local _isFullScreen = false;

function DlgMgr.Init()
    -- uiroot直接做成prefab了，上面的组件不想在代码里添加了，反正出问题了代码里也好改
    local prefab = CS.UnityEngine.Resources.Load("Prefabs/UISystem/UIRoot", typeof(CS.UnityEngine.GameObject))
    ---@type CS.UnityEngine.GameObject
    local go = CS.UnityEngine.GameObject.Instantiate(prefab);

    CS.UnityEngine.GameObject.DontDestroyOnLoad(go)
    _root = go.transform

    DlgMgr._AddLayer(DlgLayer.Bottomest)
    DlgMgr._AddLayer(DlgLayer.Bottom)
    DlgMgr._AddLayer(DlgLayer.Middle)
    DlgMgr._AddLayer(DlgLayer.Top)
    DlgMgr._AddLayer(DlgLayer.Toppest)

    _uiEnable = true;
end

function DlgMgr.Destroy()
    _uiEnable = false;

    _sortTimer = TimeService.ClearTimer(_sortTimer);

    DlgMgr.Event_DlgClose:Clear();
    DlgMgr.Event_DlgWillClose:Clear();

    for k, dlgArray in pairs(_existedDlgDict) do
        for i = 1, #dlgArray do
            dlgArray[i]:Destroy()
        end
    end

    if (NotNull(_root)) then
        CS.UnityEngine.GameObject.Destroy(_root.gameObject);
    end
    _layerDict = {};
    _existedDlgDict = {};
    _showedDlgArray = {};
    _root = nil;
end

--- 页面系统是否可用，
--- 不可用的时候别创建新页面
function DlgMgr.IsEnable()
    return _uiEnable;
end

---@param layerName string
function DlgMgr._AddLayer(layerName, sortingOrder)
    if IsNull(_layerDict[layerName]) then
        local go = CS.UnityEngine.GameObject(layerName, typeof(CS.UnityEngine.RectTransform))
        CS.UnityEngine.GameObject.DontDestroyOnLoad(go)
        ---@type CS.UnityEngine.RectTransform
        local rectTrans = go.transform
        rectTrans:SetParent(_root)
        rectTrans.anchorMin = CS.UnityEngine.Vector2(0, 0)
        rectTrans.anchorMax = CS.UnityEngine.Vector2(1, 1)
        rectTrans.offsetMin = CS.UnityEngine.Vector2.zero
        rectTrans.offsetMax = CS.UnityEngine.Vector2.zero
        rectTrans.localScale = CS.UnityEngine.Vector3.one

        _layerDict[layerName] = rectTrans
    end
end

---@param layerName string
---@return CS.UnityEngine.RectTransform
function DlgMgr._GetLayer(layerName)
    return _layerDict[layerName]
end

---@generic T
---@param dlgClass T
---@return T
---获取已经存在的Dlg
function DlgMgr._GetExistedDlg(dlgClass)
    local dlgArray = _existedDlgDict[dlgClass]
    if (dlgArray and #dlgArray > 0) then
        return dlgArray[1]
    else
        return nil
    end
end

---@generic T
---@param dlgClass T
---@param dlg BaseDlg
function DlgMgr._AddExistedDlg(dlgClass, dlg)
    local dlgArray = _existedDlgDict[dlgClass]

    if (not dlgArray) then
        dlgArray = {}
        _existedDlgDict[dlgClass] = dlgArray
    end

    table.insert(dlgArray, dlg)
end

---@param dlg BaseDlg
function DlgMgr._RemoveExistedDlg(dlg)
    local dlgClass = getmetatable(dlg)

    local dlgArray = _existedDlgDict[dlgClass]

    if (dlgArray and #dlgArray > 0) then
        for i = 1, #dlgArray do
            if (dlgArray[i] == dlg) then
                table.remove(dlgArray, i)
                break
            end
        end
    end
end

--- 获取一个Dlg并展示出来，如果没有就会创建一个出来
---@generic T
---@param dlgClass T
---@return T
function DlgMgr.FetchDlg(dlgClass)
    ---@type BaseDlg
    local dlg = nil

    dlg = DlgMgr._GetExistedDlg(dlgClass)

    if (not dlg) then
        dlg = DlgMgr.CreateDlg(dlgClass)
    else
        dlg:Show()
    end

    return dlg
end

--- 创建一个Dlg并展示出来，不管当前有无现存Dlg
---@generic T
---@param dlgClass T
---@return T
function DlgMgr.CreateDlg(dlgClass)
    if (not DlgMgr.IsEnable()) then
        LogUtil.Error('不要在页面系统销毁阶段创建新的页面, 如有需要请添加DlgMgr.IsEnable()判断OnDestroy时页面系统是否正在销毁')
    end
    local layer = DlgMgr._GetLayer(dlgClass:GetLayerName())
    if (layer == nil) then
        return nil;
    end
    ---@type BaseDlg
    local dlg = CCC(dlgClass)

    -- 根据dlg的信息，加载对应的prefab
    local prefabPath = dlg:GetPrefabPath()
    if (not prefabPath) then
        LogUtil.Error('请实现一下GetPrefabPath()方法')
    end

    ---@type CS.UnityEngine.GameObject
    local dlgPrefab = nil
    if (dlg:IsInPackage()) then
        dlgPrefab = CS.UnityEngine.Resources.Load(prefabPath, typeof(CS.UnityEngine.GameObject))
    else
        dlgPrefab = LuaAssetsManager.LoadAssetWithClass(prefabPath, CS.UnityEngine.GameObject)
    end

    ---@type CS.UnityEngine.GameObject
    local dlgRoot = CS.UnityEngine.GameObject.Instantiate(dlgPrefab, layer)

    dlgRoot.transform.localScale = CS.UnityEngine.Vector3.one
    dlgRoot.transform.localPosition = CS.UnityEngine.Vector3.zero

    DlgMgr._AddExistedDlg(dlgClass, dlg)
    dlg:Init(dlgRoot)
    dlg:Show()

    return dlg
end

--- 关闭一个Dlg
---@param dlg BaseDlg
function DlgMgr.CloseDlg(dlg)
    dlg:Close()
end

function DlgMgr.CloseAllDlgs()
    for _, v in pairs(_existedDlgDict) do
        for _, i in ipairs(v) do
            DlgMgr.CloseDlg(i);
        end
    end
    _existedDlgDict = {}
end

function DlgMgr.HideAllDlgs()
    for _, v in pairs(_existedDlgDict) do
        for _, i in ipairs(v) do
            i:Hide();
        end
    end
end
---@param dlg BaseDlg
function DlgMgr.OnDlgWillClose(dlg)
    DlgMgr._RemoveExistedDlg(dlg);
    DlgMgr.Event_DlgWillClose:Execute(dlg);
end

---@generic T
---@param dlgClass T
---@return T
function DlgMgr.GetDlg(dlgClass)
    return DlgMgr._GetExistedDlg(dlgClass)
end

function DlgMgr.OnDlgShow(dlg)
    for i = 1, #_showedDlgArray do
        if (_showedDlgArray[i] == dlg) then
            return;
        end
    end

    table.insert(_showedDlgArray, dlg);
end

function DlgMgr.OnDlgHide(dlg)
    for i = 1, #_showedDlgArray do
        if (_showedDlgArray[i] == dlg) then
            table.remove(_showedDlgArray, i)
            break;
        end
    end
end

function DlgMgr.UpdateFullScreenState()
    local hasFullScreenDlg = false;

    -- 一般情况下，_showedDlgArray都是按照DlgSortingOrder从小到大排好序的，直接用
    for i = #_showedDlgArray, 1, -1 do
        if (hasFullScreenDlg) then
            _showedDlgArray[i]:SetUnderFullScreen(true)
        else
            if (_showedDlgArray[i]:IsFullScreen() and _showedDlgArray[i]:IsVisible()) then
                hasFullScreenDlg = true;
            end
            _showedDlgArray[i]:SetUnderFullScreen(false)
        end
    end

    DlgMgr.Event_FullScreenChanged:Execute(hasFullScreenDlg)
end

---@param dlg1 BaseDlg
---@param dlg2 BaseDlg
local function _CompareShowedDlg(dlg1, dlg2)
    return (DlgLayerOrder[dlg1:GetLayerName()] * 100000 + dlg1:GetXRoot().transform:GetSiblingIndex()) < (DlgLayerOrder[dlg2:GetLayerName()] * 100000 + dlg2:GetXRoot().transform:GetSiblingIndex())
end

local function _SortDelay(_)
    table.sort(_showedDlgArray, _CompareShowedDlg)
    local order = 1
    for i = 1, #_showedDlgArray do
        if (_showedDlgArray[i]:IsVisible()) then
            _showedDlgArray[i]:SetDlgSortingOder(order);
            order = order + 1
        end
    end
end

function DlgMgr.ResortDlg()
    TimeService.WaitNextLateUpdate(nil, _SortDelay);
end

---@return number @获取当前显示范围的逻辑宽度（）
function DlgMgr.GetLogicWidth()
    return _root.rect.width;
end

---@return number @获取当前显示范围的逻辑高度
function DlgMgr.GetLogicHeight()
    return _root.rect.height;
end

--- 获取逻辑上显示的页面，
--- 若要获取实际上显示的页面，请使用dlg:IsVisible()过滤返回结果
---@return BaseDlg[]
function DlgMgr.GetShowedDlgArray()
    return _showedDlgArray;
end

function DlgMgr.GetSortingOrderInterval()
    return _SortingOrderInterval;
end