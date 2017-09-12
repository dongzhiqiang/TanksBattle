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
    public class PlayFlyerCfg : NodeCfg
    {
        public string flyer = "";
        public FxCreateCfg createCfg = new FxCreateCfg();

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            using(new AutoBeginHorizontal())
            {
                createCfg.name = EditorGUILayout.TextField("特效", createCfg.name);
                if (GUILayout.Button("打开"))
                {
                    Transform source = null;
                    if(Application.isPlaying && n!=null)
                    {
                        Role owner = n.GetOwner<Role>();
                        if (owner != null && owner.State == Role.enState.alive)
                            source = owner.transform;
                    }

                    EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FX_EDITOR, "特效", createCfg, source);
                }
            }

            using (new AutoBeginHorizontal())
            {
                flyer = EditorGUILayout.TextField("飞出物", flyer);
                if (GUILayout.Button("打开"))
                {
                    FlyerCfg flyerCfg = string.IsNullOrEmpty(flyer) ? null : FlyerCfg.Get(flyer);
                    Action<string> onSel = (string flyerId) => flyer = flyerId;
                    EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FLYER_EDITOR, flyerCfg, onSel);
                }
            }
             
                

        }
#endif
        public override void OnPreLoad() {
            //预加载
            createCfg.PreLoad();
            if (!string.IsNullOrEmpty(flyer))
                FlyerCfg.PreLoad(flyer);
        }

    }


    public class PlayFlyer : Aciton
    {
        PlayFlyerCfg CfgEx { get { return (PlayFlyerCfg)m_cfg; } }
        
        
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {
            
        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            CfgEx.createCfg.Create(owner, null, owner.transform.position,enElement.none,
                OnLoad, new object[] { owner, null, null });

            return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            
        }

        void OnLoad(GameObject go, object param)
        {
            object[] pp = (object[])param;
            Role source = (Role)pp[0];
            Role target = (Role)pp[1];
            Skill parentSkill = (Skill)pp[2];
            Flyer.Add(go, CfgEx.flyer, source, target, parentSkill);

            //如果飞出物上没有任何销毁的脚本，那么提示下
            if (!FxDestroy.HasDelay(go))
            {
                Debuger.LogError("特效上没有绑销毁脚本，特效事件的特效也没有指定销毁时间.特效名:{0}", go.name);
            }
        }
    }
}