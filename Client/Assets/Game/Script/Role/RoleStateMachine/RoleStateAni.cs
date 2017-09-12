using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：动作序列状态
 
 * 日期：2016.3.8
 * 描述：行为上类似待机动作
 * *********************************************************
 */

//例如:xuanyun:循环:3|gongji01:单次:-1|xuanyun:来回:-2,移动|战斗
public class RoleStateAniCxt 
{
    public SimpleAnimationsCxt anis;
    public HashSet<enRoleState> avoidRoleStates = new HashSet<enRoleState>();
    

    public static RoleStateAniCxt Parse(string param)
    {
        if (string.IsNullOrEmpty(param))
            return null;

        string[] pp = param.Split(',');
        return Parse(pp[0], pp.Length < 2 ? null : pp[1]);
    }

    public static RoleStateAniCxt Parse(string anisParam, string avoidParam)
    {
        SimpleAnimationsCxt anis = SimpleAnimationsCxt.Parse(anisParam);
        if (anis == null)
            return null;

        RoleStateAniCxt cxt = new RoleStateAniCxt();
        cxt.anis = anis;

        if (!RoleStateMachine.TryParse(avoidParam, ref cxt.avoidRoleStates))
            return null;
        
        return cxt;
    }

}
public class RoleStateAni : RoleState
{
    #region Fields
    bool m_canLeave = false;
    RoleStateAniCxt m_cxt;
    #endregion

    #region Properties
    public override enRoleState Type { get { return enRoleState.ani; } }
    public bool IsPlaying { get{return m_cxt!=null;}}
    public RoleStateAniCxt Cxt { get { return m_cxt; } }
    #endregion

    #region Frame
    public RoleStateAni(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }



    public override void Enter(object param)
    {
        Do(param);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        m_canLeave = false;
        
        
        TranPart.ResetHight();//重置模型高度，可能在空中
        AniPart.Play(m_cxt.anis); //播放动作
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        if(m_canLeave)
            return true;

        //待机状态不可以退出这个状态,并且报错，一般是不会执行到待机状态的
        if(nextState.Type == enRoleState.free){
            Debuger.LogError("逻辑错误有待机状态想要替代动作序列状态");
            return false;
        }

        return !IsAvoidState(nextState.Type);//nextState被免疫的话则不能取消这个状态
    }

    public override void Leave()
    {
      
    }

    public override void Update()
    {
        if (TimeMgr.instance.IsPause)
            return;
        //动作序列播放结束，这个状态就会自己退出
        if(AniPart.IsAnisOver(m_cxt.anis))
        {
           CheckLeave(m_cxt);
        }
    }
    //被对象池回收的时候
    public override void OnDestroy() {
        m_cxt = null;
    }
    #endregion

    public bool Goto(RoleStateAniCxt cxt,bool force=false)
    {
        //检错
        if (m_cxt == cxt){
            Debuger.LogError("逻辑错误，动画序列状态重复传进来上下文");
            return false;
        }
        if (cxt == null && IsCur)
        {
            Debuger.LogError("逻辑错误，动画序列状态已经在播放中了");
            return true;
        }
        
        if (cxt == null && m_cxt == null)
        {
            Debuger.LogError("RoleStateAni逻辑错误传进来的上下文为空");
            return false;
        }

        if (cxt != null)
            m_cxt = cxt;        

        if (!force &&cxt != null)
                force = cxt.avoidRoleStates.Contains(RSM.CurStateType);
        return m_rsm.GotoState(Type, cxt, force);//如果这个上下文免疫了运行中状态，那么强制进入本状态
    }

    //上层希望离开这个状态，前提是这个状态正在运行的上下文和上层的id一样
    public void CheckLeave(RoleStateAniCxt cxt)
    {
        if (m_cxt == null || m_cxt != cxt) return;

        m_cxt = null;

        // 如果当前处在这个状态那么离开
        if (IsCur)
        {
            m_canLeave = true;
            RSM.CheckFree();
        }
        
    }

    public bool IsAvoidState(enRoleState st)
    {
        if (!IsPlaying)
        {
            Debuger.LogError("逻辑错误，调用这个接口前要先判断下RSM的IsAnis为true才能调用");
            return false;
        }

        return m_cxt.avoidRoleStates.Contains(st);//nextState被免疫的话则不能取消这个状态
    }
    
    
}
