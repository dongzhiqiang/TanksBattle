using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ArenaBasicCfg
{
    public int openLevel;
    public int limitTime;
    public int freeChance;
    public int coolDown;
    public int buyChance;
    public int buyChancePrice;
    public string roomId;
    public string movieName; 
    public float[] itsPos1;
    public float[] itsPos2;
    public float[] itsPos3;
    public float[] myPos1;
    public float[] myPos2;
    public float[] myPos3;
    public int addHateValue;
    public string itsHeroBornType;
    public string itsHeroDeadType;
    public string itsPetDeadType;
    public string myHeroDeadType;
    public string myPetDeadType;
    public string spartaPropRateId;
    public int heroShieldBuff;
    public int maxRankNum;
    public int showRankNum;
    public int chooseUpwards;
    public int chooseDownwards;
    public int chooseGapMinFactor;
    public int chooseGapMaxFactor;
    public int logNum;
    public int dayRewardTime;
    public int weekRewardDay;
    public int weekRewardTime;
    public int winRewardId;
    public int loseRewardId;
    public int heroBuffId;
    public int enemyBuffId;
    public string ruleIntro;


    public static List<ArenaBasicCfg> m_cfgs = new List<ArenaBasicCfg>();

    public static ArenaBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<ArenaBasicCfg>("activity/arenaBasicCfg");
    }

    public static List<int> GetArenaPos(string arenaPosStr)
    {
        List<int> arenaPos = new List<int>();

        if (!string.IsNullOrEmpty(arenaPosStr))
        {
            string[] arenaPosStrs = arenaPosStr.Split(',');
            for (int i = 0; i < arenaPosStrs.Length; i++)
            {
                arenaPos.Add(int.Parse(arenaPosStrs[i]));
            }
        }
        return arenaPos;
    }
}
