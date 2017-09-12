#region Header
/**
 * 名称：PlayFlyer
 
 * 日期：2016.7.12
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
    public class PlayEventGroupCfg : NodeCfg
    {
        public string eventGroupId;

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            using(new AutoBeginHorizontal())
            {
                eventGroupId = EditorGUILayout.TextField("事件组", eventGroupId);
                if (GUILayout.Button("打开"))
                {
                    EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.EVENT_GROUP_EDITOR);
                }
            }
        }
#endif
        public override void OnPreLoad() {
            //预加载
            SkillEventGroupCfg.PreLoad(eventGroupId);
        }

    }


    public class PlayEventGroup : Aciton
    {
        PlayEventGroupCfg CfgEx { get { return (PlayEventGroupCfg)m_cfg; } }
        
        
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {
            
        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            CombatMgr.instance.PlayEventGroup(owner, CfgEx.eventGroupId, owner.transform.position);
            
            return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            
        }
    }
}