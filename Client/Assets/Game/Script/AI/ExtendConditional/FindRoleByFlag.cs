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
    public class FindRoleByFlagCfg : ConditionalCfg
    {
        public string flag = "";
        public ValueRole ret = new ValueRole(enValueRegion.tree);
        public bool notRefindIfExist = false;
        public enOrderRole orderType = enOrderRole.normal;

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            flag = EditorGUILayout.TextField("标记", flag);
            
            ret.Draw("角色", this, n);
            using (new AutoEditorTipButton("如果要设置的角色已经存在了，不重新查找"))
                notRefindIfExist = EditorGUILayout.Toggle("不重复查找", notRefindIfExist);

            orderType = (enOrderRole)EditorGUILayout.Popup("查找类型", (int)orderType, RoleMgr.OrderRoleTypeName);
        }
#endif
        
    }


    public class FindRoleByFlag : Conditional
    {
        FindRoleByFlagCfg CfgEx { get { return (FindRoleByFlagCfg)m_cfg; } }
      

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            //如果角色已经存在则不重新查找
            if (CfgEx.notRefindIfExist && GetValue(CfgEx.ret) != null)
                return enNodeState.success;

            if (string.IsNullOrEmpty(CfgEx.flag))
                return enNodeState.failure;

            Role r = RoleMgr.instance.GetRoleByFlag(CfgEx.flag,-1,null, owner,CfgEx.orderType);
            SetValue(CfgEx.ret, r);
            return r == null ? enNodeState.failure : enNodeState.success;
        }

        

        
    }
}