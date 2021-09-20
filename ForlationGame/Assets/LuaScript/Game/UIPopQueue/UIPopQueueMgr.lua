--TODO： 弹窗队列，用于诸如游戏开始时逐个弹出一堆弹窗，或者领取奖励后一系列奖励页面逐个弹出等场景
---@class UIPopQueueMgr : BaseMgr
UIPopQueueMgr = DefineInheritedClass(UIPopQueueMgr, BaseMgr)


function UIPopQueueMgr:_OnInit()
    -- todo 需要在FF_Init.lua中自行调用Init()方法
end

function UIPopQueueMgr:_OnDestroy()
    -- todo 需要在FF_Destroy.lua中自行调用Destroy()方法
end