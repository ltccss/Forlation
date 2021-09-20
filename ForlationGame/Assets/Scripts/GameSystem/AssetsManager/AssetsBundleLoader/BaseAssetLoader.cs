using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseAssetLoader
{

    public abstract T LoadAsset<T>(string assetPath) where T : UnityEngine.Object;
    public abstract UnityEngine.Object LoadAsset(string assetPath, Type type);
    public abstract void LoadAssetAsync(string assetPath, Action<UnityEngine.Object> callback);
    /// <summary>
    /// 当前assets是否可以用
    /// assets所需要的bundle都在本地，视作可用，否则为不可用
    /// </summary>
    /// <returns></returns>
    public abstract bool IsAssetsAvailable(string[] assetPathArray, string[] extraFileArray);

    public abstract bool IsAssetExistInVersion(string assetPath);

    public abstract string[] GetAssetsAllDependencies(string[] assetPathArray);

    public abstract bool IsFileExist(string fileName);

    public abstract FileDownloadHandler DownloadFiles(string[] fileNames, Action finishCallback);

    public virtual void SetLongTermAliveBundles(string[] bundles)
    {

    }

    public abstract void Clear();
}
