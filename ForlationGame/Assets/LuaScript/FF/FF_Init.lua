function FF_InitCo(enableCoroutine)
    -- TODO: 各个Mgr的初始化因为不确定是否有依赖关系，所以目前手动添加
    FF.TestMgr:Init()
end
