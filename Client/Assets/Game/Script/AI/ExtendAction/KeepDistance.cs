#region Header
/**
 * 名称：UseSkill
 
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
    public class KeepDistanceCfg : NodeCfg
    {
        public ValueRole target = new ValueRole(enValueRole.cloesetEnemy);
        public float dis1= 4;
        public float dis2 = 7;
        public float angle1 = 0;
        public float angle2 = 0;
        public bool keepIfTargetMove = true;
        

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            using (new AutoEditorTipButton("作为包围参考的目标，注意不能填自己，不然会有异常情况"))
                target.Draw("目标", this, n);

            using (new AutoEditorTipButton("保持距离的范围，如果在范围内则不作任何行为，在最小范围内跑离目标，最大范围外跑向目标"))
            {
                EditorGUILayout.PrefixLabel("距离范围");
                dis1 = EditorGUILayout.FloatField(dis1);
                dis2 = EditorGUILayout.FloatField(dis2);
            }

            using (new AutoEditorTipButton("如果在角色前方的最小角度内，跑向最小角度到最大角度之间"))
            {
                EditorGUILayout.PrefixLabel("角度范围");
                angle1 = EditorGUILayout.FloatField(angle1);
                angle2 = EditorGUILayout.FloatField(angle2);
            }

            using (new AutoEditorTipButton("如果不勾选，那么只计算一次终点，否则目标移动的时候会实时计算"))
                keepIfTargetMove = EditorGUILayout.Toggle("实时计算",keepIfTargetMove);
        }
#endif

    }


    public class KeepDistance : Aciton
    {
        KeepDistanceCfg CfgEx { get { return (KeepDistanceCfg)m_cfg; } }

        const float KeepMoveAdjustDuration = 0.3f;//保持移动的时候的调整时间
        const float KeepMoveMaxDuration = 1.5f;//保持移动的时候的最小移动计算时间
        const float KeepMovePreCheckDuration = 0.2f;//保持移动的时候的预先重新计算时间，不能等到跑到寻路点再跑向下一个，这样角色动作会一卡一卡
        const int Stuck_Stop_Limit = 3;//卡住停止的次数上限，如果卡住了这么多次，那么马上算执行成功

        Quaternion m_rotate;
        float m_dis;
        float m_lastMoveTime = 0;
        float m_duration = 0;
        Vector3 m_lastPos = Vector3.zero;
        Vector3 m_targetPos = Vector3.zero;
        bool m_isTrace = false;
        bool m_lastIsMoving = false;//为了多算一帧，以达到真正的位置而不是调整后的位置
        int m_stuckCounter = 0;
        float m_lastExecuteTime = 0;

        public Vector3 TargetPos { get { return m_targetPos; } }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_lastMoveTime = 0;
            m_lastExecuteTime = 0;
            m_stuckCounter = 0;
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            Role target = GetValue(CfgEx.target);
            if (target == null )
                return enNodeState.failure;

            var movePart = owner.MovePart;
            float speed = owner.GetFloat(enProp.speed);

            //首次调用的话，计算一些东西
            Vector3 forward = target.transform.forward;
            forward.y = 0;
            float dis = -1;
            if (m_lastMoveTime == 0)
            {
                float angle = CfgEx.angle1 <= 0 ? 0 : UnityEngine.Random.Range(CfgEx.angle1, CfgEx.angle2);
                m_dis = UnityEngine.Random.Range(CfgEx.dis1, CfgEx.dis2);


                Vector3 link = owner.transform.position - target.transform.position;
                link.y = 0;

                //角度和距离都在范围内就返回成功
                bool needChangeAngle = CfgEx.angle1 > 0 && Vector3.Angle(forward, link) < CfgEx.angle1;
                dis = link.magnitude;
                if (!needChangeAngle && dis >= CfgEx.dis1 && dis <= CfgEx.dis2)
                    return enNodeState.success;

                //是要远离还是要追向角色
                m_isTrace = dis < CfgEx.dis1;


                //先调整link的角度
                if (needChangeAngle)
                {
                    if (Vector3.Cross(forward, link).y > 0)//右边
                        link = Quaternion.Euler(0, angle, 0) * forward;
                    else
                        link = Quaternion.Euler(0, -angle, 0) * forward;
                }

                //计算出旋转
                m_rotate = Quaternion.FromToRotation(forward, link);
                
            }
            else
            {
                Vector3 curPos = owner.transform.position;
                float d = Vector3.Distance(curPos, m_lastPos);//实际移动
                float d2 = speed * (TimeMgr.instance.logicTime - m_lastExecuteTime);//计算出来的移动
                if (d <(d2 / 10) )
                    ++m_stuckCounter;
            }

            m_lastExecuteTime = TimeMgr.instance.logicTime;
            m_lastPos = owner.transform.position;

            //移动，否则检查是不是移动完了
            bool targetIsMoving = target.MovePart.IsMoveing;
            bool needKeep = targetIsMoving || (m_lastIsMoving && !targetIsMoving);//注意哪怕目标停下来了也得多判断一次.以达到正确的位置，而不是修正后的
            if (m_lastMoveTime == 0 ||(CfgEx.keepIfTargetMove && needKeep && (TimeMgr.instance.logicTime- m_lastMoveTime)>= m_duration))
            {
                m_lastIsMoving = targetIsMoving;
                m_lastMoveTime = TimeMgr.instance.logicTime;
                m_targetPos = target.transform.position + m_rotate * (forward * m_dis);
                float d = Util.XZMagnitude(m_lastPos, m_targetPos);
                
                if (speed == 0)
                {
                    Debuger.LogError("角色速度为0，移动会卡住：{0}", owner.Cfg.id);
                    speed = 0.5f;
                }
                m_duration = d/speed;

                //为了保持持续移动，这里要再修正下，要下次判断时间要在寻路停止前，如果这个时间太短，那么要反向延长寻路距离，以保证移动持续
                if (CfgEx.keepIfTargetMove&& targetIsMoving)
                {
                    m_duration = Mathf.Min(m_duration - KeepMovePreCheckDuration, KeepMoveMaxDuration);
                    if(  m_duration < KeepMoveAdjustDuration)
                    {
                        m_duration = KeepMoveAdjustDuration;
                        float needDistance = (m_duration + KeepMovePreCheckDuration) * speed;
                        Vector3 link = m_targetPos - m_lastPos;
                        link.y = 0;
                        m_targetPos = m_lastPos + link.normalized * needDistance;
                    }
                }

                
                if (!movePart.MovePos(m_targetPos))
                    return enNodeState.failure;
            }
            else if(!movePart.IsMoveing)
                    return enNodeState.success;
            //如果移动过久可能是卡住了，这里主动退出下
            else if (m_stuckCounter>= Stuck_Stop_Limit)
                return enNodeState.success;



            return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;

            var curSt = owner.RSM.CurStateType;

            //自动朝向
            Role target = GetValue(CfgEx.target);
            if(target != null && (curSt == enRoleState.move || curSt == enRoleState.free))
            {
                owner.TranPart.SetDir(target.transform.position - owner.transform.position);
            }

            //停止移动
            var movePart = owner.MovePart;
            if(curSt == enRoleState.move)
            {
                owner.RSM.CheckFree();
            }
                

        }


#if UNITY_EDITOR
        public override void OnDrawGizmos() {
            Color old = Gizmos.color;
            Gizmos.color = this.State == enNodeState.running ? Color.red : Color.yellow;
            Gizmos.DrawSphere(this.TargetPos, 0.3f);
            Gizmos.color = old;
        }
#endif
    }
}
