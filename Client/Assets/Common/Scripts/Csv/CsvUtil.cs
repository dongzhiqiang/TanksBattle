#region Header
/**
 * 名称：csv工具类
 * 日期：2015.9.16
 * 描述：外部一般用这个类就够了
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace Csv
{
    public class CsvUtil
    {
        #region Fields
        static List<string> s_fields=null;//避免读取多张表格的时候多次申请内存
        static List<string> s_descs = null;
        static Dictionary<string,FieldInfo> s_fieldInfos= null;
        #endregion

        #region Properties
        static List<string> Fields{get{
            if(s_fields == null)
                s_fields=  new List<string>();
            return s_fields;            
        }}

        static List<string> Descs {get{
                if (s_descs == null)
                    s_descs = new List<string>();
                return s_descs;
            }
        }

        static Dictionary<string,FieldInfo> FieldInfos{get{
            if(s_fieldInfos == null)
                s_fieldInfos=  new Dictionary<string,FieldInfo>();
            return s_fieldInfos;            
        }}
        #endregion
        public static void Clear(){
            s_fields = null;
            s_fieldInfos = null;
            s_descs = null;
        }

        public static CsvReader LoadReader(string path)
        {
            TextAsset a = Resources.Load<TextAsset>(path);
            if (a == null)
            {
                Debuger.LogError(string.Format("找不到{0}.csv", path));
                return null;
            }

            return new CsvReader(a.bytes);
        }
        public static List<T> Load<T>(string csvName) where T : new()
        {
            List<T> l = new List<T>();//用反射设置每个字段
            Type type = typeof(T);
            //string csvName = type.Name+"Config";
            CsvReader r = LoadReader(csvName);
            if (r == null)
                return l;

            Fields.Clear();
            FieldInfos.Clear();
            bool endOfRow = true;

            //第一行为描述，不解析
            while (r.Read() && !r.IsEndOfRow) { }

            //第二行为字段名
            while (r.Read())
            {
                if (!string.IsNullOrEmpty(r.Value))
                    Fields.Add(r.Value);
                if (r.IsEndOfRow)
                    break;
            }
            if (Fields.Count == 0)
            {
                Debuger.LogError(string.Format("{0}.csv解析不到列名", csvName));
                return l;
            }

            //找到csv字段对应的类的字段
            foreach (FieldInfo info in type.GetFields ()) {
                FieldInfos[info.Name] = info;
            }
            foreach(string f in Fields){
                if(!FieldInfos.ContainsKey(f))
                    Debuger.LogError(string.Format("{0}.csv 不包含列名:{1}", csvName, f));
            }
            
            T row=default(T);
            FieldInfo fieldInfo;
            string val;
            object outVal;
            
            //剩下的行数据
            while (r.Read())
            {
                if (endOfRow)
                {
                    row = new T();
                    l.Add(row);
                }

                endOfRow = r.IsEndOfRow;
                if (r.Col >= Fields.Count)
                    continue;

                val = r.Value;
                fieldInfo = FieldInfos.Get(Fields[r.Col]);
                if (fieldInfo == null || string.IsNullOrEmpty(val))//空字符的话算默认值
                    continue;

                if (StringUtil.TryParse(val, fieldInfo.FieldType, out outVal))
                    fieldInfo.SetValue(row, outVal);
                else
                    Debuger.LogError(string.Format("{0}.csv解析出错  行:{1} 列:{2} 列名:{3} ", csvName, r.Row, r.Col, Fields[r.Col]));
            }
            
            return l;
        }

        public static Dictionary<TKeyType, T> Load< TKeyType,T>(string csvName,string key,Func<T,string,CsvReader,string,bool> onParseEx= null) where T : new()
        {
            Dictionary<TKeyType, T> l = new Dictionary<TKeyType, T>();//用反射设置每个字段
            Type type = typeof(T);
            //string csvName = type.Name + "Config";
            CsvReader r = LoadReader(csvName);
            if (r == null)
                return l;

            Fields.Clear();
            FieldInfos.Clear();
            bool endOfRow = true;

            //第一行为描述，不解析
            while (r.Read() && !r.IsEndOfRow) { }

            //第二行为字段名
            while (r.Read())
            {
                Fields.Add(r.Value);
                if (r.IsEndOfRow)
                    break;
            }
            if (Fields.Count == 0)
            {
                Debuger.LogError(string.Format("{0}.csv解析不到列名", csvName));
                return l;
            }

            //找到csv字段对应的类的字段
            FieldInfo keyField=null;
            foreach (FieldInfo info in type.GetFields())
            {
                FieldInfos[info.Name] = info;
                if (key == info.Name)
                    keyField=info;
            }
           
            if (keyField == null)
            {
                Debuger.LogError(string.Format("{0}.csv 类找不到对应的键值字段:{1}", csvName, key));
                return l;
            }
            if (!keyField.FieldType.Equals(typeof(TKeyType)))
            {
                Debuger.LogError(string.Format("{0}.csv 键值字段的类型和Dictionary不一致:{1}", csvName, key));
                return l;
            }
            if (!Fields.Contains(key))
            {
                Debuger.LogError(string.Format("{0}.csv 不包含键值列:{1} 是否被删除？", csvName, key));
                return l;
            }


            T row = default(T);
            FieldInfo fieldInfo;
            string val;
            object outVal;
            TKeyType k= default(TKeyType);
            
            //剩下的行数据
            while (r.Read())
            {
                
                if (endOfRow)
                {
                    if(row != null)
                    {
                        if(l.ContainsKey(k))
                             Debuger.LogError(string.Format("{0}.csv 行:{1} 键值({2})重复 是不是没有填 ", csvName, r.Row,key));
                        l[k]= row;
                    }
                    row = new T();
                }
                
                endOfRow = r.IsEndOfRow;
                val = r.Value;
                if(r.Col>=Fields.Count)
                {
                    continue;
                }
                fieldInfo = FieldInfos.Get(Fields[r.Col]);
                if (onParseEx!= null)
                    if (!onParseEx(row, Fields[r.Col], r, val))//如果扩展解析出错了，那么这一行不算
                        continue;
                
                if (fieldInfo == null || string.IsNullOrEmpty(val))//空字符的话算默认值
                    continue;

                if (StringUtil.TryParse(val, fieldInfo.FieldType, out outVal)){
                    fieldInfo.SetValue(row, outVal);
                    if (fieldInfo.Name == key)
                        k = (TKeyType)outVal;
                }
                else
                    Debuger.LogError(string.Format("{0}.csv解析出错  行:{1} 列:{2} 列名:{3} ", csvName, r.Row, r.Col, Fields[r.Col]));
            }
            if (row != null&&!l.ContainsKey(k))//最后一行补上
            {
                l[k] = row;
            }
            return l;
        }
        
        //注意fields是共用的，要确保在下次load之后不使用
        public static bool Load(string path, out List<Dictionary<string, string>> l, out List<string> fields, out List<string> descs)
        {
            l = new List<Dictionary<string, string>>();//用反射设置每个字段
            Fields.Clear();
            Descs.Clear();
            fields = Fields;
            descs = Descs;
            CsvReader r = LoadReader(path);
            if(r == null)
                return false;
            Dictionary<string, string> d=null ;
            bool endOfRow = true;
            
            
            //第一行为描述，不解析
            while (r.Read()){
                Descs.Add(r.Value);
                if (r.IsEndOfRow)
                    break;
            }

            //第二行为字段名
            while (r.Read())
            {
                Fields.Add(r.Value);
                if (r.IsEndOfRow)
                    break;
            }
            if(Fields.Count == 0)
            {
                Debuger.LogError(string.Format("{0}.csv解析不到列名", path));
                return false;
            }

            //剩下的行数据
            while (r.Read())
            {
                if (endOfRow)
                {
                    d = new Dictionary<string,string>();
                    l.Add(d);
                }

                endOfRow =r.IsEndOfRow;

                if (r.Col >= Fields.Count || string.IsNullOrEmpty(Fields[r.Col]))
                    continue;

                if (d.ContainsKey(Fields[r.Col]))
                {
                    Debuger.LogError(string.Format("{0}.csv解析出错 字段名重复", path, Fields[r.Col]));
                    continue;
                }
                d[Fields[r.Col]]=r.Value;
            }
            
            if(r.IsError)
                Debuger.LogError(string.Format("{0}.csv解析出错 行:{1} 列:{2}", path, r.Row,r.Col));

            return !r.IsError;
        }

        public void Save(string name, List<string> desc, List<string> fields, List<List<string>> data)
        {
            System.IO.File.WriteAllText(Application.dataPath + "/Config/Resources/" + name + ".csv",
                    CsvWriter.WriteAll(desc, fields, data));
            Debuger.Log("保存{0}.csv成功", name);
        }
    }
}
