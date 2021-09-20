LogUtil = {}
---@param str string
function LogUtil.Log(str)
    CS.LogUtil.Log(str);
end

---@param str string
function LogUtil.LogFormat(str, ...)
    CS.LogUtil.LogFormat(str, ...)
end

---@param str string
---@param obj CS.UnityEngine.Object
function LogUtil.LogObject(str, obj)
    CS.LogUtil.LogObject(str, obj)
end

---@param str string
function LogUtil.Warn(str)
    CS.LogUtil.Warn(str .. "\n" .. debug.traceback());
end

---@param str string
function LogUtil.WarnFormat(str, ...)
    CS.LogUtil.WarnFormat(str .. "\n" .. debug.traceback(), ...)
end

---@param str string
---@param obj CS.UnityEngine.Object
function LogUtil.WarnObject(str, obj)
    CS.LogUtil.WarnObject(str .. "\n" .. debug.traceback(), obj)
end

---@param str string
function LogUtil.Error(str)
    CS.LogUtil.Error(str .. "\n" .. debug.traceback());
end

---@param str string
function LogUtil.ErrorFormat(str, ...)
    CS.LogUtil.ErrorFormat(str .. "\n" .. debug.traceback(), ...)
end

---@param str string
---@param obj CS.UnityEngine.Object
function LogUtil.ErrorObject(str, obj)
    CS.LogUtil.ErrorObject(str .. "\n" .. debug.traceback(), obj)
end

---@param cond boolean
---@param log string
function LogUtil.Assert(cond, log)
    CS.LogUtil.Assert(cond, log .. "\n" .. debug.traceback())
end

---@param str string
---@param color CS.UnityEngine.Color
---@return string
function LogUtil.CombineColor(str, color)
    return CS.LogUtil.CombineColor(str, color);
end