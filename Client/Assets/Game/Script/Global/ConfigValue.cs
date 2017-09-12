using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfigValueCfg
{
    public string name;
    public string desc;
    public string value;
    public Property.enType type;
}

public class ConfigValue
{
    public static Dictionary<string, Property> mData = new Dictionary<string, Property>();

    public static float unshieldDuation;
    public static float shieldRateAdd;
    public static float shieldRateLimit;
    public static string defRoleHead;
    public static string clientLang;

    public static void Init()
    {
        mData.Clear();
        List<ConfigValueCfg> cfgTable = Csv.CsvUtil.Load<ConfigValueCfg>("other/configValue");

        foreach(ConfigValueCfg cfg in cfgTable)
        {
            mData[cfg.name] = new Property(cfg.value, cfg.type);
        }

        //一些频繁取的值，这里先取下来，提升效率
        unshieldDuation = GetFloat("unshieldDuation");
        shieldRateAdd = GetFloat("shieldRateAdd");
        shieldRateLimit = GetFloat("shieldRateLimit");
        defRoleHead = GetString("defRoleHead");
        clientLang = GetString("clientLang");

        //时间参数初始化一下
        var sundayFirst = GetInt("sundayFirst");
        var dayBreakPoint = GetInt("dayBreakPoint");
        TimeMgr.instance.SetParameter(sundayFirst, dayBreakPoint);
    }

    public static string GetString(string name)
    {
        Property p;
        mData.TryGetValue(name, out p);
        if (p == null)
        {
            Debuger.LogError("表里没有配置的值{0}", name);
            return "";
        }

        return p.String;
    }


    public static float GetFloat(string name)
    {
        Property p;
        mData.TryGetValue(name, out p);
        if (p == null)
        {
            Debuger.LogError("表里没有配置的值{0}", name);
            return 0;
        }

        return p.Float;
    }

    public static long GetLong(string name)
    {
        Property p;
        mData.TryGetValue(name, out p);
        if (p == null)
        {
            Debuger.LogError("表里没有配置的值{0}", name);
            return 0;
        }

        return p.Long;
    }

    public static int GetInt(string name)
    {
        Property p;
        mData.TryGetValue(name, out p);
        if (p == null)
        {
            Debuger.LogError("表里没有配置的值{0}", name);
            return 0;
        }

        return p.Int;
    }
}
