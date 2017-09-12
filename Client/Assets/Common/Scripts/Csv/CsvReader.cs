#region Header
/**
 * 名称：CsvReader
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Csv
{
    // 定义状态  
    public enum enReadState
    {
        FieldStart,// 新字段开始 
        NonQuotesField,// 非引号字段  
        QuotesField,// 引号字段  
        QuoteInQuotesField,// 引号字段中的引号  
        Error,// 语法错误  
    };

    public class CsvReader
    {
        public class StateRow
        {
            public readonly bool NeedChange;//如果解析到不在handles里的字符需不需要切状态
            public readonly enReadState StateTo;//如果解析到不在handles里的字符要切的状态
            public readonly bool AddValue;

            public Dictionary<char, StateCol> handles = new Dictionary<char,StateCol>();
            public StateCol this[char c]{
                get{ 
                    StateCol col;
                    if (!handles.TryGetValue(c, out col))
                        return null;
                    return col;
                }
                set { handles[c]=value;}
            }
            public StateRow() { AddValue = false;NeedChange = false; StateTo = enReadState.Error; }
            public StateRow(enReadState s) { AddValue = false; NeedChange = true; StateTo = s; }
            public StateRow(enReadState s, bool addValue) { AddValue = addValue; NeedChange = true; StateTo = s; }
        }


        public class StateCol
        {
            public readonly bool NeedChange ;
            public readonly bool AddValue ;
            public readonly bool RowEnd ;//行尾
            public readonly enReadState StateTo;

            public StateCol() { RowEnd = false; AddValue = false; NeedChange = false; StateTo = enReadState.Error; }
            public StateCol(enReadState s) { RowEnd = false; AddValue = false; NeedChange = true; StateTo = s; }

            public StateCol(enReadState s, bool addValue){ RowEnd = false; AddValue = addValue; NeedChange = true; StateTo = s; }
            public StateCol(bool rowEnd){ RowEnd = true; AddValue = false; NeedChange = true; StateTo = enReadState.FieldStart; }
        }
            
        #region Fields
        static Dictionary<enReadState, StateRow> s_parseTable;//状态表，比状态机更方便的实现
        static char[] s_charCache = null;

        bool m_isEndOfRow = true;//当前值是不是行尾
        StringBuilder m_value = new StringBuilder();
        enReadState m_state = enReadState.FieldStart;
        char[] m_buffer;
        int m_bufferLength;
        int m_idx=0;//索引到什么地方了
        int m_row=-1;
        int m_col=0;

        #endregion

        #region Public Properties
        public string Value{get { return m_value.ToString(); }}

        public bool IsError { get{return m_state == enReadState.Error;}}

        public bool IsEndOfRow { get{return m_isEndOfRow;}}

        public int Row { get { return m_row == -1 ? 0 : m_row; } }

        public int Col { get { return m_col; } }
        #endregion

        #region Constructors
        static CsvReader()
        {
            s_parseTable = new Dictionary<enReadState, StateRow>();

            StateRow table = new StateRow(enReadState.NonQuotesField,true);
            s_parseTable[enReadState.FieldStart] = table;
            table[','] = new StateCol(enReadState.FieldStart);
            table['"'] = new StateCol(enReadState.QuotesField);
            table['\r'] = new StateCol();
            table['\n'] = new StateCol(true);

            table = new StateRow();
            s_parseTable[enReadState.NonQuotesField] = table;
            table[','] = new StateCol(enReadState.FieldStart);
            table['"'] = new StateCol(enReadState.Error);
            table['\r'] = new StateCol();
            table['\n'] = new StateCol(true);

            table = new StateRow();
            s_parseTable[enReadState.QuotesField] = table;
            table['"'] = new StateCol(enReadState.QuoteInQuotesField);

            table = new StateRow(enReadState.Error);
            s_parseTable[enReadState.QuoteInQuotesField] = table;
            table[','] = new StateCol(enReadState.FieldStart);
            table['"'] = new StateCol(enReadState.QuotesField,true);
            table['\r'] = new StateCol();
            table['\n'] = new StateCol(true);

        }
        
        public static char[] GetChar(int lenght)
        {
            if (s_charCache == null || s_charCache.Length < lenght)
            {
                s_charCache = new char[lenght];
            }

            return s_charCache;
        }

        //加载完所有配置后一定要清空这里
        public static void Clear()
        {
            s_charCache = null;
        }

        public CsvReader(string csvText)
        {
            m_buffer = csvText.ToCharArray();
            m_bufferLength = m_buffer.Length;
        }
        //使用共享的缓存会提升效率
        public CsvReader(byte[] bytes)
        {
            m_buffer = GetChar(bytes.Length);
            m_bufferLength = System.Text.Encoding.UTF8.GetChars(bytes, 0, bytes.Length, m_buffer, 0);
        }

        #endregion

        #region Static Methods

        #endregion

        #region Private Methods
        
        #endregion

        //读取一个单元格的数据，返回值为false表示读取不到数据或者出错了
        public bool Read()
        {
            if (m_state == enReadState.Error )
                return false;

            //到最尾了
            if (m_idx >= m_bufferLength)
            {
                m_isEndOfRow = true;
                if (m_value.Length > 0)
                    m_value.Remove(0, m_value.Length);
                return false;
            }

            //检错下
            if (m_state != enReadState.FieldStart)
            {
                Debuger.LogError("逻辑出错");
                m_state = enReadState.FieldStart;
            }
    
            //初始化下
            bool oldEndOfRow = m_isEndOfRow;
            m_isEndOfRow = false;
            if(m_value.Length >0)
                m_value.Remove(0,m_value.Length);

            //开始逐个解析
            char c;
            StateRow table;
            StateCol handle;
            do{
                c = m_buffer[m_idx++];
                table =s_parseTable[m_state];
                handle = table[c];

                if (handle != null)
                {
                    if(handle.NeedChange) m_state = handle.StateTo;
                    if(handle.AddValue) m_value.Append(c);
                    if(handle.RowEnd)
                    {
                        m_isEndOfRow =true;
                        if (m_state != enReadState.FieldStart) Debuger.LogError(string.Format("没有读取完却标志了换行标识 行:{0} 列:{1}", m_row, m_col));                           
                    }
                        
                }
                else if (table.NeedChange){
                    m_state = table.StateTo;
                    if (table.AddValue) m_value.Append(c);
                }
                else
                    m_value.Append(c);

                if (m_state == enReadState.FieldStart || (m_idx >= m_bufferLength && m_state == enReadState.NonQuotesField) || (m_idx >= m_bufferLength && m_state == enReadState.QuoteInQuotesField))
                {
                    //计算行列
                    if (oldEndOfRow)
                    {
                        ++m_row;
                        m_col = 0;
                    }
                    else
                        ++m_col;
                    return true;
                }

                //判断各种出错情况
                if (m_state == enReadState.Error || m_idx >= m_bufferLength  )
                {
                    //if (m_value.Length > 0)m_value.Remove(0, m_value.Length);
                    m_state = enReadState.Error;
                    return false;
                }

                
                    
            }while(true);
        }


    }

}
