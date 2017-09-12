using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PlayerStateSelectRole : PlayerState
{
    public void Enter(object param)
    {
        var roleList = (RoleListVo)param;
        if (roleList.roleList.Count > 0)
        {
            var heroId = roleList.roleList[0].heroId;
            NetMgr.instance.AccountHandler.SendActivateRole(heroId);
        }
        else
        {
            //没角色，进入序章
            NetMgr.instance.AccountHandler.SendGetDemoHeroData(AccountHandler.DEF_HERO_ROLEID);
        }
    }
    public void Leave()
    {
        UIMgr.instance.CloseAll();
        TeachMgr.instance.ClearPlayQueue();
        TeachMgr.instance.StartPlay(false);
        //if (UIMgr.instance.Get<UICreateRole>().IsOpen)
        //    UIMgr.instance.Close<UICreateRole>();
    }
    public void OnStateMsg(object param)
    {
    }
    public bool OnNetMsgFilter(int module, int command)
    {
        return module == MODULE.MODULE_ACCOUNT;
    }
    public enPlayerState GetStateType()
    {
        return enPlayerState.selectRole;
    }
}