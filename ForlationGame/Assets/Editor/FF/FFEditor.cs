using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class FFEditor : AssetPostprocessor
{
    static string templateAssetPath = "Assets/LuaScript/FF/FF.template.lua";

    static string targetAssetPath = "Assets/LuaScript/FF/FF.lua";

    static string watchFolderAssetPath = "Assets/LuaScript/Game";

    static string startTag = "-- FF_Insert_Start_Tag";
    static string endTag = "-- FF_Insert_End_Tag";


    static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
    {
        bool needGenerate = CheckArray(importedAssets) 
            || CheckArray(deletedAssets)
            || CheckArray(movedAssets)
            || CheckArray(movedFromAssetPaths);

        if (needGenerate)
        {
            GenerateFFLuaFile();
        }
    }

    static bool CheckArray(string[] pathArray)
    {
        if (pathArray != null)
        {
            for (int i = 0; i < pathArray.Length; i++)
            {
                // todo : 先简单粗暴判断了，index==0的情况也不管了
                if (pathArray[i].IndexOf("Mgr.lua") > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [MenuItem("XLua/手动更新Mgr入口文件(FF)")]
    static void GenerateFFLuaFile()
    {
        string assetRootPath = Path.Combine(Application.dataPath, "..");

        string templatePath = Path.Combine(assetRootPath, templateAssetPath);
        string targetPath = Path.Combine(assetRootPath, targetAssetPath);
        
        string watchFolderPath = Path.Combine(assetRootPath, watchFolderAssetPath);

        // 删除旧文件
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        // 遍历Game目录，找到所有Mgr类
        var mgrNameArray = WalkFolder(watchFolderPath);

        // 排序
        var list = new List<string>(mgrNameArray);
        list.Sort((a, b) => {
            return a.CompareTo(b);
        });

        mgrNameArray = list.ToArray();

        string mgrText = "";

        // 拼装文本
        if (mgrNameArray != null)
        {
            for (int i = 0; i < mgrNameArray.Length; i++)
            {
                mgrText += string.Format("FF.{0} = CCC({0})\n", mgrNameArray[i]);
            }
        }

        // 读取模版文件内容
        string content = File.ReadAllText(templatePath);

        int startIndex = content.IndexOf(startTag) + startTag.Length + 1;
        int endIndex = content.IndexOf(endTag);


        content = content.Substring(0, startIndex) + mgrText + content.Substring(endIndex);

        var output = File.CreateText(targetPath);
        output.Write(content);
        output.Flush();
        output.Close();
        output.Dispose();

        AssetDatabase.Refresh();

        Debug.Log("FF file generated : " + targetPath);
    }

    /// <summary>
    /// 遍历目录，返回符合的Mgr类
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    static string[] WalkFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            var dirInfo = new DirectoryInfo(folderPath);

            var fileInfos = dirInfo.GetFiles("*.lua", SearchOption.AllDirectories);

            if (fileInfos == null)
            {
                return null;
            }
            List<string> mgrList = new List<string>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.LastIndexOf("Mgr.lua") > 0)
                {
                    mgrList.Add(fileInfos[i].Name.Replace(".lua", ""));
                }
            }
            return mgrList.ToArray();
        }

        return null;
    }
}
