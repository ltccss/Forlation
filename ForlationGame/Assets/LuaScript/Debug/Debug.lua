
function print_func_ref_by_csharp()
    LogUtil.Warn("========= 没有被正确销毁的lua委托 Start =======")
    local registry = debug.getregistry()
    for k, v in pairs(registry) do
        if type(k) == 'number' and type(v) == 'function' and registry[v] == k then
            local info = debug.getinfo(v)
            LogUtil.Warn(string.format('%s:%d, \n[trace]%s', info.short_src, info.linedefined, DebugGetFunctionTrace(v)))
        end
    end
    LogUtil.Warn("========= 没有被正确销毁的lua委托 End =======")
end

function dump_memory()
    collectgarbage("collect")
    local mri = require("Debug/MemoryReferenceInfo")
    mri.m_cMethods.DumpMemorySnapshot(CS.System.IO.Path.Combine(CS.UnityEngine.Application.dataPath, "../output/"), "lua_dump", -1)
end
