using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EliteLevelCfg
{
    public int id;
    public string name;
    public string subname;
    public string roomId;
    public int openLevel;
    public string openPassLvl;
    public int openPassEltLvl;
    public string messageNotOpen;
    public int firstReward;
    public string titleImage;
    public string contentImage;


    public static Dictionary<int, EliteLevelCfg> m_cfgs = new Dictionary<int, EliteLevelCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, EliteLevelCfg>("activity/eliteLevel", "id");
    }
}