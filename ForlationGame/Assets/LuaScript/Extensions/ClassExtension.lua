local _Class_Tag = "@cls#^##$#$"
local _Class_Instance_Tag = "@ins#$@%%^"
local _Class_BaseAgent_Tag = "@base$%^#$$"


if (getmetatable(_G)) then
    LogUtil.Warn("G表的元表将被重新赋值")
end

---@type table<any, string>
local _globalDefineTable = {}

setmetatable(_G, {
    __newindex = function(t, k, v) 
        _globalDefineTable[v] = k
        rawset(t, k, v)
    end
})

local function _DefineRawClass(class)
    class = class or {}
    if (class[_Class_Tag]) then
        LogUtil.Error("Class被重复定义");
    end
    
    class.__index = class;

    class[_Class_Tag] = true;
    return class;
end

--- 定义一个类，会自动继承自XObject
function DefineClass(class)
    class = _DefineRawClass(class)
    if (getmetatable(class)) then
        LogUtil.Error("class已经继承了父类");
    end

    setmetatable(class, XObject)
    return class
end

--- 定义一个派生类
function DefineInheritedClass(class, baseClass)
    if (baseClass) then
        class = _DefineRawClass(class)
        if (getmetatable(class)) then
            LogUtil.Error("class已经继承了父类");
        end

        setmetatable(class, baseClass);
        return class
    else
        LogUtil.Error("baseClass不能为空，是否是require的顺序写错了?")
    end

end

---@generic T
---@param class T
---@return T
--- 根据传入的class创建实例，
--- 如果有默认构造函数_Ctor()，将会自动调用
--- 注意：如果需要给构造函数传参数，要当心参数截断问题
function CreateClassInstance(class, ...)
    return CCC(class, ...);
end

local _weakObjTable = {};
setmetatable(_weakObjTable, {__mode = "k"});
local _weakObjTraceTable = {};
setmetatable(_weakObjTraceTable, {__mode =  "k"});
local _weakFuncTraceTable = {};
setmetatable(_weakFuncTraceTable, {__mode =  "k"});
local _needRecordInstance = CS.AppSetting.IsDebugMode

---@generic T
---@param class T
---@return T
--- 根据传入的class创建实例，
--- 如果有默认构造函数_Ctor()，将会自动调用
--- 注意：如果需要给构造函数传参数，要当心参数截断问题
--- (CreateClassInstance函数的别名)
function CCC(class, ...)
    if (not class) then
        LogUtil.Error("传入的class为空, 是否忘记require对应的脚本了？")
    end
    local o = {};
    o[_Class_Instance_Tag] = true
    setmetatable(o, class);

    if (o._Ctor) then
        o:_Ctor(...)
    end

    if (_needRecordInstance) then
        _weakObjTable[o] = o;
        _weakObjTraceTable[o] = debug.traceback();
    end
    return o;
end

function ForceUnlinkAllAliveRefs()
    for k, v in pairs(_weakObjTable) do
        if (v and not (IsClassInstance(v, LuaGameManager) or IsClassInstance(v, TimeService))) then
            for k2, v2 in pairs(v) do
                -- 这里主要是为了清理对Cs Object的引用，
                rawset(v, k2, nil);
            end
        end
    end
end

--- 打印指定类的所有实例（包含子类）的创建堆栈
---@param class  any @不填默认打印所有class的实例
function DebugShowClassInstanceTrace(class)
    CS.CommonUtil.GC();

    LogUtil.Log("============== start ===============")
    for k, v in pairs(_weakObjTable) do
        if (IsClassInstance(v, class)) then
            LogUtil.Log(_weakObjTraceTable[k]);
        end

    end
    LogUtil.Log("============== end ===============")
end

--- 显示当前所有类实例的数量统计
function DebugShowClassInstanceStats()
    CS.CommonUtil.GC();

    local classTable = {}
    for k, v in pairs(_weakObjTable) do
        
        local class = GetClass(v)
        if (not classTable[class]) then
            classTable[class] = 0
        end
        classTable[class] = classTable[class] + 1
    end

    LogUtil.Log("============== start ===============")

    for class, count in pairs(classTable) do
        LogUtil.LogFormat("class name : {0}, count : {1}", GetClassName(class), count)
    end

    LogUtil.Log("============== end ===============")
end

--- 获取类实例的创建堆栈
function DebugGetClassInstanceTrace(instance)
    return _weakObjTraceTable[instance] or "instance create trace not found"
end

--- 获取函数的生成堆栈
function DebugGetFunctionTrace(func)
    return _weakFuncTraceTable[func] or "function trace not found"
end

--- 记录函数的生成堆栈（应在生成后调用记录）
function DebugRecordFunctionTrace(func)
    _weakFuncTraceTable[func] = debug.traceback()
end

function DebugShowNonClassGlobalDefines()
    LogUtil.Log("========= start ========")
    for obj, objName in pairs(_globalDefineTable) do
        if (type(obj) ~= 'table' or not obj[_Class_Tag]) then
            LogUtil.Log("name : " .. objName .. " type : " .. type(obj))
        end
    end
    LogUtil.Log("========= error ========")
end

---@param obj any
---@param class any @类
---@return bool @判断obj是否是class的实例，如果class不填或者为nil，则判断obj是否为任意一个class的实例
function IsClassInstance(obj, class)
    return obj and rawget(obj, _Class_Instance_Tag) == true and (not class or IsInheritedFrom(obj, class))
end

---@param instanceOrSubClass any
---@param class any
function IsInheritedFrom(instanceOrSubClass, class)
    if (not instanceOrSubClass or not class) then
        return false
    end
    local parent = getmetatable(instanceOrSubClass)
    while (parent ~= class and parent ~= nil) do
        parent = getmetatable(parent)
    end
    return parent ~= nil
end

--- 获取实例所属类
---@param instance any@类型的实例，如果为空或者不是通过CreateClassInstance/CCC创建的实例，则返回nil
function GetClass(instance)
    if (instance) then
        if (rawget(instance, _Class_Instance_Tag)) then
            return getmetatable(instance)
        else
            LogUtil.Error("传入的instance并不是一个有效的实例")
            return nil;
        end
    else
        return nil;
    end
end

--- 传入参数是实例时，获取实例所属类的基类
--- 传入参数是类时，获取该类的基类
function GetBaseClass(instanceOrClass)
    if (instanceOrClass) then
        if (IsClassInstance(instanceOrClass)) then
            return getmetatable(GetClass(instanceOrClass))
        else
            return getmetatable(instanceOrClass)
        end
    end

    return nil
end

---@return string
function GetClassName(class)
    if (class[_Class_Tag]) then
        return _globalDefineTable[class]
    else
        LogUtil.Error("传入的不是class")
        return nil
    end
end

---@param className string
---@return class
function GetClassByName(className)
    return _G[className]
end


---@class XObject @所有通过该套OOP系统创建出来的实例都会自动继承自该类，方便塞一些通用的基类方法，如果不使用这些基类方法则无需在注解中声明此基类
XObject = _DefineRawClass(XObject)


--- （实验性质）
--- 保守点还是直接BaseClass.Func(self, ...)，这个方法只是为了更直观地告知目前正在调用基类的方法
--- 注意：返回的是一个可以冒号调用基类方法的代理对象，并非实例本身，其方法也都为代理方法，并非基类方法
---@generic T
---@param baseClass T
---@return T
function XObject:ToBase(baseClass)
    local baseAgentTable = rawget(self, _Class_BaseAgent_Tag)
    if (baseAgentTable and baseAgentTable[baseClass]) then
        return baseAgentTable[baseClass]
    end

    if (not IsInheritedFrom(self, baseClass)) then
        LogUtil.Error("传入的baseClass并非该实例的基类")
        return nil
    end

    local baseAgent = {}

    setmetatable(baseAgent, {
        -- 从baseAgent中获取方法，会生成一个临时方法，满足冒号调用条件时强制把第一个参数改成self
        __index = function(t, k) 
            
            local func = baseClass[k];

            if (func and type(func) == "function") then

                local retFunc = function(callee, ...)

                    if (callee and callee == baseAgent) then
                        callee = self
                    end

                    func(callee, ...)
                end

                -- 缓存下生成的函数
                baseAgent[k] = retFunc

                return retFunc
            end
        end,
    })

    if (not baseAgentTable) then
        baseAgentTable = {}
        rawset(self, _Class_BaseAgent_Tag, baseAgentTable)
    end
    baseAgentTable[baseClass] = baseAgent

    return baseAgent
end