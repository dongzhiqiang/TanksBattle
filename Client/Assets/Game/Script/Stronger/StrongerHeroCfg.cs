using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StrongerHeroCfg
{
    public int level;
    public int equipAdvLv;
    public int equipStar;
    public int equipSkillLv;
    public int weaponTalentLv;
    public string treasure;
    public string flame;

    public static Dictionary<int, StrongerHeroCfg> m_cfgs = new Dictionary<int, StrongerHeroCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, StrongerHeroCfg>("stronger/strongerHero", "level");
    }


    public  List<Flame> GetFlames()
    {       
        List<Flame> result = new List<Flame>();
        if (flame == null)
        {
            return result;
        }
        string[] flames = flame.Split(',');
        foreach (string flameStr in flames)
        {
            if (flameStr != "")
            {
                string[] flamePair = flameStr.Split('|');
                Flame newflame = Flame.Create(new FlameVo(int.Parse(flamePair[0]), int.Parse(flamePair[1]),0));          
                result.Add(newflame);
            }
        }
        return result;
    }

    public List<int> GetTreasure()
    {
        List<int> treasures = new List<int>();

        if (!string.IsNullOrEmpty(treasure))
        {
            string[] treasureStr = treasure.Split('|');
            for (int i = 0; i < treasureStr.Length; i++)
            {
                treasures.Add(int.Parse(treasureStr[i]));
            }
        }
        return treasures;
    }


}
