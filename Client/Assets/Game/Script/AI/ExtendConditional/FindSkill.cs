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
    public class FindSkillCfg : ConditionalCfg
    {
        public ValueRole target = new ValueRole(enValueRegion.tree);
        public Value<string> skillId=new Value<string>("",enValueRegion.tree);
        public Value<float> range= new Value<float>(0, enValueRegion.tree);

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            target.Draw("目标",this,n);
            skillId.Draw("设置技能",this,n);
            range.Draw("设置范围", this, n);
        }
#endif
        
    }


    public class FindSkill : Conditional
    {
        FindSkillCfg CfgEx { get { return (FindSkillCfg)m_cfg; } }
      

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            var parent = GetOwner<Role>();
            if (parent == null)
                return enNodeState.failure;

            var target = GetValue(CfgEx.target);
            if (target == null)
                return enNodeState.failure;

            Vector3 srcPos = parent.transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 link = targetPos - srcPos;
            link.y = 0;
            Skill s = parent.CombatPart.AutoFindSkill(link.magnitude);
            if (s != null)
            {
                SetValue(CfgEx.skillId, s.Cfg.skillId);
                SetValue(CfgEx.range, s.Cfg.attackRange.distance);
                return enNodeState.success;
            }
            else
            {
                SetValue(CfgEx.skillId, "");
                SetValue(CfgEx.range, 0);
                return enNodeState.failure;
            }
                
        }

        

        
    }
}