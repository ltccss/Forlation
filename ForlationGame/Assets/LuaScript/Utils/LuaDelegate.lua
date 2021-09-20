---@class DelegateObj @没有实体，只是声明下格式方便人看
---@field caller any
---@field func function
---@field valid bool

---@class LuaDelegate : XObject
---@field _array DelegateObj[]
---@field _exeArray DelegateObj[]
---@field _arrayChanged bool
LuaDelegate = DefineClass(LuaDelegate)

function LuaDelegate:_Ctor()
    self._array = {}
    self._exeArray = {}
    self._arrayChanged = true
end

---@param caller any
---@param func function
function LuaDelegate:Add(caller, func)
    ---@type DelegateObj
    local delegateObj = {
        caller = caller,
        func = func,
        valid = true,
    }

    table.insert(self._array, delegateObj)
    self._arrayChanged = true
end

---@param caller any
---@param func function
function LuaDelegate:AddIfNotExist(caller, func)
    for i = 1, #self._array do
        if (self._array[i].caller == caller and self._array[i].func == func) then
            return
        end
    end
    self:Add(caller, func)
end

---@param caller any
---@param func function
function LuaDelegate:Remove(caller, func)
    for i = #self._array, 1, -1 do
        if (self._array[i].caller == caller and self._array[i].func == func) then
            self._array[i].valid = false
            table.remove(self._array, i)
            self._arrayChanged = true
        end
    end
end

function LuaDelegate:Clear()
    self._array = {}
    self._arrayChanged = true
end

function LuaDelegate:Execute(...)
    if (self._arrayChanged) then
        self._exeArray = {table.unpack(self._array)}
        self._arrayChanged = false
    end

    for i = 1, #self._exeArray do
        if (self._exeArray[i] and self._exeArray[i].valid) then
            FuncCall(self._exeArray[i].caller, self._exeArray[i].func, ...)
        end
    end
end