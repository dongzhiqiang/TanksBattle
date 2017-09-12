#region Header
/**
 * 名称：SetFloat
 
 * 日期：2016.5.18
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
    public class BehaviorHandleCfg : NodeCfg
    {

        public ValueGameObject go = new ValueGameObject();
        public Handle handle = new Handle();

        
        public override void OnReset() {
            if (handle.m_animationCurve.length == 0)
            {
                handle.m_animationCurve.AddKey(new Keyframe(0f, 0f, 0f, 1f));
                handle.m_animationCurve.AddKey(new Keyframe(1f, 1f, 1f, 0f));
                if (handle.m_go != null)
                    GameObject.DestroyImmediate(handle.m_go);
            }

        }

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            go.Draw("对象", this, n);

            
            if (!Application.isPlaying &&go.region == enValueRegion.constant&& !string.IsNullOrEmpty(go.path))
            {
                GameObject g = null;
                g = go.Val;
                if (g != handle.m_go)
                    handle.m_go = g;
            }
                
            

            //类型
            GUI.changed = false;
            int type = UnityEditor.EditorGUILayout.Popup("类型", (int)handle.m_type, Handle.TypeName);
            if (GUI.changed)
                handle.SetType((Handle.Type)type);

            //具体内容
            if (handle.CurHandle != null)
                handle.CurHandle.OnDraw(null, handle, null);

        }
#endif
    }

    public class BehaviorHandle : Aciton
    {
        BehaviorHandleCfg CfgEx { get { return (BehaviorHandleCfg)m_cfg; } }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            GameObject go = GetValue(CfgEx.go);
            if (go == null)
                return;
            CfgEx.handle.m_go = go;
            if (CfgEx.handle.IsPlaying)
            {
                Debuger.LogError("Handle已经在播放了，是不是逻辑错误");
                CfgEx.handle.End();
            }

            CfgEx.handle.Start();
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (CfgEx.handle.IsPlaying)
                CfgEx.handle.Update();

            if (CfgEx.handle.IsPlaying)
                return enNodeState.running;
            else if (GetValue(CfgEx.go) == null)
                return enNodeState.failure;
            else
                return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            if (CfgEx.handle.IsPlaying)
                CfgEx.handle.End();
        }
    }
}