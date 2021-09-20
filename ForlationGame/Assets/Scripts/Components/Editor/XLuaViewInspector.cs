using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

public class XLuaViewHelper
{
    public static List<GameObject> copiedGoList = new List<GameObject>();

    [MenuItem("GameObject/CopyForXLuaView %e", priority = 11)]
    static void CopyForXLuaView()
    {
        copiedGoList.Clear();

        var goArray = Selection.gameObjects;
        if (goArray == null || goArray.Length == 0)
        {
            return;
        }

        for (int i = 0; i < goArray.Length; i++)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(goArray[i])))
            {
                EditorUtility.DisplayDialog("", "拷贝的GameObject不能是在project内的资源", "ojbk");
                return;
            }
        }

        copiedGoList.AddRange(Selection.gameObjects);

        Debug.Log("xLuaView: Copied");
    }
}

[CustomEditor(typeof(XLuaView))]
public class XLuaViewInspector : Editor
{
    Dictionary<string, bool> _expandDict = new Dictionary<string, bool>();

    // properties

    override public void OnInspectorGUI()
    {
        // DrawDefaultInspector();

        // helper vars
        XLuaView view = target as XLuaView;

        string luaFilePath = view.luaFilePath;
        if (luaFilePath == null)
        {
            luaFilePath = "";
        }
        UnityEngine.Object luaFileAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(luaFilePath);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LuaFile:", GUILayout.Width(100.0f));

        var newLuaFileAsset = EditorGUILayout.ObjectField(luaFileAsset, typeof(UnityEngine.Object), true,
                    GUILayout.Width(200.0f));
        string newLuaFilePath = "";
        if (newLuaFileAsset != null)
        {
            newLuaFilePath = AssetDatabase.GetAssetPath(newLuaFileAsset.GetInstanceID());
            if (Path.GetExtension(newLuaFilePath) != ".lua")
            {
                newLuaFilePath = "";
            }
        }
        view.luaFilePath = newLuaFilePath;

        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying)
        {
            string runtimeLuaFilePath = view.luaFilePathRuntime;

            EditorGUILayout.BeginHorizontal();
            GUIStyle s = new GUIStyle(EditorStyles.textField);
            //Debug.Log(">> runtime lua " + runtimeLuaFilePath);
            //Debug.Log(">>  lua " + luaFilePath);
            if (runtimeLuaFilePath == luaFilePath || (!string.IsNullOrEmpty(runtimeLuaFilePath) && !string.IsNullOrEmpty(luaFilePath) && Path.GetFullPath(runtimeLuaFilePath) == Path.GetFullPath(luaFilePath)))
            {
                s.normal.textColor = Color.white;
            }
            else
            {
                s.normal.textColor = Color.yellow;
            }
            

            EditorGUILayout.LabelField("LuaFile(RunTime):", s, GUILayout.Width(120.0f));
            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(runtimeLuaFilePath), typeof(UnityEngine.Object), true,
                        GUILayout.Width(200.0f));
            EditorGUILayout.EndHorizontal();
        }
        

        var gameObjectArray = view.gameObjects;

        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("gameObjects", GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("size : " + (gameObjectArray == null ? 0 : gameObjectArray.Length).ToString(),
            GUILayout.Width(80.0f));
        EditorGUILayout.EndHorizontal();

        if (gameObjectArray != null)
        {
            for (int i = 0; i < gameObjectArray.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUIStyle indexTextStyle = new GUIStyle(EditorStyles.textField);
                indexTextStyle.normal.textColor = gameObjectArray[i] == null ? Color.yellow : Color.white;
                
                EditorGUILayout.LabelField(i.ToString(), indexTextStyle, GUILayout.Width(24.0f));
                var obj = EditorGUILayout.ObjectField(gameObjectArray[i], typeof(GameObject), true,
                    GUILayout.Width(200.0f));
                if (obj != gameObjectArray[i])
                {
                    // 检查拖入物体是否是view的孩子节点
                    if (IsChild(obj as GameObject, view.gameObject))
                    {
                        Undo.RecordObject(view, "Change GameObject Element");
                        gameObjectArray[i] = obj as GameObject;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("", string.Format("拖入物体{0}不是该View的子节点", obj.name), "ojbk");
                    }
                }


                bool addButton = GUILayout.Button("Add", GUILayout.Width(50.0f));
                if (addButton)
                {
                    Undo.RecordObject(view, "Add GameObject Element");
                    view.gameObjects = this.InsertEmptyToArray<GameObject>(view.gameObjects, i);
                }

                bool removeButton = GUILayout.Button("Del", GUILayout.Width(50.0f));
                if (removeButton)
                {
                    Undo.RecordObject(view, "Remove GameObject Element");
                    view.gameObjects = this.DeleteEmptyToArray<GameObject>(view.gameObjects, i);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.BeginHorizontal();
        bool addToLastButton =
            GUILayout.Button("Add", new GUILayoutOption[] {GUILayout.Width(70.0f), GUILayout.Height(28.0f)});
        if (addToLastButton)
        {
            Undo.RecordObject(view, "Add GameObject Element");
            view.gameObjects = this.InsertEmptyToArray<GameObject>(view.gameObjects, int.MaxValue);
        }

        bool pasteButton = GUILayout.Button("粘贴",
            new GUILayoutOption[] {GUILayout.Width(70.0f), GUILayout.Height(28.0f)});
        if (pasteButton)
        {
            if (XLuaViewHelper.copiedGoList.Count > 0)
            {
                List<GameObject> goList = new List<GameObject>();
                if (view.gameObjects != null)
                {
                    goList.AddRange(view.gameObjects);
                }

                bool hasError = false;

                for (int i = 0; i < XLuaViewHelper.copiedGoList.Count; i++)
                {
                    // 去除拷贝列表里的重复项
                    var go = XLuaViewHelper.copiedGoList[i];
                    if (!goList.Contains(go))
                    {
                        goList.Add(go);

                        if (!IsChild(go, view.gameObject))
                        {
                            EditorUtility.DisplayDialog("", string.Format("拷贝列表中有物体不属于该View的子节点: {0}", go.name),
                                "ojbk");
                            hasError = true;
                            break;
                        }
                    }
                }

                if (!hasError)
                {
                    Undo.RecordObject(view, "Add GameObject Element");
                    view.gameObjects = goList.ToArray();
                }

                XLuaViewHelper.copiedGoList.Clear();
            }
        }

        bool checkDuplicateButton =
            GUILayout.Button("去重", new GUILayoutOption[] {GUILayout.Width(70.0f), GUILayout.Height(28.0f)});
        if (checkDuplicateButton)
        {
            List<GameObject> goList = new List<GameObject>();
            if (view.gameObjects != null)
            {
                goList.AddRange(view.gameObjects);

                Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();

                bool hasError = false;
                bool needModify = false;

                List<GameObject> newList = new List<GameObject>();

                for (int i = 0; i < goList.Count; i++)
                {
                    if (goList[i] == null)
                    {
                        continue;
                    }

                    if (dict.ContainsKey(goList[i].name))
                    {
                        if (dict[goList[i].name] != goList[i])
                        {
                            EditorUtility.DisplayDialog("",
                                string.Format("有同名的不同GameObject : {0}，请先调整", goList[i].name), "ojbk");
                            hasError = true;
                            break;
                        }

                        needModify = true;
                    }
                    else
                    {
                        newList.Add(goList[i]);
                        dict.Add(goList[i].name, goList[i]);
                    }
                }

                if (!hasError && needModify)
                {
                    Undo.RecordObject(view, "Remove Duplicated GameObject Element");
                    view.gameObjects = newList.ToArray();
                }
            }
        }

        bool needSortButton =
            GUILayout.Button("排序", new GUILayoutOption[] {GUILayout.Width(70.0f), GUILayout.Height(28.0f)});
        if (needSortButton)
        {
            List<GameObject> goList = new List<GameObject>();
            if (view.gameObjects != null)
            {
                goList.AddRange(view.gameObjects);

                goList.Sort((a, b) =>
                {
                    if (a == null)
                    {
                        return 1;
                    }

                    if (b == null)
                    {
                        return -1;
                    }

                    return a.name.CompareTo(b.name);
                });

                Undo.RecordObject(view, "Sort GameObject Element");
                view.gameObjects = goList.ToArray();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();


        this.DrawXLVArray<XLVInt>(view.ints, "ints", view);
        this.DrawXLVArray<XLVLong>(view.longs, "longs", view);
        this.DrawXLVArray<XLVBool>(view.bools, "bools", view);
        this.DrawXLVArray<XLVDouble>(view.doubles, "doubles", view);
        this.DrawXLVArray<XLVString>(view.strings, "strings", view);
        this.DrawXLVArray<XLVVector3>(view.vector3s, "vector3s", view);

        this.DrawXLVArray<XLVColor>(view.colors, "colors", view);
        this.DrawXLVArray<XLVSprite>(view.sprites, "sprites", view);

        // 调试方法区域绘制
        if (view.testFuncList != null && view.testFuncList.Count > 0)
        {
            this.DrawLine();
            for (int i = 0; i < view.testFuncList.Count; i++)
            {
                this.DrawTestButton(view.testFuncList[i]);
            }
        }
 
        // store changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(view);
        }
    }

    void DrawTestButton(XLVTestData data)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(data.funcName, GUILayout.Width(100.0f));

        bool click = GUILayout.Button("Run", GUILayout.Width(120.0f));

        if (click)
        {
            data.func.Call();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawXLVArray<XLVType>(XLVType[] array, string fieldName, XLuaView view)
    {
        this.DrawLine();

        bool expand = false;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(fieldName, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("size : " + (array == null ? 0 : array.Length).ToString(), GUILayout.Width(80.0f));

        if (_expandDict.ContainsKey(fieldName))
        {
            expand = _expandDict[fieldName];
        }
        else
        {
            if (array != null && array.Length > 0)
            {
                // 数组里有数据时初始展开，以防查看面板时数据被疏忽
                expand = true;
            }
            _expandDict[fieldName] = expand;
        }

        bool expandButton = GUILayout.Button(expand ? "Collapse" : "Expand", GUILayout.Width(70.0f));
        if (expandButton)
        {
            _expandDict[fieldName] = !_expandDict[fieldName];
            expand = !expand;
        }

        EditorGUILayout.EndHorizontal();

        if (!expand)
        {
            EditorGUILayout.Separator();
            return;
        }

        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(24.0f));

                var ele = array[i];

                string name = Convert.ToString(ele.GetType().GetField("name").GetValue(ele));

                Type valueType = ele.GetType().GetField("value").FieldType;

                var value = ele.GetType().GetField("value").GetValue(ele);

                EditorGUILayout.LabelField("key:", GUILayout.Width(26.0f));

                string newName = EditorGUILayout.TextField(name, GUILayout.Width(80.0f));

                if (newName != name)
                {
                    Undo.RecordObject(view, "SetName");
                    ele.GetType().GetField("name").SetValue(ele, newName);
                }

                if (valueType != typeof(Vector3))
                {
                    EditorGUILayout.LabelField("value:", GUILayout.Width(36.0f));
                }

                if (valueType == typeof(bool))
                {
                    bool newValue = EditorGUILayout.Toggle(Convert.ToBoolean(value), GUILayout.Width(80.0f));
                    if (newValue != Convert.ToBoolean(value))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(float))
                {
                    float newValue = EditorGUILayout.FloatField(Convert.ToSingle(value), GUILayout.Width(80.0f));
                    if (newValue != Convert.ToSingle(value))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(double))
                {
                    double newValue = EditorGUILayout.DoubleField(Convert.ToDouble(value), GUILayout.Width(80.0f));
                    if (newValue != Convert.ToDouble(value))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(int))
                {
                    int newValue = EditorGUILayout.IntField(Convert.ToInt32(value), GUILayout.Width(80.0f));
                    if (newValue != Convert.ToInt32(value))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(long))
                {
                    long newValue = EditorGUILayout.LongField(Convert.ToInt64(value), GUILayout.Width(80.0f));
                    if (newValue != Convert.ToInt64(value))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(Vector3))
                {
                    Vector3 v = (Vector3) value;
                    Vector3 newValue = EditorGUILayout.Vector3Field("", v, GUILayout.Width(200.0f));
                    if (newValue != v)
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(Color))
                {
                    Color c = (Color)value;
                    Color newValue = EditorGUILayout.ColorField(c, GUILayout.Width(180.0f));
                    if (newValue != c)
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else if (valueType == typeof(Sprite))
                {
                    Sprite sp = (Sprite)value;
                    Sprite newValue = EditorGUILayout.ObjectField(sp, typeof(Sprite), false, GUILayout.Width(180.0f)) as Sprite;
                    if (newValue != sp)
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }
                else
                {
                    string newValue = EditorGUILayout.TextField(value == null? "" : value.ToString(), GUILayout.Width(80.0f));
                    if ((value == null && !string.IsNullOrEmpty(newValue)) || (value != null && newValue != value.ToString()))
                    {
                        Undo.RecordObject(view, "SetValue");
                        ele.GetType().GetField("value").SetValue(ele, Convert.ChangeType(newValue, valueType));
                    }
                }

                bool addButton = GUILayout.Button("Add", GUILayout.Width(50.0f));
                if (addButton)
                {
                    Undo.RecordObject(view, "AddValue");
                    view.GetType().GetField(fieldName).SetValue(view, this.InsertEmptyToArray<XLVType>(array, i));
                }

                bool removeButton = GUILayout.Button("Del", GUILayout.Width(50.0f));
                if (removeButton)
                {
                    Undo.RecordObject(view, "RemoveValue");
                    view.GetType().GetField(fieldName).SetValue(view, this.DeleteEmptyToArray<XLVType>(array, i));
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.BeginHorizontal();
        bool addToLastButton =
            GUILayout.Button("Add", new GUILayoutOption[] {GUILayout.Width(100.0f), GUILayout.Height(28.0f)});
        if (addToLastButton)
        {
            Undo.RecordObject(view, "AddValue");
            view.GetType().GetField(fieldName).SetValue(view, this.InsertEmptyToArray<XLVType>(array, int.MaxValue));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
    }

    T[] InsertEmptyToArray<T>(T[] array, int index)
    {
        if (array != default(T[]))
        {
            List<T> list = new List<T>(array);
            if (list.Count > index)
            {
                list.Insert(index + 1, default(T));
            }
            else
            {
                list.Add(default(T));
            }

            return list.ToArray();
        }
        else
        {
            return new T[1];
        }
    }

    T[] DeleteEmptyToArray<T>(T[] array, int index)
    {
        if (array != default(T[]))
        {
            List<T> list = new List<T>(array);
            if (list.Count > index)
            {
                list.RemoveAt(index);
            }

            return list.ToArray();
        }

        return default(T[]);
    }

    void DrawLine(int lineHeight = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);

        rect.height = lineHeight;

        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    bool IsChild(GameObject node, GameObject parent)
    {
        Transform trans = node.transform;

        while (trans != null)
        {
            if (trans == parent.transform)
            {
                return true;
            }

            trans = trans.parent;
        }

        return false;
    }
}