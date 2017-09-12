#region Header
/**
 * 名称：角色配置
 
 * 日期：2015.9.21
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum enSkillType
{   
    none,//空
    atkUp,//普通攻击
    skill1,//技能1
    skill2,//技能2
    skill3,//技能
    block,//格挡
    commonMax,//之前的都是通用技能，之后的都是主角才有的

    atkPress,//普通攻击(按紧)
    slider,//摇杆划动技能
    unique,//怒气技能
    treasure,//宝物技能(已经弃用)
    qte,//小qte
    qtePress,//qte按紧
    
}

public enum enSkillPos  {
    atk     = 0,    //普通攻击
    skill1  = 1,    //技能1
    skill2  = 2,    //技能2
    skill3  = 3,    //技能3
    max     = 4,
};

public class WeaponCfg 
{
    
    public int id;
    public string name="";//名字
    public string desc = "";//描述
    public string icon = null;//图标
    public string modRight = string.Empty;//右手武器模型(左右手都没有的话就是模型上的weapon_mesh)
    public string modLeft = string.Empty;//左手武器模型(左右手都没有的话就是模型上的weapon_mesh)
    public string uiMod = string.Empty;//ui上显示的武器模型
    public string postfix = "";//后缀
    public float behitRate = 1;//武器僵直系数(越大被击的敌人的僵直时间越长)
    public float pauseFrameRate = 1;//武器卡帧系数(越大则自己的卡帧事件的时间越长)
    public string hitProp;//打击属性
    public string sliderSkill = string.Empty;//摇杆划动技能
    public string blockSkill = string.Empty;//格挡按钮
    public string atkUpSkill = string.Empty;//普通攻击
    public string atkPressSkill = string.Empty;//普通攻击键(按紧)
    public string airAtk = string.Empty;//普通攻击(空中)
    public string qteSkill = string.Empty;//qte键
    public string qtePressSkill = string.Empty;//qte按紧
    public List<string> skills = new List<string>();//技能
    public List<string> airSkills = new List<string>();//空中技能

    HitPropCfg _hitPropCfg;

    public HitPropCfg HitPropCfg { get { return _hitPropCfg; } }



    static Dictionary<int, WeaponCfg> s_cfgs;
    
    public static void Init()
    {
        s_cfgs = Csv.CsvUtil.Load<int, WeaponCfg>("role/weapon", "id");
        foreach(var cfg in s_cfgs.Values)
        {
            cfg._hitPropCfg = HitPropCfg.Get(cfg.hitProp);
        }
    }

    public static WeaponCfg Get(int id)
    {
        if(id == 0 || id == -1)
            return null;
        WeaponCfg cfg = s_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("武器不存在，请检查武器表:{0}", id);
        return cfg;
    }

    
    //预加载
    public static void PreLoad()
    {
        foreach(WeaponCfg cfg in s_cfgs.Values){
            if(!string.IsNullOrEmpty(cfg.modRight))
                GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(cfg.modRight);
            if (!string.IsNullOrEmpty(cfg.modLeft))
                GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(cfg.modLeft);
            if (!string.IsNullOrEmpty(cfg.uiMod))
                GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(cfg.uiMod);

            
        }
    }

   
    public string GetSkillId(enSkillType skillType,bool air)
    {
        switch (skillType)
        {
            case enSkillType.slider:return sliderSkill;
            case enSkillType.block: return blockSkill;
            case enSkillType.atkUp: return air ? airAtk : atkUpSkill;
            case enSkillType.atkPress: return atkPressSkill;
            case enSkillType.skill1: return !air ? (skills.Count <= 0 ? null : skills[0]) : (airSkills.Count <= 0 ? null : airSkills[0]);
            case enSkillType.skill2: return !air ? (skills.Count <= 1 ? null : skills[1]) : (airSkills.Count <= 1 ? null : airSkills[1]);
            case enSkillType.skill3: return !air ? (skills.Count <= 2 ? null : skills[2]) : (airSkills.Count <= 2 ? null : airSkills[2]);
            case enSkillType.qte: return qteSkill;
            case enSkillType.qtePress: return qtePressSkill;
            default: Debuger.LogError("未知的类型{0}", skillType); return null;
        }
    }

    public string GetSkillId(enSkillPos pos)
    {
        if (pos == enSkillPos.atk)
            return this.atkUpSkill;
        else
            return this.skills.Get((int)pos - 1);

    }
}
