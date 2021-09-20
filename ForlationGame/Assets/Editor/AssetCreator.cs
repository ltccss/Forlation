namespace UnityExtension.AssetCreator
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.ProjectWindowCallback;
    using UnityEngine;

    internal class CreateFileAsset : EndNameEditAction
    {
        public override void Action(System.Int32 instanceId, System.String pathName, System.String resourceFile)
        {
            Object obj = CreateAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(obj);
        }

        //Create asset from file template
        internal Object CreateAssetFromTemplate(string path, string resourceFile)
        {
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            //Replace .lua class name.
            text = text.Replace("#LuaClass", fileNameWithoutExtension);
            StreamWriter streamWriter = new StreamWriter(path);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }
    }

    internal class AssetCreator
    {
        internal static void CreateAsset(string fullFileName, string fullTemplateName)
        {
            Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
            string folder = AssetDatabase.GetAssetPath(objects[0]);

            if (!Directory.Exists(folder))
            {
                folder = Path.GetDirectoryName(folder);
            }

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateFileAsset>(),
                folder + "/" + fullFileName, null, "Assets/Editor/Templates/" + fullTemplateName);
        }

        [MenuItem("Assets/Create/Lua (Empty)", false, 80)]
        internal static void CreateLuaEmptyScript()
        {
            CreateAsset("Lua.lua", "LuaTemplate_Empty.lua.template");
        }

        [MenuItem("Assets/Create/Lua (Class)", false, 80)]
        internal static void CreateLuaClassScript()
        {
            CreateAsset("Lua.lua", "LuaTemplate_Class.lua.template");
        }

        [MenuItem("Assets/Create/Lua (ViewWrapper)", false, 80)]
        internal static void CreateLuaViewWrapperScript()
        {
            CreateAsset("Lua.lua", "LuaTemplate_ViewWrapper.lua.template");
        }

        [MenuItem("Assets/Create/Lua (Dialog)", false, 80)]
        internal static void CreateLuaDialogScript()
        {
            CreateAsset("Lua.lua", "LuaTemplate_Dialog.lua.template");
        }

        [MenuItem("Assets/Create/Lua (Manager)", false, 80)]
        internal static void CreateLuaManagerScript()
        {
            CreateAsset("Lua.lua", "LuaTemplate_Manager.lua.template");
        }

        //[MenuItem("Assets/Create/Lua", false, 80)]
        //internal static void CreateLua()
        //{
        //    CreateAsset("Lua.lua.txt", "LuaTemplate.lua");
        //}


    }
}