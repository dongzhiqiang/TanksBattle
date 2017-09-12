#region Header
/**
 * 名称：UseSkill
 
 * 日期：2016.6.2
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
    public class UseSkillCfg : NodeCfg
    {
        public ValueRole target = new ValueRole( enValueRegion.tree);
        public Value<string> skillId = new Value<string>("", enValueRegion.tree);
        public Value<bool> autoFindSkill= new Value<bool>(false);
        public Value<bool> autoFindTarget = new Value<bool>(true);
        public Value<bool> errorIfNoTarget = new Value<bool>(true);
        public Value<bool> errorIfDis = new Value<bool>(true);
		public bool waitIfCD = false;

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            target.Draw("目标",this,n);
            skillId.Draw("技能",this,n);
            using (new AutoEditorTipButton("技能为空的时候自动找一个技能"))
                autoFindSkill.Draw("自动找技能", this, n);
            using (new AutoEditorTipButton("技能目标为空的时候自动找一个目标，找的规则见技能自动朝向规则"))
                autoFindTarget.Draw("自动找目标", this, n);

            if(!(autoFindSkill.region == enValueRegion.constant &&autoFindSkill.Val ))
            {
                using (new AutoEditorTipButton("当目标为空时报错"))
                    errorIfNoTarget.Draw("目标空报错", this, n);
                using (new AutoEditorTipButton("要使用技能的时候和目标在范围外报错"))
                    errorIfDis.Draw("范围报错", this, n);
            }
            
			using (new AutoEditorTipButton ("如果cd就等待到cd完再执行，否则返回失败"))
				waitIfCD = EditorGUILayout.Toggle ("冷却等待",waitIfCD);
        }
#endif
        
    }


    public class UseSkill : Aciton
    {
        UseSkillCfg CfgEx { get { return (UseSkillCfg)m_cfg; } }

        Skill m_skill = null;
        bool m_isPlayMain = false;
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_isPlayMain = false;
            string skillId = GetValue(CfgEx.skillId);
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;

            m_skill = string.IsNullOrEmpty(skillId) ? null : owner.CombatPart.GetSkill(skillId);
            if (m_skill != null)
                return;
            
            if (!GetValue(CfgEx.autoFindSkill))
                return;

            m_skill = owner.CombatPart.AutoFindSkill();
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (m_skill == null)
                return enNodeState.failure;
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;
            CombatPart combatPart = owner.CombatPart;
            
            //技能正在播放中
            Skill curSkill = combatPart.CurSkill;
            //1 不是当前技能返回失败
            if(curSkill != null &&curSkill.ComboFirst != m_skill)
                return enNodeState.failure;

            //2 取ComboSkill比较耗，这里先判断下可以立即返回的情况
            if (curSkill != null)
            {
                //还不能设置连击
                if (!curSkill.CanComboNext)
                    return enNodeState.running;

                //已经设置了连击
                if (curSkill.HasComboBuff)
                    return enNodeState.running;
            }

            //3可能别的地方释放了技能
            if (curSkill != null && curSkill.ComboFirst == m_skill&& !m_isPlayMain)
                m_isPlayMain = true;

            //4 主技能还没有播放过却在cd中，那么等待cd
			if (!m_isPlayMain && m_skill.CDNeed > 0) {
				if(CfgEx.waitIfCD)
					return enNodeState.running;
				else
					return enNodeState.failure;
			}
                

            Skill comboSkill = m_skill.ComboSkill;
            //5 技能播放完了
			if (m_isPlayMain &&(comboSkill == null||comboSkill == m_skill)&& !m_skill.IsPlaying)
                return enNodeState.success;
            
            //6 检错下，连击技能不可能为空
            if(comboSkill == null)
            {
                LogError("逻辑出错，连击技能找不到，角色:{0} 技能:{1}",owner.Cfg.id,m_skill.Cfg.skillId);
                return enNodeState.failure;
            }
            
            //mp不足
            if(!comboSkill.IsMpEnough)
                return enNodeState.failure;

            //7 找到目标
            Role target = GetValue(CfgEx.target);

            //8 找不到目标
            bool autoFindTarget = GetValue(CfgEx.autoFindTarget);
            if (target == null && !autoFindTarget)
            {
                //首次使用报错，连击的就没有必要了，失败就失败了
                if (!m_isPlayMain &&GetValue(CfgEx.errorIfNoTarget))
                    LogError("目标为空，且不自动查找，释放不了技能，角色:{0} 技能:{1}", owner.Cfg.id, m_skill.Cfg.skillId);
                return enNodeState.failure;
            }

            //9 目标距离限制
            if (target != null)
            {
                float disSq = Util.XZSqrMagnitude(owner.transform.position, target.transform.position);
                float disLimit = comboSkill.Cfg.attackRange.distance +0.5f;
                if (disSq > disLimit* disLimit)
                {
                    //首次就失败范围failure
                    if (!m_isPlayMain)
                    {
                        if(GetValue(CfgEx.errorIfDis))
                            LogError("目标不在技能范围内，如果希望不在范围内也释放请设置自动找敌人，角色:{0} 技能:{1}", owner.Cfg.id, m_skill.Cfg.skillId);
                        return enNodeState.failure;
                    }
                    else
                        return enNodeState.success;
                }
            }

            //10 播放
            m_isPlayMain = true;
            //CombatPart.m_debugSkillPlay = true;
            CombatPart.enPlaySkill playSkill = combatPart.Play(comboSkill, target, true, false);
            //CombatPart.m_debugSkillPlay = false;
            if (playSkill == CombatPart.enPlaySkill.fail)
            {
                LogError("逻辑检测出错，不能使用技能");
                return enNodeState.failure;
            }
            return  enNodeState.running ;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            m_isPlayMain = true;
        }
    }
}
