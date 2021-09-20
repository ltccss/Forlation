LuaCoroutineUtil = DefineClass(LuaCoroutineUtil)
local move_end = {}

local generator_mt = {
    __index = {
        MoveNext = function(self)
            self.Current = self.co()
            if self.Current == move_end then
                self.Current = nil
                return false
            else
                return true
            end
        end;
        Reset = function(self)
            -- 创建出协程
            self.__co = coroutine.create(self.w_func)

            -- 堆栈记录
            local traceInfoArray = {};

            table.insert(traceInfoArray, debug.traceback(nil, 3))

            self.co = function ()
                
                -- 每次调用时,resume被挂起的协程，如果包裹的函数执行完了，最后会return move_end
                local success, r = coroutine.resume(self.__co)
                if (success) then
                    return r
                else
                    table.insert(traceInfoArray, debug.traceback(self.__co))
                    LogUtil.Error(r .. "\n[trace:]\n" .. traceInfoArray[2] .. "\n" .. traceInfoArray[1] .."\n[end trace]")
                    return move_end;
                end
            end
        end
    }
}

local function cs_generator(func, ...)
    local params = {...}
    local generator = setmetatable({
        w_func = function()
            func(unpack(params))
            return move_end
        end
    }, generator_mt)
    generator:Reset()
    return generator
end

---@param callee any
---@param func function
---@return CS.UnityEngine.Coroutine
function LuaCoroutineUtil.StartCoroutine(callee, func)

    local execFunc

    if (IsNull(callee)) then
        execFunc = func
    else
        execFunc = function() func(callee) end
    end

    -- todo : 有空考虑自己维护coroutine.create/yield/resume，现在这样子真正的协程其实被隐藏了，访问协程状态不方便
    local coHandler = cs_generator(execFunc)

    local co = CS.CoroutineUtil.StartCoroutine(coHandler)
    return co
end

---@param co CS.UnityEngine.Coroutine @注意：在协程内部使用此方法结束自身时，协程还是会运行到下一个挂起点，如有必要请直接在协程内return
function LuaCoroutineUtil.StopCoroutine(co)
    if (NotNull(co)) then
        CS.CoroutineUtil.StopCoroutine(co)
    end
end