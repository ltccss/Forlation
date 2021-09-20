--TODO: 页面跳转记录堆栈，用于需要页面跳转后再回溯至历史页面的需求
---@class UIJumpStackMgr : BaseMgr
UIJumpStackMgr = DefineInheritedClass(UIJumpStackMgr, BaseMgr)


function UIJumpStackMgr:_OnInit()
    -- todo 需要在FF_Init.lua中自行调用Init()方法
end

function UIJumpStackMgr:_OnDestroy()
    -- todo 需要在FF_Destroy.lua中自行调用Destroy()方法
end