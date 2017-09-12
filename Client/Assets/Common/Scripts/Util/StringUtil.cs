#region Header
/**
 * 名称：字符串工具
 
 * 日期：2015.9.17
 * 描述：主要负责进入游戏时各个系统初始化，以及跳转到登录态
 **/
#endregion
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class StringUtil
{
    #region Fields
    static char[] s_indentSplit = new char[] { ',', '|', ':', ';', '^' };
    #endregion

    public static string Limit(string str,int limit,string dots = "...")
    {
        return str.Length > limit ? str.Substring(0, limit) + dots : str;
    }

    public static bool TryParse(string[] a, ref List<int> l)
    {
        l.Clear();
        if (a == null || a.Length == 0)
            return true;
        foreach (string s in a)
        {
            int i;
            if (!int.TryParse(s, out i))
                return false;

            l.Add(i);
        }
        return true;
    }

    public static string[] ToArr<T>(T[] a)
    {
        if (a == null || a.Length == 0)
            return null;
        return System.Array.ConvertAll<T, string>(a, obj=>obj.ToString());
    }
    
    public static string Join<T>(T[] a, string split)
    {
        if (a == null || a.Length == 0)
            return "";
        return string.Join(split, ToArr<T>(a));
    }

    public static string Join<T>(List<T> a, string split=",")
    {
        if (a == null || a.Count== 0)
            return "";
        return string.Join(split, ToArr<T>(a.ToArray()));
    }

    

    public static bool TryParse(string s,out bool v){
        v =false;
        if(string.IsNullOrEmpty(s))
            return false;
        if (s == "1") { v = true; return true;}
        if (s == "0") { v = false; return true; }
        return false;
    }

    public static void Parse(string s, ref List<int> l, char split = ',')
    {
        l.Clear();
        if (string.IsNullOrEmpty(s))
            return ;
        int i;
        string[] ss = s.Split(split);
        foreach (string item in ss)
        {
            if (int.TryParse(item, out i))
                l.Add(i);
        }
    }

    public static bool TryParse(string s ,out Vector3 v,char split=',')
    {
        v = Vector3.zero;
        if (string.IsNullOrEmpty(s))
        {
            return true;
        }
            
        string[] pp = s.Split(split);
        if (pp.Length < 3)
        {
            Debuger.LogError("ParseVector error :" + s);
            return false;
        }

        float temFloat;
        for (int j = 0; j < pp.Length &&j<3; j++)
        {
            if (float.TryParse(pp[j], out temFloat))
                v[j] = temFloat;
            if (float.TryParse(pp[j].Trim(), out temFloat))
                v[j] = temFloat;
            else
            {
                string[] pp2 = pp[j].Split('=');
                if (pp2 == null || pp2.Length < 2)
                {
                    Debuger.LogError("ParseVector error2 :" + s);
                    return false;
                }

                if (float.TryParse(pp2[1].Trim(), out temFloat))
                    v[j] = temFloat;
                else
                {
                    Debuger.LogError("ParseVector error3 :" + s);
                    return false; 
                }

            }
             
        }
        return true;
    }

    public static bool TryParse(string val,Type type,out object outVal)
    {
        return TryParse(0,val,type,out outVal);
    }

    static Type[] stringParam = new Type[] { typeof(string) };
    static Type[] stringArrParam = new Type[] { typeof(string[]) };
    static bool TryParse(int indent,string val,Type type,out object outVal)
    {   
        outVal = null;
        

        if (type.Equals(typeof(string)))
            outVal = val;
        else if (type.Equals(typeof(bool)))
        {
            bool temBool;
            if (StringUtil.TryParse(val, out temBool))
                outVal =temBool;
            else
                return false;
        }
        else if (type.Equals(typeof(int)))
        {
            int temInt;
            if (int.TryParse(val, out temInt))
                outVal = temInt;
            else
                return false;
        }
        else if (type.Equals(typeof(float)))
        {
            float temFloat;
            if (float.TryParse(val, out temFloat))
                outVal =  temFloat;
            else
                return false;
        }
        else if (type.Equals(typeof(Vector3)))
        {
            Vector3 temV3;
            if (StringUtil.TryParse(val, out temV3,s_indentSplit[indent]))
                outVal = temV3;
            else
                return false;
        }
        else if (type.IsEnum)
        {
            int temInt;
            if (int.TryParse(val, out temInt))
                outVal = Enum.ToObject(type, temInt);
            else
                outVal= Enum.Parse(type, val);
        }
        else if (type.Equals(typeof(long)))
        {
            long temLong;
            if (long.TryParse(val, out temLong))
                outVal = temLong;
            else
                return false;
        }
        else if (type.Equals(typeof(double)))
        {
            double temDouble;
            if (double.TryParse(val, out temDouble))
                outVal= temDouble;
            else
                return false;
        }
        else if (type.IsArray)//数组
        {
            string[] pp = val.Split(s_indentSplit[indent]);
            if (pp != null && pp.Length > 0)
            {
                Type elemType = type.GetElementType();
                Array arr = Array.CreateInstance(elemType, pp.Length);
                object arrObj;
                for (int i = 0; i < pp.Length; ++i)
                {
                    if(TryParse(indent+1,pp[i],elemType,out arrObj))
                        arr.SetValue(arrObj, i);
                    else
                        return false;
                }
                outVal = arr;
            }
        }
        else if (type.GetInterface ("System.Collections.IList") != null)//list
        {
            string[] pp = val.Split(s_indentSplit[indent]);
            if (pp != null && pp.Length > 0)
            {
                MethodInfo m = type.GetMethod("get_Item");//list的索引器的返回值就是对应类型
                if(m==null)
                {
                    Debuger.LogError("找不到索引器 不能确定类型:" + type.Name);
                    return false;
                }

                IList list= (IList)Activator.CreateInstance(type);
                object arrObj;
                for (int i = 0; i < pp.Length; ++i)
                {
                    if (TryParse(indent + 1, pp[i], m.ReturnType, out arrObj))
                        list.Add(arrObj);
                    else
                        return false;
                }
                outVal = list;
            }
                
        }
        else
        {
            //如果有带一个string参数的构造函数
            ConstructorInfo constructor =type.GetConstructor(stringParam);
            if(constructor != null)
            {
                outVal = Activator.CreateInstance(type, val);
            }
            //如果有个Init(string)函数，这个主要是用于一些类必须构造函数没有参数的情况
            else 
            {
                string[] pp = val.Split(s_indentSplit[indent]);
                MethodInfo initMethod = type.GetMethod("Init", stringArrParam);
                if (initMethod != null)
                {
                    outVal = Activator.CreateInstance(type);
                    initMethod.Invoke(outVal,new object[] { pp });
                }
                else
                    return false;
            }    
            
        }
            
        return true;
    }
    public static string ToHexString(byte[] bytes,int idx=0,int length=0) // 0xae00cf => "AE00CF "
    {
        string hexString = string.Empty;
        if (bytes != null)
        {
            if(length == 0)
                length = bytes.Length -idx;
            StringBuilder strB = new StringBuilder();

            for (int i = idx; i < idx+length; i++)
            {
                strB.Append(bytes[i].ToString("X2"));
            }
            hexString = strB.ToString();
        }
        return hexString;
    }

    //可以解析整数或者百分比，比如30%，30
    public static bool TryParsePercent(string s, out bool isPercent, out float val)
    {
        isPercent = s.EndsWith("%");
        val = 0;
        if (isPercent)//百分比
        {
            if (!float.TryParse(s.Substring(0, s.Length - 1), out val))
                return false;

            val /= 100f;//百分比的话要除以100
        }
        else
        {
            if (!float.TryParse(s, out val))
                return false;
        }

        return true;
    }

    //向一个数字左边填充0，以达到多少个字符,fillCount表示要填充够多少个字符
    public static string FillNum(int num, int fillCount)
    {
        string str;
        if (num < 0)
            str = (-num).ToString();
        else
            str = num.ToString();

        for (int i = str.Length; i < fillCount; ++i)
        {
            str = "0" + str;
        }

        return str;
    }

    //90=> 01:30
    public static string SceIntToMinSceStr2(int sce)
    {
        if (sce <= 0)
            return "00分00秒";
        long tick = 10000000 * (long)sce;
        System.TimeSpan span = new TimeSpan(tick);
        return string.Format("{0}分{1}秒", FillNum(span.Minutes, 2), FillNum(span.Seconds, 2));
    }

    //90=> 01:30
    public static string SceIntToMinSceStr(int sce)
    {
        if (sce <= 0)
            return "00:00";
        long tick = 10000000 * (long)sce;
        System.TimeSpan span = new TimeSpan(tick);
        return string.Format("{0}:{1}", FillNum(span.Minutes, 2), FillNum(span.Seconds, 2));
    }

    //90=> 00:01:30
    public static string SceIntToHourMinSceStr(int sce)
    {
        long tick = 10000000 * (long)sce;
        System.TimeSpan span = new TimeSpan(tick);
        return string.Format("{0}:{1}:{2}", FillNum(span.Hours, 2), FillNum(span.Minutes, 2), FillNum(span.Seconds, 2));
    }

    public static int ToInt(string s, int def = 0)
    {
        int n;
        if (!int.TryParse(s, out n))
            return def;
        return n;
    }

    public static long ToLong(string s, long def = 0)
    {
        long n;
        if (!long.TryParse(s, out n))
            return def;
        return n;
    }

    public static float ToFloat(string s, float def = 0)
    {
        float n;
        if (!float.TryParse(s, out n))
            return def;
        return n;
    }

    public static double ToDouble(string s, double def = 0)
    {
        double n;
        if (!double.TryParse(s, out n))
            return def;
        return n;
    }

    //不到1小时展示xx分钟前，不到1天展示xx小时前，其余展示xx天前
    public static string MinToStr(int sce)
    {
        long tick = 10000000 * (long)sce;
        System.TimeSpan span = new TimeSpan(tick);
        if (span.Days != 0)
            return string.Format("{0}天前", Mathf.Max(0, span.Days));
        else if (span.Hours != 0)
            return string.Format("{0}小时前", Mathf.Max(0, span.Hours));
        else
            return string.Format("{0}分钟前", Mathf.Max(0, span.Minutes));
    }

    public static string FormatTimeSpan(long seconds)
    {
        string str = "";

        if (seconds >= 86400)
        {
            str += ((int)(seconds / 86400)) + "天";
            seconds = seconds % 86400;

            if (seconds >= 3600)
            {
                str += ((int)(seconds / 3600)) + "时";
                seconds = seconds % 3600;
            }
        }
        else if (seconds >= 3600)
        {
            str += ((int)(seconds / 3600)) + "时";
            seconds = seconds % 3600;

            if (seconds >= 60)
            {
                str += ((int)(seconds / 60)) + "分";
                seconds = seconds % 60;
                if (seconds >= 0)
                {
                    str += seconds + "秒";
                }
            }
        }
        else if (seconds >= 60)
        {
            str += ((int)(seconds / 60)) + "分";
            seconds = seconds % 60;

            if (seconds >= 0)
            {
                str += seconds + "秒";
            }
        }
        else
        {
            str += seconds + "秒";
        }

        return str;
    }
    public static string FormatTimeSpan2(long sec, int maxDay=30, int minMinutes=3)
    {
        string timeText = string.Empty;
        System.TimeSpan ts = new System.TimeSpan((TimeMgr.instance.GetTimestamp() - sec) * System.TimeSpan.TicksPerSecond);
        if (ts.TotalDays > maxDay)
        {
            timeText = "很久以前";
        }
        else if (ts.TotalDays >= 1 && ts.TotalDays <= maxDay)
        {
            timeText = string.Format("{0}天前", (int)ts.TotalDays);
        }
        else if (ts.TotalHours >= 1 && ts.TotalHours < 24)
        {
            timeText = string.Format("{0}小时前", (int)ts.TotalHours);
        }
        else if (ts.TotalMinutes > minMinutes && ts.TotalMinutes < 60)
        {
            timeText = string.Format("{0}分钟前", (int)ts.TotalMinutes);
        }
        else
        {
            timeText = "刚刚";
        }
        return timeText;
    }

    //格式化时间
    public static string FormatDateTime(long timeStamp, string formatType = "yyyy年MM月dd日 HH:mm:ss")
    {
        DateTime dt = TimeMgr.instance.TimestampToClientDateTime(timeStamp);
        return dt.ToString(formatType);
    }
    

    public static string LimitNumLength(long num, int maxLen = 5)
    {
#if ART_DEBUG
        return num.ToString();
#else
        string numStr = num.ToString();
        int strLen = numStr.Length;
        if (strLen <= maxLen)
            return numStr;
        else
        {
            if (ConfigValue.clientLang == "zh-cn")
            {
                if (num >= 10000)
                    return string.Format("{0}万", num / 10000);
                else if (num >= 100000000)
                    return string.Format("{0}亿", num / 100000000);
                else
                    return num.ToString();
            }
            else
            {
                if (num >= 1000)
                    return string.Format("{0}K", num / 1000);
                else if (num >= 1000000)
                    return string.Format("{0}M", num / 1000000);
                else
                    return num.ToString();
            }
        }
#endif
    }
    
    //计算字符串 1个中文=2个字节 1个英文=1个字节
    public static int CountStrLength(string str)
    {
        int len = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] > 127 || str[i] == 94)
                len += 2;
            else
                len++;
        }
        return len;
    }

}
