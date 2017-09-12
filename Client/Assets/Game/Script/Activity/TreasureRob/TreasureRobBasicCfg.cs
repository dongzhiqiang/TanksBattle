using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TreasureRobBasicCfg
{
    public int dayMaxCnt;
    public int dayMaxRob;
    public int fleshGold;
    public float minPowerRate;
    public float maxPowerRate;
    public string roomId;
    public float[] itsPos1;
    public float[] itsPos2;
    public float[] itsPos3;
    public float[] myPos1;
    public float[] myPos2;
    public float[] myPos3;
    public string itsHeroBornType;
    public string itsHeroDeadType;
    public string itsPetDeadType;
    public string myHeroDeadType;
    public string myPetDeadType;
    public int addHateValue;
    public int heroShieldBuff;
    public int heroBuffId;
    public int enemyBuffId;

    public static List<TreasureRobBasicCfg> m_cfgs = new List<TreasureRobBasicCfg>();

    public static TreasureRobBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<TreasureRobBasicCfg>("activity/treasureRobBasic");
    }
}