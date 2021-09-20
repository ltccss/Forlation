---@class TimeUtil @todo
TimeUtil = DefineClass(TimeUtil)

local _startTime = 0;
local _startUTCTime = 0;
local _realStartTime = 0;
local _realStartUTCTime = 0;

function TimeUtil.Init()
    _startTime = (CS.System.DateTime.Now:ToLocalTime().Ticks - 621355968000000000) / 10000000 - CS.UnityEngine.Time.time
    _realStartTime = (CS.System.DateTime.Now:ToLocalTime().Ticks - 621355968000000000) / 10000000 - CS.UnityEngine.Time.unscaledTime

    _startUTCTime = (CS.System.DateTime.UtcNow.Ticks - 621355968000000000) / 10000000 - CS.UnityEngine.Time.time
    _realStartUTCTime = (CS.System.DateTime.UtcNow.Ticks - 621355968000000000) / 10000000 - CS.UnityEngine.Time.unscaledTime
end

---@param currentServerUTCTime number 秒
function TimeUtil.Sync(currentServerUTCTime)
    _realStartTime = currentServerUTCTime + (_startTime - _startUTCTime) - CS.UnityEngine.Time.unscaledTime
    _realStartUTCTime = currentServerUTCTime - CS.UnityEngine.Time.unscaledTime
end

local _TimeClass = CS.UnityEngine.Time;
---@return number 当前本地的时间戳，秒，如果是业务相关的地方，请使用真实时间
function TimeUtil.GetCurrentTime()
    return _startTime + _TimeClass.time
end

---@return number 当前UTC时间戳，秒，如果是业务相关的地方，请使用真实时间
function TimeUtil.GetCurrentUTCTime()
    return _startUTCTime + _TimeClass.time
end

---@return number 返回服务器的当前时间，该时间不受时间缩放影响
function TimeUtil.GetCurrentRealTime()
    return _realStartTime + _TimeClass.unscaledTime
end

---@return number 返回服务器的当前UTC时间，该时间不受时间缩放影响
function TimeUtil.GetCurrentUTCRealTime()
    return _realStartUTCTime + _TimeClass.unscaledTime
end

function TimeUtil.GetElapseTime()
    return CS.UnityEngine.Time.deltaTime;
end

---@param utcTime number
---@return number
function TimeUtil.ToLocalTime(utcTime)
    return utcTime + (_startTime - _startUTCTime);
end

---@param localTime number
---@return number
function TimeUtil.ToUtcTime(localTime)
    return localTime + (_startUTCTime - _startTime);
end

local date = require("Utils/date")
local _beginDate = date("1970-1-1 8:0:0 GMT")

--- 解析日期
---@param s string YYYY-MM-DD hh:mm:ss
---@return number 时间戳，秒
function TimeUtil.ParseDate(s)
    local d = date(s);
    return date.diff(d, _beginDate):spanseconds();
end

---@param year number
---@param month number
---@param day number
---@param hour number
---@param min number
---@param sec number
---@param ticks number
---@return number 时间戳，秒
function TimeUtil.ParseDateSep(year, month, day, hour, min, sec, ticks)
    local d = date(year, month, day, hour, min, sec, ticks);
    return date.diff(d, _beginDate):spanseconds();
end

--- 解析时间段
---@param s string hh:mm:ss
---@return number 秒
function TimeUtil.ParseSpanTime(s)
    local d = date(s);
    return d:spanseconds();
end

--- 格式化时间, dd hh:mm:ss
---@param s number 秒
---@param pattern TimeFormatPattern @ 不填默认DDHHMMSS_trimDDHH todo
---@return string
function TimeUtil.FormatTime(s, pattern)
    if (IsNull(pattern)) then
        pattern = TimeFormatPattern.DDHHMMSS_trimDDHH;
    end

    return pattern(s);
end

---@return number, number, number, number @day, hour, min, second
function TimeUtil.SplitTime(s)
    local second = math.floor(s) % 60;
    local minute = math.floor(s / 60) % 60;
    local hour = math.floor(s / 3600) % 24;
    local day = math.floor(s / (3600 * 24));
    return day, hour, minute, second;
end

---@param UTCTargetTime number
---@return number
function TimeUtil.FormatUTCToLocal(UTCTargetTime)
    local offset = StringUtil.FormatNum(TimeUtil.GetCurrentRealTime() - TimeUtil.GetCurrentUTCRealTime());
    return(UTCTargetTime + offset)
end

TimeFormatPattern = 
{
    --- 2day 12:23:23，如果day、hour、minute连续为0，会省略连续为0的部分
    DDHHMMSS_trimDDHH = function(s)
        local second = math.floor(s) % 60;
        local minute = math.floor(s / 60) % 60;
        local hour = math.floor(s / 3600) % 24;
        local day = math.floor(s / (3600 * 24));

        local result = "";
        if (day > 0) then
            result = result .. day .. "d "
        end

        if (hour > 0) then
            result = result .. hour .. ":"
        elseif (day > 0) then
            result = result .. "00:";
        end

        result = result .. string.format("%02d:%02d", minute, second);

        return result;
    end,

    --- 23:45:43
    HHMMSS = function(s)
        local second = math.floor(s) % 60;
        local minute = math.floor(s / 60) % 60;
        local hour = math.floor(s / 3600);

        return string.format("%02d:%02d:%02d", hour, minute, second);
    end,

    --- 超过1天只显示天数，少于一天只显示HHMMSS部分
    DD_OR_HHMMSS = function(s)
        local second = math.floor(s) % 60;
        local minute = math.floor(s / 60) % 60;
        local hour = math.floor(s / 3600) % 24;
        local day = math.floor(s / (3600 * 24));

        local result;
        if (day > 0) then
            if (day > 1) then
                result = tostring(day) .. " days"
            else
                result = tostring(day) .. " day"
            end
        else
            result = string.format("%02d:%02d:%02d", hour, minute, second);
        end

        return result;
    end,

    --- 超过1天只显示天数，少于一天只显示HHMMSS部分
    DD_OR_HHMMSS_CAP = function(s)
        local second = math.floor(s) % 60;
        local minute = math.floor(s / 60) % 60;
        local hour = math.floor(s / 3600) % 24;
        local day = math.floor(s / (3600 * 24));

        local result;
        if (day > 0) then
            if (day > 1) then
                result = tostring(day) .. " DAYS"
            else
                result = tostring(day) .. " DAY"
            end
        else
            result = string.format("%02d:%02d:%02d", hour, minute, second);
        end

        return result;
    end
}