---@param thisObj any
---@param func function
function FuncCall(thisObj, func, ...)
    if (not func) then
        -- LogUtil.Error("func不能为空")
        return;
    end
    if (thisObj) then
        return func(thisObj, ...)
    else
        return func(...)
    end
end

---@param thisObj any
---@param func function
---@return any @如果func执行正常，就返回[true, ...func的返回值]，如果执行失败，就返回[false, 错误原因]
function FuncCallSave(thisObj, func, ...)
    if (not func) then
        LogUtil.Error("func不能为空")
    end

    if (thisObj) then
        return pcall(func, thisObj, ...);
    else
        return pcall(func, ...)
    end
end

---@param thisObj any
---@param func function
---@param params any[] @函数参数数组
---@param paramCount number @因为nil截断问题，params中可能有nil值，请自行指定paramCount
function FuncApply(thisObj, func, params, paramCount)
    if (not func) then
        LogUtil.Error("func不能为空")
    end

    if (params) then
        if (thisObj) then
            if (not paramCount) then
                LogUtil.Error("paramCount不能为空")
            end
            return func(thisObj, table.unpack(params, 1, paramCount))
        else
            return func(table.unpack(params, 1, paramCount))
        end
    else
        return func(thisObj)
    end

end