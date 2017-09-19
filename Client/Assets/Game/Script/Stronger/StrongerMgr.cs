using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class StrongerMgr : Singleton<StrongerMgr>
{
    public bool IsStrongerOpen(string strongerType)
    {
        enStrongerType type = (enStrongerType)Enum.Parse(typeof(enStrongerType), strongerType);
        string errMsg;
        switch (type)
        {
            case enStrongerType.equipAdvLv:
            case enStrongerType.equipStar:
                return SystemMgr.instance.IsEnabled(enSystem.hero, out errMsg);                
            case enStrongerType.equipSkillLv:
            case enStrongerType.weaponTtalentLv:
                return SystemMgr.instance.IsEnabled(enSystem.weapon, out errMsg);     
            case enStrongerType.treasure:
                return SystemMgr.instance.IsEnabled(enSystem.treasure, out errMsg);
            case enStrongerType.flame:
                return SystemMgr.instance.IsEnabled(enSystem.flame, out errMsg);
            case enStrongerType.dailyTask:
            case enStrongerType.growthTask:
                return SystemMgr.instance.IsEnabled(enSystem.dailyTask, out errMsg);
            case enStrongerType.warriorTried:
                return SystemMgr.instance.IsEnabled(enSystem.warriorTried, out errMsg);
            case enStrongerType.normalLevel:
                return SystemMgr.instance.IsEnabled(enSystem.scene, out errMsg);            
            case enStrongerType.arena:
                return SystemMgr.instance.IsEnabled(enSystem.arena, out errMsg);           
            case enStrongerType.levelReward:
                return SystemMgr.instance.IsEnabled(enSystem.opActivity, out errMsg);
            case enStrongerType.goldLevel:
                return SystemMgr.instance.IsEnabled(enSystem.goldLevel, out errMsg);
            case enStrongerType.recharge:
                return true;
        }
        return true;
    }

    public int GetStrongerProgress(string strongerType)
    {
        enStrongerType type = (enStrongerType)Enum.Parse(typeof(enStrongerType), strongerType);
        int progress=0;
        float target = 0;
        float current = 0;
        float baseNum = 0;
        Role hero = RoleMgr.instance.Hero;
        int currentLv = hero.GetInt(enProp.level);
        StrongerHeroCfg heroCfg = StrongerHeroCfg.m_cfgs[currentLv];
        StrongerHeroCfg heroCfgBase = StrongerHeroCfg.m_cfgs[1];
        switch (type)
        {
            case enStrongerType.equipAdvLv:
                {
                    EquipsPart equipsPart = hero.EquipsPart;                    
                    for (int i = 0; i < (int)enEquipPos.maxNormal - (int)enEquipPos.minNormal + 1; ++i)
                    {
                        Equip equip = equipsPart.GetEquip((enEquipPos)i);
                        current += equip.AdvLv;
                        target += heroCfg.equipAdvLv;
                        baseNum += heroCfgBase.equipAdvLv;
                    }
                    Weapon curWeapon = hero.WeaponPart.CurWeapon;
                    current += curWeapon.Equip.AdvLv;
                    target += heroCfg.equipAdvLv;
                    baseNum += heroCfgBase.equipAdvLv;
                    break;
                }
            case enStrongerType.equipStar:
                {
                    EquipsPart equipsPart = hero.EquipsPart;                   
                    for (int i = 0; i < (int)enEquipPos.maxNormal - (int)enEquipPos.minNormal + 1; ++i)
                    {
                        Equip equip = equipsPart.GetEquip((enEquipPos)i);
                        int star = EquipCfg.m_cfgs[equip.EquipId].star;
                        current += star;
                        target += heroCfg.equipStar;
                        baseNum += heroCfgBase.equipStar;
                    }
                    Weapon curWeapon = hero.WeaponPart.CurWeapon;
                    int weaponStar = EquipCfg.m_cfgs[curWeapon.Equip.EquipId].star;
                    current += weaponStar;
                    target += heroCfg.equipStar;
                    baseNum += heroCfgBase.equipStar;
                    break;
                }
            case enStrongerType.equipSkillLv:
                {
                    WeaponPart weaponPart = hero.WeaponPart;
                    /*for (int i = 0; i < (int)enEquipPos.maxWeapon - (int)enEquipPos.minWeapon + 1; ++i)
                    {
                        Weapon weapon = weaponPart.GetWeapon(i);
                        for (int j = 0; j < (int)enSkillPos.max; ++j)
                        {
                            WeaponSkill skill = weapon.GetSkill(j);
                            current += skill.lv;
                            target += heroCfg.equipSkillLv;
                        }
                    }*/
                    Weapon weapon = weaponPart.CurWeapon;
                    for (int j = 0; j < (int)enSkillPos.max; ++j)
                    {
                        WeaponSkill skill = weapon.GetSkill(j);
                        current += skill.lv;
                        target += heroCfg.equipSkillLv;
                        baseNum +=heroCfgBase.equipSkillLv;
                    }
                    break;
                }
            case enStrongerType.weaponTtalentLv:
                {
                    WeaponPart weaponPart = hero.WeaponPart;
                    /*for (int i = 0; i < (int)enEquipPos.maxWeapon - (int)enEquipPos.minWeapon + 1; ++i)
                    {
                        Weapon weapon = weaponPart.GetWeapon(i);
                        for (int j = 0; j < (int)enSkillPos.max; ++j)
                        {                        
                            WeaponSkill skill = weapon.GetSkill(j);
                            for (int k = 0; k < skill.TalentCount; ++k)
                            {
                                WeaponSkillTalent talent = skill.GetTalent(k);
                                current += talent.lv;
                                target += heroCfg.weaponTalentLv;
                            }
                        }
                    }*/
                    Weapon weapon = weaponPart.CurWeapon;
                    for (int j = 0; j < (int)enSkillPos.max; ++j)
                    { 
                        WeaponSkill skill = weapon.GetSkill(j);                 
                        for (int k = 0; k < skill.TalentCount; ++k)
                        {                            
                            WeaponSkillTalent talent = skill.GetTalent(k);
                            current += talent.lv;
                            target += heroCfg.weaponTalentLv;
                            baseNum += heroCfgBase.weaponTalentLv;
                        }
                    }
                    break;
                }
            case enStrongerType.treasure:
                TreasurePart treasurePart = hero.TreasurePart;
                List<int> treasures = heroCfg.GetTreasure();
                List<int> baseTreasures = heroCfgBase.GetTreasure();
                foreach(Treasure treasure in treasurePart.Treasures.Values)
                {
                    current += treasure.level;
                }
                target = treasures[0] * treasures[1];
                baseNum = baseTreasures[0] * baseTreasures[1];
                break;
            case enStrongerType.flame:
                {
                    FlamesPart flamesPart = hero.FlamesPart;
                    List<Flame> flames = heroCfg.GetFlames();
                    List<Flame> baseFlames = heroCfgBase.GetFlames();
                    for(int i=0;i<flames.Count;++i)
                    {
                        Flame flame = flamesPart.GetFlame(flames[i].FlameId);
                        if(flame!=null)
                        {
                            current += flame.Level;                            
                        }
                        target += flames[i].Level;
                        baseNum += baseFlames[i].Level;
                    }
                    break;
                }
        }
        progress = target - baseNum == 0 ? 100 : Mathf.RoundToInt((current - baseNum) / (target - baseNum) * 100);
        if (progress > 100)
            progress = 100;
        return progress;
    }   

    public void GoStronger(string strongerType)
    {
        enStrongerType type = (enStrongerType)Enum.Parse(typeof(enStrongerType), strongerType);
        
        switch(type)
        {
            case enStrongerType.equipAdvLv:
            case enStrongerType.equipStar:
                UIMgr.instance.Open<UIEquip>().SelectEquipUpGrade();
                break;
            case enStrongerType.equipSkillLv:
                UIMgr.instance.Open<UIWeapon>();
                break;
            case enStrongerType.weaponTtalentLv:
                UIMgr.instance.Open<UIWeapon>().m_tab.SetSel(1);
                break;
            case enStrongerType.treasure:
                UIMgr.instance.Open<UITreasure>();
                break;
            case enStrongerType.flame:
                UIMgr.instance.Open<UIFlame>();
                break;
            case enStrongerType.dailyTask:
                UIMgr.instance.Open<UITask>();
                break;
            case enStrongerType.warriorTried:
                UIMgr.instance.Open<UIWarriorsTried>();
                break;
            case enStrongerType.normalLevel:
                UIMgr.instance.Open<UILevelSelect>();
                break;
            case enStrongerType.growthTask:
                UIMgr.instance.Open<UITask>().btnsGroup.SetSel(1);
                break;
            case enStrongerType.arena:
                UIMgr.instance.Open<UIArena>();
                break;
            case enStrongerType.recharge:
                break;
            case enStrongerType.levelReward:
                UIMgr.instance.Open<UIOpActivity>().SelectOpActivity(1);
                break;
            case enStrongerType.goldLevel:
                UIMgr.instance.Open<UIGoldLevel>();
                break;
        }
    }
    
}

public enum enStrongerType
{
    equipAdvLv,//英雄身上装备等阶
    equipStar,//英雄身上装备觉醒星级
    equipSkillLv,//英雄技能等级
    weaponTtalentLv,//铭文等级
    treasure,//神器个数、等级
    flame,//圣火相关
    dailyTask,//每日任务
    warriorTried,//勇士试炼
    normalLevel,//主线副本
    growthTask,//成长任务
    arena,//竞技场
    recharge,//充值
    levelReward,//冲级豪礼
    goldLevel,//金币副本
    eliteLv, //众神传
    prophetTower,//预言者之塔
    treasureRob,//神奇争夺

}

