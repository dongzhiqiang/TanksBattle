using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：RoleSubGetUp
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */



public class GetUpCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.getUp; } }
 
}

public  class RoleSubGetUp:RoleBehitSubState
{
    #region Fields
    
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.getUp; } }
    public GetUpCxt MyCxt { get { return (GetUpCxt)Cxt; } }
    #endregion

    #region Frame
    public RoleSubGetUp(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }


    public override bool CanParentLeave(){
        string aniName = AniFxMgr.Ani_QiShen;
        if (Parent.IsHero)  //主角能切换武器 动作名有后缀
        {
            CombatPart combPart = Parent.CombatPart;
            if (combPart != null && combPart.FightWeapon != null)
                aniName = aniName + combPart.FightWeapon.postfix;
        }
        AniPart aniPart = Parent.AniPart;
        AnimationState st =aniPart.CurSt;
        if (st == null || st.name != aniName)
            return true;
        return st.normalizedTime>=1f;
    }
    
    //能不能切换到目标上下文
    public override IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        //小怪起身未完成就要取消保护，不然一半小怪的倒地有0.2s无敌+起身1s无敌，玩家的攻击间隔太久了，不爽
        if ((cxt.Type == enBehit.behit || cxt.Type == enBehit.befloat || cxt.Type == enBehit.beFly)&&CanMove())
        {
            return cxt;
        }

        return base.CanStateTo(cxt);
    }
    public override void Leave()
    {

    }

    public override void Enter()
    {
        Do();
    }

    public override void Do()
    {
        AniPart aniPart = Parent.AniPart;
        aniPart.Play(AniFxMgr.Ani_QiShen, WrapMode.ClampForever, Ani_Fade,1,true);
    }
    public override void Update()
    {

    }

    #endregion

    #region Private Methods

    #endregion
    //小怪起身未完成就要取消保护，不然一半小怪的倒地有0.2s无敌+起身1s无敌，玩家的攻击间隔太久了，不爽
    public bool CanMove(){
        
        if (Parent.Cfg.RolePropType == enRolePropType.monster )
        {
            AnimationState st = Parent.AniPart.CurSt;
            if (st.normalizedTime >= 0.5f && st.length - st.time < 0.3f)//播放超过一半，且快要完成
            {
                return true;
            }
        }

        return false;
    }
}
