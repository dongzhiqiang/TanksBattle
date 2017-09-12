#region Header
/**
 * 名称：SetRole
 
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
    public class GetPosDistanceCfg : ConditionalCfg
    {
        public string posName = "";
        public Value<float> ret = new Value<float>(0, enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            posName = EditorGUILayout.TextField("关卡路径点名", posName);
            ret.Draw("结果", this, n);
        }
#endif

        
    }


    public class GetPosDistance : Conditional
    {
        GetPosDistanceCfg CfgEx { get { return (GetPosDistanceCfg)m_cfg; } }
        
        
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {
            
        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            var poss = SceneMgr.instance.SceneData.GetPossCfgByNames(CfgEx.posName);
            if (poss == null || poss.ps.Count == 0)
            {
                Debuger.LogError("找不到关卡路径点或者路径点个数为0:{0}", CfgEx.posName);
                return enNodeState.failure;
            }
            SetValue(CfgEx.ret, poss.GetClosestDis(owner.transform.position));
            return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            
        }

      
    }
}