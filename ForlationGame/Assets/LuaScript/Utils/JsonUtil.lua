JsonUtil = DefineClass(JsonUtil)

local rapidjson = require 'rapidjson' 

---@param o any
---@return string
--- 把lua对象转化成json文本
function JsonUtil.Encode(o)
    return rapidjson.encode(o);
end

---@param text string
---@return any
---把json文本转化成lua对象
function JsonUtil.Decode(text)
    return rapidjson.decode(text);
end