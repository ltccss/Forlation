---@class LocalStorage
LocalStorage = DefineClass(LocalStorage)

-- 暂时先离散化存储数据了
-- todo : 不同用户不同数据？
local Store_Dir = CS.System.IO.Path.Combine(CS.AppSetting.PersistentDataPath, "_local_data")

if (not CS.FileUtil.isDirectoryExist(Store_Dir)) then
    CS.FileUtil.CreateDirectory(Store_Dir);
end

local function _MakeItemFilePath(key)
    return CS.System.IO.Path.Combine(Store_Dir, key)
end

function LocalStorage.Init()
end

---@param key string
---@return string
function LocalStorage.GetItem(key)
    local fullPath = _MakeItemFilePath(key);
    if (CS.FileUtil.IsFileExist(fullPath)) then
        return CS.FileUtil.ReadAllText(fullPath);
    else
        return nil;
    end
end

---@param key string
---@param text string
function LocalStorage.SetItem(key, text)
    local fullPath = _MakeItemFilePath(key)
    CS.FileUtil.WriteAllText(fullPath, text)
end

function LocalStorage.RemoveItem(key)
    local fullPath = _MakeItemFilePath(key)
    if (CS.FileUtil.IsFileExist(fullPath)) then
        CS.FileUtil.DeleteFile(fullPath);
    end
end

function LocalStorage.ClearAll()
    if (CS.FileUtil.isDirectoryExist(Store_Dir)) then
        CS.FileUtil.DeleteDirectory(Store_Dir, true);
        CS.FileUtil.CreateDirectory(Store_Dir);
    end
end

