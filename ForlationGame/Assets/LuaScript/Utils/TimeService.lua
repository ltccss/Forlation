---@class TimerData @没有实体，只是声明下格式方便人看
---@field interval number @间隔，秒
---@field startTime number @开始时间
---@field repeatCount number @总共需要重复的次数
---@field triggerCount number @已经触发的次数
---@field targetTime number @下一次的执行时间
---@field thisObj any
---@field func function
---@field handle TimerHandle
---@field isReal bool @是否是真实时间
---@field delay number  @第一次触发的延时

---@class TimerHandle @没有实体，只是声明下格式方便人看
---@field isFinish bool

---@class TimeService @定时服务
TimeService = DefineClass(TimeService)

---@type table<TimerHandle, TimerData>
local _timerDict = {}
---@type Array<TimerData>
local _triggerArray = {}

---@type LuaDelegate
local _lateUpdateDelegate;


function TimeService.Init()
    _lateUpdateDelegate = CCC(LuaDelegate);
end

---@return TimerHandle
function TimeService._GenerateTimerHandle()
    return {
        isFinish = false
    }
end

---@param interval number @间隔，秒
---@param repeatCount nmber @重复次数，负数为无限
---@param thisObj any @你懂的
---@param func function @你懂的
---@param isReal bool @是否是真实时间，不填默认false
---@param delay number  @第一次执行前的延时
---@return TimerHandle
function TimeService.AddTimer(interval, repeatCount, thisObj, func, isReal, delay)
    if (repeatCount == 0) then
        return nil
    end

    local handle = TimeService._GenerateTimerHandle()

    if (IsNull(isReal)) then
        isReal = false;
    end

    local startTime;
    if (isReal) then
        startTime = TimeUtil.GetCurrentRealTime()
    else
        startTime = TimeUtil.GetCurrentTime()
    end

    if (IsNull(delay)) then
        delay = interval;
    end
    ---@type TimerData
    local timerData = {
        interval = interval,
        startTime = startTime,
        repeatCount = repeatCount,
        triggerCount = 0,
        targetTime = 0,

        thisObj = thisObj,
        func = func,

        handle = handle,

        isReal = isReal,
        delay = delay,
    }

    TimeService._UpdateTargetTime(timerData)

    _timerDict[handle] = timerData

    return handle
end

---@param repeatCount nmber @重复次数，负数为无限
---@param thisObj any @你懂的
---@param func function @你懂的
---@param isReal bool @是否是真实时间，不填默认true
---@return TimerHandle
function TimeService.AddFrameTimer(repeatCount, thisObj, func, isReal)
    return TimeService.AddTimer(0, repeatCount, thisObj, func, isReal, 0.003)
end

---@param thisObj any @你懂的
---@param func function @你懂的
---@param isReal bool @是否是真实时间，不填默认true
---@return TimerHandle
function TimeService.AddEndlessFrameTime(thisObj, func, isReal)
    return TimeService.AddFrameTimer(-1, thisObj, func, isReal)
end

---@param second number @延迟的时间，秒
---@param thisObj any @你懂的
---@param func function @你懂的
---@param isReal bool @是否是真实时间，不填默认true
---@return TimerHandle
function TimeService.Delay(second, thisObj, func, isReal)
    return TimeService.AddTimer(second, 1, thisObj, func, isReal)
end

---@param handle TimerHandle
function TimeService.ClearTimer(handle)
    if (handle and _timerDict[handle]) then
        _timerDict[handle] = nil
        handle.isFinish = true
    end

    return nil
end

function TimeService.Update()
    TimeService._Tick()
end

function TimeService.LateUpdate()
    _lateUpdateDelegate:Execute();
    _lateUpdateDelegate:Clear();
end


function TimeService.WaitNextLateUpdate(thisObj, func)
    _lateUpdateDelegate:AddIfNotExist(thisObj, func);
end

function TimeService._Tick()
    if (#_triggerArray > 0) then
        _triggerArray = {}
    end

    for key in pairs(_timerDict) do
        local timerData = _timerDict[key]
        if (timerData.isReal) then
            if (TimeUtil.GetCurrentRealTime() >= timerData.targetTime) then
                table.insert(_triggerArray, timerData)
            end
        else
            if (TimeUtil.GetCurrentTime() >= timerData.targetTime) then
                table.insert(_triggerArray, timerData)
            end
        end

    end

    if (#_triggerArray > 0) then
        -- 触发之前按照时间排个序，可能一下子卡住后，后面的定时器会在同一帧内一起执行
        table.sort(_triggerArray, function(a, b)
            return a.targetTime < b.targetTime
        end)

        for i = 1, #_triggerArray do

            ---@type TimerData
            local timerData = _triggerArray[i]

            if (not timerData.handle.isFinish) then
                local isSuccess, reason = FuncCallSave(timerData.thisObj, timerData.func)
                if (not isSuccess) then
                    LogUtil.Error(reason)
                end
            end

            timerData.triggerCount = timerData.triggerCount + 1
            if (timerData.triggerCount >= timerData.repeatCount and timerData.repeatCount >= 0) then
                _timerDict[timerData.handle] = nil
                timerData.handle.isFinish = true
            else
                TimeService._UpdateTargetTime(timerData)
            end
        end
    end
end

---@param timerData TimerData
function TimeService._UpdateTargetTime(timerData)
    ---@type number
    local currentTime;
    if (timerData.isReal) then
        currentTime = TimeUtil.GetCurrentRealTime()
    else
        currentTime = TimeUtil.GetCurrentTime()
    end
    if (timerData.triggerCount == 0) then
        timerData.targetTime = currentTime + timerData.delay
    else
        timerData.targetTime = currentTime + timerData.interval
    end
end