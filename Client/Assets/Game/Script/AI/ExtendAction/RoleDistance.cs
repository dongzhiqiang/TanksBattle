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
    public class RoleDistanceCfg : NodeCfg
    {
        public ValueRole v1= new ValueRole( enValueRegion.constant);
        public ValueRole v2 = new ValueRole(enValueRegion.constant);
        public Value<float> ret= new Value<float>(0, enValueRegion.tree);
        
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            v1.Draw("角色1",this,n);
            v2.Draw("角色2", this,n);
            ret.Draw("结果", this, n);
        }
#endif
        
    }


    public class RoleDistance : Aciton
    {
        RoleDistanceCfg CfgEx { get { return (RoleDistanceCfg)m_cfg; } }
        
        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role r1 = GetValue(CfgEx.v1);
            if (r1 == null)
            {
                SetValue(CfgEx.ret, 0);
                return enNodeState.failure;
            }
            
            Role r2 = GetValue(CfgEx.v2);
            if (r2== null)
            {
                SetValue(CfgEx.ret, 0);
                return enNodeState.failure;
            }

            Vector3 link = r1.transform.position - r2.transform.position;
            link.y = 0;
            SetValue(CfgEx.ret, link.magnitude);
            return enNodeState.success;
        }

        
    }
}
