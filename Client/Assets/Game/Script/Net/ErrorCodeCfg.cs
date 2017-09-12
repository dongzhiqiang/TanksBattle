using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ErrorCodeCfg
{
    public int module;
    public int errCode;
    public string moduleName;
    public string errCodeName;
    public string desc;


    public static Dictionary<string, string> m_cfgs = new Dictionary<string, string>();
    public static void Init()
    {
        List<ErrorCodeCfg> cfgs = Csv.CsvUtil.Load<ErrorCodeCfg>("other/errorCode");
        foreach (var item in cfgs)
        {
            m_cfgs[item.module + "_" + item.errCode] = item.desc;
        }
    }

    public static string GetErrorDesc(int module, int errCode, string errMsg = null)
    {
        if (!string.IsNullOrEmpty(errMsg))
            return errMsg;
        //如果错误码是<=0，则是通用错误码，模块号是0
        module = errCode > 0 ? module : 0;
        string desc;
        if (m_cfgs.TryGetValue(module + "_" + errCode, out desc))
            return desc;
        else
            return "发生未知错误，错误码：" + errCode;
    }
}
