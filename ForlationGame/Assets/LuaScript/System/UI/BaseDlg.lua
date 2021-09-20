---@class BaseDlg : BaseViewWrapper
---@field _prefabPath string
---@field _layerName DlgLayer
---@field _closed bool
---@field _sysCloseButton CS.UnityEngine.UI.Button @关闭按钮
---@field _sysBlack CS.UnityEngine.UI.Button @黑色遮罩，不过为了方便实质上也是button
---@field _dialogAnim CS.DialogAnim @ 界面动画
---@field _canvas CS.UnityEngine.Canvas
---@field _dlgOrder number
---@field _orderCtrl CS.UISortingOrderController
---@field _isShowing bool
---@field _visible bool
---@field _isFullScreen bool
---@field _isUnderFullScreen bool
---@field _becomeFullScreenTimer TimerHandle
BaseDlg = DefineInheritedClass(BaseDlg, BaseViewWrapper)

---@param prefabPath string @暂定todo
---@param layerName string @DlgLayer，不填默认DlgLayer.middle
function BaseDlg:_Ctor(prefabPath, layerName)

    -- self:ToBase(BaseViewWrapper):_Ctor()
    BaseViewWrapper._Ctor(self)

    self._prefabPath = prefabPath
    self._layerName = layerName or DlgLayer.Middle

    self._closed = true
    self._isShowing = false;

    self._isFullScreen = false;
    self._isUnderFullScreen = false;
end

--- to override
---@return string
--- 非包内(Resource)资源请填入AssetPath，包内资源请填入ResourcePath，
--- AssetPath可通过右键Assets弹出的菜单复制其路径
function BaseDlg:GetPrefabPath()
    return nil
end

--- to override
---@return DlgLayer
function BaseDlg:GetLayerName()
    return DlgLayer.Middle
end

--- to override
--- 是否是包内(Resource)资源
function BaseDlg:IsInPackage()
    return false
end

--- to override
---@param visible bool
function BaseDlg:_OnVisible(visible)
end

--- 请勿重写
---@return bool
function BaseDlg:IsFullScreen()
    return self._isFullScreen;
end

function BaseDlg:_PostInit()
    self._closed = false

    self:_SysInit()

    self:_InitSortingSys();

    self:_OnInit()
end

function BaseDlg:_SysInit()
    self._sysCloseButton = self:GetUIButton("Sys_Close")
    if (NotNull(self._sysCloseButton)) then
        self:RegisterButtonClick(self._sysCloseButton, self.Close)
    end

    self._sysBlack = self:GetUIButton("Sys_Black")
    if (NotNull(self._sysBlack)) then
        self:RegisterButtonClick(self._sysBlack, self.Close)
    end

    self._dialogAnim = self._xRoot.gameObject:GetComponentInChildren(typeof(CS.DialogAnim));
    if (NotNull(self._dialogAnim)) then
        self._dialogAnim:PlayOpenAnim()
    end
end

function BaseDlg:_InitSortingSys()
    self._canvas = self._xRoot.gameObject:AddComponent(typeof(CS.UnityEngine.Canvas));
    self._canvas.overrideSorting = true;
    self._canvas.sortingLayerName = "UI";
    
    self._xRoot.gameObject:AddComponent(typeof(CS.UnityEngine.UI.GraphicRaycaster));
    self._xRoot.gameObject:AddComponent(typeof(CS.UISortingOrderItem));

    self._orderCtrl = self._xRoot.gameObject:AddComponent(typeof(CS.UISortingOrderController));
end

function BaseDlg:SetDlgSortingOder(order)
    if (self._dlgOrder ~= order) then
        self._dlgOrder = order;
        self._orderCtrl.StartOrder = self._dlgOrder * DlgMgr.GetSortingOrderInterval();
    end
    self:_OnDlgSorted()
end

--- dlgOrder * DlgMgr.GetSortingOrderInterval() 即最终页面canvas的sorting order
function BaseDlg:GetDlgOrder()
    return self._dlgOrder;
end

--- to override
function BaseDlg:_OnDlgSorted()

end

---@return bool @初始化后到关闭调用前这段时间页面算打开，其他时候算关闭
function BaseDlg:IsClosed()
    return self._closed
end


--- 页面是否处于逻辑上的显示状态中（可能实际上因为某些原因隐藏了）
function BaseDlg:IsShowing()
    return self._isShowing;
end


function BaseDlg:Show()
    self._isShowing = true;
    self:_UpdateVisibility();
end

function BaseDlg:Hide()
    self._isShowing = false;
    self:_UpdateVisibility();
end

---@param isUnderFullScreen bool
function BaseDlg:SetUnderFullScreen(isUnderFullScreen)
    self._isUnderFullScreen = isUnderFullScreen
    self:_UpdateVisibility();
end

--- 页面实际上是否可见，这个可见性由用户逻辑上操作页面显示与否的结果(IsShowing)和系统内部遮蔽优化判断共同决定
function BaseDlg:IsVisible()
    return self._visible;
end

--- to override
---@return bool
function BaseDlg:_OnCheckVisibility()
    return self._isShowing and not self._isUnderFullScreen;
end

function BaseDlg:_UpdateVisibility()
    local oldVisible = self._visible;
    self._visible = self:_OnCheckVisibility();

    if (oldVisible == self._visible) then
        return;
    end
    
    if (self._visible) then
        DlgMgr.OnDlgShow(self);
        self._xRoot.gameObject:SetActive(true)
        self:AdjustToTop()
        self:_OnVisible(true)
        -- canvas在隐藏后再次开启不强制重设下order会有问题
        self._canvas.sortingOrder = self._canvas.sortingOrder;
    else
        DlgMgr.OnDlgHide(self);
        self._xRoot.gameObject:SetActive(false)
        self:_OnVisible(false)
    end
end

--- 将该页面排到本层顶部
function BaseDlg:AdjustToTop()
    self._xRoot.transform:SetAsLastSibling()
    DlgMgr.ResortDlg()
end

--- 将该页面排到本层底部
function BaseDlg:AdjustToBottom()
    self._xRoot.transform:SetAsFirstSibling()
    DlgMgr.ResortDlg()
end

---@param immediate bool @是否无视UI动画立刻关闭界面，默认false
function BaseDlg:Close(immediate)
    if (self._closed) then
        return
    end
    if IsNull(immediate) then
        immediate = false
    end
    self._closed = true
    -- 进入关闭阶段的页面提前退出维护状态
    DlgMgr.OnDlgWillClose(self)
    self:_OnWillClose()
    if NotNull(self._dialogAnim) 
    and immediate == false 
    and NotNull(self._xRoot) 
    and self._xRoot.gameObject.activeInHierarchy then
        self._dialogAnim:PlayCloseAnim(function()
            DlgMgr.OnDlgHide(self)
            self:Destroy()
            DlgMgr.Event_DlgClose:Execute(self);
        end)
    else
        DlgMgr.OnDlgHide(self)
        self:Destroy()
        DlgMgr.Event_DlgClose:Execute(self);
    end
    
end

--- to override
function BaseDlg:_OnWillClose()
end

--- 切换成全屏，全屏dlg下的所有dlg都将被隐藏，并且当页面系统中有全屏dlg时，也会通知外部系统进行相关操作（比如隐藏底层场景优化性能）
---@param toFullScreen bool
---@param delay number @切换成全屏的延迟，不填默认0，延迟只对切换成全屏状态有效，对从全屏状态退回无效
function BaseDlg:BecomeFullScreen(toFullScreen, delay)
    if (not DlgMgr.IsEnable()) then
        return;
    end
    delay = delay or 0;
    -- todo : 理论上在每个界面Hide/Show的时候还要DlgMgr.UpdateMainDlgVisibility();一下，现在懒得弄了
    self._becomeFullScreenTimer = self:ClearTimer(self._becomeFullScreenTimer);
    if (toFullScreen) then
        if (delay <= 0) then
            delay = 0.001;
        end

        -- 无论如何，都至少延迟到下一帧执行，方便在OnInit里就调用BecomeFullScreen(true)的情况
        self._becomeFullScreenTimer = self:Delay(delay, function()
            if (not self._closed) then
                self._isFullScreen = true;
                DlgMgr.UpdateFullScreenState();
            end
        end)
    else
        self._isFullScreen = false;
        DlgMgr.UpdateFullScreenState();
    end
end