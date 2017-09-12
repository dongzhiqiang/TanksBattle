#region Header
/**
 * 名称：死亡部件
 
 * 日期：2015.9.21
 * 描述：控制角色死亡(可以带延迟)
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DeadPart:RolePart
{
    #region Fields
    
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.dead; } }
    
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {

    }
    #endregion


    #region Private Methods
    
    #endregion
    //是不是可以死亡了
    public bool Check()
    {
        if(m_parent.RoleModel.m_undead)return false;
        if (DebugUI.instance.unDead) return false;
        int hp = m_parent.GetInt(enProp.hp);
        if (hp > 0)
            return false;

        return true;
    }

    //如果可以死亡处理死亡
    public bool CheckAndHandle(bool checkState = true)
    {
        if (!Check())
            return false;

        RoleStateBehit behit = RSM.StateBehit;
        //被击浮空中的话，等倒地结束做死亡动作
        if (checkState && behit.IsCur && behit.CurStateType != enBehit.behit && behit.CurStateType != enBehit.ground)
            return false;

        //如果是在空中，进入浮空状态
        RoleStateFall fall =RSM.StateFall;
        if (checkState && RSM.IsAir)
        {
            //进入浮空下落状态
            RSM.GotoState(enRoleState.beHit,null);
            return false;
        }

        if (behit.CurStateType == enBehit.ground)
            Handle(false, true);
        else
            Handle(false);
        return true;
    }

    //处理死亡
    public void Handle(bool destroy, bool isGround = false, bool bHeroKill = true)
    {
        if (m_parent == null || m_parent.State == Role.enState.dead)
        {
            Debuger.LogError("已经死亡或者被销毁，不能再次死亡");
            return;
        }
        //ai先暂停
        AIPart.Pause();

        int poolId = m_parent.Id;
        Role r = m_parent;
        Role lastBeHit = this.HatePart.GetLastBehit();//最后的攻击者
        //如果有最后的攻击者，广播杀死消息
        if (lastBeHit != null)
            lastBeHit.Fire(MSG_ROLE.KILL, Parent);
        if (r.IsUnAlive(poolId))//FIX:防止逻辑会在别的地方销毁角色
            return;

        if (destroy|| RSM.IsModelEmpty)
        {
            if (bHeroKill)
                Parent.Fire(MSG_ROLE.DEAD, true, lastBeHit);
            if (r.IsUnAlive(poolId))
                return;
            RoleMgr.instance.DestroyRole(m_parent);
            return;
        }

        //死亡特效(不需要做死亡动作)
        if (!string.IsNullOrEmpty(Parent.Cfg.deadFx) && RoleFxCfg.Play(Parent.Cfg.deadFx, Parent, Parent) != null)
        {
            if (bHeroKill)
                Parent.Fire(MSG_ROLE.DEAD, true, lastBeHit);
            if(r.IsUnAlive(poolId))
                return;
            RoleMgr.instance.DestroyRole(m_parent);
            return;
        }

        //先广播死亡消息
        if (bHeroKill)
            Parent.Fire(MSG_ROLE.DEAD, false, lastBeHit);
        if (r.IsUnAlive(poolId))//FIX:防止逻辑会在别的地方销毁角色
            return;
        
        //没有死亡特效就进入死亡状态
        RoleMgr.instance.DeadRole(m_parent, isGround, true, bHeroKill);
        
    }

}
