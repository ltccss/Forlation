using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// 快速启动
/// </summary>
public class FastLaunchMenu
{
    [MenuItem("快速启动/切换至启动场景")]
    public static void GoToLaunchScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameLaunch.unity");
    }

    [MenuItem("快速启动/开始游戏")]
    public static void LaunchGame()
    {
        if (!EditorApplication.isPlaying)
        {
            GoToLaunchScene();
            EditorApplication.isPlaying = true;
        }

    }

    private static string GetStreamOutput(StreamReader stream)
    {
        var outputReadTask = System.Threading.Tasks.Task.Run(() => stream.ReadToEnd());
        return outputReadTask.Result;
    }

    [MenuItem("快速启动/切换至测试场景")]
    public static void GoToTestScene()
    {
        if (!File.Exists(Path.Combine(Application.dataPath, "Scenes/TestScene.unity")))
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/TestScene.unity");
        }
        else
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
        }
        
    }
}
