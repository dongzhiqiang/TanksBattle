using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerStateSelectServer : PlayerState
{

    public void Enter(object param)
    {
        //有可能从游戏态、角色选择态过来的，先关闭网络
        NetMgr.instance.Close();
        UIMgr.instance.Open<UILogin2>();

        //获取服务器列表  
        NetMgr.instance.AccountHandler.FetchServerList();

    }
    public void Leave()
    {
        UIMgr.instance.Close<UILogin2>();
        UIMgr.instance.Close<UISelectServer>();
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
        return enPlayerState.selectServer;
    }
}