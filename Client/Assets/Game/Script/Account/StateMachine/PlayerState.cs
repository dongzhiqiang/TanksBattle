using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

interface PlayerState
{
    void Enter(object param);
    void Leave();
    void OnStateMsg(object param);
    bool OnNetMsgFilter(int module, int command);
    enPlayerState GetStateType();
}
