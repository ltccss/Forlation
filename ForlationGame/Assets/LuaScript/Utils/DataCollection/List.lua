---@class List : XObject @如果需求的数组是允许存在有很多nil的情况，用这个数组
---@field _count number
---@field _array array
List = DefineClass(List)

function List:_Ctor()
    self._count = 0
    self._array = {}
end

function List:Count()
    return self._count
end

--- 从1开始数起
function List:At(index)
    if (index < 1 or index > self._count) then
        LogUtil.Error("index out of range")
    end
    return self._array[index]
end

function List:Add(e)
    self._array[self._count + 1] = e
    self._count = self._count + 1
end

---@param pList List
function List:AddRange(pList)
    if (pList) then
        for i = 1, pList:Count() do
            self:Add(pList:At(i))
        end
    else
        LogUtil.Error("pList is nil")
    end
end

---@param index number
---@param v any
function List:Insert(index, v)
    if (index >= 1) then
        if (index > self._count + 1) then
            index = self._count + 1
        end
        table.insert(self._array, index, v)
        self._count = self._count + 1
    else
        LogUtil.Error("index is smaller than 1")
    end
end

---@param index number
---@param pList List
function List:InsertRange(index, pList)
    if (index >= 1) then
        if pList and pList:Count() > 0 then

            -- 先把原有元素往后移
            for i = self._count, index, -1 do
                self._array[i + pList:Count()] = self._array[i]
            end
            -- 写入新元素
            for i = 1, pList:Count() do
                self._array[index + i - 1] = pList:At(i)
            end

            self._count = self._count + pList:Count()
        end
    else
        LogUtil.Error("index is smaller than 1")
    end
end

---@return number @小于1表示不存在
function List:IndexOf(v)
    for i = 1, self._count do
        if (self._array[i] == v) then
            return i
        end
    end
    return -1
end

---@return bool
function List:Contains(v)
    return self:IndexOf(v) >= 1
end

---@return any
function List:RemoveAt(index)
    if (index >= 1 and index <= self._count) then
        local o = self._array[index]
        table.remove(self._array, index)
        self._count = self._count - 1
        return o
    else
        LogUtil.Error("index out of range")
    end
end

---@return any
function List:RemoveFirst()
    return self:RemoveAt(1)
end

---@return any
function List:RemoveLast()
    return self:RemoveAt(self._count)
end

function List:RemoveWhen(matchFunc)
    if (not matchFunc) then
        LogUtil.Error("matchFunc is nil")
    end
    local o = {}
    for i = self._count, 1, -1 do
        if matchFunc(self._array[i]) then
            table.insert(o, self:RemoveAt(i))
        end
    end
    return o
end

function List:Clear()
    self._array = {}
    self._count = 0
end

---@param compareFunc fun(a, b)
function List:Sort(compareFunc)
    table.sort(self._array, compareFunc)
end

function List:Find(matchFunc)
    for i = 1, self._count do
        if (matchFunc(self._array[i])) then
            return self._array[i]
        end
    end
    return nil
end

---@return array
function List:FindAll(matchFunc)
    local o = {}
    for i = 1, self._count do
        if (matchFunc(self._array[i])) then
            table.insert(o, self._array[i])
        end
    end
    return o
end

---@return array
function List:ToTable()
    return {table.unpack(self._array, 1, self._count)}
end

---@param arr array
function List:FromTable(arr)
    if (arr) then
        self._array = arr
        self._count = #arr
    else
        LogUtil.Error("arr is nil")
    end
end

function List:Unpack()
    return table.unpack(self._array, 1, self._count)
end

function List:ToString()
    local s = ""

    for i = 1, self._count do
        s = s .. " " .. tostring(self._array[i])
    end
    return s
end