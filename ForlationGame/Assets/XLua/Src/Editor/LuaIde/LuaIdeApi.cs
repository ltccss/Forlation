
using CSObjectWrapEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Assets.XLua.Src.Editor.LuaIde
{
    public class LuaIdeApi : ScriptableObject
    {
        public TextAsset Template;
        static List<Type> LuaCallCSharp;

        static List<Type> CSharpCallLua;

        static List<Type> GCOptimizeList;

        static Dictionary<Type, List<string>> AdditionalProperties;

        static List<Type> ReflectionUse;

        static List<List<string>> BlackList;

        static Dictionary<Type, OptimizeFlag> OptimizeCfg;


        [MenuItem("XLua/导出EmmyLua Api注解")]
        public static void GenLinkXml()
        {
            var d = ScriptableObject.CreateInstance<LuaIdeApi>();
            Generator.CustomGen(Application.dataPath + "/Xlua/Src/Editor/LuaIde/LuaIdeApi.tpl.txt", GetTasks);
        }
        public static void addLuaIdeInfo(Type type)
        {

            LuaIdeInfo.luaInfo = new LuaIdeInfo();
            LuaIdeInfo.luaInfo.oType = type;
            LuaIdeInfo.luaInfo.tableName = LuaIdeInfo.MakeTypeStr(type);

            if (type.BaseType != null)
            {
                LuaIdeInfo.luaInfo.baseName = LuaIdeInfo.MakeTypeStr(type.BaseType);
            }
            List<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsDefined(typeof(ExtensionAttribute), false) || method.DeclaringType != type)
                .Where(method => !isMethodInBlackList(method) && (!method.IsGenericMethod)).ToList();
            LuaIdeInfo.luaInfo.methods = methods;

            List<PropertyInfo> ps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
            .Where(prop => prop.Name != "Item" && !isObsolete(prop) && !isMemberInBlackList(prop)).ToList();
            foreach (PropertyInfo p in ps)
            {
                LuaIdeInfo.luaInfo.addVar(new LuaIdeVarInfo()
                {
                    propertyInfo = p,
                });
            }
            List<FieldInfo> fs = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
             .Where(field => !isObsolete(field) && !isMemberInBlackList(field)).ToList();
            foreach (FieldInfo f in fs)
            {
                string value = "";
                if (type.IsEnum)
                {
                    if (f.Name != "value__")
                    {
                        value = Convert.ToInt32(f.GetValue(null)).ToString();
                    }
                    else
                    {
                        continue;
                    }

                }
                LuaIdeInfo.luaInfo.addVar(new LuaIdeVarInfo()
                {
                    fieldInfo = f,
                    value = value
                });
            }

            LuaIdeInfo.luaInfo.ctorList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase).ToList();
            LuaIdeInfo.luaInfos.Add(LuaIdeInfo.luaInfo);
            LuaIdeInfo.luaInfoDict.Add(type, LuaIdeInfo.luaInfo);



        }
        public static IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg)
        {
            string luaIdeDirPath = Path.Combine(Application.dataPath, "LuaIde");
            if (Directory.Exists(luaIdeDirPath))
            {
                Directory.Delete(luaIdeDirPath, true);
            }

            Directory.CreateDirectory(luaIdeDirPath);


            LuaIdeClassDoc.checkDoc();
            LuaIdeInfo.luaInfos = new List<LuaIdeInfo>();
            LuaIdeInfo.luaInfoDict = new Dictionary<Type, LuaIdeInfo>();
            LuaCallCSharp = user_cfg.LuaCallCSharp.ToList();// Generator.LuaCallCSharp;
            CSharpCallLua = user_cfg.CSharpCallLua.ToList();// Generator.CSharpCallLua;
            BlackList = Generator.BlackList;
            foreach (Type type in LuaCallCSharp)
            {

                addLuaIdeInfo(type);
            }

            // 归并扩展方法

            foreach (Type type in LuaCallCSharp)
            {
                var methodArray = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
                for (int i = 0; i < methodArray.Length; i++)
                {
                    if (methodArray[i].IsDefined(typeof(ExtensionAttribute), false))
                    {
                        var paramArray = methodArray[i].GetParameters();
                        if (paramArray.Length > 1)
                        {
                            if (LuaIdeInfo.luaInfoDict.ContainsKey(paramArray[0].ParameterType))
                            {
                                LuaIdeInfo.luaInfoDict[paramArray[0].ParameterType].extendMethods.Add(methodArray[i]);
                            }
                        }
                    }
                }

            }

            // TODO: 每一个类作为一个文件单独存放 ojbk
            // 生成一份命名空间关系 ojbk
            // 扩展方法支持 ojbk
            // 嵌套类支持 ojbk

            // 生成命名空间父子级关系描述
            // 遍历所有type，拿到所有前缀，整理所有前缀
            Dictionary<string, NameSpaceNode> rootNameSpaceDict = new Dictionary<string, NameSpaceNode>();

            foreach (LuaIdeInfo luainfo in LuaIdeInfo.luaInfos)
            {

                if (luainfo.tableName != null)
                {
                    //Debug.Log(">> path : " + luainfo.tableName);
                    string filePath = Path.Combine(luaIdeDirPath, luainfo.tableName.Replace('<', '_').Replace('>', '_') + ".lua");
                    File.WriteAllText(filePath, luainfo.toStr());
                }

                string[] nodePathArray;
                if (luainfo.tableName.Contains('<'))
                {
                    nodePathArray = luainfo.tableName.Substring(0, luainfo.tableName.IndexOf('<')).Split('.');
                }
                else
                {
                    nodePathArray = luainfo.tableName.Split('.');
                }
                NameSpaceNode node = null;

                if (!rootNameSpaceDict.ContainsKey(nodePathArray[0]))
                {
                    rootNameSpaceDict[nodePathArray[0]] = new NameSpaceNode();
                    rootNameSpaceDict[nodePathArray[0]].node = nodePathArray[0];
                    rootNameSpaceDict[nodePathArray[0]].fullName = nodePathArray[0];
                }

                node = rootNameSpaceDict[nodePathArray[0]];

                for (int i = 1; i < nodePathArray.Length; i++)
                {
                    var nodePath = nodePathArray[i];

                    if (!node.tree.ContainsKey(nodePath))
                    {
                        node.tree[nodePath] = new NameSpaceNode();
                        node.tree[nodePath].node = nodePath;
                        node.tree[nodePath].fullName = node.fullName + "." + nodePath;
                    }

                    node = node.tree[nodePath];
                }
            }

            var npSb = new StringBuilder();

            foreach (var kvp in rootNameSpaceDict)
            {
                AppendNameSpaceStr(kvp.Value, npSb, true);
            }

            string nameSpaceFilePath = Path.Combine(luaIdeDirPath, "_NameSpace.lua");
            File.WriteAllText(nameSpaceFilePath, npSb.ToString());
            
            
            Debug.Log("Api 创建成功," + Path.GetFullPath(luaIdeDirPath));
            LuaTable data = lua_env.NewTable();
            List<string> dd = new List<string>();

            data.Set("infos", dd);
            yield return new CustomGenTask
            {
                Data = data,
                Output = new StreamWriter(luaIdeDirPath + "/readMe.txt",
        false, Encoding.UTF8)
            };
        }

        static bool isObsolete(MemberInfo mb)
        {
            if (mb == null) return false;
            return mb.IsDefined(typeof(System.ObsoleteAttribute), false);
        }

        static bool isMemberInBlackList(MemberInfo mb)
        {
            if (mb.IsDefined(typeof(BlackListAttribute), false)) return true;

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.FullName == exclude[0] && mb.Name == exclude[1])
                {
                    return true;
                }
            }

            return false;
        }

        static bool isMethodInBlackList(MethodBase mb)
        {
            if (mb.IsDefined(typeof(BlackListAttribute), false)) return true;

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.FullName == exclude[0] && mb.Name == exclude[1])
                {
                    var parameters = mb.GetParameters();
                    if (parameters.Length != exclude.Count - 2)
                    {
                        continue;
                    }
                    bool paramsMatch = true;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.FullName != exclude[i + 2])
                        {
                            paramsMatch = false;
                            break;
                        }
                    }
                    if (paramsMatch) return true;
                }
            }
            return false;
        }

        static void AppendNameSpaceStr(NameSpaceNode node, StringBuilder sb, bool isRoot)
        {
            if (node.tree.Count > 0)
            {
                sb.AppendLine(string.Format("---@class {0}", node.fullName));
                foreach (var kvp in node.tree)
                {
                    sb.AppendLine(string.Format("---@field {0} {1}", kvp.Value.node, kvp.Value.fullName));
                }

                if (isRoot)
                {
                    sb.AppendLine(string.Format("{0} = {{}}", node.fullName));
                }

                sb.AppendLine("");

                foreach (var kvp in node.tree)
                {
                    AppendNameSpaceStr(kvp.Value, sb, false);
                }
            }


        }
    }

    class NameSpaceNode
    {
        public string node;
        public string fullName;

        public Dictionary<string, NameSpaceNode> tree = new Dictionary<string, NameSpaceNode>();
    }

    
}
