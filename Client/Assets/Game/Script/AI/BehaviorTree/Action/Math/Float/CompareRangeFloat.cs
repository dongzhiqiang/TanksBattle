#region Header
/**
 * 名称：CompareFloat
 
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
    public enum enCompRange
    {
        contain,
        unContain,
    }


    public class CompareRangeFloatCfg : ConditionalCfg
    {
        public static string[] CompareTypeNames = new string[] { "范围内", "范围外" };
        public Value<float> v = new Value<float>(0, enValueRegion.tree);
        public float left = 0;
        public float right = 1;
        public enCompRange comp = enCompRange.contain;
        public Value<bool> ret = new Value<bool>(false);
        public bool alwaysSuccess = false;
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {

            using (new AutoEditorTipButton("范围,闭区间"))
            {
                EditorGUILayout.PrefixLabel("范围");
                left = EditorGUILayout.FloatField(left);
                right = EditorGUILayout.FloatField(right);
            }
            v.Draw("值", this, n);
            comp = (enCompRange)EditorGUILayout.Popup("比较类型",(int)comp, CompareTypeNames);
            ret.Draw("结果",this,n);
            using (new AutoEditorTipButton("如果不勾选，那么比较结果为真才返回成功，相当于当成行为用；勾选的话那么始终为真，相当于当成行为用了"))
                alwaysSuccess = EditorGUILayout.Toggle("始终为真", alwaysSuccess);
        }
#endif

    }


    public class CompareRangeFloat : Conditional
    {
        CompareRangeFloatCfg CfgEx { get { return (CompareRangeFloatCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType)
        {
            var v = GetValue(CfgEx.v);
            
            bool ret=false;
            switch(CfgEx.comp)
            {
                case enCompRange.contain: ret= v >= CfgEx.left && v <= CfgEx.right; break;
                case enCompRange.unContain: ret = v < CfgEx.left|| v > CfgEx.right; break;
                default:
                    {
                        Debuger.LogError("未知的类型:{0}", CfgEx.comp);
                        ret = false;
                    };break;
            }

            if(CfgEx.ret.region!= enValueRegion.constant)
                SetValue(CfgEx.ret, ret);
            return CfgEx.alwaysSuccess || ret ? enNodeState.success:enNodeState.failure;

        }
    }
}