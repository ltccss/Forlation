using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AssetsManager
{
    private static AssetsManager _me;
    public static AssetsManager Me
    {
        get
        {
            return _me;
        }
    }

    public static void Init()
    {
        if (_me != null && _me._assetLoader != null)
        {
            _me._assetLoader.Clear();
        }
        _me = new AssetsManager();
    }

    private BaseAssetLoader _assetLoader;

    public AssetsManager()
    {
        if (AppSetting.simulatePhoneMode)
        {
            // TODO: DeviceAssetLoader
            this._assetLoader = new EditorAssetLoader();
        }
        else
        {
            this._assetLoader = new EditorAssetLoader();
        }
    }

    public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        return this._assetLoader.LoadAsset<T>(assetPath);
    }

    public UnityEngine.Object LoadAsset(string assetPath, Type type)
    {
        return this._assetLoader.LoadAsset(assetPath, type);
    }

    public void LoadAssetSync(string assetPath, Action<UnityEngine.Object> callback)
    {
        this._assetLoader.LoadAssetAsync(assetPath, callback);
    }

    public bool IsAssetsAvailable(string[] assetPathArray, string[] extraFileArray)
    {
        return this._assetLoader.IsAssetsAvailable(assetPathArray, extraFileArray);
    }

    public bool IsAssetExistInVersion(string assetPath)
    {
        return this._assetLoader.IsAssetExistInVersion(assetPath);
    }

    public bool IsFileExist(string fileName)
    {
        return this._assetLoader.IsFileExist(fileName);
    }

    public FileDownloadHandler DownloadAssetsAllDependencies(string[] assetPathArray, string[] extraFileArray, Action finishCallback)
    {
        List<string> fileList = new List<string>();

        string[] bundleNames = this._assetLoader.GetAssetsAllDependencies(assetPathArray);

        fileList.AddRange(bundleNames);

        for (int i = 0; i < extraFileArray.Length; i++)
        {
            if (!fileList.Contains(extraFileArray[i]))
            {
                fileList.Add(extraFileArray[i]);
            }
        }

        // 剔除掉本地已经有的
        for (int i = fileList.Count - 1; i >= 0; i--)
        {
            if (this._assetLoader.IsFileExist(fileList[i]))
            {
                fileList.RemoveAt(i);
            }
        }

        if (fileList.Count == 0)
        {
            if (finishCallback != null)
            {
                finishCallback();
            }
            return new FileDownloadHandler(null, null, finishCallback);
        }

        return this._assetLoader.DownloadFiles(fileList.ToArray(), finishCallback);
    }

    public FileDownloadHandler DownloadFiles(string[] fileNames, Action finishCallback)
    {
        return this._assetLoader.DownloadFiles(fileNames, finishCallback);
    }

    public string[] GetAssetsAllDependencies(string[] assetPathArray)
    {
        return this._assetLoader.GetAssetsAllDependencies(assetPathArray);
    }

    /// <summary>
    /// 设置长期存活的bundle名单
    /// </summary>
    public void SetLongTermAliveBundleList(string[] bundles)
    {
        this._assetLoader.SetLongTermAliveBundles(bundles);
    }
}


