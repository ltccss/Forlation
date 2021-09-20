---@param t any[]
---@param news any[]
---@return any[]
---把table news插入到table t，返回table t,
---【注意】仅适用于table为数组的情况
function table.addRange(t, news)
    if (t and news) then
        for i = 1, #news do
            table.insert(t, news[i])
        end
        return t;
    else
        if (not t) then
            LogUtil.Error("table [t] is nil");
        else
            LogUtil.Error("table [news] is nil");
        end
        
    end
end

---@param t any[]
---@return any[]
--- 浅拷贝数组形式的table,
---【注意】仅适用于table为数组的情况
function table.copyAsArray(t)
    return {table.unpack(t)}
end

---@param t table<any, any>
---@return number
--- 计算一个table的长度
function table.len(t)
    if not IsTable(t) then
        return 0;
    end
    local r = 0;
    for _, v in pairs(t) do
        r = r + 1;
    end
    return r;
end

---@param table any
---@param key any
---@return boolean
function table.containsKey(table,key)
    for k,v in pairs(table) do
        if k == key then
            return true;
        end
    end
    return false;
end

---@param table any
---@param value any
---@return boolean
function table.containsValue(table,value)
    for k,v in pairs(table) do
        if v == value then
            return true;
        end
    end
    return false;
end

---@generic T
---@param array1 T[]
---@param array2 T[]
---@return T[] newArray
function table.combineArrays(array1,array2)
    local newArray = {};
    for i = 1, #array1 do
        table.insert(newArray,array1[i]);
    end
    for i = 1, #array2 do
        table.insert(newArray,array2[i]);
    end
    return newArray;
end

---@generic T
---@param table T[]
---@param func fun(a:T):boolean
function table.any(table,func)
    for i = 1,#table do
        if func(table[i]) then
            return true;
        end
    end
    return false;
end

---@generic T
---@param table T[]
---@param func fun(a:T):T
function table.first(table,func)
    for i = 1,#table do
        if func(table[i]) then
            return table[i];
        end
    end
    return nil;
end

function table.shuffle(array)
    for i = 1, #array do
        local a = array[i];
        local index = math.random(1, #array);
        local b = array[index];
        array[index] = a;
        array[i] = b;
    end
end

---@param array any[]
---@param func fun(a:any, b:any):bool
function table.bubbleSort(array, func)
    if (array and #array > 1) then
        for i = 1, #array do
            for k = 1, #array - 1 do
                local result = func(array[k], array[k+1]);
                if (result == false) then
                    local ele = array[k];
                    array[k] = array[k+1];
                    array[k+1] = ele;
                end
            end
        end
    end
end