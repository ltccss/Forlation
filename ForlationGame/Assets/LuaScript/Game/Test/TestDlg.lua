---@class TestDlg : BaseDlg
---@field _titleText CS.UnityEngine.UI.Text
---@field _contentText CS.UnityEngine.UI.Text
---@field _wrapTableView CS.WrapTableView
---@field _scrollRect CS.UnityEngine.UI.ScrollRect
---@field _cardWrapperTable table<string, TestCardItemWrapper>
---@field _cardUpdateFunc fun(dataIndex:number, obj:CS.UnityEngine.GameObject)
---@field _cardDataArray TestCardData[]
TestDlg = DefineInheritedClass(TestDlg, BaseDlg)

function TestDlg:GetPrefabPath()
    return "Assets/Assets_UI/Test/Prefabs/TestDlg.prefab"
end

function TestDlg:_OnInit()
    -- self:BecomeFullScreen(true, 1);

    self._titleText = self:GetUIText("Text_Title")
    self._contentText = self:GetUIText("Text_Content")

    self._titleText.text = "随便选点啥"

    self._contentText.text = "阿巴阿巴阿巴"

    -- 随便生成一堆数据
    self._cardDataArray = {};
    local nameArray = {
        "ヾ(･ω･`｡)", 
        "ԅ(¯﹃¯ԅ)", 
        "o(￣ヘ￣o＃)", 
        "o(￣▽￣)d", 
        "o(≧口≦)o",
        "Ψ(￣∀￣)Ψ",
        "ヽ(✿ﾟ▽ﾟ)ノ",
        "ლ(⌒▽⌒ლ)",
    };

    for i = 1, 100 do
        ---@type TestCardData
        local data = {};
        data.name = nameArray[math.random(1, #nameArray)]
        data.star = math.random(3, 9);
        table.insert(self._cardDataArray, data);
    end

    self._wrapTableView = self:GetComponentWithClass("ScrollView", CS.WrapTableView);
    self._scrollRect = self:GetUIScrollRect("ScrollView");

    self._cardUpdateFunc = function(dataIndex, obj)
        self:_OnUpdateItem(dataIndex, obj);
    end

    self._wrapTableView.OnUpdateCell = AddToCsDelegate(self._wrapTableView.OnUpdateCell, self._cardUpdateFunc);

    self._cardWrapperTable = {};

    self:_Refresh();

    self:_ListenUpdateLifeCycleEvent()
    self:_ListenLateUpdateLifeCycleEvent()
    self:_ListenFixedUpdateLifeCycleEvent()
    self:_ListenCommonLifeCycleEvent()
    self:_ListenApplicationPauseLifeCycleEvent()

    self:Delay(0.5, function()
        self:_UnlistenUpdateLifeCycleEvent();
        self:_UnlistenLateUpdateLifeCycleEvent();
        self:_UnlistenFixedUpdateLifeCycleEvent();
        self:_UnlistenCommonLifeCycleEvent();
        self:_UnlistenApplicationPauseLifeCycleEvent();
    end)
end

function TestDlg:_Refresh()

    -- 前后各需要2个item站位
    self._wrapTableView.ChildCount = #self._cardDataArray + 4;
end

---@param dataIndex number
---@param obj CS.UnityEngine.GameObject
function TestDlg:_OnUpdateItem(dataIndex, obj)
    local objId = obj:GetInstanceID();

    local wrapper = self._cardWrapperTable[objId];

    if (not wrapper) then
        wrapper = CCC(TestCardItemWrapper);
        wrapper:Init(obj);
        self._cardWrapperTable[objId] = wrapper;
    end

    wrapper:ResetData(self._cardDataArray[dataIndex + 1 - 2], dataIndex + 1 - 2);
end

function TestDlg:_TestTimer()
    self._contentText.text = "阿巴阿巴阿巴？？？  " .. TimeUtil.GetCurrentTime()
    LogUtil.Log("test timer")
end

function TestDlg:_OnDestroy()
    for k in pairs(self._cardWrapperTable) do
        self._cardWrapperTable[k]:Destroy();
        self._cardWrapperTable[k] = nil;
    end

    self._wrapTableView.OnUpdateCell = RemoveFromCsDelegate(self._wrapTableView.OnUpdateCell, self._cardUpdateFunc)
end

function TestDlg:_OnWillClose()
    -- self:BecomeFullScreen(false);
end

function TestDlg:_OnCsAwake()
    LogUtil.Log("_OnCsAwake")
end

function TestDlg:_OnCsStart()
    LogUtil.Log("_OnCsStart")
end

function TestDlg:_OnCsEnable()
    LogUtil.Log("_OnCsEnable")
end

function TestDlg:_OnCsDisable()
    LogUtil.Log("_OnCsDisable")
end

---@param pause bool
function TestDlg:_OnCsApplicationPause(pause)
    LogUtil.Log("_OnCsApplicationPause " .. tostring(pause))
end

function TestDlg:_OnCsUpdate()
    LogUtil.Log("_OnCsUpdate")
end

function TestDlg:_OnCsLateUpdate()
    LogUtil.Log("_OnCsLateUpdate")
end

function TestDlg:_OnCsFixedUpdate()
    LogUtil.Log("_OnCsFixedUpdate")
end

---@class TestCardData
---@field name string
---@field star number