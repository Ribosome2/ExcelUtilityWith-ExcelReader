using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.Collections;

//[System.Serializable]
//public class RoleInfo : TableData //代码生成的类
//{
//    public int id;
//    public string Name;
//    public int Level;
   
//    public override int Key
//    {
//        get { return id; }
//    }

//    public override void Decode(byte[] byteArr, ref int bytePos)
//    {
//        ReadInt32(ref byteArr,ref bytePos,out id);
//        ReadString(ref byteArr,ref bytePos,out Name);
//        ReadInt32(ref byteArr,ref bytePos,out Level);
//    }
//}

//public class RoleInfoManager : TableManager<RoleInfo>
//{
    
//}

///这里是所有表类通用的泛型
public interface ITableManager
{
    void LoadData(string path);
}

public abstract class TableData
{
    public abstract int Key { get; }
    const int INT32_LENGTH = 4;
    const int INT64_LENGTH = 8;
    const int SHORT16_LEN = 2;

    public abstract void Decode(byte[] byteArr, ref int bytePos);
  

    protected static void ReadUShort(ref byte[] byteArray ,ref int mByteOffset,out ushort outputValue)
    {
        outputValue = BitConverter.ToUInt16(byteArray, mByteOffset);
        mByteOffset += SHORT16_LEN;
    }

    protected static void ReadShort(ref byte[] byteArray, ref int mByteOffset, out short outputValue)
    {
        outputValue = BitConverter.ToInt16(byteArray, mByteOffset);
        mByteOffset += SHORT16_LEN;
    }

    protected static void ReadInt32(ref byte[] byteArray, ref int mByteOffset, out int outputValue)
    {
        outputValue = BitConverter.ToInt32(byteArray, mByteOffset);
        mByteOffset += INT32_LENGTH;
    }
    protected static void ReadUInt32(ref byte[] byteArray, ref int mByteOffset, out uint outputValue)
    {
        outputValue = BitConverter.ToUInt32(byteArray, mByteOffset);
        mByteOffset += INT32_LENGTH;
    }

    protected static void ReadInt64(ref byte[] byteArray, ref int mByteOffset, out long outputValue)
    {
        outputValue = BitConverter.ToInt32(byteArray, mByteOffset);
        mByteOffset += INT64_LENGTH;
    }
    protected static void ReadUInt64(ref byte[] byteArray, ref int mByteOffset, out ulong outputValue)
    {
        outputValue = BitConverter.ToUInt32(byteArray, mByteOffset);
        mByteOffset += INT32_LENGTH;
    }

    protected static void ReadInt8(ref byte[] byteArray, ref int mByteOffset, out byte outputValue)
    {
        outputValue = byteArray[mByteOffset];
        mByteOffset += 1;
    }
    protected static void ReadString(ref byte[] byteArray, ref int mByteOffset, out string str)
    {
        ushort len;
        ReadUShort(ref  byteArray, ref  mByteOffset,out len);
        str = Encoding.UTF8.GetString(byteArray, mByteOffset, len);
        mByteOffset += len;
    }

}

public class TableManager<T> where T : TableData
{

    protected List<T> mDataList = new List<T>();
    public T FindByKey(int key)
    {
        for (int i = 0; i < mDataList.Count; i++)
        {
            if (mDataList[i].Key == key)
            {
                return mDataList[i];
            }
        }

        return null;
    }
    public T GetByIndex(int index)
    {
        
        return mDataList[index];
    }
    public int Size()
    {
        return mDataList.Count;
    }

    /// <summary>
    /// 用二进制数据初始化
    /// </summary>
    /// <param name="tableBytes"></param>
    public void LoadData(byte[] tableBytes)
    {
        mDataList.Clear();
        int pos = 0;
        int dataCount =  BitConverter.ToInt32(tableBytes,pos);
        pos += Marshal.SizeOf(pos);
        for (int i = 0; i < dataCount ; i++)
        {
            T data = Activator.CreateInstance<T>();
            data.Decode(tableBytes,ref pos);
            mDataList.Add(data);
        }
    }
}