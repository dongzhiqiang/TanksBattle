using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色换武器状态
 
 * 日期：2016.1.19
 * 描述：
 * *********************************************************
 */

public class RoleStateSwitchWeapon : RoleState
{
    #region Fields
    bool m_modSwitch =false;
    bool m_canLeave =false;
    #endregion

    #region Properties
    public override enRoleState Type { get { return enRoleState.switchWeapon; } }
    #endregion

    #region Frame
    public RoleStateSwitchWeapon(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }



    public override void Enter(object param)
    {
        m_rsm.AniPart.Play("huanwuqi", WrapMode.ClampForever,0f);
        m_modSwitch = false;
        m_canLeave =false;
        RenderPart.ClearWeapon();
        
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        m_modSwitch =false;
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_canLeave;
    }

    public override void Leave()
    {

    }

    public override void Update()
    {
        if (!m_modSwitch&& (AniPart.CurSt == null || AniPart.CurSt.normalizedTime >= 0.1f))
        {
            m_modSwitch= true;
            WeaponCfg weapon =CombatPart.FightWeapon;
            if (weapon == null)
                RenderPart.ChangeWeapon(null);
            else
                RenderPart.ChangeWeapon(weapon.modRight, weapon.modLeft);

        }
        if ((AniPart.CurSt == null || AniPart.CurSt.normalizedTime >= 1f))
        {
            m_canLeave= true;
            //下落
            if (m_rsm.CheckFall())
                return;
            //移动中切移动
            else if (m_rsm.CheckMove())
                return;
            //待机
            else if (m_rsm.CheckFree())
                return;
        }
    }
    #endregion
}
