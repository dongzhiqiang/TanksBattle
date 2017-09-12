using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerStatePlayGame : PlayerState
{
    private int ob=EventMgr.Invalid_Id;

    private void OnMainCitySceneStart(object arg1)
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
        string roomId = (string)arg1;
        if (roomId == LevelMgr.MainRoomID)
            UIMgr.instance.Close<UIMainCity>();
    }

    public void Enter(object param)
    {
        FullRoleInfoVo info = (FullRoleInfoVo)param;
        //创建英雄
        RoleMgr.instance.CreateHero(info);
        //进入主城
        LevelMgr.instance.GotoMaincity();
    }
    public void Leave()
    {
        UIMgr.instance.CloseAll();
        TeachMgr.instance.ClearPlayQueue();
        TeachMgr.instance.StartPlay(false);

        if (!LevelMgr.instance.IsMainCity())
        {
            LevelMgr.instance.GotoMaincity();
            ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.START, OnMainCitySceneStart);
        }
    }

    public void OnStateMsg(object param)
    {
    }
    public bool OnNetMsgFilter(int module, int command)
    {
        return true;
    }
    public enPlayerState GetStateType()
    {
        return enPlayerState.playGame;
    }
}