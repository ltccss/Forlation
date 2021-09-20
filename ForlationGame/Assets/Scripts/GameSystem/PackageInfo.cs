using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// todo
/// 包内资源信息
/// </summary>
public class PackageInfo
{
    public static int luaVersion;
    public static string luaMd5;
    public static int cfgVersion;
    public static string cfgMd5;

    /// <summary>
    /// 是否是单机版
    /// </summary>
    public static bool isStandalone = false;
    /// <summary>
    /// 单机版的版本信息(json)
    /// </summary>
    public static string standaloneVersionInfo;

    /// <summary>
    /// filename to md5
    /// 理论上包内塞了多少资源文件是不固定的
    /// 这里的key：filename不带md5后缀，但实际上的文件是filename_md5
    /// </summary>
    public static Dictionary<string, string> resFileDict = new Dictionary<string, string>();
    public static Dictionary<string, int> resFileEncryptDict = new Dictionary<string, int>();

    /// <summary>
    /// 包内是否存在lua文件
    /// </summary>
    public static bool hasLua = false;
    /// <summary>
    /// 包内是否存在cfg文件
    /// </summary>
    public static bool hasCfg = false;

    public static bool HasResFile(string filename, string md5)
    {
        if (resFileDict.ContainsKey(filename))
        {
            return md5 == resFileDict[filename];
        }
        return false;
    }

    /// <summary>
    /// 获取全名，形式为filename_md5
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string GetResFileFullName(string filename)
    {
        if (resFileDict.ContainsKey(filename))
        {
            return filename + "_" + resFileDict[filename];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 获取加密参数
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static int GetResFileEncryptParam(string filename)
    {
        if (resFileEncryptDict.ContainsKey(filename))
        {
            return resFileEncryptDict[filename];
        }
        else
        {
            return 0;
        }
    }
}
