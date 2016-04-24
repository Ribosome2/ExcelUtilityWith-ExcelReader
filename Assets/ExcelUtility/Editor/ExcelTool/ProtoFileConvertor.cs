using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Collections;
public static class CommmonVarType
{
    public const string UINT64 = "uint64";
    public const string INT64 = "int64";
    public const string UINT32 = "uint32";
    public const string INT32 = "int32";
    public const string STRING = "string";
    public const string UINT8 = "uint8";
    public const string INT8 = "int8";
    public const string UINT16 = "uint16";
    public const string INT16 = "int16";
}
public class ProtoFileConvertor
{
    

    public Dictionary<string, string> DictVarDecodeMethod = new Dictionary<string, string>()
    {
        {CommmonVarType.UINT64, "ReadUInt64"},
        {CommmonVarType.INT64, "ReadInt64"},
        {CommmonVarType.UINT32, "ReadUInt32"},
        {CommmonVarType.INT32, "ReadInt32"},
        {CommmonVarType.STRING, "ReadString"},
        {CommmonVarType.INT8, "ReadInt8"},
        {CommmonVarType.UINT8, "ReadInt8"},
        {CommmonVarType.UINT16, "ReadUShort"},
        {CommmonVarType.INT16, "ReadShort"},
    };
    private string outPutLines = "";
    private List<CustomStuct> mCustomStucts = new List<CustomStuct>();
    private string mSrcText;
    private int index;
    public  string destClassName;
    public string SrcFileName;
    public string KeyPropertyName;
    List<PropertyPair> allPropertyPairs=new List<PropertyPair>(); 
    /// <summary>
    /// 生成的类的实例和表字段的属性类型映射，在用表生成二进制的时候要用
    /// </summary>
    public  Dictionary<string, string> propertyTableMap=new Dictionary<string, string>(); 


    public static Dictionary<string, string> CSharpTypes = new Dictionary<string, string>()
        {
            {CommmonVarType.UINT64, "ulong"},
            {CommmonVarType.INT64, "long"},
            {CommmonVarType.UINT32, "uint"},
            {CommmonVarType.INT32, "int"},
            {CommmonVarType.STRING, "string"},
            {CommmonVarType.INT8, "byte"},
            {CommmonVarType.UINT8, "byte"},
            {CommmonVarType.UINT16, "ushort"},
            {CommmonVarType.INT16, "short"},
        };




    public ProtoFileConvertor(string srcText)
    {
        mSrcText = srcText;
        index = 0;
        mCustomStucts.Clear();
        ProcessText();
    }

    public string GetFileName()
    {
        return destClassName + ".cs";
    }

   

    private void ProcessText()
    {
        outPutLines = "";
        ProcessHeaderInfo();

        while (index < mSrcText.Length)
        {
            string word1 = FindWord();
            if (string.IsNullOrEmpty(word1))
            {
                Debug.Log("已经找不到单词，即将结束" + index);
            }
            else
            {
                if (word1 == "struct")
                {
                    CreateCustomClass();
                }
                else
                {
                    if (IsValidVarType(word1))
                    {
                        string varName = FindWord();
                        string Line = string.Format("    public {0}  {1};", CSharpTypes[word1], varName);
                        outPutLines += "\n" + (Line);
                        allPropertyPairs.Add( new PropertyPair(word1,varName));
                        string mapMark = FindWord(null, false);
                        if (mapMark != "option")
                        {
                            Debug.LogError(string.Format("{0}没有定义表对应字段", varName));
                        }
                        else
                        {
                            string tableField = GetNextString();
                            if (propertyTableMap.ContainsKey(tableField))
                            {
                                Debug.LogError("字段重复" + tableField);
                            }
                            else
                            {
                                propertyTableMap.Add(tableField, word1);
                            }
                            
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("文件{0}变量类型{1}不合法", destClassName, word1));
                    }
                }
            }
        }
        outPutLines += ClassAppendix;
        outPutLines += "\n}";
    }

    private void ProcessHeaderInfo()
    {
        string firstMarker = FindWord();
        if (firstMarker != "class")
        {
            throw new Exception("没有文件标记class类型");
        }
        else
        {
            string name = FindWord();
            destClassName = name;
            outPutLines += (string.Format("public class {0}: TableData", name)) + "\n{";

            string keyStr = FindWord(null, false);
            if (keyStr != "key")
            {
                Debug.LogError("没有定义Key");
            }
            else
            {
                keyStr = FindWord();
                KeyPropertyName = keyStr;
            }

            string sourceFile = FindWord();
            if (sourceFile != "source")
            {
                Debug.LogError("没有定义源文件source");
            }
            SrcFileName = GetNextString();

        }
    }

    private void CreateCustomClass()
    {
        string structName = FindWord();
        if (string.IsNullOrEmpty(structName))
        {
            throw new System.Exception("结构名为空");
        }
        CustomStuct newStuct = new CustomStuct(structName);
        mCustomStucts.Add(newStuct);
        outPutLines += "\n" + (string.Format("    public class {0}", structName) + "\n    {");
        ProcessAngleBraket();
        while (index < mSrcText.Length)
        {
            char c = mSrcText[index];
            index++;
            if (c == '}' || c == ';')
            {
                break;
            }
            char[] stopTokens = new[] { '}' };
            string varType = FindWord(stopTokens);
            if (string.IsNullOrEmpty(varType) == false)
            {
                if (IsValidVarType(varType))
                {
                    string varName = FindWord();
                    outPutLines += "\n" + (string.Format("       public {0}  {1} ;", varType, varName));
                    newStuct.properties.Add(new PropertyPair(varType, varName));
                }
                else
                {
                    throw new Exception(string.Format("变量类型{0}不合法", varType));
                }
            }
        }
        outPutLines += ("\n    }");
    }

    /// <summary>
    /// 寻找下一个单词， 
    /// </summary>
    /// <param name="stopChars">需要检测中途退出的字符</param>
    /// <returns></returns> 
    private string FindWord(char[] stopChars = null,bool skipAngleBracket=true)
    {
        string word = "";
        while (index < mSrcText.Length)
        {
            char c = mSrcText[index];
            index++;
            if (stopChars != null && stopChars.Contains(c))
            {
                break;
            }
            if (c == '[' && skipAngleBracket) //"["
            {
                ProcessAngleBraket();
            }
            else if (c == '/')
            {
                ProcessComments();
            }
            else //如果已经找到非空的单词
                if (string.IsNullOrEmpty(word) == false)
                {
                    if (IsValidCharacter(c))
                    {
                        word += c;
                    }
                    else
                    {
                        return word;
                    }
                }
                else //首字母还没有
                {
                    if (IsValidFirstCharacter(ref c))
                    {
                        word += c;
                    }
                }
        }

        Debug.Log("word: " + word);
        return word;
    }

    private string GetNextString()
    {
        string word = "";
        bool isStarted=false;
        while (index < mSrcText.Length)
        {
            char c = mSrcText[index];
            index++;
            if (c == '"')
            {
                if (isStarted)
                {
                    break;
                }
                else
                {
                    isStarted = true;
                }
            }
            else
            {
                if (isStarted)
                {
                    word += c;
                }
            }
        }
        return word;
    }

    private void ProcessAngleBraket()
    {
        while (index < mSrcText.Length)
        {
            char c = mSrcText[index];
            index++;
            if (c == ']')
            {
                break;
            }
        }
    }

    private void ProcessComments()
    {
        outPutLines += '/';
        string comment = "";
        while (index < mSrcText.Length)
        {
            char c = mSrcText[index];
            index++;
            if (c == '\r')
            {
                outPutLines += comment;
                Debug.Log("Comment:　" + comment);
                break;
            }
            else
            {
                comment += c;
            }
        }
    }

    private bool IsValidCharacter(char c)
    {
        if ((c > 47 && c < 58) || IsAphabatic(c) //数字
          || c == '_' /* "_"*/)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否合法的的首字母
    /// </summary> 
    /// <param name="c"></param>
    /// <returns></returns> 
    private bool IsValidFirstCharacter(ref char c)
    {
        if (IsAphabatic(c))
        {
            return true;
        }
        return false;
    }

    public bool IsAphabatic(char c)
    {
        if ((c > 64 && c < 91) ||
            (c > 96 && c < 123))
        {
            return true;
        }
        return false;
    }


    private bool IsValidVarType(string str)
    {
        if ((CSharpTypes.ContainsKey(str)))
        {
            return true;
        }
        else
        {
            return mCustomStucts.Find(delegate(CustomStuct myStuct) { return myStuct.Name == str; }) != null;
        }
    }


    public string OutPutText
    {
        get
        {
            string prefix = "//This file is generated by code, DO NOT EDIT !!!! \n";
            prefix += "using UnityEngine;\n";
            return prefix + outPutLines + FileAppendix;
        }
    }

    /// <summary>
    /// 创建的类后面要跟的信息
    /// </summary>
    public string ClassAppendix
    {
        get
        {
            string str = "";
            str += "\n    public override int Key\n    {\n";
            str += "        get { ";
            str +=String.Format(" return {0};", KeyPropertyName);
            str += " }\n    }";

            str += DecodeMethod;
            return str;
        }
    }

    public string DecodeMethod
    {
        get
        {
            string str = "";
            str += "\n  public override void Decode(byte[] byteArr, ref int bytePos)\n   {";
            for (int i = 0; i < allPropertyPairs.Count; i++)
            {
                PropertyPair pair = allPropertyPairs[i];
                if (DictVarDecodeMethod.ContainsKey(pair.type))
                {
                    string decodeMethod = DictVarDecodeMethod[pair.type];
                    str += string.Format("\n        {0}(ref byteArr,ref bytePos,out {1});", decodeMethod, pair.name);

                }
                else
                {
                    Debug.LogError("类型"+pair.type+"没有Decode函数");
                }
            }

            str += "\n  }";
            return str;
        }
    }

    

    public string FileAppendix
    {
        get
        {
            string tableManager = "\n/// <summary>\n///表文件" + SrcFileName + "管理类\n/// </summary>";
             tableManager += string.Format("\npublic class {0}TableManager : TableManager<{0}>\n", destClassName);
            tableManager += "{\n";
            tableManager += string.Format("   public static readonly {0}TableManager instance=new {0}TableManager();", destClassName);
            tableManager += "\n}";
            return tableManager;
        }
    }
}


public class CustomStuct
{
    public string Name;
    public List<PropertyPair> properties = new List<PropertyPair>();

    public CustomStuct(string structName)
    {
        Name = structName;
    }
}

public class PropertyPair
{
    public string type;
    public string name;

    public PropertyPair(string pType, string pName)
    {
        type = pType;
        name = pName;
    }
}