using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using SimpleJson;
using UnityEngine.Networking;

// TODO: 目前还没实现打包和热更部分，仅供参考

public class DeviceAssetLoader : BaseAssetLoader
{
    /// <summary>
    /// asset加载历史，根据一段时间内asset重复加载的情况，决定其后续加载时给予的生命周期长度
    /// 简单来说，加载次数越多的asset，其生命周期越长
    /// </summary>
    class AssetLoadHistory
    {
        static float RefreshInterval = 60f * 3f; // 刷新间隔,单位：秒
        static int ConsumeCount = 10 * 3; // 每次刷新历史，扣除的加载次数
        static int MaxLiveFrame = 60 * 1 * 60;
        static Dictionary<string, int> assetPathToLoadCountDict = new Dictionary<string, int>();
        static float lastCheckTime = 0;

        static List<string> waitToRemoveKeyList = new List<string>();

        // 刷新历史
        public static void Refresh()
        {
            if (Time.realtimeSinceStartup > lastCheckTime + RefreshInterval)
            {
                lastCheckTime = Time.realtimeSinceStartup;
                waitToRemoveKeyList.Clear();
                foreach (var kvp in assetPathToLoadCountDict)
                {
                    int loadCount = kvp.Value;
                    loadCount -= ConsumeCount;
                    if (loadCount <= 0)
                    {
                        waitToRemoveKeyList.Add(kvp.Key);
                    }
                    else
                    {
                        assetPathToLoadCountDict[kvp.Key] = loadCount;
                    }
                }

                for (int i = 0; i < waitToRemoveKeyList.Count; i++)
                {
                    assetPathToLoadCountDict.Remove(waitToRemoveKeyList[i]);
                }
            }
        }

        // 记录一次加载
        public static void Record(string assetPath)
        {
            if (assetPathToLoadCountDict.ContainsKey(assetPath))
            {
                int count = assetPathToLoadCountDict[assetPath];
                count += 1;
                count = Math.Min(count, ConsumeCount);
                assetPathToLoadCountDict[assetPath] = count;
            }
            else
            {
                assetPathToLoadCountDict.Add(assetPath, 1);
            }
        }

        // 根据一段时间内记录的加载次数，计算其生存时间（帧）
        public static int GetLiveFrame(string assetPath)
        {
            if (assetPathToLoadCountDict.ContainsKey(assetPath))
            {
                int count = assetPathToLoadCountDict[assetPath];
                return count * MaxLiveFrame / ConsumeCount;
            }
            else
            {
                return 60;
            }
        }
    }
    class AssetCache
    {
        public string assetPath;
        public UnityEngine.Object asset;
        public int liveFrame = 0;

        public void UpdateLife()
        {
            AssetLoadHistory.Record(this.assetPath);
            this.liveFrame = Time.frameCount + AssetLoadHistory.GetLiveFrame(this.assetPath);
        }
    }
    class BundleCache
    {
        public string bundleName;
        public AssetBundle bundle;
        public int liveFrame = 0;
        public Dictionary<string, AssetRequestCache> assetRequestDict = new Dictionary<string, AssetRequestCache>();

        public void UpdateLife()
        {
            this.liveFrame = Time.frameCount + 72;
        }
    }
    class AssetRequestCache
    {
        public string assetPath;
        public AssetBundleRequest request;
        public Action<UnityEngine.Object> finishCallback;
    }

    static readonly int MaxRequestCount = 3;
    static readonly int MaxRetryCount = 20;
    static readonly int LongTermAliveFrameCount = 60 * 3600;

    private Dictionary<string, AssetBundleManifest> _bundleNameToManifestDict;

    private Dictionary<string, string> _assetPathToBundleNameDict;

    private LinkedList<RequestHandler> _requestHandlerList; // 正在请求的队列
    private Queue<string> _waitedFileQueue; // 等待下载的file
    private HashSet<string> _downloadingFileSet; // 处于等待下载或者正在下载中的file
    private List<FileDownloadHandler> _downloadHandlerList; // 未完成的下载

    private Dictionary<string, AssetCache> _assetCacheDict;
    private Dictionary<string, BundleCache> _bundleCacheDict;

    private HashSet<string> _longTermAliveBundleSet;

    private Dictionary<string, string> _downloadFileMap;
    private Dictionary<string, int> _downloadFileSizeDict;

    private string _fileRootPath;

    private Coroutine _downloadCo;
    private Coroutine _cacheCo;

    private float _lastGcTime = 0f;

    public DeviceAssetLoader()
    {
        this._fileRootPath = AppSetting.GameCachePath;

        this._assetPathToBundleNameDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        this._bundleNameToManifestDict = new Dictionary<string, AssetBundleManifest>();


        this._downloadFileMap = new Dictionary<string, string>();
        this._downloadFileSizeDict = new Dictionary<string, int>();


        for (int i = 0; i < AppSetting.resCollectionNames.Length; i++)
        {
            string collectionName = AppSetting.resCollectionNames[i];

            // assetInfoName是一个记录unity一次打包后所有AssetPath对应BundleName的映射json
            // 结构是： {"bundleName1": ["AssetPath1", "AssetPath2"], "bundleName2": ["AssetPath3", "AssetPath4"], ...}
            string assetInfoName = collectionName + "_assets_info.json";

            // manifestAssetName是unity一次打包后生成的包含打包信息的ManifestAsset所在的bundle文件名
            string manifestAssetName = collectionName + "_info";

            // json文件记录的是unity一次打包后生成的bundle文件的映射
            // 结构是: ["文件名1|md5|size", "文件名2|md5|size", ...]
            string jsonName = collectionName + ".json";

            this._LoadAssetPathInfo(assetInfoName);
            this._LoadManifest(manifestAssetName);
            this._LoadFileMapList(jsonName);
        }

        this._requestHandlerList = new LinkedList<RequestHandler>();
        this._waitedFileQueue = new Queue<string>();
        this._downloadingFileSet = new HashSet<string>();
        this._downloadHandlerList = new List<FileDownloadHandler>();

        this._assetCacheDict = new Dictionary<string, AssetCache>();
        this._bundleCacheDict = new Dictionary<string, BundleCache>();

        this._longTermAliveBundleSet = new HashSet<string>();

        this._downloadCo = CoroutineUtil.StartCoroutine(this._UpdateDownload());
        this._cacheCo = CoroutineUtil.StartCoroutine(this._UpdateCache());

        
    }

    public override void Clear()
    {
        if (this._downloadCo != null)
        {
            CoroutineUtil.StopCoroutine(this._downloadCo);
        }
        if (this._cacheCo != null)
        {
            CoroutineUtil.StopCoroutine(this._cacheCo);
        }
        foreach (var kvp in _bundleCacheDict)
        {
            kvp.Value.bundle.Unload(true);
        }
    }

    private void _LoadAssetPathInfo(string assetFileName)
    {
        string assetJsonCompressFilePath = Path.Combine(this._fileRootPath, assetFileName);
        string json = File.ReadAllText(assetJsonCompressFilePath);
        JsonObject infoJson = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(json);

        foreach (var kvp in infoJson)
        {
            string bundleName = kvp.Key;
            JsonArray assetObjArray = (JsonArray)infoJson[kvp.Key];
            for (int k = 0; k < assetObjArray.Count; k++)
            {
                string assetPath = Convert.ToString(assetObjArray[k]);
                this._assetPathToBundleNameDict[assetPath] = bundleName;
            }
        }
    }

    private void _LoadFileMapList(string fileName)
    {
        string filePath = Path.Combine(this._fileRootPath, fileName);
        string json = File.ReadAllText(filePath);
        JsonArray infoJson = (JsonArray)SimpleJson.SimpleJson.DeserializeObject(json);

        for (int i = 0; i < infoJson.Count; i++)
        {
            string line = Convert.ToString(infoJson[i]);
            string[] words = line.Split('|');

            string itemName = words[0];
            string remoteItemName = words[0] + "_" + words[1];
            int size = Convert.ToInt32(words[2]);
            if (!this._downloadFileMap.ContainsKey(itemName))
            {
                this._downloadFileMap[itemName] = remoteItemName;
            }
            if (!this._downloadFileSizeDict.ContainsKey(itemName))
            {
                this._downloadFileSizeDict[itemName] = size;
            }
        }
    }

    private void _LoadManifest(string manifestName)
    {
        // todo
        string manifestPath = Path.Combine(this._fileRootPath, manifestName);

        AssetBundle bundle = AssetBundle.LoadFromFile(manifestPath);
        var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        var bundleNames = manifest.GetAllAssetBundles();
        for (int i = 0; i < bundleNames.Length; i++)
        {
            if (!this._bundleNameToManifestDict.ContainsKey(bundleNames[i]))
            {
                this._bundleNameToManifestDict[bundleNames[i]] = manifest;
            }
        }

        // 压缩包直接释放掉
        bundle.Unload(false);
        bundle = null;

    }

    private string _FindBundleName(string assetPath)
    {
#if DEBUG
        if (assetPath.IndexOf('\\') >= 0)
        {
            LogUtil.ErrorFormat("FindBundleName: assetPath里不能含有反斜杠 {0}", assetPath);
        }
#endif
        
        if (this._assetPathToBundleNameDict.ContainsKey(assetPath))
        {
            return this._assetPathToBundleNameDict[assetPath].ToLower();
        }
        else
        {
            LogUtil.ErrorFormat("FindBundleName: 该assetPath找不到其对应的bundleName {0}", assetPath);
            return null;
        }
    }

    public override T LoadAsset<T>(string assetPath)
    {
        return this.LoadAsset(assetPath, typeof(T)) as T;
    }

    public override UnityEngine.Object LoadAsset(string assetPath, Type type)
    {
        if (this._assetCacheDict.ContainsKey(assetPath))
        {
            AssetCache assetCache = this._assetCacheDict[assetPath];
            assetCache.UpdateLife();
            return assetCache.asset;
        }
        string bundleName = this._FindBundleName(assetPath);

        var bundle = this._PrepareBundleWithDependencies(bundleName);

        var asset = bundle.LoadAsset(assetPath, type);

        AssetCache newAssetCache = new AssetCache();
        newAssetCache.asset = asset;
        newAssetCache.assetPath = assetPath;
        newAssetCache.UpdateLife();
        this._assetCacheDict[assetPath] = newAssetCache;

        return asset;
    }

    public override void LoadAssetAsync(string assetPath, Action<UnityEngine.Object> callback)
    {
        // 检查asset缓存里有没有现成的
        if (this._assetCacheDict.ContainsKey(assetPath))
        {
            AssetCache assetCache = this._assetCacheDict[assetPath];
            assetCache.UpdateLife();
            if (callback != null)
            {
                callback(assetCache.asset);
            }
            return;
        }

        string bundleName = this._FindBundleName(assetPath);

        // 检查bundle缓存里有没有正在异步下载的
        if (this._bundleCacheDict.ContainsKey(bundleName))
        {
            BundleCache bundleCache = this._bundleCacheDict[bundleName];
            if (bundleCache.assetRequestDict.ContainsKey(assetPath))
            {
                bundleCache.assetRequestDict[assetPath].finishCallback += callback;
                return;
            }
        }

        // 各种缓存里都找不到，就开始加载
        // 先准备bundle，lz4格式的bundle就不异步了
        var bundle = this._PrepareBundleWithDependencies(bundleName);

        var request = bundle.LoadAssetAsync(assetPath);
        AssetRequestCache requestCache = new AssetRequestCache();
        requestCache.assetPath = assetPath;
        requestCache.finishCallback = callback;
        requestCache.request = request;

        BundleCache bundleCache2 = this._bundleCacheDict[bundleName];
        bundleCache2.assetRequestDict.Add(assetPath, requestCache);

        CoroutineUtil.StartCoroutine(_LoadAssetDelay(requestCache, bundleCache2));
    }

    IEnumerator _LoadAssetDelay(AssetRequestCache assetRequestCache, BundleCache bundleCache)
    {
        yield return assetRequestCache.request;
        bundleCache.assetRequestDict.Remove(assetRequestCache.assetPath);
        bundleCache.liveFrame = Time.frameCount + 70;

        if (this._assetCacheDict.ContainsKey(assetRequestCache.assetPath))
        {
            // 可能异步加载的时间内，有其他同步的操作加载了这个asset
            this._assetCacheDict[assetRequestCache.assetPath].UpdateLife();
        }
        else
        {
            // todo :考虑要不要把assetRequestCache.request.allAssets也加入缓存?
            AssetCache newAssetCache = new AssetCache(); 
            newAssetCache.asset = assetRequestCache.request.asset;
            newAssetCache.assetPath = assetRequestCache.assetPath;
            newAssetCache.UpdateLife();
            this._assetCacheDict[assetRequestCache.assetPath] = newAssetCache;
        }
        

        assetRequestCache.finishCallback(assetRequestCache.request.asset);
    }

    /// <summary>
    /// 当前assets是否可以用
    /// assets所需要的bundle都在本地，视作可用，不然就不可用
    /// </summary>
    /// <returns></returns>
    public override bool IsAssetsAvailable(string[] assetPathArray, string[] extraFileArray)
    {
        if (extraFileArray != null)
        {
            for (int i = 0; i < extraFileArray.Length; i++)
            {
                if (!this.IsFileExist(extraFileArray[i]))
                {
                    return false;
                }
            }
        }

        HashSet<string> bundleNameSet = new HashSet<string>();
        // 获取所有依赖的bundle
        for (int i = 0; i < assetPathArray.Length; i++)
        {
            if (!this.IsAssetExistInVersion(assetPathArray[i]))
            {
                // asset不在版本里直接返回false
                return false;
            }
            string[] bundleNames = this._GetAllDependentBundles(assetPathArray[i]);
            for (int k = 0; k < bundleNames.Length; k++)
            {
                if (!bundleNameSet.Contains(bundleNames[k]))
                {
                    bundleNameSet.Add(bundleNames[k]);
                }
            }
        }

        // 剔除掉本地已经有的
        bundleNameSet.RemoveWhere((bundleName) =>
        {
            return this.IsFileExist(bundleName);
        });
        
        // 没有缺的bundle，就是可用的
        return bundleNameSet.Count == 0;
    }

    public override string[] GetAssetsAllDependencies(string[] assetPathArray)
    {
        HashSet<string> fileNameSet = new HashSet<string>();
        // 获取所有依赖的bundle
        for (int i = 0; i < assetPathArray.Length; i++)
        {
            string[] fileNames = this._GetAllDependentBundles(assetPathArray[i]);
            for (int k = 0; k < fileNames.Length; k++)
            {
                if (!fileNameSet.Contains(fileNames[k]))
                {
                    fileNameSet.Add(fileNames[k]);
                }
            }
        }

        string[] result = new string[fileNameSet.Count];
        fileNameSet.CopyTo(result);

        return result;
    }

    public override FileDownloadHandler DownloadFiles(string[] fileNames, Action finishCallback)
    {
        int[] sizeArray = new int[fileNames.Length];
        for (int i = 0; i < fileNames.Length; i++)
        {
            sizeArray[i] = this._downloadFileSizeDict[fileNames[i]];
        }
        var downloadHandler = new FileDownloadHandler(fileNames, sizeArray, finishCallback);
        this._downloadHandlerList.Add(downloadHandler);
        for (int i = 0; i < fileNames.Length; i++)
        {
            if (!this._downloadingFileSet.Contains(fileNames[i]))
            {
                this._downloadingFileSet.Add(fileNames[i]);
                this._waitedFileQueue.Enqueue(fileNames[i]);
            }
        }
        foreach (var handler in this._requestHandlerList)
        {
            // 已经在下载的尝试把下载句柄保存下来
            downloadHandler.StartDownloadFile(handler.fileName, handler);
        }

        return downloadHandler;
    }

    private IEnumerator _UpdateDownload()
    {
        while (true)
        {
            // 暂定每帧只发送一个下载请求
            if (this._requestHandlerList.Count < MaxRequestCount)
            {
                if (this._waitedFileQueue.Count > 0)
                {
                    var bundleName = this._waitedFileQueue.Dequeue();

                    string remoteFileName = this._downloadFileMap[bundleName];

                    // resUrl = "http://hostname.com/res/{0}"
                    UnityWebRequest request = UnityWebRequest.Get(string.Format(AppSetting.resUrl, remoteFileName));
                    request.timeout = 10;

                    var requestHandler = new RequestHandler();
                    requestHandler.request = request;
                    requestHandler.fileName = bundleName;
                    requestHandler.errorCount = 0;
                    requestHandler.requestDisposed = false;
                    requestHandler.isDone = false;

                    requestHandler.StartRequest();

                    this._requestHandlerList.AddLast(requestHandler);

                    // 通知downloadHandler, 文件要下载了
                    // todo : 下载任务超多的情况下downloadHandler和file name之间应该做个缓冲映射，而不是直接遍历_downloadHandlerList
                    for (int i = 0; i < this._downloadHandlerList.Count; i++)
                    {
                        this._downloadHandlerList[i].StartDownloadFile(requestHandler.fileName, requestHandler);
                    }
                }
            }

            // 暂定每帧只处理一个完成的下载
            if (this._requestHandlerList.Count > 0)
            {
                var node = this._requestHandlerList.First;
                while (node != null)
                {
                    var requestHandler = node.Value;
                    bool isRequestFinish = true;
                    if (requestHandler.request.isDone)
                    {
                        // 处理下载完成的bundle
                        if (string.IsNullOrEmpty(requestHandler.request.error))
                        {
                            // 将bundle保存在本地
                            string targetFilePath = Path.Combine(AppSetting.GameCachePath, requestHandler.fileName);
                            using (var fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(requestHandler.request.downloadHandler.data, 0, requestHandler.request.downloadHandler.data.Length);
                            }

                            // 通知downloadHandler, bundle下载完成了
                            // todo : 下载任务超多的情况下downloadHandler和file name之间应该做个缓冲映射，而不是直接遍历_downloadHandlerList
                            for (int i = 0; i < this._downloadHandlerList.Count; i++)
                            {
                                this._downloadHandlerList[i].FinishDownloadFile(requestHandler.fileName);
                            }

                            // 删除任务完成的downloadHandler
                            for (int i = this._downloadHandlerList.Count - 1; i >= 0; i--)
                            {
                                if (this._downloadHandlerList[i].HasFinished())
                                {
                                    this._downloadHandlerList.RemoveAt(i);
                                }
                            }
                            requestHandler.isDone = true;
                        }
                        else
                        {
                            // todo: 出错处理
                            LogUtil.WarnFormat("_UpdateDownload: error when download bundle{0}, {1}", requestHandler.fileName, requestHandler.request.error);
                            requestHandler.errorCount += 1;
                            if (requestHandler.errorCount <= MaxRetryCount)
                            {
                                requestHandler.request.Dispose();

                                string remoteFileName = this._downloadFileMap[requestHandler.fileName];

                                UnityWebRequest newRequest = UnityWebRequest.Get(string.Format("{0}/res/{1}", AppSetting.resUrl, remoteFileName));
                                requestHandler.request = newRequest;
                                isRequestFinish = false;

                                LogUtil.WarnFormat("_UpdateDownload: retry download bundle {0}", requestHandler.fileName);

                                requestHandler.isDone = false;

                                requestHandler.StartRequest();
                            }
                        }

                        if (isRequestFinish)
                        {
                            requestHandler.request.Dispose();
                            this._requestHandlerList.Remove(node);
                            this._downloadingFileSet.Remove(requestHandler.fileName);

                            requestHandler.requestDisposed = true;
                        }
                        
                        break;
                    }

                    node = node.Next;
                }
            }
            

            yield return 0;
        }
    }

    /// <summary>
    /// 缓存逻辑
    /// </summary>
    /// <returns></returns>
    private IEnumerator _UpdateCache()
    {
        // 这里缓存设计目标是保证短期频繁读取效率，长期逐渐把内存交出去
        // 当asset或者bundle被使用到时，延长其在缓存中的存活周期，到期就移出缓存
        // 如果有特殊需求需要长期缓存asset，这个应该由上层逻辑去实现自己的缓存池
        while (true)
        {
            if (this._assetCacheDict.Count > 0)
            {
                string[] assetCacheKeys = new string[this._assetCacheDict.Count];
                this._assetCacheDict.Keys.CopyTo(assetCacheKeys, 0);
                int assetIterateCount = 0;
                bool hasRemoveAsset = false;
                for (int i = 0; i < assetCacheKeys.Length; i++)
                {
                    string key = assetCacheKeys[i];
                    if (this._assetCacheDict.ContainsKey(key))
                    {
                        var assetCache = this._assetCacheDict[key];
                        if (assetCache.liveFrame < Time.frameCount)
                        {
                            this._assetCacheDict.Remove(key);
                            hasRemoveAsset = true;
                        }
                        assetIterateCount += 1;

                    }
                    if (assetIterateCount > 5)
                    {
                        assetIterateCount = 0;
                        yield return 0;
                    }
                }

                if (hasRemoveAsset)
                {
                    if ((this._lastGcTime + 90) < Time.realtimeSinceStartup)
                    {
                        this._lastGcTime = Time.realtimeSinceStartup;
                        yield return Resources.UnloadUnusedAssets();
                    }
                }
            }
            
            if (this._bundleCacheDict.Count > 0)
            {
                string[] bundleCacheKeys = new string[this._assetCacheDict.Count];
                this._assetCacheDict.Keys.CopyTo(bundleCacheKeys, 0);
                int bundleIterateCount = 0;
                for (int i = 0; i < bundleCacheKeys.Length; i++)
                {
                    string key = bundleCacheKeys[i];
                    if (this._bundleCacheDict.ContainsKey(key))
                    {
                        var bundleCache = this._bundleCacheDict[key];
                        if (bundleCache.liveFrame < Time.frameCount && bundleCache.assetRequestDict.Count == 0)
                        {
                            bundleCache.bundle.Unload(false);
                            this._bundleCacheDict.Remove(key);
                        }
                    }
                    bundleIterateCount += 1;
                    if (bundleIterateCount > 2)
                    {
                        bundleIterateCount = 0;
                        yield return 0;
                    }
                }
            }

            yield return 0;

            AssetLoadHistory.Refresh();

            yield return 0;
        }
    }

    private string[] _GetAllDependencies(string bundleName)
    {
        AssetBundleManifest manifest = this._bundleNameToManifestDict[bundleName];
        return manifest.GetAllDependencies(bundleName);
    }

    /// <summary>
    /// 获取asset所依赖的所有bundle
    /// </summary>
    /// <param name="assetPath"></param>
    private string[] _GetAllDependentBundles(string assetPath)
    {
        string bundleName = this._FindBundleName(assetPath);
        string[] dependentBundleNamsArray = this._GetAllDependencies(bundleName);
        if (dependentBundleNamsArray != null && dependentBundleNamsArray.Length > 0)
        {
            string[] resultArray = new string[dependentBundleNamsArray.Length + 1];
            Array.Copy(dependentBundleNamsArray, 0, resultArray, 1, dependentBundleNamsArray.Length);
            resultArray[0] = bundleName;
            return resultArray;
        }
        else
        {
            return new string[] { bundleName };
        }
    }

    /// <summary>
    /// 准备bundle以及所有它依赖的bundle
    /// 如果需要的bundle本地并没有，会凉凉
    /// </summary>
    /// <param name="bundleName"></param>
    private AssetBundle _PrepareBundleWithDependencies(string bundleName)
    {
        // 目前的缓存机制不能保证bundle已加载时，其依赖的bundle一定也加载上了
        // 所以需要挨个确认所有bundle都在不在
        string[] dependentBundleNamsArray = this._GetAllDependencies(bundleName);

        if (dependentBundleNamsArray != null)
        {
            for (int i = 0; i < dependentBundleNamsArray.Length; i++)
            {
                if (!this._bundleCacheDict.ContainsKey(dependentBundleNamsArray[i]))
                {
                    AssetBundle dependentBundle = this._LoadBundle(dependentBundleNamsArray[i]);
                    BundleCache dependentBundleCache = new BundleCache();
                    dependentBundleCache.bundleName = dependentBundleNamsArray[i];
                    dependentBundleCache.bundle = dependentBundle;
                    if (this._longTermAliveBundleSet.Contains(dependentBundleNamsArray[i]))
                    {
                        dependentBundleCache.liveFrame = Time.frameCount + LongTermAliveFrameCount;
                    }
                    else
                    {
                        dependentBundleCache.liveFrame = Time.frameCount + 70;
                    }
                    
                    this._bundleCacheDict[dependentBundleNamsArray[i]] = dependentBundleCache;
                }
                else
                {
                    BundleCache dependentBundleCache = this._bundleCacheDict[dependentBundleNamsArray[i]];
                    if (this._longTermAliveBundleSet.Contains(dependentBundleNamsArray[i]))
                    {
                        dependentBundleCache.liveFrame = Time.frameCount + LongTermAliveFrameCount;
                    }
                    else
                    {
                        dependentBundleCache.liveFrame = Time.frameCount + 70;
                    }
                    
                }
            }
        }

        AssetBundle bundle;
        if (!this._bundleCacheDict.ContainsKey(bundleName))
        {
            bundle = this._LoadBundle(bundleName);
            BundleCache bundleCache = new BundleCache();
            bundleCache.bundleName = bundleName;
            bundleCache.bundle = bundle;
            if (this._longTermAliveBundleSet.Contains(bundleName))
            {
                bundleCache.liveFrame = Time.frameCount + LongTermAliveFrameCount;
            }
            else
            {
                bundleCache.liveFrame = Time.frameCount + 60;
            }
            
            this._bundleCacheDict[bundleName] = bundleCache;
        }
        else
        {
            BundleCache bundleCache = this._bundleCacheDict[bundleName];
            bundle = bundleCache.bundle;
            if (this._longTermAliveBundleSet.Contains(bundleName))
            {
                bundleCache.liveFrame = Time.frameCount + LongTermAliveFrameCount;
            }
            else
            {
                bundleCache.liveFrame = Time.frameCount + 60;
            }
        }

        return bundle;
    }

    /// <summary>
    /// file是否在本地存在
    /// </summary>
    /// <param name="fileName"></param>
    public override bool IsFileExist(string fileName)
    {
        // todo : 做缓存避免io？
        string filePath = Path.Combine(this._fileRootPath, fileName);
        return File.Exists(filePath);
    }

    private AssetBundle _LoadBundle(string bundleName)
    {
        string bundlePath = Path.Combine(this._fileRootPath, bundleName);

        if (!File.Exists(bundlePath))
        {
            LogUtil.Error("bundle文件在本地不存在，请确定使用相关assets时所需依赖是否都下载到本地了？ " + bundlePath);
        }

        var bundle = AssetBundle.LoadFromFile(bundlePath);

        return bundle;
    }

    public override bool IsAssetExistInVersion(string assetPath)
    {
        return this._assetPathToBundleNameDict.ContainsKey(assetPath);
    }

    /// <summary>
    /// 设置长期存活的bundle名单
    /// </summary>
    /// <param name="bundles"></param>
    public override void SetLongTermAliveBundles(string[] bundles)
    {
        this._longTermAliveBundleSet.Clear();

        for (int i = 0; i < bundles.Length; i++)
        {
            if (!this._longTermAliveBundleSet.Contains(bundles[i]))
            {
                this._longTermAliveBundleSet.Add(bundles[i]);
            }
        }
    }
}
