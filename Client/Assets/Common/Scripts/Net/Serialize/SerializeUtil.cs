#region Header
/**
 * 名称: 
 
 * 日期：2015.10.10
 * 描述：
 *  放一些常用函数和枚举
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace NetCore
{
    
    //类型
    public enum enSerializeType
    {
        Fields,     //不是序列化对象，但是持有着为序列化对象的字段
        Boolean,
        Byte,
        Short,
        Int,
        Long,
        Float,
        Double,
        String,
        Array,
        Map,
    }

    //修改类型
    public enum enSerializeChangeType
    {
        add,
        remove,
        change,
        clear,//删除全部
    }

    public class SerializeUtil
    {
        static string log="";
        static int indent = 0;
        public static void AddLog(ISerializableObject obj)
        {
            SerializeUtil.log += string.Format("{0}:{1} ",  obj.GetType().ToString(), obj.ToString());
        }
        public static void BeginParentLog(ISerializableObject obj, int changeCount,bool serialize)
        {
            if (obj.Parent == null)
            {
                log += string.Format("\n{0}{1}{2} 变化:{3} {{\n", serialize ? "序列化" : "反序列化", "".PadLeft(indent * 4, ' '), obj.GetType().ToString(), changeCount);
            }
            else if (!string.IsNullOrEmpty(obj.FieldName))
            {
                log += string.Format("变化:{0} {{\n", changeCount);
            }
            else
            {
                log += string.Format("\n{0}{1} 变化:{2} {{\n", "".PadLeft(indent * 4, ' '), obj.GetType().ToString(), changeCount);
            }
            
            ++indent;
        }
        public static void EndParentLog(ISerializableObject obj)
        {
            --indent;
            SerializeUtil.log += string.Format("{0}}} ", "".PadLeft(indent * 4, ' '));
            if (obj.Parent == null)
            {
                Debuger.Log(log);
                log = "";
            }
        }
        public static void BeginChangeLog(int idx, enSerializeChangeType type, string head = "")
        {
            SerializeUtil.log += string.Format("{0}{1} {2} {3} [", "".PadLeft(indent * 4, ' '), ""/*idx*/, type == enSerializeChangeType.change ? "":type.ToString(), head == "-1" ? "" : head);
            ++indent;
        }
        public static void EndChangeLog()
        {
            --indent;
            SerializeUtil.log += string.Format("]\n");
        }
        
    }
}