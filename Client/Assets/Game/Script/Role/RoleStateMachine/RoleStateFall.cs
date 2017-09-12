using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色下落状态
 
 * 日期：2016.1.12
 * 描述：
 * *********************************************************
 */
public enum enFall
{
    air,//下落中
    ground,//落地

}
public class RoleStateFall : RoleState
{
    #region Fields
    enFall m_state; 
    int m_hangCount=0;//滞空次数
    float m_firstHangDuration = 2;//首次滞空事件
    float m_hangDuration = 1;
    float m_beginTime =0;
    float m_fallSpeed = 0;//下落初速度
    float m_fallAccelerate = 0;//下落加速度
    float m_cameraMoveTime = 0.5f;//相机渐变时间
    float m_cameraOverTime = 0.5f;//相机结束渐变时间
    TranPartCxt m_cxt;
    TranPartCxt m_jumpCxt;
    #endregion

    #region Properties
    public override enRoleState Type { get { return enRoleState.fall; } }
    public enFall CurStateType { get { return m_state; } }
    public float FirstHangDuration { get{return m_firstHangDuration;}set{m_firstHangDuration =value;}}
    public float HangDuration { get { return m_hangDuration; } set { m_hangDuration = value; } }
    public float Duration { get{return m_hangCount==1?m_firstHangDuration:m_hangDuration;}}
    public float FallSpeed { get { return m_fallSpeed; } set { m_fallSpeed = value; } }
    public float FallAccelerate { get { return m_fallAccelerate; } set { m_fallAccelerate = value; } }
    public float CameraMoveTime { get { return m_cameraMoveTime; } set { m_cameraMoveTime = value; } }
    public float CameraOverTime { get { return m_cameraOverTime; } set { m_cameraOverTime = value; } }
    public TranPartCxt JumpCxt { get { return m_jumpCxt; } set{
        if(m_jumpCxt!= null){
            Debuger.LogError("起跳上下文重复，可能重复起跳");
            if (TranPart != null)
                TranPart.RemoveCxt(m_jumpCxt);
            m_jumpCxt = null;
        }
        m_jumpCxt = value;
            
    }}
    #endregion

    #region Frame
    public RoleStateFall(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }

    public override void Enter(object param)
    {
        if(JumpCxt != null)
            JumpCxt = null;
        m_state = enFall.air;
        m_rsm.AniPart.Play("xialuo01", WrapMode.Loop,0.2f,1f,true);
        m_beginTime = Time.time;
        ++m_hangCount;
    }

    
    public override void Do(object param)
    {
        Debuger.LogError("重复进入下落状态"); 
    }
    
    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_state == enFall.ground && (AniPart.CurSt == null || AniPart.CurSt.normalizedTime >= 1);
    }

    public override void Leave()
    {
        if (TranPart != null)
            TranPart.RemoveCxt(m_cxt);
        m_cxt = null;
    }

    public override void Update()
    {
        //进入落地状态
        if (m_state == enFall.air && TranPart.IsGrounded)
        {
            AniPart.Play("xialuo02", WrapMode.ClampForever, 0.2f, 1f, true); 
            m_state = enFall.ground; 
            m_rsm.IsAir =false;
        }
        //添加落地加速度
        else if (m_state == enFall.air && m_cxt == null && Time.time - m_beginTime > Duration)
        {
            m_cxt = TranPart.AddCxt();
            m_cxt.moveType = TranPartCxt.enMove.dir;
            m_cxt.SetMoveDir(Vector3.up,enValidAxis.vertical);
            m_cxt.speed = m_fallSpeed;
            m_cxt.accelerate = m_fallAccelerate;
        }
        //落地结束
        else if (m_state == enFall.ground && (AniPart.CurSt == null || AniPart.CurSt.normalizedTime>=1))
        {
            //移动中切移动
            if (m_rsm.CheckMove())
                return;
            //待机
            else if (m_rsm.CheckFree())
                return;
        }    
    }

    #endregion
    //重置下滞空次数
    public void ResetHang()
    {
        m_hangCount = 0;
        JumpCxt = null;
    }

    
}
