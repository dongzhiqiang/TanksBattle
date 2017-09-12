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
    public class RoundCfg : NodeCfg
    {
        public ValueRole target = new ValueRole(enValueRole.groupRound);
        public enRoleRoundType type = enRoleRoundType.auto;
        public float round1 = 4;//包围范围的随机下限
        public float round2 =7;//包围范围的随机上限
        public float run =15;//跑向范围
        public float speedRate =0.5f;//前移、后移、左右三种类型时的速度比例，注意跑向的速度不受这个值影响
        public float angle1 =20;//左右移动角度的随机下限
        public float angle2 =40;//左右移动角度的随机上限
        public float look1 =2;//盯着时间的随机下限
        public float look2 =4;//盯着时间的随机上限


#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            using (new AutoEditorTipButton("作为包围参考的目标，注意不能填自己，不然会有异常情况"))
                target.Draw("目标", this, n);

            using (new AutoEditorTipButton(
@"和目标目标保持一定位置关系,有下面几种类型，可以自己指定也可以自动选择
1.跑向，跑向目标，达到包围范围内时完成 (如果两者距离超过跑向范围，被自动选择)
2.同步，保持和目标移动方向相同，目标停下来时完成 (如果目标在移动,被自动选择)
3.前移，前移动到包围范围 (如果两者距离超过包围范围上限，被自动选择；自动选择的情况下，如果目标突然移动，切换到同步)
4.后移，后移动到包围范围 (如果两者距离小于包围范围下限，被自动选择；自动选择的情况下，如果目标突然移动，切换到同步)
5.左右，那么随机左右移动一次 (如果两者距离在包围范围内，被自动选择 ；自动选择的情况下，如果目标突然移动，切换到同步)
6.盯着，始终原地盯着目标 (不会被自动选择)"))
            {
                type = (enRoleRoundType)EditorGUILayout.Popup("类型", (int)type, RoleStateRound.TypeNames);
            }

            if(type != enRoleRoundType.stillLook)
            {
                using (new AutoEditorTipButton("包围范围,如果是跑向，那么双方距离达到范围就会退出包围"))
                {
                    EditorGUILayout.PrefixLabel("包围范围");
                    round1 = EditorGUILayout.FloatField(round1);
                    round2 = EditorGUILayout.FloatField(round2);
                }
            }

            if (type == enRoleRoundType.auto || type == enRoleRoundType.run)
            {
                using (new AutoEditorTipButton("跑向范围,如果两者距离超过跑向范围，被自动选择"))
                    run = EditorGUILayout.FloatField("跑向范围", run);

                
            }

            if (type == enRoleRoundType.auto || type == enRoleRoundType.leftRight|| type == enRoleRoundType.foward|| type == enRoleRoundType.back)
            {
                using (new AutoEditorTipButton("前移、后移、左右三种类型时的速度比例将乘以移动速度，注意跑向的速度不受这个值影响"))
                    speedRate = EditorGUILayout.FloatField("移动速度比", speedRate);
            }

            

            if (type == enRoleRoundType.auto || type == enRoleRoundType.leftRight)
            {
                using (new AutoEditorTipButton("左右移动角度，角色左右移动的时候始终以目标到角色为半径的圆上做移动，这个时候填角度比填距离更方便理解"))
                {
                    EditorGUILayout.PrefixLabel("移动角度");
                    angle1 = EditorGUILayout.FloatField(angle1);
                    angle2 = EditorGUILayout.FloatField(angle2);
                }
            }

            if (type == enRoleRoundType.stillLook)
            {
                using (new AutoEditorTipButton("盯着的时间"))
                {
                    EditorGUILayout.PrefixLabel("盯着时间");
                    look1 = EditorGUILayout.FloatField(look1);
                    look2 = EditorGUILayout.FloatField(look2);
                }
            }
        }
#endif
        
    }


    public class Round : Aciton
    {
        RoundCfg CfgEx { get { return (RoundCfg)m_cfg; } }

        
        int m_cxtId = -1;
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_cxtId = -1;
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {

            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return enNodeState.failure;

            Role target = GetValue(CfgEx.target);
            if (target == null )
                return enNodeState.failure;

            RoleStateRound round = owner.RSM.StateRound;
            if (m_cxtId == -1)
            {
                RoleStateRoundCxt cxt = IdTypePool<RoleStateRoundCxt>.Get();
                m_cxtId = cxt.Id;
                cxt.target = target;
                cxt.roundType = CfgEx.type;
                cxt.roundRangeMin = CfgEx.round1;//包围范围的随机下限
                cxt.roundRangeMax = CfgEx.round2;//包围范围的随机上限
                cxt.runRange = CfgEx.run;//跑向范围
                cxt.speedRate = CfgEx.speedRate;//前移、后移、左右三种类型时的速度比例，注意跑向的速度不受这个值影响
                cxt.leftRightAngleMin = CfgEx.angle1;//左右移动角度的随机下限
                cxt.leftRightAngleMax = CfgEx.angle2;//左右移动角度的随机上限
                cxt.lookDurationMin = CfgEx.look1;//盯着时间的随机下限
                cxt.lookDurationMax = CfgEx.look2;//盯着时间的随机上限
                //进入包围状态
                if (!round.GotoState(cxt,false,true))
                {
                    return enNodeState.failure;
                }
            }
            //检查是不是结束
            else
            {
                if(!round.IsCur|| round.RoundCxtId != m_cxtId)
                {
                    return enNodeState.success;
                }
                
            }
            return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            int cxtId = m_cxtId;
            m_cxtId = -1;
            Role owner = GetOwner<Role>();
            if (owner == null || owner.State != Role.enState.alive)
                return;

            RoleStateRound round = owner.RSM.StateRound;
            if (!round.IsCur || round.RoundCxtId != cxtId)
            {
                return;
            }
            owner.RSM.CheckFree();
        }
    }
}
