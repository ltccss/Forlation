UITool = {}

--- 在某个节点下添加一组物体并且用对用的WrapperClass包裹之
---@generic WrapperClass
---@param count number
---@param targetTrans CS.UnityEngine.Transform
---@param prefab CS.UnityEngine.GameObject
---@param wrapperClass WrapperClass
---@return WrapperClass[]
function UITool.FastCreatePrefabItemWrappers(count, targetTrans, prefab, wrapperClass)
    local array = {};
    for i = 1, count do
        local tmpObj = CS.UnityEngine.GameObject.Instantiate(prefab, targetTrans);
         ---@type BaseViewWrapper
        local wrapper = CCC(wrapperClass);
        wrapper:Init(tmpObj);
        table.insert(array, wrapper);
    end
    return array;
end

--- 在某个节点添加一组物体并返回这组物体
---@param count number
---@param targetTrans CS.UnityEngine.Transform
---@param prefab CS.UnityEngine.GameObject
---@return CS.UnityEngine.GameObject[]
function UITool.FastAddPrefabItems(count, targetTrans, prefab)
    local array = {};
    for i = 1, count do
        local tmpObj = CS.UnityEngine.GameObject.Instantiate(prefab, targetTrans);
        table.insert(array, tmpObj);
    end
    return array;
end

---@generic WrapperClass
---@param targetTrans CS.UnityEngine.Transform
---@param usedItemArray WrapperClass[]
---@param recycledItemArray WrapperClass[]
---@param prefab CS.UnityEngine.GameObject
---@param wrapperClass WrapperClass
---@return WrapperClass
function UITool.FetchNewPrefabItem(targetTrans, usedItemArray, recycledItemArray, prefab, wrapperClass)
    if (#recycledItemArray > 0) then
        ---@type BaseViewWrapper
        local wrapper = recycledItemArray[#recycledItemArray];
        table.remove(recycledItemArray, #recycledItemArray);

        local childTrans = wrapper:GetXRoot().transform;
        childTrans:SetParent(targetTrans);
        childTrans.localScale = CS.UnityEngine.Vector3.one;
        table.insert(usedItemArray, wrapper);

        return wrapper;
    else
        local tmpObj = CS.UnityEngine.GameObject.Instantiate(prefab, targetTrans);
        ---@type BaseViewWrapper
        local wrapper = CCC(wrapperClass);
        wrapper:Init(tmpObj);
        table.insert(usedItemArray, wrapper);

        return wrapper;
    end
end

---@param recycleTrans CS.UnityEngine.Transform
---@param usedItemArray BaseViewWrapper[]
---@param recycledItemArray BaseViewWrapper[]
function UITool.RecyclePrefabItem(recycleTrans, usedItemArray, recycledItemArray)
    if (#usedItemArray > 0) then
        local wrapper = usedItemArray[#usedItemArray];
        table.remove(usedItemArray, #usedItemArray);
        table.insert(recycledItemArray, wrapper);
        wrapper:GetXRoot().transform:SetParent(recycleTrans);
    end
end

---@generic WrapperClass
---@param targetTrans CS.UnityEngine.Transform
---@param recycleTrans CS.UnityEngine.Transform
---@param recycledItemArray WrapperClass[]
---@param wrapperClass WrapperClass
---@param match string 匹配字符串，targetTrans节点下的物体若名字中包含该字符串，该物体才会被收集；为空时不进行匹配判断
function UITool.CollectFreePrefabItem(targetTrans, recycleTrans, recycledItemArray, wrapperClass, match)
    ---@type CS.UnityEngine.Transform[]
    local childTransArray = {};
    local childCount = targetTrans.childCount;
    for i = 0, (childCount - 1) do
        local tmpObj = targetTrans:GetChild(i).gameObject;
        local pass = true;
        if (match) then
            if (not string.find(tmpObj.name, match)) then
                pass = false;
            end
        end
        if (pass) then
            ---@type BaseViewWrapper
            local wrapper = CCC(wrapperClass);
            wrapper:Init(tmpObj);
            table.insert(recycledItemArray, wrapper);
            table.insert(childTransArray, tmpObj.transform);
        end
    end

    for i = 1, #childTransArray do
        childTransArray[i]:SetParent(recycleTrans);
        childTransArray[i].localScale = CS.UnityEngine.Vector3.one;
    end
end

---@param recycleTrans CS.UnityEngine.Transform
---@param usedItemArray BaseViewWrapper[]
---@param recycledItemArray BaseViewWrapper[]
function UITool.RecycleAllPrefabItem(recycleTrans, usedItemArray, recycledItemArray)
    local count = #usedItemArray;
    for i = 1, count do
        UITool.RecyclePrefabItem(recycleTrans, usedItemArray, recycledItemArray);
    end
end

---@generic WrapperClass
---@param recycleTrans CS.UnityEngine.Transform
---@param recycledItemArray BaseViewWrapper[]
---@param prefab CS.UnityEngine.GameObject
---@param cacheCount number
---@param wrapperClass WrapperClass
function UITool.CachePrefabItem(recycleTrans, recycledItemArray, prefab, cacheCount, wrapperClass)
    for i = 1, cacheCount do
        local tmpObj = CS.UnityEngine.GameObject.Instantiate(prefab, recycleTrans);
        ---@type BaseViewWrapper
        local wrapper = CCC(wrapperClass);
        wrapper:Init(tmpObj);
        table.insert(recycledItemArray, wrapper);
    end
end

---@generic WrapperClass
---@param targetCount number
---@param recycleTrans CS.UnityEngine.Transform
---@param targetTrans CS.UnityEngine.Transform
---@param usedItemArray WrapperClass[]
---@param recycledItemArray WrapperClass[]
---@param prefab CS.UnityEngine.GameObject
---@param wrapperClass WrapperClass
function UITool.AdjustPrefabItemCount(targetCount, recycleTrans, targetTrans, usedItemArray, recycledItemArray, prefab, wrapperClass)
    local diffCount = targetCount - #usedItemArray;

    if (diffCount > 0) then
        for i = 1, diffCount do
            UITool.FetchNewPrefabItem(targetTrans, usedItemArray, recycledItemArray, prefab, wrapperClass);
        end
    else
        for i = 1, -diffCount do
            UITool.RecyclePrefabItem(recycleTrans, usedItemArray, recycledItemArray);
        end
    end
end

---@param wrapperArray BaseViewWrapper[]
---@param destroyView bool 默认false
function UITool.DestroyWrappers(wrapperArray, destroyView)
    if (IsNull(destroyView)) then
        destroyView = false;
    end

    if (destroyView) then
        for i = 1, #wrapperArray do
            wrapperArray[i]:Destroy();
        end
    else
        for i = 1, #wrapperArray do
            wrapperArray[i]:DestroyWithoutView();
        end
    end

end

---@param image CS.UnityEngine.UI.Image
---@param spriteAssetPath string
function UITool.SetImageSprite(image, spriteAssetPath)
    local sprite = LuaAssetsManager.LoadAssetWithClass(spriteAssetPath, CS.UnityEngine.Sprite);
    image.sprite = sprite;
end
---@param trans CS.UnityEngine.Transform
function UITool.ClearAllChildGameObjects(trans)
    if (trans.childCount > 0) then
        for i = trans.childCount - 1, 0, -1 do
            local child = trans:GetChild(i);
            CS.UnityEngine.GameObject.Destroy(child.gameObject);
        end
    end
end

---@param assetPath string
---@param trans CS.UnityEngine.Transform
---@return CS.UnityEngine.GameObject
function UITool.AddPrefab(trans, assetPath)
    local prefab = LuaAssetsManager.LoadGameObjectAsset(assetPath);
    if (NotNull(prefab)) then
        local go = CS.UnityEngine.GameObject.Instantiate(prefab, trans);
        return go;
    else
        LogUtil.Error("prefab is null : " .. assetPath)
        return nil;
    end
end

---@param parentTrans CS.UnityEngine.Transform
function UITool.DestroyChildren(parentTrans)
    local childCount = parentTrans.childCount;
    if (childCount > 0) then
        
        ---@type CS.UnityEngine.Transform[]
        local array = {};
        for i = 0, childCount - 1 do
            table.insert(array, parentTrans:GetChild(i));
        end

        for i = 1, #array do
            CS.UnityEngine.GameObject.Destroy(array[i].gameObject)
        end
    end
    
end

---@param parentTrans CS.UnityEngine.Transform
---@param exclude string @节点名字匹配该字符串将不会被删除
function UITool.DestroyChildrenExclude(parentTrans, exclude)
    local childCount = parentTrans.childCount;
    if (childCount > 0) then
        
        ---@type CS.UnityEngine.Transform[]
        local array = {};
        for i = 0, childCount - 1 do
            local trans = parentTrans:GetChild(i);
            if not (exclude and string.find(trans.gameObject.name, exclude)) then
                table.insert(array, parentTrans:GetChild(i));
            end
        end

        for i = 1, #array do
            CS.UnityEngine.GameObject.Destroy(array[i].gameObject)
        end
    end
    
end

---@param parentTrans CS.UnityEngine.Transform
---@param exclude string @节点名字匹配该字符串将不会被收集
---@return CS.UnityEngine.GameObject[]
function UITool.CollectChildren(parentTrans, exclude)
    local childCount = parentTrans.childCount;
    if (childCount > 0) then
        local array = {};
        for i = 0, childCount - 1 do
            local trans = parentTrans:GetChild(i);
            if not (exclude and string.find(trans.gameObject.name, exclude)) then
                table.insert(array, parentTrans:GetChild(i).gameObject);
            end
        end
        return array;
    else
        return {};
    end
end

---@generic WrapperClass
---@param parentTrans CS.UnityEngine.Transform
---@param exclude string @节点名字匹配该字符串将不会被收集
---@param wrapperClass WrapperClass
---@return WrapperClass[]
function UITool.CollectChildrenWithWrapper(parentTrans, exclude, wrapperClass)
    local objArray = UITool.CollectChildren(parentTrans, exclude);
    
    local wrapperArray = {};
    for i = 1, #objArray do
        ---@type BaseViewWrapper
        local wrapper = CCC(wrapperClass)
        wrapper:Init(objArray[i]);
        table.insert(wrapperArray, wrapper);
    end

    return wrapperArray;
end