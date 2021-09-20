using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏启动入口
/// </summary>
public class GameEntry : MonoBehaviour
{
    static GameObject root;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {

        if (root != null)
        {
            Destroy(root);
            root = null;
        }
        DontDestroyOnLoad(this.gameObject);
        ForlationGameManager.Me.Init(this.gameObject);
        root = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        ForlationGameManager.Me.Update();
    }
    void LateUpdate()
    {
        ForlationGameManager.Me.LateUpdate();
    }

    void OnApplicationPause(bool pause)
    {
        ForlationGameManager.Me.OnGamePause(pause);
    }

    void OnApplicationQuit()
    {
        ForlationGameManager.Me.OnGameQuit();
    }
}
