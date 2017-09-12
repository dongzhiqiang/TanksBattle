#region Header
/**
 * 名称：CsvWriter
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CsvWriter {
    static System.Text.StringBuilder s_buff = new StringBuilder();//避免频繁申请内存
    public static void Clear() { s_buff= new StringBuilder();}

    //写一个单元格
    public static void Write(string value)
    {
        int beginIdx = s_buff.Length;
        bool isNeedQuotes =false;//需要用引号括起来
        for (int i = 0; i < value.Length; ++i)
        {
            if(isNeedQuotes==false&&(value[i] == ',' || value[i] == '\r' || value[i] == '\n' || value[i] == '"'))
                isNeedQuotes = true;

            if(value[i] == '"')
                s_buff.Append('"');

            s_buff.Append(value[i]);
        }

        if (isNeedQuotes)
        {
            s_buff.Insert(beginIdx, '"');
            s_buff.Append('"');
        }
    }

    public static string WriteAll(List<string> desc, List<string> fields, List<List<string>> data)
    {
        s_buff.Remove(0,s_buff.Length);

        //描述
        int emptyCount =0;//用于计算每一行最后有几格空的，这几格不用加逗号
        for(int i=0;i<desc.Count;++i){
            if (!string.IsNullOrEmpty(desc[i]))
            {
                emptyCount = 0;
                Write(desc[i]);
            }
            else
                ++emptyCount;

            if(i != desc.Count-1)
                s_buff.Append(',');
        }
        if (emptyCount != 0)//删掉多余的逗号
            s_buff.Remove(s_buff.Length - emptyCount, emptyCount);
        s_buff.Append('\n');

        //字段
        emptyCount = 0;
        for (int i = 0; i < fields.Count; ++i)
        {
            if (!string.IsNullOrEmpty(fields[i]))
            {
                emptyCount = 0;
                Write(fields[i]);
            }
            else
                ++emptyCount;

            if (i != fields.Count - 1)
                s_buff.Append(',');
        }
        if (emptyCount != 0)//删掉多余的逗号
            s_buff.Remove(s_buff.Length - emptyCount, emptyCount);
        if (data.Count != 0)
            s_buff.Append('\n');

        //数据
        List<string> row;
        for(int i =0;i<data.Count;++i){
            row = data[i];
            emptyCount = 0;
            for (int j = 0; j < row.Count; ++j)
            {
                if (!string.IsNullOrEmpty(row[j]))
                {
                    emptyCount = 0;
                    Write(row[j]);
                }
                else
                    ++emptyCount;

                if (j != row.Count - 1)
                {
                    s_buff.Append(',');
                }
            }
            if (emptyCount != 0)//删掉多余的逗号
                s_buff.Remove(s_buff.Length - emptyCount, emptyCount);
            if (i != data.Count - 1)
                s_buff.Append('\n');
            
        }

        return s_buff.ToString();
    }
}
