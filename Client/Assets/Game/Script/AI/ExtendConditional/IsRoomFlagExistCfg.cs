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
    public class IsRoomFlagExistCfg : ConditionalCfg
    {
        public string flag = "";
        public Value<bool> ret = new Value<bool>(false);
		public bool alwaysSuccess = false;
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            flag = EditorGUILayout.TextField("标记", flag);
            ret.Draw("结果", this, n);
			using (new AutoEditorTipButton("如果不勾选，那么比较结果为真才返回成功，相当于当成条件用；勾选的话那么始终为真，相当于当成行为用了"))
				alwaysSuccess = EditorGUILayout.Toggle("始终为真", alwaysSuccess);
        }
#endif


    }


    public class IsRoomFlagExist : Conditional
    {
        IsRoomFlagExistCfg CfgEx { get { return (IsRoomFlagExistCfg)m_cfg; } }


        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {

        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType)
        {
			bool b = LevelMgr.instance.LevelFlag.ContainsKey (CfgEx.flag);
			if (CfgEx.ret.region != enValueRegion.constant)
				SetValue(CfgEx.ret,b );
            
			return CfgEx.alwaysSuccess || b ? enNodeState.success : enNodeState.failure;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {

        }


    }
}