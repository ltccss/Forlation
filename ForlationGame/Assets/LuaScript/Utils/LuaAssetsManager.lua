---@class AssetAsyncHandle
---@field _finish bool
---@field _asset CS.UnityEngine.Object

AssetAsyncHandle = DefineClass(AssetAsyncHandle)

function AssetAsyncHandle:_Ctor()
    self._finish = false;
end

function AssetAsyncHandle:_Ctor()
    self.Current = {};
    self.step = 0;
end

---@return CS.UnityEngine.Coroutine
function AssetAsyncHandle:Coroutine()
    return LuaCoroutineUtil.StartCoroutine(self, self._Co);
end

function AssetAsyncHandle:_Co()
    while (not self._finish) do
        coroutine.yield(0);
    end
end

function AssetAsyncHandle:GetAsset()
    return self._asset;
end


---@class LuaAssetsManager
LuaAssetsManager = DefineClass(LuaAssetsManager)

---@generic T : CS.UnityEngine.Object
---@param assetPath string
---@param class T
---@return T
function LuaAssetsManager.LoadAssetWithClass(assetPath, class)
    if (IsNull(class)) then
        LogUtil.Error("请填入class类型")
    end
    return CS.AssetsManager.Me:LoadAsset(assetPath, typeof(class));
end

---@param assetPath string
---@param func fun(obj:CS.UnityEngine.Object)
function LuaAssetsManager.LoadAssetAsync(assetPath, func)
    CS.AssetsManager.Me:LoadAssetSync(assetPath, func);
end

---@param assetPath string
---@return AssetAsyncHandle
function LuaAssetsManager.LoadAssetCo(assetPath)
    local co = CCC(AssetAsyncHandle);
    LuaAssetsManager.LoadAssetAsync(assetPath, function(asset)
        co._asset = asset;
        co._finish = true;
    end)
    return co;
end

---@param assetPath string
---@return CS.UnityEngine.GameObject
function LuaAssetsManager.LoadGameObjectAsset(assetPath)
    return LuaAssetsManager.LoadAssetWithClass(assetPath, CS.UnityEngine.GameObject)
end

---@param assetPath string
---@return CS.UnityEngine.Sprite
function LuaAssetsManager.LoadSpriteAsset(assetPath)
    return LuaAssetsManager.LoadAssetWithClass(assetPath, CS.UnityEngine.Sprite);
end

---@param assetPathArray string[]
---@return CS.System.Boolean @当前assets是否直接可直接加载（依赖都在本地了）
function LuaAssetsManager.IsAssetsAvailable(assetPathArray)
    return CS.AssetsManager.Me:IsAssetsAvailable(assetPathArray);
end

---@param assetPathArray string[]
---@param extraFileArray string[]
---@param finishCallback fun()
---@return CS.FileDownloadHandler
--- 确保asset所需要的各种依赖bundle都在本地，没的话就下载
function LuaAssetsManager.DownloadAssetsAllDependencies(assetPathArray, extraFileArray, finishCallback)
    return CS.AssetsManager.Me:DownloadAssetsAllDependencies(assetPathArray, extraFileArray, finishCallback);
end

---@param assetPath string
---@return bool @asset是否在版本文件清单中(无论在不在本地)
function LuaAssetsManager.IsAssetExistInVersion(assetPath)
    return CS.AssetsManager.Me:IsAssetExistInVersion(assetPath)
end

---@param fileNames string[]
---@param finishCallback fun()
---@return CS.FileDownloadHandler
function LuaAssetsManager.DownloadFiles(fileNames, finishCallback)
    
    return CS.AssetsManager.Me:DownloadFiles(fileNames, finishCallback)
end

---@param assetPathArray string[]
function LuaAssetsManager.GetAssetsAllDependencies(assetPathArray)
return CS.AssetsManager.Me:GetAssetsAllDependencies(assetPathArray)
end

---@param fileName string
function LuaAssetsManager.IsFileExist(fileName)
    return CS.AssetsManager.Me:IsFileExist(fileName)
end