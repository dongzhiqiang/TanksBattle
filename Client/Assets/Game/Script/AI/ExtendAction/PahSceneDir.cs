#region Header
/**
 * 名称：PahSceneDir
 
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
    public class PahSceneDirCfg : NodeCfg
    {
       

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
                
        }
#endif
        
    }


    public class PahSceneDir : Aciton
    {
        PahSceneDirCfg CfgEx { get { return (PahSceneDirCfg)m_cfg; } }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (!SceneMgr.instance.IsDirShow)
                return enNodeState.success;
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;
            MovePart movePart = owner.MovePart;
            if (!movePart.IsMoveing)
                movePart.MovePos(SceneMgr.instance.CurFindPos);
            return  enNodeState.running ;
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
