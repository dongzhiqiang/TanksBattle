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
    public class RoleAniCfg : NodeCfg
    {
        public string param = "xuanyun:循环:5|beiji01:单次";
        public bool errorIfNoFindAni = true;

        bool cache = false;
        RoleStateAniCxt cxt;
        public RoleStateAniCxt Cxt
        {
            get
            {
                if (!cache)
                {
                    cache = true;
                    cxt = RoleStateAniCxt.Parse(param);
                }
                return cxt;
            }

        }

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            
            using (new AutoEditorTipButton(
@"动作序列上下文,免疫行为列表
角色进入一个播放一系列动作的行为中，这个行为的将替代待机行为
动作序列上下文,动作1:循环方式:持续时间:渐变时间|动作2:循环方式:持续时间:渐变时间|动作3:循环方式:持续时间:渐变时间
免疫行为列表，此行为能免疫某些行为,移动|战斗|被击|死亡|下落|换武器|包围"))
            {
                string s = EditorGUILayout.TextField("参数", param);
                if (s != param)
                {
                    param = s;
                    cache = false;
                }
            }
            using (new AutoEditorTipButton("找不到动作的时候报错下"))
                errorIfNoFindAni = EditorGUILayout.Toggle("找不到报错",errorIfNoFindAni);

        }
#endif
        
    }


    public class RoleAni : Aciton
    {
        RoleAniCfg CfgEx { get { return (RoleAniCfg)m_cfg; } }

        bool m_hasEnter = false;
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_hasEnter = false;
            
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            var cxt = CfgEx.Cxt;
            if (cxt == null)
                return enNodeState.failure;

            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            if(!m_hasEnter)
            {
                m_hasEnter = true;

                //如果没有找到动作那么直接返回
                if (!owner.AniPart.HasAnis(cxt.anis))
                {
                    if (CfgEx.errorIfNoFindAni)
                    {
                        LogError("找不到动作:{0}",CfgEx.param);
                    }
                    return enNodeState.failure;
                }

                //播放动作序列
                if (!owner.RSM.StateAni.Goto(cxt))
                {
                    return enNodeState.failure;
                }
            }
            //检查是不是结束
            else
            {
                if(!owner.RSM.StateAni.IsPlaying || owner.RSM.StateAni.Cxt!=cxt)
                {
                    return enNodeState.success;
                }
                
            }
            return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            var cxt = CfgEx.Cxt;
            if (cxt == null)
                return;

            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;
            owner.RSM.StateAni.CheckLeave(cxt);
        }
    }
}
