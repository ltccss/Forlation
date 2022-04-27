---@class Queue : XObject
---@field _beginIndex number @指向第一个使用的索引
---@field _endIndex number @指向下一个空的索引
---@field _array array
Queue = DefineClass(Queue)

function Queue:_Ctor()
    self._beginIndex = 1;
    self._endIndex = 1;
    self._array = {};
end

function Queue:Count()
    return self._endIndex - self._beginIndex;
end

--- 从1开始数起
function Queue:At(index)
    if (index < 1 or index > self:Count()) then
        LogUtil.Error("index out of range");
    end
    return self._array[self._beginIndex + index - 1]
end

--- 访问首个元素，不改变队列
function Queue:First()
    if (self:Count() > 0) then
        return self._array[self._beginIndex];
    else
        LogUtil.Error("the queue has no element");
    end
end

--- 访问最后个元素，不改变队列
function Queue:Last()
    if (self:Count() > 0) then
        return self._array[self._endIndex - 1];
    else
        LogUtil.Error("the queue has no element");
    end
end

function Queue:Enqueue(e)
    self._array[self._endIndex] = e;
    self._endIndex = self._endIndex + 1;
    -- TODO: endIndex到达整型最大上限时尝试调整startIndex和endIndex
end

function Queue:Dequeue()
    if (self:Count() > 0) then
        local e = self._array[self._beginIndex];
        self._beginIndex = self._beginIndex + 1;
        
        return e;
    else
        LogUtil.Error("the queue has no element")
    end
end

function Queue:Clear()
    self._beginIndex = 1;
    self._endIndex = 1;
    self._array = {};
end