#region Header
/**
 * 名称：IsRoleState
 
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
    public class IsRoleStateCfg : ConditionalCfg
    {
        public ValueRole role=new ValueRole(enValueRegion.constant);
        public string states = "空闲|移动|战斗|包围";

        bool cache = false;
        HashSet<enRoleState> sts = new HashSet<enRoleState>();
        public HashSet<enRoleState> States
        {
            get {
                if(!cache)
                {
                    cache = true;
                    if (!RoleStateMachine.TryParse(states, ref sts))
                        LogError("状态名解析出错");
                }
                return sts;
            }

        }

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            role.Draw("角色",this,n);
            using (new AutoEditorTipButton("空闲|移动|战斗|被击|动作序列|包围"))
            {
                string s = EditorGUILayout.TextField("状态", states);
                if (s != states)
                {
                    states = s;
                    cache = false;
                }
            }
            
        }
#endif
        
    }

    public class IsRoleState : Conditional
    {
        IsRoleStateCfg CfgEx { get { return (IsRoleStateCfg)m_cfg; } }
        
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
        
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role r = GetValue(CfgEx.role);
            if (r == null)
                return enNodeState.failure;
            
            return CfgEx.States.Contains(r.RSM.CurStateType)? enNodeState.success: enNodeState.failure;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            
        }
    }
}