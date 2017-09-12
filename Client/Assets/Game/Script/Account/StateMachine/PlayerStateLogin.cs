using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerStateLogin : PlayerState
{

    public void Enter(object param)
    {
        UIMgr.instance.Open<UILogin>();
    }
    public void Leave()
    {
        UIMgr.instance.Close<UILogin>();
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
        return enPlayerState.login;
    }
}