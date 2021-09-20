-- 加载全部的lua脚本(卡顿再说)
require("GameRequires")
require("FF/FFFirstCustom")

luaGameManager = CCC(LuaGameManager)
luaGameManager:Init()
CS.ForlationGameManager.Me:SetLuaMgrUpdate(luaGameManager, luaGameManager.Update)
CS.ForlationGameManager.Me:SetLuaLateUpdate(luaGameManager.LateUpdate)