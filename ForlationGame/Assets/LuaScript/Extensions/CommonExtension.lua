-- 某些插件不识别bool，需要处理一下
---@alias bool boolean

unpack = unpack or table.unpack

-- 深拷贝对象
function DeepCopy(object)
	local lookup_table = {}
	
	local function _copy(object)
		if type(object) ~= "table" then
			return object
		elseif lookup_table[object] then
			return lookup_table[object]
		end

		local new_table = {}
		lookup_table[object] = new_table
		for index, value in pairs(object) do
			new_table[_copy(index)] = _copy(value)
		end

		return setmetatable(new_table, getmetatable(object))
	end

	return _copy(object)
end

---@param s string
---@return boolean
function IsStringEmptyOrNull(s)
	return IsNull(s) or s == ""
end

local rapidjson = require 'rapidjson' 
local JsonNull = rapidjson.null

local csCommonUtil = CS.CommonUtil;
---是否为空
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsNull(target)
    if (target == nil or target == JsonNull) then
        return true
    end

    if (IsUserData(target) and csCommonUtil.IsUnityObject(target)) then
		return csCommonUtil.IsUnityObjectNull(target)
	end
	
	return false
end

---你懂的
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function NotNull(target)
    return not IsNull(target)
end

---是否是 boolean
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsBoolean(target)
    return "boolean" == type(target)
end
---是否是 number
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsNumber(target)
    return "number" == type(target)
end
---是否是 string
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsString(target)
    return "string" == type(target)
end
---是否是 function
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsFunction(target)
    return "function" == type(target)
end
---是否是 table
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsTable(target)
    return "table" == type(target)
end
---是否是 userdata
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsUserData(target)
    return "userdata" == type(target)
end
---是否是 thread
---@type fun(target:any):boolean
---@param target any 数据对象
---@return boolean
function IsThread(target)
    return "thread" == type(target)
end

--- Table转String
---@param value	any
---@return string
function ToStringEx(value)
	if type(value)=='table' then
		return TableToStr(value)
	elseif type(value)=='string' then
		return "\'"..value.."\'"
	else
		return tostring(value)
	end
end
--- Table转String 外部调用这个，上面那个是辅助函数
---@param t	any
---@return string
function TableToStr(t)
	if t == nil or not IsTable(t) then return "" end
	local retstr= "{"

	local i = 1
	for key,value in pairs(t) do
		local signal = ","
		if i==1 then
			signal = ""
		end

		if key == i then
			retstr = retstr..signal..ToStringEx(value)
		else
			if type(key)=='number' or type(key) == 'string' then
				retstr = retstr..signal..'['..ToStringEx(key).."]="..ToStringEx(value)
			else
				if type(key)=='userdata' then
					retstr = retstr..signal.."*s"..TableToStr(getmetatable(key)).."*e".."="..ToStringEx(value)
				else
					retstr = retstr..signal..key.."="..ToStringEx(value)
				end
			end
		end

		i = i+1
	end

	retstr = retstr.."}"
	return retstr
end

--- 返回csDelegate
--- 用法举例: XXX.OnUpdate = AddToCsDelegate(XXX.OnUpdate, function() end);
function AddToCsDelegate(csDelegate, func)
	if (NotNull(csDelegate)) then
		csDelegate = csDelegate + func
		return csDelegate;
	else
		return func;
	end
end

--- 返回csDelegate
--- 用法举例： XXX.OnUpdate = RemoveFromCsDelegate(XXX.OnUpdate, function() end);
function RemoveFromCsDelegate(csDelegate, func)
	if (NotNull(csDelegate)) then
		csDelegate = csDelegate - func;
		return csDelegate;
	else
		return nil;
	end
end

---@param condition boolean
---@param resultTrue any
---@param resultFalse any
function TernaryCon(condition,resultTrue,resultFalse)
	if condition then
		return resultTrue;
	else
		return resultFalse
	end
end