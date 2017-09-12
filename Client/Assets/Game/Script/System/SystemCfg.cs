#region Header
/**
 * 名称：道具属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class SystemCfg 
{
    public int id;
    /** 名称*/
    public string name;
    /** 是否可见*/
    public string visibility;
    /** 激活条件*/
    public string activeCond;
    /** 开启条件*/
    public string openCond;
    /** 重置时间*/
    public string resetTime;

    public SystemCond visibilityCond;
    public List<SystemCond> activeConds;
    public List<SystemCond> openConds;

    public int visibilityParam;

    public CronTime resetCronTime;

    public static Dictionary<int, SystemCfg> m_cfgs = new Dictionary<int, SystemCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, SystemCfg>("system/system", "id");

        foreach(SystemCfg cfg in m_cfgs.Values)
        {
            cfg.visibilityCond = SystemMgr.instance.CreateVisibleCond(cfg.visibility);

            cfg.activeConds = new List<SystemCond>();
            if (!string.IsNullOrEmpty(cfg.activeCond))
            {
                string[] activeStrs = cfg.activeCond.Split(',');
                foreach (string activeStr in activeStrs)
                {
                    if (!string.IsNullOrEmpty(activeStr))
                    {
                        SystemCond cond = SystemMgr.instance.CreateActiveCond(activeStr);
                        if (cond != null)
                        {
                            cfg.activeConds.Add(cond);
                        }
                    }
                }
            }

            cfg.openConds = new List<SystemCond>();
            if (!string.IsNullOrEmpty(cfg.openCond))
            {
                string[] openStrs = cfg.openCond.Split(',');
                foreach (string openStr in openStrs)
                {
                    if (!string.IsNullOrEmpty(openStr))
                    {
                        SystemCond cond = SystemMgr.instance.CreateOpenCond(openStr);
                        if (cond != null)
                        {
                            cfg.openConds.Add(cond);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(cfg.resetTime))
                cfg.resetCronTime = new CronTime(cfg.resetTime);
        }
    }
}
