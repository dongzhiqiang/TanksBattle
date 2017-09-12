#region Header
/**
 * 名称：CompareBool
 
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
    
    public class CompareRoleCfg : ConditionalCfg
    {
        
        public ValueRole v1 = new ValueRole(enValueRole.none);
        public ValueRole v2 = new ValueRole(enValueRegion.tree);
        public enBoolCompare comp = enBoolCompare.equal;
        public Value<bool> ret = new Value<bool>(false);
        public bool alwaysSuccess = false;
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            v1.Draw("值1",this,n);
            v2.Draw("值2",this,n);
            comp = (enBoolCompare)EditorGUILayout.Popup("比较类型",(int)comp, CompareBoolCfg.CompareTypeNames);
            ret.Draw("结果",this,n);
            using (new AutoEditorTipButton("如果不勾选，那么比较结果为真才返回成功，相当于当成行为用；勾选的话那么始终为真，相当于当成行为用了"))
                alwaysSuccess = EditorGUILayout.Toggle("始终为真", alwaysSuccess);
        }
#endif

    }


    public class CompareRole : Conditional
    {
        CompareRoleCfg CfgEx { get { return (CompareRoleCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType)
        {
            Role r1 = GetValue(CfgEx.v1);
            if (!CfgEx.alwaysSuccess&&!CfgEx.v1.IsAlwaysNull&& r1 == null)
                return enNodeState.failure;

            Role r2 = GetValue(CfgEx.v2);
            if (!CfgEx.alwaysSuccess && !CfgEx.v2.IsAlwaysNull && r2 == null)
                return enNodeState.failure;

            bool ret=false;
            switch(CfgEx.comp)
            {
                case enBoolCompare.equal: ret= r1 == r2; break;
                case enBoolCompare.unEqual: ret = r1 != r2; break;
                default:
                    {
                        Debuger.LogError("未知的类型:{0}", CfgEx.comp);
                        ret = false;
                    };break;
            }

            if(CfgEx.ret.region!= enValueRegion.constant)
                SetValue(CfgEx.ret, ret);
            return CfgEx.alwaysSuccess || ret ? enNodeState.success : enNodeState.failure;

        }
    }
}