using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif

public class WebImageDownloader
{

    public static readonly string LocalFolder = Path.Combine(AppSetting.PersistentDataPath, "webimage");

    private static Dictionary<string, UnityWebRequest> s_remoteRequestDict = new Dictionary<string, UnityWebRequest>();

    private static int s_downloadCount = 0;

    private string _url;
    private string _localFileName;
    private string _localFilePath;
    private Action _callback;
    private Texture _texture;
    private Sprite _sprite;
    private bool _done = false;
    private string _error;
    private MonoBehaviour _coObj;
    private Coroutine _co;

#if UNITY_EDITOR
    UnityWebRequest _editorRequest;
#endif

    public Texture Texture
    {
        get
        {
            return this._texture;
        }
    }

    public Sprite ToSprite()
    {
        if (this._texture)
        {
            if (this._sprite == null)
            {
                this._sprite = Sprite.Create(this._texture as Texture2D, new Rect(0, 0, this._texture.width, this._texture.height), new Vector2(0.5f, 0.5f));
            }
            return this._sprite;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 只要下载结束，无论成败，都算完成
    /// </summary>
    public bool Done
    {
        get
        {
            return this._done;
        }
    }

    public string Error
    {
        get
        {
            return this._error;
        }
    }

    public string Url
    {
        get
        {
            return this._url;
        }
    }

    public string LocalFileName
    {
        get
        {
            return this._localFileName;
        }
    }

    public string LocalFilePath
    {
        get
        {
            return this._localFilePath;
        }
    }

    public static WebImageDownloader Create(string url, MonoBehaviour coroutineObj, Action callback)
    {
        var downloader = new WebImageDownloader();
        downloader._Init(url, coroutineObj, callback);
        return downloader;
    }

    void _Init(string url, MonoBehaviour coroutineObj, Action callback)
    {
        this._url = url;
        this._callback = callback;
        this._localFileName = _MakeLocalFileName(this._url);
        this._localFilePath = Path.Combine(LocalFolder, this._localFileName);
        this._coObj = coroutineObj;
    }


    static string _MakeLocalFileName(string url)
    {
        return SecurityUtil.Md5(Encoding.UTF8.GetBytes(url));
    }

    public static void ClearLocalCache(string url)
    {
        string fileName = _MakeLocalFileName(url);
        string filePath = Path.Combine(LocalFolder, fileName);
        if (FileUtil.IsFileExist(filePath))
        {
            FileUtil.DeleteFile(filePath);
        }
    }

    public static void ClearAllLocalCache()
    {
        if (FileUtil.isDirectoryExist(LocalFolder))
        {
            FileUtil.DeleteDirectory(LocalFolder, true);
        }
    }

    public void Stop()
    {
        this._callback = null;
        if (this._coObj != null && this._co != null)
        {
            this._coObj.StopCoroutine(this._co);
        }
    }

    
    public void StartDownloadImage()
    {
        // todo : 同一个url如果已经存在了，应该搞个缓存机制
        if (!string.IsNullOrEmpty(this._url))
        {
            if (Application.isPlaying)
            {
                if (File.Exists(this._localFilePath))
                {
                    this._co = this._coObj.StartCoroutine(_DownloadLocalImage());
                }
                else
                {
                    this._co = this._coObj.StartCoroutine(_DownloadRemoteImage());
                }
                s_downloadCount += 1;
                if (s_downloadCount > 10)
                {
                    // 临时方案，先不做缓存池了，连着下载多次就主动释放下无用资源
                    s_downloadCount = 0;
                    Resources.UnloadUnusedAssets();
                }
            }
            else
            {
#if UNITY_EDITOR

                EditorDownloadRemoteImage();
#endif
            }
        }

    }
#if UNITY_EDITOR
    void UpdateEditor()
    {
        if (!this._editorRequest.isDone)
        {
            return;
        }
        EditorApplication.update -= UpdateEditor;
        if (string.IsNullOrEmpty(this._editorRequest.error))
        {
            // 读出贴图
            this._texture = (this._editorRequest.downloadHandler as DownloadHandlerTexture).texture;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }
        else
        {
            this._error = this._editorRequest.error;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }

    }

    void EditorDownloadRemoteImage()
    {
        this._editorRequest = UnityWebRequest.Get(this._url);
        var downloadHandlerTexture = new DownloadHandlerTexture();
        this._editorRequest.downloadHandler = downloadHandlerTexture;

        this._editorRequest.SendWebRequest();

        EditorApplication.update += UpdateEditor;
    }
#endif
    IEnumerator _DownloadRemoteImage()
    {
        UnityWebRequest request = null;
        if (s_remoteRequestDict.ContainsKey(this._url))
        {
            // 已经有相同url在下载了的话，就直接等它完成
            request = s_remoteRequestDict[this._url];
            while (!request.isDone)
            {
                yield return 0;
            }
        }
        else
        {
            request = UnityWebRequest.Get(this._url);
            var downloadHandlerTexture = new DownloadHandlerTexture();
            request.downloadHandler = downloadHandlerTexture;
            s_remoteRequestDict[this._url] = request;
            yield return request.SendWebRequest();
        }
        

        if (string.IsNullOrEmpty(request.error))
        {
            // 转存到本地
            if (!Directory.Exists(Path.GetDirectoryName(this._localFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this._localFilePath));
            }
            File.WriteAllBytes(this._localFilePath, request.downloadHandler.data);
            // 读出贴图
            this._texture = (request.downloadHandler as DownloadHandlerTexture).texture;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }
        else
        {
            this._error = request.error;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }
        s_remoteRequestDict.Remove(this._url);
    }

    IEnumerator _DownloadLocalImage()
    {
        UnityWebRequest request = UnityWebRequest.Get("file:///" + this._localFilePath);
        var downloadHandlerTexture = new DownloadHandlerTexture();
        request.downloadHandler = downloadHandlerTexture;
        yield return request.SendWebRequest();
        if (string.IsNullOrEmpty(request.error))
        {
            // 读出贴图
            this._texture = (request.downloadHandler as DownloadHandlerTexture).texture;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }
        else
        {
            this._error = request.error;
            this._done = true;
            if (this._callback != null)
            {
                this._callback();
            }
        }
    }
}
