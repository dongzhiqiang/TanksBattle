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

    public class OperationLimitCfg : ConditionalCfg
    {
        public float limit = 1f;
        public Value<bool> ret = new Value<bool>(false);
        public bool alwaysSuccess = false;

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            limit = EditorGUILayout.FloatField("超时时间",limit);
            ret.Draw("结果", this, n);
            using (new AutoEditorTipButton("如果不勾选，那么比较结果为真才返回成功，相当于当成条件用；勾选的话那么始终为真，相当于当成行为用了"))
                alwaysSuccess = EditorGUILayout.Toggle("始终为真", alwaysSuccess);
        }
        
#endif

    }


    public class OperationLimit : Conditional
    {
        OperationLimitCfg CfgEx { get { return (OperationLimitCfg)m_cfg; } }
      

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            float lastOperation = owner.AIPart.LastOperation;
            bool ret = lastOperation == -1|| TimeMgr.instance.logicTime - lastOperation >= CfgEx.limit;
            if (CfgEx.ret.region != enValueRegion.constant)
                SetValue(CfgEx.ret, ret);
            return CfgEx.alwaysSuccess || ret ? enNodeState.success : enNodeState.failure;
        }

        

        
    }
}