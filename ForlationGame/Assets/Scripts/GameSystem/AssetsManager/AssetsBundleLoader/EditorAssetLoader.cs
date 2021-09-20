using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

class EditorAssetLoader : BaseAssetLoader
{

    public override T LoadAsset<T>(string assetPath)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
        throw new NotImplementedException();
#endif
    }

    public override UnityEngine.Object LoadAsset(string assetPath, Type type)
    {
#if UNITY_EDITOR
        //Debug.Log(">>>> assetPath " + assetPath);
        return AssetDatabase.LoadAssetAtPath(assetPath, type);
#else
        throw new NotImplementedException();
#endif
    }

    public override void LoadAssetAsync(string assetPath, Action<UnityEngine.Object> callback)
    {
#if UNITY_EDITOR
        CoroutineUtil.StartCoroutine(_LoadAssetDelay(assetPath, callback));
#else
        throw new NotImplementedException();
#endif
    }

    IEnumerator _LoadAssetDelay(string assetPath, Action<UnityEngine.Object> callback)
    {
        yield return new WaitForEndOfFrame();
        if (callback != null)
        {
            callback(this.LoadAsset<UnityEngine.Object>(assetPath));
        }
    }

    public override bool IsAssetsAvailable(string[] assetPathArray, string[] extraFileArray)
    {
#if UNITY_EDITOR
        return true;
#else
        throw new NotImplementedException();
#endif
    }

    public override bool IsAssetExistInVersion(string assetPath)
    {
        return File.Exists(Path.Combine(Application.dataPath, "..", assetPath));
    }

    public override FileDownloadHandler DownloadFiles(string[] fileNames, Action finishCallback)
    {
#if UNITY_EDITOR
        if (finishCallback != null)
        {
            finishCallback();
        }
#else
        throw new NotImplementedException();
#endif
        // 不会吧不会吧，editor模式下这个返回值有人会用？
        return new FileDownloadHandler(fileNames, new int[fileNames.Length], finishCallback);
    }

    public override string[] GetAssetsAllDependencies(string[] assetPathArray)
    {
#if UNITY_EDITOR
        HashSet<string> bundleSet = new HashSet<string>();
        for (int i = 0; i < assetPathArray.Length; i++)
        {
            string bundleName = AssetDatabase.GetImplicitAssetBundleName(assetPathArray[i]);
            if (!bundleSet.Contains(bundleName))
            {
                bundleSet.Add(bundleName);
            }
        }

        string[] result = new string[bundleSet.Count];
        bundleSet.CopyTo(result);
        return result;
#else
        throw new NotImplementedException();
#endif
    }

    public override bool IsFileExist(string fileName)
    {
#if UNITY_EDITOR
        string extName = Path.GetExtension(fileName);
        if (extName == ".bnk")
        {
            return File.Exists(Path.Combine(Application.dataPath, "Audio/GeneratedSoundBanks/Android", fileName));
        }
        else
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (bundleNames[i] == fileName)
                {
                    return true;
                }
            }
            return false;
        }
#else
        throw new NotImplementedException();
#endif
    }

    public override void Clear()
    {
        
    }
}
