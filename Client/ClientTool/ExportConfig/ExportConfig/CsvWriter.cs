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
    private System.Text.StringBuilder m_buff = new StringBuilder();//避免频繁申请内存
    public void Clear() { m_buff= new StringBuilder();}

    //写一个单元格
    public void Write(string value)
    {
        int beginIdx = m_buff.Length;
        bool isNeedQuotes =false;//需要用引号括起来
        for (int i = 0; i < value.Length; ++i)
        {
            if(isNeedQuotes==false&&(value[i] == ',' || value[i] == '\r' || value[i] == '\n' || value[i] == '"'))
                isNeedQuotes = true;

            if(value[i] == '"')
                m_buff.Append('"');

            m_buff.Append(value[i]);
        }

        if (isNeedQuotes)
        {
            m_buff.Insert(beginIdx, '"');
            m_buff.Append('"');
        }
    }

    public string WriteAll(List<string> desc, List<string> fields, List<List<string>> data)
    {
        m_buff.Remove(0,m_buff.Length);

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
                m_buff.Append(',');
        }
        if (emptyCount != 0)//删掉多余的逗号
            m_buff.Remove(m_buff.Length - emptyCount, emptyCount);
        m_buff.Append('\n');

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
                m_buff.Append(',');
        }
        if (emptyCount != 0)//删掉多余的逗号
            m_buff.Remove(m_buff.Length - emptyCount, emptyCount);
        if (data.Count != 0)
            m_buff.Append('\n');

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
                    m_buff.Append(',');
                }
            }
            if (emptyCount != 0)//删掉多余的逗号
                m_buff.Remove(m_buff.Length - emptyCount, emptyCount);
            if (i != data.Count - 1)
                m_buff.Append('\n');
            
        }

        return m_buff.ToString();
    }
}
