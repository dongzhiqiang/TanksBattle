using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerStateStart : PlayerState
{

    public void Enter(object param)
    {
    }
    public void Leave()
    {
    }
    public void OnStateMsg(object param)
    {
    }
    public bool OnNetMsgFilter(int module, int command)
    {
        return false;
    }
    public enPlayerState GetStateType()
    {
        return enPlayerState.start;
    }
}