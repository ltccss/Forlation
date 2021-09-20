---@class BaseViewWrapper : XObject @ view辅助基类
---@field protected _xRoot CS.XLuaView
---@field private _b_gameObjectTable table<name, CS.UnityEngine.GameObject>
---@field private _b_buttonEventTable table<CS.UnityEngine.UI.Button, table<function, function>>
---@field private _b_buttonTable table<CS.UnityEngine.UI.Button, CS.UnityEngine.UI.Button>
---@field private _b_timerHandleArray TimerHandle[]
---@field private _b_delegateTable table<LuaDelegate, DelegateWithFuncs>
---@field private _b_remindObjTable table<RemindType, RemindType>
---@field private _b_remindRefreshFuncTable table<RemindType, RemindType>
---@field private _b_neverInited bool
---@field private _b_applicationPauseCycle CS.XLVApplicationPauseLifeCycle
---@field private _b_updateCycle CS.XLVUpdateLifeCycle
---@field private _b_lateUpdateCycle CS.XLVLateUpdateLifeCycle
---@field private _b_fixedUpdateCycle CS.XLVFixedUpdateLifeCycle
---@field private _b_childWrapperTable table<BaseViewWrapper, BaseViewWrapper>
BaseViewWrapper = DefineClass(BaseViewWrapper)

local _isEditor = CS.AppSetting.platform == "editor";
local _needRecordFuncTrace = CS.AppSetting.IsDebugMode
---@type table<BaseViewWrapper, BaseViewWrapper>
local _aliveWrapperTable = {};
local _destroyed = false;


function BaseViewWrapper.DebugShowLeakedWrappers()
    -- unity有个w猥琐的特性，销毁的object==null会返回true，
    -- 如果发现object为空了，但是wrapper还alive，说明泄漏了
    CS.CommonUtil.GC();

    LogUtil.Log("===== DebugShowLeakedWrappers Start====")
    for k, wrapper in pairs(_aliveWrapperTable) do
        if (IsNull(wrapper._xRoot)) then
            local dt = debug.getinfo(wrapper._OnInit);
            if ("System/UI/BaseViewWrapper" == dt.short_src) then
                LogUtil.Warn(DebugGetClassInstanceTrace(wrapper))
            else
                LogUtil.Log(dt.short_src .. "\n" .. DebugGetClassInstanceTrace(wrapper))
            end
            
        end
    end
    LogUtil.Log("===== DebugShowLeakedWrappers End ====")

end

function BaseViewWrapper.ForceReleaseWrappers()
    for k, wrapper in pairs(_aliveWrapperTable) do
        wrapper:DestroyWithoutView();
    end
    _destroyed = true
end

function BaseViewWrapper:_Ctor()

    self._xRoot = nil;

    -- 一个空表40 bytes
    self._b_gameObjectTable = {};
    self._b_buttonEventTable = {};
    self._b_buttonTable = {};

    self._b_timerHandleArray = {}

    self._b_delegateTable = {}

    self._b_remindObjTable = {}
    self._b_remindRefreshFuncTable = {};
    self._b_neverInited = true;

    self._b_childWrapperTable = {};
end

function BaseViewWrapper:_OnInit()
end

function BaseViewWrapper:_OnDestroy()
end

---@return CS.XLuaView
function BaseViewWrapper:GetXRoot()
    return self._xRoot
end

---@param root CS.UnityEngine.GameObject
function BaseViewWrapper:Init(root)
    self._xRoot = root:GetComponent(typeof(CS.XLuaView))

    if (IsNull(self._xRoot)) then
        LogUtil.ErrorObject("root上没有XLuaView脚本", root)
    end

    -- editor模式下注册XLuaView Runtime LuaFile
    if (_isEditor) then
        local dt = debug.getinfo(self._OnInit)
        if (dt and dt.short_src) then
            self._xRoot:SetRuntimeLuaPath(dt.short_src);
        end
    end

    for i = 0, self._xRoot.gameObjects.Length - 1 do
        local obj = self._xRoot.gameObjects[i]
        if (NotNull(obj) and obj.name and IsNull(self._b_gameObjectTable[obj.name])) then
            self._b_gameObjectTable[obj.name] = obj
        else
            LogUtil.ErrorObject("BaseViewWrapper.Init : XLuaView.gameObjects中是否有未命名或者重名物体?  gameObjects索引:" .. i, root)
        end
    end

    self:_PostInit()

    self._b_neverInited = false;
    _aliveWrapperTable[self] = self;
end

function BaseViewWrapper:_PostInit()
    self:_OnInit()
end

--- 只清理Wrapper本身的数据，不销毁所持有的xRoot，xRoot由外部销毁，
--- 有些情况下可能Wrapper持有的xRoot所在节点是其他Wrapper所持有的xRoot的孩子节点，或者xRoot所在节点被其他组件管理
function BaseViewWrapper:DestroyWithoutView()
    self:_OnDestroy()

    -- 清理绑定的定时器
    for i = 1, #self._b_timerHandleArray do
        TimeService.ClearTimer(self._b_timerHandleArray[i])
    end
    self._b_timerHandleArray = {}

    -- 清理绑定的事件
    for key, delegateWithFuncs in pairs(self._b_delegateTable) do
        local funcArray = delegateWithFuncs.funcArray
        local delegate = delegateWithFuncs.delegate
        if (funcArray and delegate) then
            for i = 1, #funcArray do
                delegate:Remove(self, funcArray[i])
            end
        end
    end
    self._b_delegateTable = {}

    -- 清理按钮事件
    for k, button in pairs(self._b_buttonTable) do
        self:ClearButtonClick(button);
    end
    self._b_buttonEventTable = {};
    self._b_buttonTable = {};

    -- 清理绑定的红点提醒
    for remindType in pairs(self._b_remindObjTable) do
        FF.RemindMgr:ClearRemindObject(remindType);
    end

    self._b_remindObjTable = {};

    for remindType in pairs(self._b_remindRefreshFuncTable) do
        FF.RemindMgr:ClearRefreshFunc(remindType);
    end

    self._b_remindRefreshFuncTable = {};

    self._b_gameObjectTable = {};


    if (self._b_neverInited) then
        -- 防止有不按套路来的人直接把wrapper的init方法给重写了，这里做下检测，
        -- 判断这个类是不是从未初始化过
        LogUtil.Error("这个Wrapper类未被初始化过就销毁了")
    end

    -- 清理cs生命周期事件
    self:_UnlistenCommonLifeCycleEvent();
    self:_UnlistenApplicationPauseLifeCycleEvent();
    self:_UnlistenUpdateLifeCycleEvent();
    self:_UnlistenLateUpdateLifeCycleEvent();
    self:_UnlistenFixedUpdateLifeCycleEvent();

    for k, wrapper in pairs(self._b_childWrapperTable) do
        wrapper:DestroyWithoutView();
    end
    self._b_childWrapperTable = {};

    _aliveWrapperTable[self] = nil;

end

function BaseViewWrapper:Destroy()
    self:DestroyWithoutView()

    if (NotNull(self._xRoot)) then
        if (_isEditor) then
            self._xRoot.testFuncList = nil;
        end
        CS.UnityEngine.GameObject.Destroy(self._xRoot.gameObject)
        self._xRoot = nil
    end
end

local function _WalkXRootArray(csArray, name)
    -- 目测获取各种number的频率不高，先不做缓存了
    for i = 0, csArray.Length - 1 do
        local obj = csArray[i]
        if name == obj.name then
            return obj.value
        end
    end
    return nil
end

---@param name string
---@return number
function BaseViewWrapper:GetInt(name)
    return _WalkXRootArray(self._xRoot.ints, name)
end

---@param name string
---@return number
function BaseViewWrapper:GetLong(name)
    return _WalkXRootArray(self._xRoot.longs, name)
end

---@param name string
---@return number
function BaseViewWrapper:GetDouble(name)
    return _WalkXRootArray(self._xRoot.doubles, name)
end

---@param name string
---@return string
function BaseViewWrapper:GetString(name)
    return _WalkXRootArray(self._xRoot.strings, name)
end

---@param name string
---@return bool
function BaseViewWrapper:GetBool(name)
    return _WalkXRootArray(self._xRoot.bools, name)
end

---@param name string
---@return CS.UnityEngine.Color
function BaseViewWrapper:GetColor(name)
    return _WalkXRootArray(self._xRoot.colors, name);
end

---@param name string
---@return CS.UnityEngine.Sprite
function BaseViewWrapper:GetSprite(name)
    return _WalkXRootArray(self._xRoot.sprites, name);
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.GameObject
function BaseViewWrapper:GetGameObject(nameOrGameObject)
    if (IsString(nameOrGameObject)) then
        return self._b_gameObjectTable[nameOrGameObject]
    else
        return nameOrGameObject
    end
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.Transform
function BaseViewWrapper:GetTransform(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go.transform;
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.RectTransform
function BaseViewWrapper:GetRectTransform(nameOrGameObject)
    return self:GetTransform(nameOrGameObject);
end

---@param name string
---@return CS.UnityEngine.Vector3
function BaseViewWrapper:GetVector3(name)
    return _WalkXRootArray(self._xRoot.vector3s, name)
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.Image
function BaseViewWrapper:GetUIImage(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.Image))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.RawImage
function BaseViewWrapper:GetUIRawImage(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.RawImage))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.Text
function BaseViewWrapper:GetUIText(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.Text))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.Toggle
function BaseViewWrapper:GetUIToggle(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.Toggle))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.Button
function BaseViewWrapper:GetUIButton(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.Button))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.ScrollRect
function BaseViewWrapper:GetUIScrollRect(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.ScrollRect))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.InputField
function BaseViewWrapper:GetUIInputField(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.InputField))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.UI.Slider
function BaseViewWrapper:GetUISlider(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.UI.Slider))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.Animator
function BaseViewWrapper:GetAnimator(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.Animator))
    end

    return nil
end

---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@return CS.UnityEngine.CanvasGroup
function BaseViewWrapper:GetCanvasGroup(nameOrGameObject)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(CS.UnityEngine.CanvasGroup))
    end

    return nil
end

---@generic T : CS.UnityEngine.Component
---@param nameOrGameObject string | CS.UnityEngine.GameObject
---@param compClass T
---@return T
function BaseViewWrapper:GetComponentWithClass(nameOrGameObject, compClass)
    local go = self:GetGameObject(nameOrGameObject)
    if (NotNull(go)) then
        return go:GetComponent(typeof(compClass))
    end

    return nil
end

---@param button CS.UnityEngine.UI.Button
---@param func function @注意：传入的func需要保证是该类的方法(self:XXX)，不然还是得闭个包
function BaseViewWrapper:RegisterButtonClick(button, func)
    ---@type table<function, function>
    local events
    if (not self._b_buttonEventTable[button]) then
        events = {}
        self._b_buttonEventTable[button] = events
        self._b_buttonTable[button] = button;
    else
        events = self._b_buttonEventTable[button]
    end

    if not (events[func]) then
        -- todo : 这里可以校验一下func是否属于self？
        local f = function()
            func(self)
        end

        if (_needRecordFuncTrace) then
            DebugRecordFunctionTrace(f);
        end

        button.onClick:AddListener(f)
        events[func] = f
    end

end

---@param button CS.UnityEngine.UI.Button
---@param func function @注意：传入的func需要保证是该类的方法(self:XXX)，否则请直接访问button.onClick清理对应方法
function BaseViewWrapper:UnRegisterButtonClick(button, func)
    ---@type table<function, function>
    if (self._b_buttonEventTable[button]) then
        local events = self._b_buttonEventTable[button]
        if (events[func]) then
            local f = events[func]
            button.onClick:RemoveListener(f)
            button.onClick:ReleaseUnusedListeners();
            events[func] = nil
        end
    end
end

---@param button CS.UnityEngine.UI.Button @清理按钮上的所有事件
function BaseViewWrapper:ClearButtonClick(button)
    if (self._b_buttonEventTable[button]) then
        if (NotNull(button) and NotNull(button.onClick)) then
            button.onClick:RemoveAllListeners();
            button.onClick:ReleaseUnusedListeners();
        else
            LogUtil.Warn("清理buttuon事件异常，理论上在清理按钮事件之前按钮本身不该被删除： 按钮所在对象的创建堆栈:" .. DebugGetClassInstanceTrace(self));
        end
        self._b_buttonEventTable[button] = nil
        self._b_buttonTable[button] = nil;
    end

end

---@param second number
---@param func function @需要保证传入的func是self的方法，不然请直接用TimeService
---@param isReal bool @是否是真实时间，不填默认false
---@return TimerHandle
function BaseViewWrapper:Delay(second, func, isReal)
    -- todo
    local handle = TimeService.Delay(second, self, func, isReal)
    table.insert(self._b_timerHandleArray, handle)
    return handle
end

---@param interval number @间隔，秒
---@param repeatCount number @重复次数，负数为无限
---@param func function @需要保证传入的func是self的方法，不然请直接用TimeService
---@param isReal bool @是否是真实时间，不填默认false
---@return TimerHandle
function BaseViewWrapper:StartTimer(interval, repeatCount, func, isReal)
    local handle = TimeService.AddTimer(interval, repeatCount, self, func, isReal)
    -- todo : 如果必要，每次插入前清理下已经结束的计时器(handle.isFinish)
    table.insert(self._b_timerHandleArray, handle)
    return handle
end

---@param repeatCount number @重复次数，负数为无限
---@param func function @需要保证传入的func是self的方法，不然请直接用TimeService
---@param isReal bool @是否是真实时间，不填默认false
---@return TimerHandle
function BaseViewWrapper:StartFrameTimer(repeatCount, func, isReal)
    local handle = TimeService.AddFrameTimer(repeatCount, self, func, isReal)
    table.insert(self._b_timerHandleArray, handle)
    return handle
end

---@param func function @需要保证传入的func是self的方法，不然请直接用TimeService
---@param isReal bool @是否是真实时间，不填默认false
---@return TimerHandle
function BaseViewWrapper:StartEndlessFrameTimer(func, isReal)
    return self:StartFrameTimer(-1, func, isReal)
end

---@param timer TimerHandle
function BaseViewWrapper:ClearTimer(timer)
    if (timer) then
        for i = #self._b_timerHandleArray, 1, -1 do
            if (self._b_timerHandleArray[i] == timer) then
                TimeService.ClearTimer(timer)
                table.remove(self._b_timerHandleArray, i)
            end
        end
    end

    return nil
end

---@param delegate LuaDelegate
---@param func function @需要保证传入的func是self的方法，不然请直接使用相应的LuaDelegate.Add
function BaseViewWrapper:AddLuaDelegate(delegate, func)
    local delegateWithFuncs = self._b_delegateTable[delegate]
    if (not delegateWithFuncs) then
        delegateWithFuncs = {
            delegate = delegate,
            funcArray = {},
        }
        self._b_delegateTable[delegate] = delegateWithFuncs
    end

    local funcArray = delegateWithFuncs.funcArray

    for i = 1, #funcArray do
        if (funcArray[i]) == func then
            return
        end
    end

    table.insert(funcArray, func)
    delegate:AddIfNotExist(self, func)
end

---@param delegate LuaDelegate
---@param func function @需要保证传入的func是self的方法，不然请直接使用相应的LuaDelegate.Remove
function BaseViewWrapper:RemoveLuaDelegate(delegate, func)
    local delegateWithFuncs = self._b_delegateTable[delegate]

    if (not delegateWithFuncs) then
        return
    end

    local funcArray = delegateWithFuncs.funcArray
    if (funcArray) then
        for i = #funcArray, 1, -1 do
            if (funcArray[i] == func) then
                table.remove(funcArray, i)
                if (#funcArray == 0) then
                    self._b_delegateTable[delegate] = nil
                end
                break
            end
        end
    end
end

---@param remindType RemindType
---@param obj CS.UnityEngine.GameObject
function BaseViewWrapper:RegisterRemindObject(remindType, obj)
    FF.RemindMgr:RegisterRemindObject(remindType, obj);
    if (IsNull(self._b_remindObjTable[remindType])) then
        self._b_remindObjTable[remindType] = remindType;
    end
end

---@param remindType RemindType
function BaseViewWrapper:ClearRemindObject(remindType)
    if (NotNull(self._b_remindObjTable[remindType])) then
        FF.RemindMgr:ClearRemindObject(remindType);
        self._b_remindObjTable[remindType] = nil;
    end
end

---@param remindType RemindType
---@param func fun(visible:boolean)
function BaseViewWrapper:RegisterRemindRefreshFunc(remindType, func)
    FF.RemindMgr:RegisterRefreshFunc(remindType, self, func);
    if (IsNull(self._b_remindRefreshFuncTable)) then
        self._b_remindRefreshFuncTable[remindType] = remindType;
    end
end

---@param remindType RemindType
function BaseViewWrapper:ClearRemindRefreshFunc(remindType)
    if (NotNull(self._b_remindRefreshFuncTable[remindType])) then
        FF.RemindMgr:ClearRefreshFunc(remindType);
        self._b_remindRefreshFuncTable[remindType] = nil;
    end
end

---@generic T : BaseViewWrapper
---@param rootObjOrName CS.UnityEngine.GameObject | string
---@param wrapperClass T
---@return T
function BaseViewWrapper:_CreateChildWrapper(rootObjOrName, wrapperClass)
    ---@type BaseViewWrapper
    local wrapper = CCC(wrapperClass);

    ---@type CS.UnityEngine.GameObject
    local obj = self:GetGameObject(rootObjOrName);
    wrapper:Init(obj);
    self._b_childWrapperTable[wrapper] = wrapper;

    return wrapper
end

---@param wrapper BaseViewWrapper
---@param destroyView bool @是否需要销毁view，不填默认false
function BaseViewWrapper:_DestroyChildWrapper(wrapper, destroyView)
    if (destroyView) then
        wrapper:Destroy()
    else
        wrapper:DestroyWithoutView()
    end
    
    if (self._b_childWrapperTable[wrapper]) then
        self._b_childWrapperTable[wrapper] = nil
    end
end

--- 添加测试方法，会在XLuaView面板上显示对应的调用按钮，仅Editor下有效
---@param name string
---@param func fun()
function BaseViewWrapper:_AddTestFunc(name, func)
    if (_isEditor) then
        self._xRoot:AddTestFunc(name, func);
    end
end

function BaseViewWrapper:_ListenCommonLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        self._xRoot:SetCommonLifeCallback(self, self._OnCsAwake, self._OnCsStart, self._OnCsEnable, self._OnCsDisable);
    end
end

function BaseViewWrapper:_UnlistenCommonLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        self._xRoot:SetCommonLifeCallback(nil, nil, nil, nil, nil);
    end
end

function BaseViewWrapper:_ListenApplicationPauseLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        if (IsNull(self._b_applicationPauseCycle)) then
            self._b_applicationPauseCycle = self._xRoot.gameObject:AddComponent(typeof(CS.XLVApplicationPauseLifeCycle));
        end
        self._b_applicationPauseCycle:Reset(self, self._OnCsApplicationPause);
    end
end

function BaseViewWrapper:_UnlistenApplicationPauseLifeCycleEvent()
    if (NotNull(self._b_applicationPauseCycle)) then
        self._b_applicationPauseCycle:Reset(nil, nil);
    end
end

--- 注意：当前未处理当 XLuaView 组件与 XLVUpdateLifeCycle 组件处于处于不同enable/disable状态的情况，
--- 这种少数情况需要自行判断一下对应组件状态
function BaseViewWrapper:_ListenUpdateLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        if (IsNull(self._b_updateCycle)) then
            self._b_updateCycle = self._xRoot.gameObject:AddComponent(typeof(CS.XLVUpdateLifeCycle));
        end
        self._b_updateCycle:Reset(self, self._OnCsUpdate);
        self._b_updateCycle.enabled = true;
    end
end

function BaseViewWrapper:_UnlistenUpdateLifeCycleEvent()
    if (NotNull(self._b_updateCycle)) then
        self._b_updateCycle:Reset(nil, nil);
        self._b_updateCycle.enabled = false;
    end
end

--- 注意：当前未处理当 XLuaView 组件与 XLVLateUpdateLifeCycle 组件处于处于不同enable/disable状态的情况，
--- 这种少数情况需要自行判断一下对应组件状态
function BaseViewWrapper:_ListenLateUpdateLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        if (IsNull(self._b_lateUpdateCycle)) then
            self._b_lateUpdateCycle = self._xRoot.gameObject:AddComponent(typeof(CS.XLVLateUpdateLifeCycle));
        end
        self._b_lateUpdateCycle:Reset(self, self._OnCsLateUpdate);
        self._b_lateUpdateCycle.enabled = true;
    end
end

function BaseViewWrapper:_UnlistenLateUpdateLifeCycleEvent()
    if (NotNull(self._b_lateUpdateCycle)) then
        self._b_lateUpdateCycle:Reset(nil, nil);
        self._b_lateUpdateCycle.enabled = false;
    end
end

--- 注意：当前未处理当 XLuaView 组件与 XLVFixedUpdateLifeCycle 组件处于处于不同enable/disable状态的情况，
--- 这种少数情况需要自行判断一下对应组件状态
function BaseViewWrapper:_ListenFixedUpdateLifeCycleEvent()
    if (NotNull(self._xRoot)) then
        if (IsNull(self._b_fixedUpdateCycle)) then
            self._b_fixedUpdateCycle = self._xRoot.gameObject:AddComponent(typeof(CS.XLVFixedUpdateLifeCycle));
        end
        self._b_fixedUpdateCycle:Reset(self, self._OnCsFixedUpdate);
        self._b_fixedUpdateCycle.enabled = true;
    end
end

function BaseViewWrapper:_UnlistenFixedUpdateLifeCycleEvent()
    if (NotNull(self._b_fixedUpdateCycle)) then
        self._b_fixedUpdateCycle:Reset(nil, nil);
        self._b_fixedUpdateCycle.enabled = false;
    end
end

--- to override
--- 调用_ListenCommonLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsAwake()
end

--- to override
--- 调用_ListenCommonLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsStart()
end

--- to override
--- 调用_ListenCommonLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsEnable()
end

--- to override
--- 调用_ListenCommonLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsDisable()
end

--- to override
--- 调用_ListenApplicationPauseLifeCycleEvent()后使用
---@param pause bool
function BaseViewWrapper:_OnCsApplicationPause(pause)
end

--- to override
--- 调用_ListenUpdateLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsUpdate()
end

--- to override
--- 调用_ListenLateUpdateLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsLateUpdate()
end

--- to override
--- 调用_ListenFixedUpdateLifeCycleEvent()后使用
function BaseViewWrapper:_OnCsFixedUpdate()
end

---@class DelegateWithFuncs @没有实体，只是声明下格式方便人看
---@field delegate LuaDelegate
---@field funcArray function[]