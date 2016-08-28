# ExcelUtilityWith-ExcelReader
How this tool works:
1.read "*.proto" to get table structure infomations
2.generate C# table manager code files based on proto information
3.using ExcelReader to read excel file and then generate binary files based on proto file

工作原理：
1.proto文件负责描述定义源文件，以及在源文件中每个字段的数据类型
2.运行生成程序的过程中，先扫描proto文件夹中的所有.proto文件，
3.解析每个文件，获得数据表名称以及对应的数据结构，
4.根据上面获得的数据结构信息，遍历excel表，把里面的数据序列化为对应的二进制文件，
  同时生成对应的C#的解析代码
5.使用上面生成的C#代码把二进制文件加载进来，就可以直接用了




