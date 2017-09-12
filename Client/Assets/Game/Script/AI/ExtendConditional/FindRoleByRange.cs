#region Header
/**
 * 名称：FindSkill
 
 * 日期：2016.6.1
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{

    public class FindRoleByRangeCfg : ConditionalCfg
    {
        public TargetRangeCfg range = new TargetRangeCfg();
        public ValueRole ret = new ValueRole(enValueRegion.tree);
        public Value<bool> notRefindIfExist = new Value<bool>(false);

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            using (new AutoEditorTipButton(@"作用对象类型:
    释放者，总是执行到，除非角色不可战斗了
    释放者，受碰撞检测
    友方，同一阵营但是不是自己
    敌人阵营
    中立阵营
    友方和自己
    当前技能的目标，使用技能的时候可能会指定目标，否则就会自动选择目标
"))
                range.targetType = (enSkillEventTargetType)EditorGUILayout.Popup("对象", (int)range.targetType, SkillEventFrameCfg.TargetTypeName);


            
            if (range.targetType != enSkillEventTargetType.selfAlway)
            {
                if (!range.range.showRange)
                    range.range.showRange = true;

                using (new AutoEditorTipButton(@"作用范围类型:
    圆。参数(半径、垂直距离)
    扇形。参数(半径、夹角限制、垂直距离)
    矩形。参数(长、宽、垂直距离)
    碰撞。参数(无),碰撞大小在释放者的根节点的Collider组件调整。注意要加RigidBody和Collider，RigidBody的IK勾上，Collider的Trigger勾上
垂直距离，-1表示不用判断了
"))
                using (new AutoBeginHorizontal())
                {
                    range.range.type = (enRangeType)EditorGUILayout.Popup("范围类型", (int)range.range.type, RangeCfg.RangeTypeName);
                    if (range.range.type != enRangeType.collider)
                        range.range.distance = EditorGUILayout.FloatField(range.range.distance);//半径、长

                    if (range.range.type == enRangeType.sector)//角度
                        range.range.angleLimit = EditorGUILayout.FloatField(range.range.angleLimit);
                    else if (range.range.type == enRangeType.rect)//宽
                        range.range.rectLimit = EditorGUILayout.FloatField(range.range.rectLimit);

                    if (range.range.type != enRangeType.collider)
                        range.range.heightLimit = EditorGUILayout.FloatField(range.range.heightLimit);//高

                }
                range.range.beginOffsetAngle = EditorGUILayout.FloatField("角度偏移", range.range.beginOffsetAngle);
                range.range.begingOffsetPos = EditorGUILayout.Vector3Field("位置偏移", range.range.begingOffsetPos);

            }
            using (new AutoEditorTipButton("如果要设置的角色已经存在了，不重新查找"))
                notRefindIfExist.Draw("不重复查找", this, n);
            ret.Draw("设置角色", this, n);
        }

        public override void DrawGL(DrawGL draw,Node n, bool isSel)
        {
            if (!isSel || n ==null)
                return;

            Role owner = n.GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;

            Vector3 dir = owner.transform.forward;
            Vector3 pos = owner.TranPart.Pos;
            CollideUtil.Draw(range.range, draw, pos, dir, new Color(1,0,0,0.5f),  0 );
        }
#endif

    }


    public class FindRoleByRange : Conditional
    {
        FindRoleByRangeCfg CfgEx { get { return (FindRoleByRangeCfg)m_cfg; } }
      

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            var range = CfgEx.range;
            if (range.targetType == enSkillEventTargetType.selfAlway||
                range.targetType == enSkillEventTargetType.target ||
                range.targetType == enSkillEventTargetType.selfAlway ||
                range.range.type == enRangeType.collider
                )
            {
                Debuger.LogError("不支持的类型");
                return enNodeState.failure;
            }

            
            Vector3 dir = owner.transform.forward;
            Vector3 pos = owner.TranPart.Pos;

            //如果角色已经存在则不重新查找
            Role last = GetValue(CfgEx.ret);
            bool noReFind = GetValue(CfgEx.notRefindIfExist);
            if (noReFind && last != null&& CollideUtil.Hit(pos, dir, last.TranPart.Pos, last.RoleModel.Radius, range.range))
                return enNodeState.success;
            
            //遍历进行碰撞检测
            foreach (Role r in RoleMgr.instance.Roles)
            {
                //如果是上一个角色并且检查过碰撞，那么肯定不符合，不用再次检查
                if (noReFind && last != null)
                    continue;

                //陷阱不取
                if (r.Cfg.roleType == enRoleType.trap)
                    continue;

                //阵营限制
                if (!RoleMgr.instance.MatchTargetType(range.targetType, owner, r))
                    continue;

                //碰撞检测
                if (!CollideUtil.Hit(pos, dir, r.TranPart.Pos, r.RoleModel.Radius, range.range))
                    continue;
                SetValue(CfgEx.ret, r);
                return enNodeState.success;

            }
            SetValue(CfgEx.ret, null);
            return enNodeState.failure;
        }

        

        
    }
}