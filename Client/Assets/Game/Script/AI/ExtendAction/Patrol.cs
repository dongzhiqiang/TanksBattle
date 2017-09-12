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
    public enum enPatrolBegin
    {
        lastAndBegin,
        lastAndClosest,
        begin,
        closest,
    }


    public enum enPatrol
    {
        once,
        pingPong,
        loop,
    }


    public class PatrolCfg: NodeCfg
    {
        public static string[] BeginTypeNames = new string[] { "上次的点没有则从头再来", "上次的点没有则找最近的点", "从头", "最近的点" };
        public static string[] TypeNames = new string[] { "单次", "来回", "循环"};

        public enPatrolBegin beginType = enPatrolBegin.lastAndClosest;
        public enPatrol type= enPatrol.pingPong;
        public Value<float> range = new Value<float>(4f);//范围，距离当前点多少范围算达到当前点
        public string posName = "";

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            posName = EditorGUILayout.TextField("关卡路径点名", posName);
            beginType = (enPatrolBegin)EditorGUILayout.Popup("开始点", (int)beginType, BeginTypeNames);
            type = (enPatrol)EditorGUILayout.Popup("巡逻类型", (int)type, TypeNames);
            using (new AutoEditorTipButton("距离当前要走到的路径点多远算达到"))
                range.Draw("范围", this, n);
        }
#endif
        
    }


    public class Patrol : Aciton
    {
        PatrolCfg CfgEx { get { return (PatrolCfg)m_cfg; } }

        int m_last = -1;
        SceneCfg.PossCfg m_poss=null;
        int m_curRound=0;

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush()
        {
            if (m_curRound > 1 && CfgEx.type == enPatrol.once)
                return;

            m_poss = SceneMgr.instance.SceneData.GetPossCfgByNames(CfgEx.posName);
            if(m_poss == null || m_poss.ps.Count==0)
            {
                Debuger.LogError("找不到关卡路径点或者路径点个数为0:{0}", CfgEx.posName);
                m_last = -1;
                return;
            }
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
            {
                m_last = -1;
                return;
            }
                

            switch (CfgEx.beginType)
            {
                case enPatrolBegin.lastAndBegin: {
                        if (m_last == -1)
                            m_last = 0;
                    }
                    break;
                case enPatrolBegin.lastAndClosest:
                    {
                        if (m_last == -1)
                            m_last = m_poss.GetClosestPosIdx(owner.transform.position);
                    }
                    break;
                case enPatrolBegin.begin:
                    {
                        m_last = 0;
                    }
                    break;
                case enPatrolBegin.closest:
                    {
                        m_last = m_poss.GetClosestPosIdx(owner.transform.position);
                    }
                    break;
                default:Debuger.LogError("未知的类型：{0}", CfgEx.beginType);break;
            }
        }


        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (m_curRound > 1 && (CfgEx.type == enPatrol.once || m_poss.ps.Count<=1))
                return enNodeState.success;
            if ( m_last == -1)
                return enNodeState.failure;
            
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            float range = GetValue(CfgEx.range);
            Vector3 targetPos = m_poss.ps[m_last];
            bool isReach = Util.XZSqrMagnitude(targetPos, owner.transform.position)< range* range;


            //如果没有到达
            if (!isReach)
            {
                //如果因为某种原因暂停了
                if(!owner.MovePart.IsMovingToPos(targetPos))
                {
                    if (!owner.MovePart.MovePos(targetPos))
                        return enNodeState.failure;
                }
                    
            }
            //如果到达了当前路点，那么走向下一个
            else
            {
                bool reversedOrder = CfgEx.type == enPatrol.pingPong && m_curRound % 2 == 1;
                bool isReachRound = reversedOrder? m_last == 0: m_last == m_poss.ps.Count-1;
                
                //下一轮
                if(isReachRound)
                {
                    ++m_curRound;

                    //如果是单次的，达到就可以返回了
                    if (m_curRound >= 1 && (CfgEx.type == enPatrol.once || m_poss.ps.Count <= 1))
                        return enNodeState.success;

                    reversedOrder = CfgEx.type == enPatrol.pingPong && m_curRound % 2 == 1;
                    switch (CfgEx.type)
                    {
                        case enPatrol.once:break;
                        case enPatrol.pingPong:m_last = reversedOrder ? m_poss.ps.Count - 2 : 1;break;
                        case enPatrol.loop: m_last = 0; break;
                        default: Debuger.LogError("未知的类型：{0}", CfgEx.type); break;
                    }
                }
                //下一个路点
                else
                {
                    m_last = reversedOrder ? m_last - 1:m_last + 1;
                }
               
                owner.MovePart.MovePos(m_poss.ps[m_last]);
            }

            return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop()
        {
            
        }

        //重置临时数据.销毁的时候或者onEnable的时候执行，可以当成数据初始化的地方
        protected override void OnResetState() {

            m_last = -1;
            m_poss = null;
            m_curRound = 0;
        }
    }
}