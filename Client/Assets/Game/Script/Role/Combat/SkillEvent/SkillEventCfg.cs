#region Header
/**
 * 名称：帧事件
 
 * 日期：2015.9.28
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
public abstract class SkillEventCfg
{
    public const int COL_WIDTH = 20;
    public delegate bool HeaderButton(ref Rect r, string content, int width);

    public bool ingore = false;
    public int eventCountLimit =-1;//一个技能中这个事件执行的次数限制
    public int eventCountFrameLimit = -1;//帧事件次数，一帧能同时作用的对象的次数
    public int id;
    public int priority = 0;//优先级，如果这个优先级大于目标所拥有的免疫事件优先级，那么免疫无效。(比如霸体 = 免疫被击+免疫击飞+免疫浮空，目标身上有优先级为0的霸体、但是这里的优先级为1，那么霸体无效)
    public bool ingoreIfNotHero = false;


    public abstract enSkillEventType Type { get; }

    public bool Handle(Role source, Role target, SkillEventFrame eventFrame)
    {
        //一些通用的判断，放这里
        //状态的一些免疫
        RoleStateMachine rsm = target.RSM;
        bool isGetUp = false;
        bool isGround = false;
        bool isGrab = false;
        if (rsm.CurStateType == enRoleState.beHit)
        {
            var behitType = rsm.StateBehit.CurStateType;
            if (behitType == enBehit.getUp)
            {
                //小怪起身未完成就要取消保护，不然一半小怪的倒地有0.2s无敌+起身1s无敌，玩家的攻击间隔太久了，不爽
                RoleSubGetUp subGetUp = rsm.StateBehit.CurState as RoleSubGetUp;
                if (!subGetUp.CanMove())
                    isGetUp = true;
            }
                
            if (behitType == enBehit.ground)
            {
                //倒地状态的后半段时间才免疫
                RoleSubGround subGround = rsm.StateBehit.CurState as RoleSubGround;
                if (!subGround.CanMove())
                    isGround = true;
            }
            if (behitType == enBehit.beGrab)
                isGrab = true;
        }
        //起身免疫
        if (isGetUp && (Type == enSkillEventType.damage || Type == enSkillEventType.move || Type == enSkillEventType.fx || Type == enSkillEventType.pause || Type == enSkillEventType.sound || Type == enSkillEventType.buff))
            return false;
        //倒地免疫
        if (isGround && (this.Type == enSkillEventType.move))
            return false;
        //被抓取免疫
        if (isGrab && (this.Type == enSkillEventType.move))
            return false;

        return OnHandle(source, target, eventFrame);
    }

    protected Vector3 GetHitPos(bool hitAdjust, Role source, Role target, SkillEventFrame eventFrame)
    {
        if (hitAdjust && target.TranPart.IsGrounded)
        {
            //自己算下，将来也可以考虑用Collider.ClosestPointOnBounds();
            Vector3 srcPos = eventFrame.EventGroup.Root.position;
            Vector3 targetPos = target.TranPart.Pos;
            if (source.RoleModel.Model == eventFrame.EventGroup.Root)
                srcPos.y = targetPos.y = srcPos.y + source.RoleModel.Height * 0.5f + eventFrame.CurRangeTarget.range.begingOffsetPos.y;
            else //飞出物的话不用做CC偏移
                srcPos.y = targetPos.y = srcPos.y + eventFrame.CurRangeTarget.range.begingOffsetPos.y;
            Vector3 link = targetPos - srcPos;
            return targetPos - link.normalized * target.RoleModel.Radius;
        }
        else
            return target.RoleModel.Center.position;
    }
    
    public virtual void CopyFrom(SkillEventCfg cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance, "file");
    }

    public abstract bool OnHandle(Role source, Role target, SkillEventFrame eventFrame);
    public virtual void PreLoad(){}

#if UNITY_EDITOR
    public abstract bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h);
    public abstract bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran);
#endif

}


public class EmptySkillEventCfg : SkillEventCfg
{
    public override enSkillEventType Type { get{return enSkillEventType.empty;} }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        return true;
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        return true;
    }
#endif

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        Debuger.LogError("碰到了:{0}", target.Cfg.name);
        return true;
    }
}