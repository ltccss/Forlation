using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AppSetting
{
    public static string version
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.version;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Application.version;
            }
            return "1.0.0";
        }
    }

    /// <summary>
    /// 运行平台
    /// </summary>
    public static string platform
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    return "editor";
                default:
                    return "editor";
            }
        }
    }


    // debug/review/release
    public static string runningMode = "debug";

    public static bool IsDebugMode
    {
        get
        {
            return runningMode == "debug";
        }
    }

    public static bool IsReviewMode
    {
        get
        {
            return runningMode == "review";
        }
    }

    public static bool IsReleaseMode
    {
        get
        {
            return runningMode == "release";
        }
    }

    // TODO: 热更流程

    /// <summary>
    /// 模拟真机模式，这个模式下整个游戏流程和真机应该是一样的
    /// </summary>
#if UNITY_EDITOR
    public static readonly bool simulatePhoneMode = false;
#else
    public static readonly bool simulatePhoneMode = true;
#endif
    /// <summary>
    /// 模拟真机模式下直接加载Assets下的lua文件，避免频繁打lua包便于调试
    /// </summary>
#if UNITY_EDITOR
    public static readonly bool loadLuaDirectlyInSimulatePhoneMode = true;
#else
    public static readonly bool loadLuaDirectlyInSimulatePhoneMode = false;
#endif

    // TODO: 下载热更还未实现

    /// <summary>
    /// TODO: 需要自行配置各种资源下载地址
    /// </summary>
    public static readonly string resUrl = "http://hostname.com/res/{0}"; // 参数0是资源文件名

    /// <summary>
    /// TODO:资源集合名数组，这个参数应该在版本查询时获得
    /// </summary>
    public static string[] resCollectionNames = new string[] { "test1", "test2"};

    /// <summary>
    /// 资源集合清单目录的url
    /// </summary>
    public static readonly string resCollectionUrl = "http://hostname.com/resv/{0}/{1}/"; // 参数0是资源集合名，参数1是资源集合版本号

    public static readonly string luaUrl = "http://hostname.com/lua/{0}/"; // 参数0是lua文件（包）版本号

    public static readonly string cfgUrl = "http://hostname.com/cfg/{0}/"; // 参数0是配置文件（包）版本号

    /// <summary>
    /// 沙盒缓存目录下lua文件名
    /// </summary>
    public static readonly string luaFileName = "lua.ab";

    /// <summary>
    /// 沙盒缓存目录下配置文件名
    /// 加载配置的流程在lua里完成
    /// </summary>
    public static readonly string cfgFileName = "cfg.ab";

    /// <summary>
    /// 资源缓存目录名
    /// 资源热更的流程在lua里完成
    /// </summary>
    public static readonly string gameCacheFolderName = "GameCache";

    /// <summary>
    /// 渠道标识，产品代号-平台-应用市场[-地区][-变体号]
    /// </summary>
#if !UNITY_IOS
#if Debug
    public static readonly string channel = "a1-android-googleplay-global-0-test";
#else
    public static readonly string channel = "a1-android-googleplay-global-0";
#endif
#else
#if Debug
    public static readonly string channel = "a1-ios-appstore-global-0-test";
#else
    public static readonly string channel = "a1-ios-appstore-global-0";
#endif
#endif

    public static string PersistentDataPath
    {
        get
        {
            if (RuntimePlatform.Android == Application.platform)
            {
                if (Debug.isDebugBuild)
                {
                    return Application.persistentDataPath;
                }
                else
                {
                    // TODO: 安卓这个目录会暴露给用户，这里建议换成内部缓存目录，
                    // Unity没提供直接Api，需要Porting一下：context.getFilesDir()
                    return Application.persistentDataPath;
                }

            }
            else
            {
                return Application.persistentDataPath;
            }
        }
    }

    // 存放下载下来的或者包内解压出来的游戏资源文件的缓存目录
    public static string GameCachePath
    {
        get
        {
            return Path.Combine(PersistentDataPath, gameCacheFolderName);
        }
    }



}
