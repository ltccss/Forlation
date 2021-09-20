using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJson;

class PackedLuaLoader
{
    class LuaFileInfo
    {
        public string path;
        public int offset;
        public int size;
    }

    private readonly int HeadSizeLength = 16;
    private int _headDataLength;

    private Dictionary<string, LuaFileInfo> _luaInfoDict;
    

    private byte[] _fileData;

    public PackedLuaLoader(string filePath)
    {

        this._fileData = File.ReadAllBytes(filePath);

        // 读取索引文件长度
        byte[] headSizeArray = new byte[HeadSizeLength];

        Array.Copy(this._fileData, headSizeArray, HeadSizeLength);

        headSizeArray = SecurityUtil.Xor(headSizeArray, "zhugegezuikeai");

        string headSizeString = System.Text.Encoding.UTF8.GetString(headSizeArray);
        
        LogUtil.Log("headSizeString : " + headSizeString);
        // 剔除空格
        headSizeString = headSizeString.Replace(" ", "");
        

        int headSize = (Convert.ToInt32(headSizeString) - 233) / 998;
        LogUtil.Log("headSize : " + headSize);

        this._headDataLength = headSize;

        // 开始处理索引信息

        // 读取索引数据
        byte[] headArray = new byte[headSize];

        Array.Copy(this._fileData, HeadSizeLength, headArray, 0, headSize);

        // 解压索引数据成json文本

        string headString = LzmaUtil.DecompressBytesToString(headArray);

        LogUtil.Log(">> headString :" + headString);

        this._luaInfoDict = new Dictionary<string, LuaFileInfo>();

        // 解析json
        JsonArray infoJsonArray =  (JsonArray)SimpleJson.SimpleJson.DeserializeObject(headString);
        for (int i = 0; i < infoJsonArray.Count; i++)
        {
            JsonObject infoObj = (JsonObject)infoJsonArray[i];
            var fileInfo = new LuaFileInfo();
            fileInfo.path = Convert.ToString(infoObj["path"]);
            fileInfo.offset = Convert.ToInt32(infoObj["offset"]);
            fileInfo.size = Convert.ToInt32(infoObj["size"]);

            //Debug.LogFormat(">> info path:{0}, offset:{1}, size:{2}", fileInfo.path, fileInfo.offset, fileInfo.size);

            this._luaInfoDict[fileInfo.path] = fileInfo;
        }

    }

    public byte[] LoadLuaFile(ref string filePath)
    {
        string path = filePath + ".lua";
        path = path.Replace("\\\\", "\\").Replace("\\", "/");
        if (this._luaInfoDict.ContainsKey(path))
        {
            var info = this._luaInfoDict[path];
            var compressData = new byte[info.size];
            Array.Copy(this._fileData, this.HeadSizeLength + this._headDataLength + info.offset, compressData, 0, info.size);

            return LzmaUtil.DecompressBytes(compressData);
        }
        else
        {
            return null;
        }
    }
}
