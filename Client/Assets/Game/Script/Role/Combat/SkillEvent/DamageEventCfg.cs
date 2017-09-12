#region Header
/**
 * 名称：伤害事件
 
 * 日期：2015.12.23
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//击飞
public class DamageEventCfg : SkillEventCfg
{
    public float rate = 1f;//伤害系数
    public string hitFx ="";//打击特效
    public bool hitFxAdjust = true;//打击特效位置调整
    public float shield= 10;//扣的气力值
    public enElement elem = enElement.inherit;
    public bool playElemEventGroup = true;//播放元素事件组，主角攻击的时候有几率触发元素事件组，见role表的元素属性表，如果不勾选，就不触发

    public override enSkillEventType Type { get { return enSkillEventType.damage; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "伤害系数", COL_WIDTH * 3)) onTip(
@"注意:攻.开头的表示为攻击方属性，受.开头的表示为受击方属性

受.免伤率=受.防御/（受.防御+5000+100*受.等级），取值范围0-0.9，大于0.9时，取值0.9
暴击概率=攻.暴击几率-受.抗暴几率，取值范围0-1
暴击伤害倍数=2+攻.暴击伤害	
未暴击.每次攻击伤害=攻.攻击*（1-受.免伤率）*攻.攻击倍数（普攻和技能的伤害系数）
暴击.每次攻击伤害=攻.攻击*（1-受.免伤率）*攻.攻击倍数（普攻和技能的伤害系数）*（2+攻.暴击伤害）
减免伤害=每次攻击伤害-受.减免伤害，取值为>=1，小于1时，强制等于1
最终伤害=减免伤害+攻.最终伤害
属性攻击提高比例=属性攻击提高比例=（攻.元素属性-受.元素属性抗性）/攻.元素属性，取值范围为0-1（攻.元素属性为0时等于0，小于0时强制等于0）
属性攻击伤害=属性攻击伤害=（1+属性攻击提高比例）*最终伤害（计算）,向上取整
            "); return false;
            case 1: if (h(ref r, "打击特效", COL_WIDTH * 3)) onTip("打击特效,碰撞打击处的小火星或者小刀光"); return false;
            case 2: if (h(ref r, "是否调整", COL_WIDTH * 3)) onTip(@"调整并且对手没有被浮空的时候，打击点被击者的碰撞体的边缘，打击点y轴高度为攻击者的胸部。
不调整或者浮空时，打击点为被击者胸部"); return false;
            case 3: if (h(ref r, "气绝值", COL_WIDTH * 3)) onTip(@"扣除作用对象一定气力值，作用对象气力值为0的话将进入气绝状态，扣除的值 =技能气绝值/(1+角色气力系数*(1+Min(10%*气绝次数,50%))) "); return false;
            case 4: if (h(ref r, "元素属性", COL_WIDTH * 3)) onTip(@"元素属性,如果是不修改，那么普通角色的元素属性取决于role表，主角取决于当前武器的元素属性"); return false;
            case 5: if (h(ref r, "元素事件组", COL_WIDTH * 3)) onTip(@"播放元素事件组，主角攻击的时候有几率触发元素事件组，见role表的元素属性表，如果不勾选，就不触发"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 3;
                    rate = EditorGUI.FloatField(r, GUIContent.none, rate);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    
                    int idx = string.IsNullOrEmpty(hitFx)? RoleFxCfg.HitFxNames.Length-1: System.Array.IndexOf(RoleFxCfg.HitFxNames, hitFx);
                    int newIdx = EditorGUI.Popup(r, idx, RoleFxCfg.HitFxNames);
                    if (newIdx != idx && newIdx != -1)
                    {
                        if (newIdx == RoleFxCfg.HitFxNames.Length - 1)
                            hitFx = "";
                        else
                            hitFx = RoleFxCfg.HitFxNames[newIdx];
                    }
                        
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    hitFxAdjust = EditorGUI.Toggle(r, hitFxAdjust);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    shield = EditorGUI.FloatField(r, GUIContent.none, shield);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;

                    elem = (enElement)EditorGUI.Popup(r, (int)elem, ElementCfg.Element_Names);
                    
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 3;
                    playElemEventGroup = EditorGUI.Toggle(r, playElemEventGroup);
                    r.x += r.width;
                }; return false;

                
            default: return true;
        }
    }
#endif

    public override void PreLoad() { 
        if(!string.IsNullOrEmpty(hitFx))
            RoleFxCfg.ProLoad(hitFx);
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        

        //扣气力值=攻击方技能气绝值/(1+气力系数*(1+Min(10%*n,50%)))
        if (shield > 0)
        {
            int unshieldCounter = target.GetFlag("unshieldCounter");
            float counterRate = Mathf.Min(unshieldCounter * ConfigValue.shieldRateAdd, ConfigValue.shieldRateLimit);
            float v = shield / (1 + target.Cfg.shieldRate * (1 + counterRate));
            CombatMgr.instance.AddShield(target, (int)-v);
        }
        

        //计算出碰撞的点，在target上
        Vector3 hitPos = GetHitPos(hitFxAdjust, source, target, eventFrame);

        //判断需不需要生命偷取(一次群伤中只偷取第一个的生命)
        //这里分两种情况(多帧且对象次不为-1算单次群伤那么算总作用次数的第一次，多帧且对象次为-1算多次群伤那么算当前帧触发的第一次)
        bool cutTargetHp=false;
        if (eventFrame.Cfg.frameType == enSkillEventFrameType.once || eventFrame.Cfg.countTargetLimit != -1)
            cutTargetHp = eventFrame.TargetCount==0;
        else
            cutTargetHp = eventFrame.FrameTriggerCount == 0;

        //计算出技能等级系数
        float lvRate = 1;
        Skill parentSkill = eventFrame.EventGroup.ParentSkill;
        if (parentSkill != null)
        {
            SystemSkillCfg skillCfg = parentSkill.SystemSkillCfg;
            if(skillCfg != null)
            {
                lvRate =parentSkill.GetLvValue(skillCfg.damageRate);

            }
        }
        else
        {
        //    Debuger.Log("伤害事件不能计算出技能等级系数:{0},只有技能直接或者间接关联的状态才能找到技能等级系数，状态找不到", eventFrame.EventGroup.Name);
        }
#if PROP_DEBUG
        Debuger.Log("伤害事件的技能伤害系数:{0}",lvRate);
#endif

        //计算元素属性
        int srcId = source.Id;
        int targetId = target.Id;
        enElement elementType;
        string elemEventGroup=null;
        if (elem == enElement.inherit)
        {
            if(source.CombatPart.IsElementOpen)
            {
                ElementCfg elementCfg = ElementCfg.GetRoleElementCfg(source);
                if (elementCfg != null)
                {
                    elementType = elementCfg.ElementType;
                    if (playElemEventGroup && (eventFrame.TargetCount == 0 && eventFrame.FrameTriggerCount == 0))//第一次打到触发
                        elemEventGroup = elementCfg.GetEventGroup();
                }
                else
                    elementType = source.Cfg.element;
            }
            else
                elementType = enElement.none;
            
        }
        else
            elementType = elem;

        //伤害
        bool ret = CombatMgr.instance.Damage(source, target, hitPos, lvRate * rate, hitFx, cutTargetHp, elementType);
        if (source.IsUnAlive(srcId) || target.IsUnAlive(targetId))
            return ret;


        //元素属性附带事件组
        if(!string.IsNullOrEmpty(elemEventGroup))
            CombatMgr.instance.PlayEventGroup(source, elemEventGroup, hitPos,target, eventFrame.Skill);


        return ret;
    }

    
        
}