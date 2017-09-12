#region Header
/**
 * 名称：触发技能状态
 
 * 日期：2016.3.17
 * 描述：
技能id,技能作用对象类型,强制
技能作用对象类型，1释放者，2默认自动朝向,3最近的敌人，如果匹配不到则自动朝向
强制，1默认不强制
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffPlaySkillCfg : BuffExCfg
{
    public string skillId;
    public enBuffTargetType targetType;
    public bool force;


    public override bool Init(string[] pp)
    {
        if (pp.Length <= 0)
            skillId = string.Empty;
        else
            skillId = pp[0];

        int tem;
        if (pp.Length <= 1 || !int.TryParse(pp[1], out tem))
            tem =2;
        targetType = (enBuffTargetType)tem;

        if (pp.Length <= 2 || !int.TryParse(pp[2], out tem))
            tem = 1;
        force = tem == 1;
        return true;
    }
}
public class BuffPlaySkill: Buff
{
    public BuffPlaySkillCfg ExCfg { get { return (BuffPlaySkillCfg)m_cfg.exCfg; } }
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (string.IsNullOrEmpty(ExCfg.skillId))
            return;

        Role target=this.GetRole(ExCfg.targetType, null);
        
        m_parent.CombatPart.Play(ExCfg.skillId, target,false, ExCfg.force);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
       
    } 
}

