
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;

[CanEditMultipleObjects, CustomEditor(typeof(DefaultAsset))]
public class LuaInspector : Editor
{
    private GUIStyle m_TextStyle;

    public override void OnInspectorGUI()
    {
        if (this.m_TextStyle == null)
        {
            this.m_TextStyle = "ScriptText";
        }
        bool enabled = GUI.enabled;
        GUI.enabled = true;

        List<string> requireTextList = new List<string>();

        var targetObjects = this.targets;
        if (targetObjects != null && targetObjects.Length > 0)
        {
            for (int i = 0; i < targetObjects.Length; i++)
            {
                string assetPath = AssetDatabase.GetAssetPath(targetObjects[i]);
                if (assetPath.EndsWith(".lua"))
                {
                    if (assetPath.IndexOf("Assets/LuaScript") == 0)
                    {
                        string requirePath = assetPath.Substring("Assets/LuaScript/".Length);
                        requirePath = requirePath.Substring(0, requirePath.Length - 4);
                        string requireFullText = string.Format("require(\"{0}\")", requirePath);

                        requireTextList.Add(requireFullText);

                    }
                }
            }
        }

        Rect rect2 = Rect.zero;

        if (requireTextList.Count > 0)
        {
            requireTextList.Sort();

            string requireText;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < requireTextList.Count; i++)
            {
                sb.AppendLine(requireTextList[i]);
            }

            requireText = sb.ToString();

            var luaRequireTextContent = new GUIContent(requireText);
            var height = this.m_TextStyle.CalcHeight(luaRequireTextContent, EditorGUIUtility.currentViewWidth + 1f);

            rect2 = GUILayoutUtility.GetRect(new GUIContent(requireText), this.m_TextStyle);
            rect2.x = 0f;
            rect2.y -= 3f;
            rect2.width = EditorGUIUtility.currentViewWidth + 1f;
            rect2.height = height;
            GUI.TextField(rect2, requireText, this.m_TextStyle);

            var copyButtonRect = Rect.zero;
            copyButtonRect.y = 20f;
            copyButtonRect.x = rect2.x + rect2.width - 206f;
            copyButtonRect.width = 128f;
            copyButtonRect.height = 22f;

            var hasClick = GUI.Button(copyButtonRect, "Copy Require Path");
            if (hasClick)
            {
                GUIUtility.systemCopyBuffer = requireText;
            }
        }

        if (targetObjects.Length == 1 && requireTextList.Count == 1)
        {
            string assetPath = AssetDatabase.GetAssetPath(this.target);
            string luaFileText = File.ReadAllText(assetPath);
            if (luaFileText.Length > 7000)
            {
                luaFileText = luaFileText.Substring(0, 7000) + "...\n\n<...etc...>";
            }

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(luaFileText), this.m_TextStyle);
            rect.x = 0f;
            rect.y = rect2.height + 49f;
            rect.width = EditorGUIUtility.currentViewWidth + 1f;
            GUI.Box(rect, luaFileText, this.m_TextStyle);
        }

        GUI.enabled = enabled;
    }

    
}