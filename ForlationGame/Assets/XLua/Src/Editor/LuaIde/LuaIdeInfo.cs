using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


public class LuaIdeVarInfo
{
    public FieldInfo fieldInfo;
    public PropertyInfo propertyInfo;
    public EventInfo eventInfo;
    public string value = "";

}
public class LuaIdeInfo
{
    public static LuaIdeInfo luaInfo;
    public static List<LuaIdeInfo> luaInfos = new List<LuaIdeInfo>();
    public static Dictionary<Type, LuaIdeInfo> luaInfoDict = new Dictionary<Type, LuaIdeInfo>();
    public string tableName;
    public string baseName;
    public List<MethodInfo> methods;
    public List<MethodInfo> extendMethods = new List<MethodInfo>();
    public List<ConstructorInfo> ctorList;
    public bool isDefConstructorInfo = false;
    public List<LuaIdeVarInfo> varInfos = new List<LuaIdeVarInfo>();

    public Type oType;


    public List<string> events = new List<string>();

    public void addVar(LuaIdeVarInfo varInfo)
    {
        varInfos.Add(varInfo);
    }


    public string toStr()
    {

        StringBuilder sb = new StringBuilder();
        

        if (this.baseName != null)
        {
            sb.AppendLine(string.Format("---@class {0}: {1}", this.tableName, this.baseName));
        }
        else
        {
            sb.AppendLine(string.Format("---@class {0}", this.tableName));
        }

        // 字段和属性
        List<string> propertyInfoFunName = new List<string>();
        foreach (LuaIdeVarInfo info in varInfos)
        {

            string varName;
            string typeName;
            string comment;

            Type type_ = null;

            if (info.fieldInfo != null)
            {
                // 成员变量
                type_ = info.fieldInfo.FieldType;
                varName = info.fieldInfo.Name;
                typeName = MakeTypeStr(type_);
                string docKey = tableName + "." + varName;
                comment = LuaIdeClassDoc.getDoc(docKey);

                if (string.IsNullOrEmpty(comment))
                {
                    sb.AppendLine(string.Format("---@field {0} {1}", varName, typeName));
                }
                else
                {
                    sb.AppendLine(string.Format("---@field {0} {1} @{2}", varName, typeName, comment));
                }
                

            }
            else if (info.propertyInfo != null)
            {
                // 属性
                type_ = info.propertyInfo.PropertyType;
                varName = info.propertyInfo.Name;
                typeName = MakeTypeStr(type_);
                string docKey = tableName + "." + varName;

                comment = "";
                if (info.propertyInfo.GetMethod != null)
                {
                    comment += "<get> ";
                }
                if (info.propertyInfo.SetMethod != null)
                {
                    comment += "<set> ";
                }

                comment += LuaIdeClassDoc.getDoc(docKey);

                sb.AppendLine(string.Format("---@field {0} {1} @{2}", varName, typeName, comment));
            }
            else if (info.eventInfo != null)
            {
                // 委托
                type_ = info.eventInfo.EventHandlerType;
                varName = info.eventInfo.Name;
                typeName = MakeTypeStr(type_);
                string docKey = tableName + "." + varName;
                comment = LuaIdeClassDoc.getDoc(docKey);

                if (string.IsNullOrEmpty(comment))
                {
                    sb.AppendLine(string.Format("---@field {0} {1}", varName, typeName));
                }
                else
                {
                    sb.AppendLine(string.Format("---@field {0} {1} @{2}", varName, typeName, comment));
                }

            }
            
        }

        // 构造函数
        if (this.ctorList != null)
        {
            foreach (ConstructorInfo info in this.ctorList)
            {
                sb.AppendLine("");

                string paramStr = "";

                ParameterInfo[] parameterInfos = info.GetParameters();

                if (parameterInfos != null && parameterInfos.Length > 0)
                {
                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        sb.AppendLine(string.Format("---@param {0} {1}", parameterInfos[i].Name, MakeTypeStr(parameterInfos[i].ParameterType)));

                        if (i > 0)
                        {
                            paramStr += ", ";
                        }
                        paramStr += parameterInfos[i].Name;
                    }
                }

                sb.AppendLine(string.Format("---@return {0}", this.tableName));

                sb.AppendLine(string.Format("function {0}({1}) end", this.tableName, paramStr));


            }
        }

        // 方法
        foreach (MethodInfo m in methods)
        {
            if (m.Name.IndexOf("set_") == 0 || m.Name.IndexOf("get_") == 0)
            {
                continue;
            }

            sb.AppendLine("");

            string commenStr = "";

            LuaIdeClassDocInfo docInfo = LuaIdeClassDoc.getLuaIdeClassDocInfo(tableName + "." + m.Name);
            if (docInfo != null && docInfo.doc != "")
            {
                commenStr += docInfo.doc;
            }

            if (!string.IsNullOrEmpty(commenStr))
            {
                sb.AppendLine(string.Format("--- {0}", commenStr));
            }

            ParameterInfo[] parameterInfos = m.GetParameters();

            List<string> returnTypeNameList = new List<string>();
            if (m.ReturnType != null && m.ReturnType.FullName != "System.Void")
            {
                returnTypeNameList.Add(MakeTypeStr(m.ReturnType));
            }

            List<string> paramNameList = new List<string>();

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo paramInfo = parameterInfos[i];

                if (paramInfo.ParameterType.IsByRef)
                {
                    returnTypeNameList.Add(MakeTypeStr(paramInfo.ParameterType));
                }

                if (paramInfo.IsOut)
                {
                    continue;
                }

                string paramComment = "";

                if (paramInfo.ParameterType.IsByRef)
                {
                    paramComment += "<ref>";
                }

                if (docInfo != null && docInfo.paraminfo.ContainsKey(paramInfo.Name))
                {
                    paramComment += docInfo.paraminfo[paramInfo.Name];
                }

                if (string.IsNullOrEmpty(paramComment))
                {
                    sb.AppendLine(string.Format("---@param {0} {1}", paramInfo.Name, MakeTypeStr(paramInfo.ParameterType)));
                }
                else
                {
                    sb.AppendLine(string.Format("---@param {0} {1} @{2}", paramInfo.Name, MakeTypeStr(paramInfo.ParameterType), paramComment));
                }
                paramNameList.Add(paramInfo.Name);
            }

            if (returnTypeNameList.Count > 0)
            {
                string returnString = "---@return ";
                for (int i = 0; i < returnTypeNameList.Count; i++)
                {
                    if (i > 0)
                    {
                        returnString += ", ";
                    }
                    returnString += returnTypeNameList[i];
                }

                sb.AppendLine(returnString);
            }

            string paramStr = "";
            for (int i = 0; i < paramNameList.Count; i++)
            {
                if (i > 0)
                {
                    paramStr += ", ";
                }
                paramStr += paramNameList[i];
            }

            sb.AppendLine(string.Format("function {0}{1}{2}({3}) end", this.tableName, m.IsStatic ? "." : ":", m.Name, paramStr));
            
        }

        // 扩展方法
        foreach (MethodInfo m in extendMethods)
        {

            sb.AppendLine("");

            string commenStr = "";

            LuaIdeClassDocInfo docInfo = LuaIdeClassDoc.getLuaIdeClassDocInfo(tableName + "." + m.Name);
            if (docInfo != null && docInfo.doc != "")
            {
                commenStr += docInfo.doc;
            }

            if (!string.IsNullOrEmpty(commenStr))
            {
                sb.AppendLine(string.Format("--- {0}", commenStr));
            }

            ParameterInfo[] parameterInfos = m.GetParameters();

            List<string> returnTypeNameList = new List<string>();
            if (m.ReturnType != null && m.ReturnType.FullName != "System.Void")
            {
                returnTypeNameList.Add(MakeTypeStr(m.ReturnType));
            }

            List<string> paramNameList = new List<string>();

            for (int i = 1; i < parameterInfos.Length; i++)
            {
                ParameterInfo paramInfo = parameterInfos[i];

                if (paramInfo.ParameterType.IsByRef)
                {
                    returnTypeNameList.Add(MakeTypeStr(paramInfo.ParameterType));
                }

                if (paramInfo.IsOut)
                {
                    continue;
                }

                string paramComment = "";

                if (paramInfo.ParameterType.IsByRef)
                {
                    paramComment += "<ref>";
                }

                if (docInfo != null && docInfo.paraminfo.ContainsKey(paramInfo.Name))
                {
                    paramComment += docInfo.paraminfo[paramInfo.Name];
                }

                if (string.IsNullOrEmpty(paramComment))
                {
                    sb.AppendLine(string.Format("---@param {0} {1}", paramInfo.Name, MakeTypeStr(paramInfo.ParameterType)));
                }
                else
                {
                    sb.AppendLine(string.Format("---@param {0} {1} @{2}", paramInfo.Name, MakeTypeStr(paramInfo.ParameterType), paramComment));
                }
                paramNameList.Add(paramInfo.Name);
            }

            if (returnTypeNameList.Count > 0)
            {
                string returnString = "---@return ";
                for (int i = 0; i < returnTypeNameList.Count; i++)
                {
                    if (i > 0)
                    {
                        returnString += ", ";
                    }
                    returnString += returnTypeNameList[i];
                }

                sb.AppendLine(returnString);
            }

            string paramStr = "";
            for (int i = 0; i < paramNameList.Count; i++)
            {
                if (i > 0)
                {
                    paramStr += ", ";
                }
                paramStr += paramNameList[i];
            }

            sb.AppendLine(string.Format("function {0}{1}{2}({3}) end", this.tableName, ":", m.Name, paramStr));

        }

        return sb.ToString();

    }

    public static string MakeTypeStr(Type type)
    {

        string typeStr = type.FullName;

        int index = typeStr.IndexOf('+');

        if (typeStr.IndexOf('+') > -1)
        {

            typeStr = typeStr.Replace('+', '.');
        }

        if (typeStr[typeStr.Length - 1] == '&')
        {
            typeStr = typeStr.Substring(0, typeStr.Length - 1);
        }

        // 处理一下泛型声明
        // TODO: 泛型嵌套
        if (typeStr.Contains("`"))
        {
            string genericParamStr = "";

            string genericStr = typeStr.Substring(typeStr.IndexOf('`') + 3);
            genericStr = genericStr.Substring(0, genericStr.Length - 1);

            string regPattern = @"\[.*?,";
            var matchResult = Regex.Matches(genericStr, regPattern);

            for (int i = 0; i < matchResult.Count; i++)
            {
                if (i > 0)
                {
                    genericParamStr += ", ";
                }
                string s = matchResult[i].Value;
                s = s.Substring(1, s.Length - 2);
                genericParamStr += "CS." + s;
            }

            if (!string.IsNullOrEmpty(genericParamStr))
            {
                genericParamStr = "<" + genericParamStr + ">";
            }

            typeStr = typeStr.Substring(0, typeStr.IndexOf('`')) + genericParamStr;
        }


        typeStr = "CS." + typeStr;


        return typeStr;
    }
}

