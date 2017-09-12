#region Header
/**
 * 名称：SkillEventFrameCondition
 
 * 日期：2015.12.9
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif


[JsonCanInherit]
public abstract class SkillEventConditionCfg
{
    public abstract enSkillEventConditionType Type { get; }
   

#if UNITY_EDITOR
    public abstract void OnDraw(Rect r);
#endif
    public virtual void CopyFrom(SkillEventConditionCfg cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance, "file");
    }

    public abstract bool IsMatch(SkillEventFrame s);
    public abstract bool IsMatch(SkillEventFrame s, Role target);
    

}

//被击了
public class SECBehit : SkillEventConditionCfg
{
    public float time=0.3f;//多少时间内被击算做有效
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.behit; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        time = EditorGUI.FloatField(r, time);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        HatePart hatePart =s.Parent.HatePart;
        return hatePart.LastBeHitTime!=-1&& Time.time-hatePart.LastBeHitTime<=time;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        HatePart hatePart = s.Parent.HatePart;
        return hatePart.LastBeHitTime != -1 && Time.time - hatePart.LastBeHitTime <= time && hatePart.GetLastBehit() == target;
    }
}

//标记
public class SECFlag : SkillEventConditionCfg
{
    public string flag= "";//多少时间内被击算做有效
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.flag; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        flag = EditorGUI.TextField(r, flag);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        Debuger.LogError("{0} {1}。标记只能作为前置对象条件，不能作为前置条件",s.Parent.Cfg.id,s.EventGroup.Name);
        return false;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        if(string.IsNullOrEmpty(flag))
            return true;

        return target.GetFlag(flag) > 0;
    }
}

//无标记
public class SECUnflag : SkillEventConditionCfg
{
    public string flag = "";//多少时间内被击算做有效
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.unflag; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        flag = EditorGUI.TextField(r, flag);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        Debuger.LogError("{0} {1}。无标记只能作为前置对象条件，不能作为前置条件", s.Parent.Cfg.id, s.EventGroup.Name);
        return false;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        if (string.IsNullOrEmpty(flag))
            return true;

        return target.GetFlag(flag) ==0;
    }
}

//状态
public class SECBuff: SkillEventConditionCfg
{
    public int buffId= 1;
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.buff; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        buffId = EditorGUI.IntField(r, buffId);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        return s.Parent.BuffPart.GetBuffByBuffId(buffId)!=null;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return target.BuffPart.GetBuffByBuffId(buffId) != null;
    }
}

//无状态
public class SECUnBuff : SkillEventConditionCfg
{
    public int buffId = 1;
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.unBuff; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        buffId = EditorGUI.IntField(r, buffId);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        return s.Parent.BuffPart.GetBuffByBuffId(buffId) == null;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return target.BuffPart.GetBuffByBuffId(buffId) == null;
    }
}

//概率
public class SECPercent : SkillEventConditionCfg
{
    public float percent=1;
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.percent; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        percent = EditorGUI.FloatField(r, percent);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        return percent <=Random.value;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return IsMatch(s);
    }
}

//技能目标存在
public class SECSkillTarget : SkillEventConditionCfg
{
    
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.skillTarget; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        return s.Skill!=null &&s.Skill.Target !=null;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return s.Skill != null && s.Skill.Target == target;
    }
}

//技能目标不存在
public class SECUnSkillTarget : SkillEventConditionCfg
{

    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.unSkillTarget; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {

    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        return s.Skill == null || s.Skill.Target == null;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return IsMatch(s);
    }
}

//帧触发
public class SECTriggerFrame : SkillEventConditionCfg
{
    public int frameId = -1;
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.triggerFrame; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        frameId = EditorGUI.IntField(r, frameId);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        var eventGroup = s.EventGroup;
        var frame = eventGroup.GetFrameById(frameId);
        if(frame == null)
        {
            Debuger.LogError("{0} {1}。帧{2}的帧触发条件找不到帧{3}，请确认是有效的帧id", eventGroup.Parent.Cfg.id, eventGroup.Name,s.Cfg.id,frameId);
            return false;
        }
        
        return frame.FrameCount>0;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        var eventGroup = s.EventGroup;
        var frame = eventGroup.GetFrameById(frameId);
        if (frame == null)
        {
            Debuger.LogError("{0} {1}。帧{2}的帧触发条件找不到帧{3}，请确认是有效的帧id", eventGroup.Parent.Cfg.id, eventGroup.Name, s.Cfg.id, frameId);
            return false;
        }

        return frame.GetTargetCount(target.Id)>0;
    }
}

//帧触发
public class SECUnTriggerFrame : SkillEventConditionCfg
{
    public int frameId = -1;
    public override enSkillEventConditionType Type { get { return enSkillEventConditionType.unTriggerFrame; } }
#if UNITY_EDITOR
    public override void OnDraw(Rect r)
    {
        frameId = EditorGUI.IntField(r, frameId);
    }
#endif
    public override bool IsMatch(SkillEventFrame s)
    {
        var eventGroup = s.EventGroup;
        var frame = eventGroup.GetFrameById(frameId);
        if (frame == null)
        {
            Debuger.LogError("{0} {1}。帧{2}的帧触发条件找不到帧{3}，请确认是有效的帧id", eventGroup.Parent.Cfg.id, eventGroup.Name, s.Cfg.id, frameId);
            return false;
        }

        return frame.FrameCount == 0;
    }
    public override bool IsMatch(SkillEventFrame s, Role target)
    {
        return IsMatch(s);
    }
}