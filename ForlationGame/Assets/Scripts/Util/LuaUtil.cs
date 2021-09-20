using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static class LuaUtil
{
    public static LuaEnv LuaEnv;

#if UNITY_EDITOR
    static LuaUtil()
    {
        if (!Application.isPlaying)
        {
            LuaEnv = new LuaEnv();
        }
    }
#endif
    public static void Init(bool force)
    {
        if (force)
        {
            if (LuaEnv != null)
            {
                
                

                
                try
                {
                    LuaEnv.Dispose();
                    LuaEnv = null;
                }
                catch (Exception e)
                {
                    LogUtil.Error(e.ToString());
                    DoString(@"print_func_ref_by_csharp()");
                }
                finally
                {
                    if (AppSetting.platform != "editor")
                    {
                        // 正式环境下，无论旧的env卸载是否成功，都强制置空，后续再创建新的env
                        // 泄露总比报错卡住好
                        LuaEnv = null;
                    }
                }
            }
        }
        
        if (LuaEnv == null)
        {
            LuaEnv = new LuaEnv();
        }
    }

    public static LuaTable DoString(string scriptStr, LuaTable metaTable = null, string chunkName = "Buffalo")
    {
        LuaTable luaTable = LuaEnv.NewTable();
        if (metaTable == null)
        {
            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            metaTable = LuaEnv.NewTable();
            metaTable.Set("__index", LuaEnv.Global);
        }

        if (metaTable != null)
        {
            luaTable.SetMetaTable(metaTable);
            metaTable.Dispose();
        }

        LuaEnv.DoString(scriptStr, chunkName, luaTable);
        return luaTable;
    }
}