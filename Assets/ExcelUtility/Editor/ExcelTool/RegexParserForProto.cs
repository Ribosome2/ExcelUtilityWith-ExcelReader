using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public static class RegexParserForProto  {

    public static void InitParse(string strSrc)
    {

       
        Regex regex = new Regex(@"class\s+(?<className>\w+)\s+\[key=(?<keyName>\w+),source=[""](?<fileSrc>\w+.[a-z]+)[""]\](?<fields>[\s\S\w\W\d\D\\[\]\n]+)");
        Match match = regex.Match(strSrc);
        if (match.Success)
        {
            Debug.Log("FindMath: class name  "+match.Groups["className"].Value+" ID: "+match.Groups["keyName"].Value+
               " FileSrc: " + match.Groups["fileSrc"].Value );
            Debug.Log(" Fields:\n " + match.Groups["fields"].Value);

            ProtoInfo info=new ProtoInfo();
            info.ClassName = match.Groups["className"].Value;
            info.KeyName = match.Groups["keyName"].Value;
            info.SrcFile = match.Groups["fileSrc"].Value;
            string fieldsStr = match.Groups["fields"].Value;


            Match m = Regex.Match(fieldsStr, @"(?<filedType>[\w\d]+)\s+(?<filedName>[\w\d]+);?\s+\[option=[""](?<tableField>\w+)[""]\]\s?(?<comment>//(.*?)\r?\n)?");

            //匹配注释的原理：(?<comment>//(.*?)\r?\n)?尤其要注意括号后面的？表示()里的匹配内容是可选的
            while (m.Success)
            {
                ProtoInfo.ProtoFiledInfo protoFiledInfo=new ProtoInfo.ProtoFiledInfo();
                protoFiledInfo.CommentStr = m.Groups["comment"].Value;
                protoFiledInfo.FieldName = m.Groups["filedName"].Value;
                protoFiledInfo.FiledType = m.Groups["filedType"].Value;
                protoFiledInfo.TableFieldName = m.Groups["tableField"].Value;
                info.FieldList.Add(protoFiledInfo);
                Debug.Log("Found  " + protoFiledInfo.FiledType +" " +protoFiledInfo.FieldName+ "  " +protoFiledInfo.CommentStr  + " " +protoFiledInfo.TableFieldName);
                m = m.NextMatch();
            }   
        }
        else
        {
            Debug.Log("Mathch fail :\n" + strSrc);
        }

    }
}



public class ProtoInfo
{
    public class ProtoFiledInfo
    {
        public string FiledType;      //字段的数据类型
        public string FieldName;      //字段名称
        public string TableFieldName;//对应的表名称
        public bool IsRepeated;      //是否列表类型
        public string CommentStr;    //注释内容
    }

    
    public string ClassName = "";   //生成的目标类名
    public string KeyName = "";     //键值的字段名
    public string SrcFile = "";//对应的源文件路径
    public List<ProtoFiledInfo> FieldList=new List<ProtoFiledInfo>(); 
    
}
