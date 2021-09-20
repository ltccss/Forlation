/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using UnityEngine.UI;
[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour
{
    internal const float GCInterval = 1;//1 second 
    //时机函数
    protected Action luaAwake;
    protected Action luaStart;
    protected Action luaOnEnable;
    protected Action luaOnDisable;
    protected Action luaUpdate;
    protected Action luaLateUpdate;
    protected Action luaOnDestroy;
    //injections
    protected LuaTable scriptEnv;
    public LuaTable GetLuaTable()
    {
        return scriptEnv;
    }
    //主要子类显式调用
    public virtual LuaTable InitLua(string scriptStr, LuaTable metaTable = null, string chunkName = "Buffalo")
    {
        scriptEnv = LuaUtil.DoString(scriptStr, metaTable, chunkName);
        scriptEnv.Set("self", this);
        scriptEnv.Set("this", scriptEnv);
        scriptEnv.Get("awake", out luaAwake);
        scriptEnv.Get("start", out luaStart);
        scriptEnv.Get("onenable", out luaOnEnable);
        scriptEnv.Get("ondisable", out luaOnDisable);
        scriptEnv.Get("update", out luaUpdate);
        scriptEnv.Get("lateupdate", out luaLateUpdate);
        scriptEnv.Get("ondestroy", out luaOnDestroy);
        Action luaIniComplete;
        scriptEnv.Get("initComplete", out luaIniComplete);
        luaIniComplete?.Invoke();
        return scriptEnv;
    }
    protected virtual void Awake()
    {
        luaAwake?.Invoke();
    }
    protected virtual void Start()
    {
        luaStart?.Invoke();
    }
    protected virtual void OnEnable()
    {
        luaOnEnable?.Invoke();
    }
    protected virtual void OnDisable()
    {
        luaOnDisable?.Invoke();
    }
    protected virtual void Update()
    {
        luaUpdate?.Invoke();
    }
    protected virtual void LateUpdate()
    {
        luaLateUpdate?.Invoke();
    }
    protected virtual void OnDestroy()
    {
        luaOnDestroy?.Invoke();
        luaAwake = null;
        luaStart = null;
        luaOnEnable = null;
        luaOnDisable = null;
        luaUpdate = null;
        luaLateUpdate = null;
        luaOnDestroy = null;
        scriptEnv?.Dispose();
    }
}
