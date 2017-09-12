#region Header
/**
 * 名称：SimpleRoleBehitState
 
 * 日期：2015.12.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SimpleRoleBehitState : SimpleRoleState
{
    SimpleRole.enBehitType m_curBehitType = SimpleRole.enBehitType.beiji;

    //被击相关
    bool m_behitToggle = false;
    float m_behitEndTime =0;

    //浮空相关
    float m_floatSpeed = 0;
    float m_floatAccelerated = 0;
    
    //倒地相关
    float m_groundEndTime=0;

    //击飞相关
    float m_flySpeed = 0;
    float m_flyAccelerated = 0;

    public SimpleRoleBehitState(SimpleRole r) : base(r) { }
    //进入这个状态的时候
    public override void OnEnter(SimpleRole.enState lastState, object param)
    {
        SimpleRole.enBehitType  behitType = (SimpleRole.enBehitType)param;
        if (lastState != SimpleRole.enState.behit )//之前不是被击状态则可以进入，但是要先清空
            PlayBehit(behitType);
        else if(m_curBehitType.CompareTo(behitType)<=0)//被击优先级比现在的高
            PlayBehit(behitType);

        //浮空中被击有一个小的浮空力
        if (m_parent.m_floatBehitStartSpeed > m_floatSpeed && behitType == SimpleRole.enBehitType.beiji && m_curBehitType == SimpleRole.enBehitType.fukong)
        {
            //下降状态要加回上升状态
            if (m_floatSpeed <= 0)
            {
                m_parent.Ani.Play(AniFxMgr.Ani_FuKong01, WrapMode.ClampForever, m_parent.m_fade);
            }
              
            //重设初速度
            m_floatSpeed = m_parent.m_floatBehitStartSpeed;
            m_floatAccelerated = m_parent.m_floatBehitAccelerated;
        }
    }


    //用于输入检测、位移和结束判断
    public override void OnUpdate()
    {
        //输入检测
        
        m_parent.CheckBeiji();//切换到别的被击状态
        
        //倒地延迟到了，倒地起身
        if(m_curBehitType == SimpleRole.enBehitType.daodi && Time.time>m_groundEndTime  ){
            PlayBehit(SimpleRole.enBehitType.qishen);
        }

        //起身时间到
        if (m_curBehitType == SimpleRole.enBehitType.qishen && (
            m_parent.Ani.CurSt == null || m_parent.Ani.CurSt.enabled == false || m_parent.Ani.CurSt.normalizedTime>=1
            ))
        {
            m_parent.GotoAuto();
            return;
        }

        //被击时间到
        if(m_curBehitType == SimpleRole.enBehitType.beiji && Time.time>m_behitEndTime  ){
            m_parent.GotoAuto();
            return;
        }
            

        //浮空位移
        if(m_curBehitType == SimpleRole.enBehitType.fukong){
            float a =m_floatAccelerated + SimpleRole.Default_Gravity_Speed.y;
            float endSpeed = Mathf.Clamp(m_floatSpeed + a*Time.deltaTime,m_parent.m_floatSpeeDownLimit,m_parent.m_floatSpeedUpLimit);
            CollisionFlags flags =m_parent.CC.Move(Vector3.up * (m_floatSpeed + endSpeed) * Time.deltaTime / 2);
            

            //落地进入倒地状态
            if ((flags & CollisionFlags.Below) != 0)//m_parent.Model.localPosition.y <= 0
            {
                //m_parent.Model.localPosition = Vector3.zero;
                PlayBehit(SimpleRole.enBehitType.daodi);
            }
            //进入下落状态
            else if (m_floatSpeed >0 && endSpeed<=0)
            {
                m_parent.Ani.Play(AniFxMgr.Ani_FuKong02, WrapMode.ClampForever, m_parent.m_fade);
                m_floatAccelerated = m_parent.m_floatAcceleratedDown;
            }
            m_floatSpeed = endSpeed;
        }
        //击飞位移
        else if (m_curBehitType == SimpleRole.enBehitType.jifei)
        {
            float a = m_flyAccelerated + SimpleRole.Default_Gravity_Speed.y;
            float endSpeed = Mathf.Clamp(m_flySpeed + a * Time.deltaTime, m_parent.m_flySpeeDownLimit, m_parent.m_flySpeedUpLimit);
            //m_parent.Model.localPosition += Vector3.up * (m_flySpeed + endSpeed) * Time.deltaTime / 2;
            CollisionFlags flags = m_parent.CC.Move(Vector3.up * (m_flySpeed + endSpeed) * Time.deltaTime / 2);

            //落地进入倒地状态
            if ((flags & CollisionFlags.Below) != 0) //if (m_parent.Model.localPosition.y <= 0)
            {
                //m_parent.Model.localPosition = Vector3.zero;
                PlayBehit(SimpleRole.enBehitType.daodi);
            }
            //进入下落状态
            else if (m_flySpeed > 0 && endSpeed <= 0)
            {
                m_flyAccelerated = m_parent.m_flyAcceleratedDown;
            }
            m_flySpeed = endSpeed;
        }
        
    }


    void PlayBehit(SimpleRole.enBehitType behitType)
    {
        if (behitType == SimpleRole.enBehitType.beiji)
        {
            m_parent.Ani.Play(m_behitToggle ? AniFxMgr.Ani_BeiJi01 : AniFxMgr.Ani_BeiJi02, WrapMode.ClampForever, m_parent.m_fade);
            
            m_behitToggle = !m_behitToggle;
            m_behitEndTime=Time.time+m_parent.m_behitDuration;
        }
        else if (behitType == SimpleRole.enBehitType.fukong)
        {
            m_parent.Ani.Play(AniFxMgr.Ani_FuKong01, WrapMode.ClampForever, m_parent.m_fade);
            m_floatSpeed = m_parent.m_floatStartSpeed;
            m_floatAccelerated = m_parent.m_floatAcceleratedUp;
        }
        else if (behitType == SimpleRole.enBehitType.daodi)
        {
            m_parent.Ani.Play(AniFxMgr.Ani_DaoDi, WrapMode.ClampForever, m_parent.m_fade);
            m_groundEndTime = Time.time + m_parent.m_groundDuration;
        }
        else if (behitType == SimpleRole.enBehitType.qishen)
        {
            m_parent.Ani.Play(AniFxMgr.Ani_QiShen, WrapMode.ClampForever, m_parent.m_fade);
        }
        else if (behitType == SimpleRole.enBehitType.jifei)
        {
            m_parent.Ani.Play(AniFxMgr.Ani_JiFei, WrapMode.Loop, m_parent.m_fade);
            m_flySpeed = m_parent.m_flyStartSpeed;
            m_flyAccelerated = m_parent.m_flyAcceleratedUp;
        }

        m_curBehitType = behitType;
    }
    

}
