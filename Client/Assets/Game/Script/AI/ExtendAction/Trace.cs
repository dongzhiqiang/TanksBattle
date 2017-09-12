#region Header
/**
 * 名称：Trace
 
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
    public class TraceCfg : NodeCfg
    {
        public ValueRole target = new ValueRole( enValueRegion.tree);
        public Value<float> dis = new Value<float>(0);

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            target.Draw("目标",this,n);
            dis.Draw("完成距离",this,n);
        }
#endif
        
    }


    public class Trace : Aciton
    {
        TraceCfg CfgEx { get { return (TraceCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            Role target = GetValue(CfgEx.target);
            if (target == null )
                return enNodeState.failure;

            //如果已经达到完成距离了，那么退出这个行为
            MovePart movePart = owner.MovePart;
            float disLimit = Mathf.Max(GetValue(CfgEx.dis),0.5f);
            Vector3 targetPos = target.transform.position;
            float disSq = owner.DistanceSq(target);  
            if (disSq < disLimit * disLimit)
            {
                if (movePart.IsMoveing)
                    movePart.Stop();
                return enNodeState.success;
            }

            //已经在移动中了，不频繁调整
            if (movePart.IsMoveing && movePart.MoveTime < 0.5f)
                return enNodeState.running;

            movePart.MovePos(targetPos);
            return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;

            var curSt = owner.RSM.CurStateType;
            
            //停止移动
            var movePart = owner.MovePart;
            if (curSt == enRoleState.move)
            {
                owner.RSM.CheckFree();
            }


        }
    }
}
