---@class TestMgr : BaseMgr
TestMgr = DefineInheritedClass(TestMgr, BaseMgr)

function TestMgr:_OnInit()

    LogUtil.Log("init TestMgr")
end
