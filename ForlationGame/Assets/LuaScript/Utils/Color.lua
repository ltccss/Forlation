---@class Color
Color = DefineClass(Color)

---@param r number
---@param g number
---@param b number
---@param a number
---@return CS.UnityEngine.Color
function Color.From255(r, g, b, a)
    if (NotNull(a)) then
        return CS.UnityEngine.Color(r / 255, g / 255, b / 255, a / 255)
    else
        return CS.UnityEngine.Color(r / 255, g / 255, b / 255);
    end
end

local _hexTable = {}
_hexTable["0"] = 0;
_hexTable["1"] = 1;
_hexTable["2"] = 2;
_hexTable["3"] = 3;
_hexTable["4"] = 4;
_hexTable["5"] = 5;
_hexTable["6"] = 6;
_hexTable["7"] = 7;
_hexTable["8"] = 8;
_hexTable["9"] = 9;

_hexTable["a"] = 10;
_hexTable["b"] = 11;
_hexTable["c"] = 12;
_hexTable["d"] = 13;
_hexTable["e"] = 14;
_hexTable["f"] = 15;

_hexTable["A"] = 10;
_hexTable["B"] = 11;
_hexTable["C"] = 12;
_hexTable["D"] = 13;
_hexTable["E"] = 14;
_hexTable["F"] = 15;

---@param s string
---@param i number
---@return number
local function _Dehex(s, i)
    local n = string.sub(s, i, i)
    if (_hexTable[n]) then
        return _hexTable[n]
    else
        LogUtil.Error("error hex input : " .. n)
    end
end

---@param hex string
---@return CS.UnityEngine.Color
function Color.FromHex(hex)
    local len = string.len(hex)
    if (len == 8) then
        local r = _Dehex(hex, 1) * 16 + _Dehex(hex, 2);
        local g = _Dehex(hex, 3) * 16 + _Dehex(hex, 4);
        local b = _Dehex(hex, 5) * 16 + _Dehex(hex, 6);
        local a = _Dehex(hex, 7) * 16 + _Dehex(hex, 8);
        return Color.From255(r, g, b, a);
    elseif (len == 6) then
        local r = _Dehex(hex, 1) * 16 + _Dehex(hex, 2);
        local g = _Dehex(hex, 3) * 16 + _Dehex(hex, 4);
        local b = _Dehex(hex, 5) * 16 + _Dehex(hex, 6);
        return Color.From255(r, g, b);
    else
        LogUtil.Error("wrong hex format")
    end
end