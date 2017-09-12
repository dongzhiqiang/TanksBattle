using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoadingTipsCfg
{
    #region Fields
    public int id;
    public string desc;
    public string image;
    #endregion

    public static Dictionary<int, LoadingTipsCfg> m_cfgs = new Dictionary<int, LoadingTipsCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LoadingTipsCfg>("room/loadingTips", "id");
    }

    public static LoadingTipsCfg GetCfg(int id)
    {
        LoadingTipsCfg cfg;
        m_cfgs.TryGetValue(id, out cfg);
        return cfg;
    }

}
