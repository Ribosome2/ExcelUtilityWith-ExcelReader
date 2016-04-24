using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Excel;
using UnityEditor;
using UnityEngine;

public static class CodeGeneratorHelper
{
    private static string srcFolderPath
    {
        get { return Application.dataPath + "/../Doc"; }
    }

    private static string outPutFolderPath
    {
        get { return Application.dataPath + "/AIFrame/TableCode"; }
    }

    private static string outPutDataPath
    {
        get { return Application.dataPath + "/AIFrame/TableData"; }
    }

    private static string tableDataOutputPath
    {
        get { return Application.dataPath + "/AIFrame/Resources"; }
    }

    public static void ScanFiles()
    {
        string[] files = Directory.GetFiles(srcFolderPath, "*.prot");
        for (int i = 0; i < files.Length; i++)
        {
            CreateCodeFile(files[i]);
        }
    }

    private static void TableDataToBinary(ProtoFileConvertor convertor)
    {
        string path = srcFolderPath +"/"+ convertor.SrcFileName;
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0];
        int columns = table.Columns.Count;
        int rows = table.Rows.Count;
        TableEncoder encoder=new TableEncoder();
        encoder.WriteInt(rows-1);  //先标记数据量,其中第一行作为标记是不应该算进去的
        for (int i = 1; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                //第一行当用来当这一列的字段名称
                string fieldName = table.Rows[0][j].ToString();
                if (convertor.propertyTableMap.ContainsKey(fieldName))
                {
                    object data = table.Rows[i][j];
                    //Debug.Log(data.ToString());
                    string dataType = convertor.propertyTableMap[fieldName];
                    #region 根据字段类型Encode
                    if (dataType == CommmonVarType.INT16)
                    {
                        encoder.WriteShort((short)Convert.ChangeType(data, typeof(short)));
                    }
                    else if (dataType == CommmonVarType.INT32)
                    {
                        encoder.WriteInt((int)Convert.ChangeType(data,typeof(int)));
                    }
                    else if (dataType == CommmonVarType.INT64)
                    {
                        encoder.WriteInt64((long)Convert.ChangeType(data, typeof(long)));
                    }
                    else if (dataType == CommmonVarType.INT8 || dataType == CommmonVarType.UINT8)
                    {
                        encoder.WriteByte((byte)Convert.ChangeType(data,typeof(byte)));
                    }
                    else if (dataType == CommmonVarType.STRING)
                    {
                        encoder.WriteString(data as string);
                    }
                    else if (dataType == CommmonVarType.UINT16)
                    {
                        encoder.WriteUShort((ushort)Convert.ChangeType(data, typeof(ushort)));
                    }
                    else if (dataType == CommmonVarType.UINT32)
                    {
                        encoder.WriteUInt((uint)Convert.ChangeType(data, typeof(uint)));
                    }
                    else if (dataType == CommmonVarType.UINT64)
                    {
                        encoder.WriteUInt64((ulong)Convert.ChangeType(data, typeof(ulong)));
                    }
                    else
                    {
                        Debug.LogError("未实现解析类型" + dataType);
                    } 
                    #endregion

                    
                }
                else
                {
                    Debug.LogError("跳过");
                }
               
            }
        }

        byte[] fileBytes = encoder.listBytes.ToArray();
        string binPath = tableDataOutputPath + "/" + convertor.destClassName + ".kiss";
        File.WriteAllBytes(binPath,fileBytes);
    }


    public static void CreateCodeFile(string srcPath)
    {
        if (File.Exists(srcPath))
        {
            string fileText = File.ReadAllText(srcPath);
            ProtoFileConvertor con = new ProtoFileConvertor(fileText);
            string className = con.GetFileName();
            string path = outPutFolderPath + "/" + con.GetFileName();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, con.OutPutText);
            TableDataToBinary(con);
           
        }
        else
        {
            Debug.Log(string.Format("路径{0}不存在", srcPath));
        }
    }



public  class TableEncoder {

    
  
    public   List<byte> listBytes=new List<byte>();
    public  void WriteUShort(ushort intValue)
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);
       
    }
    public  void WriteShort(short intValue)
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);
       
    }

    public  void WriteInt(int intValue )
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);
       
    }

    public  void WriteUInt(uint intValue)
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);
    }

    public void WriteInt64(long intValue)
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);

    }

    public void WriteUInt64(ulong intValue)
    {
        byte[] bs = BitConverter.GetBytes(intValue);
        listBytes.AddRange(bs);
    }
    public void WriteByte(byte intValue)
    {
        listBytes.Add(intValue);
    }

    /// <summary>
    /// 写字符传，先写一个数来标记长度，后面的字节才是真正的字符串内容
    /// </summary>
    /// <param name="str"></param>
    public  void  WriteString(string str )
    {
        ushort byteCount=(ushort)Encoding.UTF8.GetByteCount(str);
        WriteUShort(byteCount);
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        listBytes.AddRange(strBytes);

    }
}

}
