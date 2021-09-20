---@class UIPool : XObject
---@field _usedItemArray any[]
---@field _recycledItemArray any[]
---@field _prefab CS.UnityEngine.GameObject
---@field _targetRoot CS.UnityEngine.Transform
---@field _recycleRoot CS.UnityEngine.Transform
---@field _wrapperClass Class
UIPool = DefineClass(UIPool)

---@param prefab CS.UnityEngine.GameObject
---@param targetRoot CS.UnityEngine.Transform
---@param recycleRoot CS.UnityEngine.Transform
---@param wrapperClass Class @wrapper类
function UIPool.Create(prefab, targetRoot, recycleRoot, wrapperClass)
    local pool = CCC(UIPool);

    pool._usedItemArray = {};
    pool._recycledItemArray = {};
    pool._prefab = prefab;
    pool._targetRoot = targetRoot;
    pool._recycleRoot = recycleRoot;
    pool._wrapperClass = wrapperClass;
    
    if (IsNull(targetRoot) or IsNull(recycleRoot)) then
        LogUtil.Error("传入的节点为空");
    end

    return pool;
end

--- 调整当前可用item的数量至目标数量
---@param count number
function UIPool:SetUsedCount(count)
    UITool.AdjustPrefabItemCount(count, self._recycleRoot, self._targetRoot, 
        self._usedItemArray, self._recycledItemArray, self._prefab, self._wrapperClass);
end

--- 获取当前可用item数组
---@generic T : BaseWrapperClass
---@param wrapperClass T @可以不填，只是为了类型推导
---@return T[]
function UIPool:GetUsedItems(wrapperClass)
    return self._usedItemArray;
end

--- 收集游离的items，并放入回收数组中
---@param match string 匹配字符串，targetTrans节点下的物体若名字中包含该字符串，该物体才会被收集；为空时不进行匹配判断
function UIPool:CollectFreeItems(match)
    UITool.CollectFreePrefabItem(self._targetRoot, self._recycleRoot, self._recycledItemArray, self._wrapperClass, match)
end

function UIPool:Destroy()
    UITool.DestroyWrappers(self._usedItemArray);
    UITool.DestroyWrappers(self._recycledItemArray);
    self.usedItemArray = nil;
    self.recycledItemArray = nil;
    self._prefab = nil;
    self._targetRoot = nil;
    self._recycleRoot = nil;
    self._wrapperClass = nil;
end