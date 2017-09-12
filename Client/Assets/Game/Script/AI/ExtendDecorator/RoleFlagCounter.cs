#region Header
/**
 * 名称：Composite
 
 * 日期：2016.5.13
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
    public class RoleFlagCounterCfg : DecoratorCfg
    {
        public ValueRole r = new ValueRole(enValueRegion.constant);
        public string flag = "";

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            r.Draw("角色",this,n);
            flag = EditorGUILayout.TextField("标记", flag);
        }
#endif
    }

    public class RoleFlagCounter : Decorator
    {
        RoleFlagCounterCfg CfgEx { get { return (RoleFlagCounterCfg)m_cfg; } }
        enNodeState m_lastNodeState = enNodeState.success;

        protected override void OnPush()
        {
            m_lastNodeState = enNodeState.success;
            Role r = GetValue(CfgEx.r);
            if (r != null&&!string.IsNullOrEmpty(CfgEx.flag))
            {
                r.AddFlag(CfgEx.flag,1);
            }
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            return m_lastNodeState;
        }

        //字节点执行完,告知父节点
        public override void OnChildPop(int childIdx, enNodeState childState)
        {
            m_lastNodeState = childState;
        }

        

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            Role r = GetValue(CfgEx.r);
            if (r != null && !string.IsNullOrEmpty(CfgEx.flag))
            {
                r.AddFlag(CfgEx.flag, -1);
            }
        }


    }
}