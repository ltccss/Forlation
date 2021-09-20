using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FileDownloadHandler
{
    private Action _finishCallback;

    private Dictionary<string, int> _finishedFileDict;
    private Dictionary<string, int> _unfinishFileDict;

    private Dictionary<string, RequestHandler> _downloadingFileDict;

    private int _totalSize;
    private int _totalFileCount;

    private int _finishedFileSize; // 处于下载中的文件大小不计入

    public FileDownloadHandler(string[] fileNames, int[] sizeArray, Action finishCallback)
    {
        if (fileNames != null)
        {
            this._finishCallback = finishCallback;

            this._finishedFileDict = new Dictionary<string, int>();
            this._unfinishFileDict = new Dictionary<string, int>();

            this._downloadingFileDict = new Dictionary<string, RequestHandler>();

            this._totalSize = 0;
            this._finishedFileSize = 0;
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!this._unfinishFileDict.ContainsKey(fileNames[i]))
                {
                    this._unfinishFileDict.Add(fileNames[i], sizeArray[i]);
                    this._totalSize += sizeArray[i];
                }
            }
            this._totalFileCount = this._unfinishFileDict.Count;
        }
        else
        {
            this._finishedFileDict = new Dictionary<string, int>();
            this._unfinishFileDict = new Dictionary<string, int>();

            this._downloadingFileDict = new Dictionary<string, RequestHandler>();

            this._totalSize = 0;
            this._finishedFileSize = 0;
        }
        
    }

    public int TotalSize
    {
        get
        {
            return this._totalSize;
        }
    }

    public int DownloadedSize
    {
        get
        {
            int size = this._finishedFileSize;
            foreach (var kvp in this._downloadingFileDict)
            {
                
                if (kvp.Value.requestDisposed)
                {
                    if (kvp.Value.isDone)
                    {
                        size += this._unfinishFileDict[kvp.Key];
                    }
                }
                else
                {
                    size += Mathf.Max(0, Mathf.RoundToInt(kvp.Value.request.downloadProgress * this._unfinishFileDict[kvp.Key]));
                }
                
            }
            return size;
        }
    }

    public void StartDownloadFile(string fileName, RequestHandler requestHandler)
    {
        if (this._unfinishFileDict.ContainsKey(fileName) && !this._downloadingFileDict.ContainsKey(fileName))
        {
            this._downloadingFileDict.Add(fileName, requestHandler);
        }
    }

    public void FinishDownloadFile(string fileName)
    {
        if (this._unfinishFileDict.ContainsKey(fileName))
        {
            this._finishedFileSize += this._unfinishFileDict[fileName];

            this._finishedFileDict.Add(fileName, this._unfinishFileDict[fileName]);
            this._unfinishFileDict.Remove(fileName);

            if (this._unfinishFileDict.Count == 0)
            {
                if (this._finishCallback != null)
                {
                    try
                    {
                        this._finishCallback();
                    }
                    catch (Exception e)
                    {
                        LogUtil.Error(e.ToString());
                    }
                }
            }
        }
        if (this._downloadingFileDict.ContainsKey(fileName))
        {
            this._downloadingFileDict.Remove(fileName);
        }
    }
    
    public bool HasFinished()
    {
        return this._finishedFileDict.Count == this._totalFileCount;
    }
}
