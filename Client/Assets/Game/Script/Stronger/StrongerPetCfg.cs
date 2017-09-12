using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StrongerPetCfg
{
    public int level;
    public int num;
    public int equipAdvLv;
    public int equipStar;
    public int petAdvLv;
    public int petStar;
    public string petSkill;
    public string petTalent;

    public static Dictionary<int, StrongerPetCfg> m_cfgs = new Dictionary<int, StrongerPetCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, StrongerPetCfg>("stronger/strongerPet", "level");
    }

    public List<int> GetPetSkill()
    {
        List<int> petSkills = new List<int>();

        if (!string.IsNullOrEmpty(petSkill))
        {
            string[] petSkillStr = petSkill.Split('|');
            for (int i = 0; i < petSkillStr.Length; i++)
            {
                petSkills.Add(int.Parse(petSkillStr[i]));
            }
        }
        return petSkills;
    }
    public List<int> GetTalent()
    {
        List<int> petTalents = new List<int>();

        if (!string.IsNullOrEmpty(petTalent))
        {
            string[] petTalentStr = petTalent.Split('|');
            for (int i = 0; i < petTalentStr.Length; i++)
            {
                petTalents.Add(int.Parse(petTalentStr[i]));
            }
        }
        return petTalents;
    }



}
