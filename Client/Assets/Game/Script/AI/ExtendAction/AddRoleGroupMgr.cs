#region Header
/**
 * 名称：AddRoleGroupMgr
 
 * 日期：2016.6.13
 * 描述：添加到角色群体管理器中，加入之后可以获取群体攻击目标和群体包围目标
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
    public class AddRoleGroupMgrCfg : NodeCfg
    {
        
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
        
        }
#endif
        
    }


    public class AddRoleGroupMgr : Aciton
    {
        AddRoleGroupMgrCfg CfgEx { get { return (AddRoleGroupMgrCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            RoleGroupMgr.instance.Add(owner);
            return enNodeState.success;
        }
    }
}
