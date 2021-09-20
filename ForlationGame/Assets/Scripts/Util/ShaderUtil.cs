using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderUtil
{
    public static Shader LoadShader(string assetPath)
    {
        if (Application.isPlaying)
        {
            if (AssetsManager.Me != null)
            {
                return AssetsManager.Me.LoadAsset<Shader>(assetPath);
            }
            else
            {
                Debug.LogWarning("AssetsManager尚未初始化 " + assetPath);
                return null;
            }
        }
        else
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
#else
            return null;
#endif

        }
    }
}
