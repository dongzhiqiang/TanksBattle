using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsBaseConfig
{
    //创建公会消耗的钻石
    public int createCost;
    //公会名字长度
    public int maxName;
    //日志保存上限
    public int logMax;
    //入会申请最大请求上限
    public int maxReq;
    //非会长退出公会冷却时间
    public int quitCorpsCd;
    //会长不上线多少时间可以弹劾
    public int CDROfftime;
    //弹劾者需要的贡献
    public int impContribute;
    //支持弹劾需要的贡献
    public int supportContribute;
    //支持弹劾人数
    public int supportNum;
    //弹劾持续时间，如果会长期间上线，弹劾取消
    public int impTime;
    //公会开启等级
    public int openLevel;
    //公会宣言最大字数限制
    public int declareLimit;
    //公会建设记录最大数
    public int buildLogMax;
}

public class CorpsBaseCfg
{
    public static List<CorpsBaseConfig> m_cfgs = new List<CorpsBaseConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<CorpsBaseConfig>("corps/corpsBase");
    }

    public static CorpsBaseConfig Get()
    {
        return m_cfgs[0];
    }
}
