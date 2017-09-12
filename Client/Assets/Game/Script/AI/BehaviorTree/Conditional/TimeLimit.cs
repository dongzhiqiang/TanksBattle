#region Header
/**
 * 名称：是否超时
 
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

    public class TimeLimitCfg : ConditionalCfg
    {
        public Value<float> v = new Value<float>(0, enValueRegion.tree);
        public float limit = 1f;
        public Value<bool> ret = new Value<bool>(false);
        public bool alwaysSuccess = false;
        public bool overSuccess = true;
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            v.Draw("值", this, n);
            limit = EditorGUILayout.FloatField("超时时间",limit);
            overSuccess = EditorGUILayout.Toggle("超时", overSuccess);
            ret.Draw("结果", this, n);
            using (new AutoEditorTipButton("如果不勾选，那么比较结果为真才返回成功，相当于当成条件用；勾选的话那么始终为真，相当于当成行为用了"))
                alwaysSuccess = EditorGUILayout.Toggle("始终为真", alwaysSuccess);
        }
        
#endif

    }


    public class TimeLimit : Conditional
    {
        TimeLimitCfg CfgEx { get { return (TimeLimitCfg)m_cfg; } }
      

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            float t1 = GetValue(CfgEx.v);
            float t2 = TimeMgr.instance.logicTime;

            bool ret = Mathf.Abs(t2-t1) >= CfgEx.limit;

            if (!CfgEx.overSuccess)
                ret = !ret;

            if (CfgEx.ret.region != enValueRegion.constant)
                SetValue(CfgEx.ret, ret);
            return CfgEx.alwaysSuccess || ret ? enNodeState.success : enNodeState.failure;
        }

        

        
    }
}