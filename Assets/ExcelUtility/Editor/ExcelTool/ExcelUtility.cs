using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Excel;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class ExcelUtility : EditorWindow {
    [MenuItem("Docs/ReadExcel")]
    public static void TestRead()
    {
        ProcessSingleFile(Application.dataPath + "/../Doc/AI角色配置.xlsx");
    }

    [MenuItem("Docs/GenerateCode")]
    public static void TestCodeGenerate()
    {
        CodeGeneratorHelper.ScanFiles();
    }

    private static  string[] COMMONTYPES =new string[] {"int,string"};


    private static void ProcessSingleFile(string path)
    {
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0];
        int columns = table.Columns.Count;
        int rows = table.Rows.Count;
        List<FieldPair> fieldPairs = new List<FieldPair>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                

               // Debug.Log(nvalue);
            }
        }
    }

    


    static void LogError(string logStr)
    {
        Debug.LogError(logStr);
    }
   public class  FieldPair
   {
       public string type;
       public string name;

       public FieldPair(string fieldType, string fieldName)
       {
           type = fieldType;
           name = fieldName;
       }
   }
	
}
