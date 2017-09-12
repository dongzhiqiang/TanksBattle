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
    public class SetRoleCfg: ConditionalCfg
    {
        public ValueRole find =new ValueRole(enValueRegion.constant);
        public ValueRole targetVal = new ValueRole(enValueRegion.tree);
        public Value<bool> notRefindIfExist = new Value<bool>(false);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            find.Draw("查找角色",this,n);
            using (new AutoEditorTipButton("如果要设置的角色已经存在了，不重新查找"))
                notRefindIfExist.Draw("不重复查找", this, n);
            targetVal.Draw("设置角色",this,n);
           
        }
#endif

        
    }


    public class SetRole: Conditional
    {
        SetRoleCfg CfgEx { get { return (SetRoleCfg)m_cfg; } }
        
        
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {
            
        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            
            //如果角色已经存在则不重新查找
            if (GetValue(CfgEx.notRefindIfExist) && GetValue(CfgEx.targetVal) != null)
                return enNodeState.success;

            Role find = GetValue(CfgEx.find);
            SetValue(CfgEx.targetVal, find);
            return find != null ? enNodeState.success: enNodeState.failure;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            
        }

        protected override bool OnCustomInterupt()
        {
            Role target = GetValue(CfgEx.targetVal);
            if (GetValue(CfgEx.notRefindIfExist)&& target != null)
                return false;
            
            
            Role find = GetValue(CfgEx.find);
            if (find != target)
            {
                SetValue(CfgEx.targetVal, find);
                return true;
            }
            else
                return false;
        }


        void ClearCache()
        {
            
        }
    }
}