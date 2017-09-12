using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiTestCfg
{
    #region Fields
    public int id;
    public string roleId;
    public string aiType;
    public string groupId;
    public string partId;
    #endregion

    public static List<AiTestCfg> m_cfgs = new List<AiTestCfg>();

    public static Dictionary<string, Dictionary<string, List<AiTestCfg>>> m_dictCfg = new Dictionary<string, Dictionary<string, List<AiTestCfg>>>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<AiTestCfg>("room/aiTest");
        
        foreach(AiTestCfg cfg in m_cfgs)
        {
            Dictionary<string, List<AiTestCfg>> partDict;
            if (!m_dictCfg.TryGetValue(cfg.partId, out partDict))
            {
                partDict = new Dictionary<string, List<AiTestCfg>>();
                m_dictCfg.Add(cfg.partId, partDict);
            }

            List<AiTestCfg> groupList;
            if (!partDict.TryGetValue(cfg.groupId, out groupList))
            {
                groupList = new List<AiTestCfg>();
                partDict.Add(cfg.groupId, groupList);
            }

            groupList.Add(cfg);
        }
    }
}
