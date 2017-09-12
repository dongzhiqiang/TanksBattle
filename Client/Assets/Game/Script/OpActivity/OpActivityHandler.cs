using UnityEngine;
using System.Collections;

[NetModule(MODULE.MODULE_OPACTIVITY)]
public class OpActivityHandler  {    

    public OpActivityHandler()
    {
        UIMainCity.AddClick(enSystem.opActivity, () =>
        {
            UIMgr.instance.Open<UIOpActivity>();                     
        });

        UIMainCity.AddClick(enSystem.lottery, () =>
        {
            UIMgr.instance.Open<UILottery>();
        });

        UIMainCity.AddClick(enSystem.vip, () =>
        {
            UIMgr.instance.Open<UIVipMain>();
        });
    }

    [NetHandler(MODULE_OPACTIVITY.PUSH_SYNC_PROP)]
    public void RoleSyncPropVo(SyncOpActivityPropVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        OpActivityPart part = role.OpActivityPart;
        part.OnSyncProps(info);
    }




    //发送,签到
    public void SendCheckIn()
    {    
        NetMgr.instance.Send(MODULE.MODULE_OPACTIVITY,MODULE_OPACTIVITY.CMD_CHECK_IN, null);
    }

    //接收,签到
    [NetHandler(MODULE_OPACTIVITY.CMD_CHECK_IN)]
    public void OnCheckIn(CheckInRes res)
    {
        //Debug.Log(res.checkInId);
        Role hero = RoleMgr.instance.Hero;      
        UIOpActivity uiOpActivity = UIMgr.instance.Get<UIOpActivity>();
        for (int i = 0; i < uiOpActivity.m_opActivityGroup.Count; i++)
        {
            UIOpActivityItem item = uiOpActivity.m_opActivityGroup.Get<UIOpActivityItem>(i);
            item.UpdateTip();
        }       
        UICheckIn uiCheckIn = uiOpActivity.UICheckIn;
        uiCheckIn.LoadCheckIn();
        uiCheckIn.GetCheckInReward(res.checkInId);

    }


    //发送，获取等级礼包
    public void SendGetLevelReward(int levelId)
    {
        LvRewardReq request = new LvRewardReq();
        request.levelId = levelId;
        NetMgr.instance.Send(MODULE.MODULE_OPACTIVITY, MODULE_OPACTIVITY.CMD_LEVEL_REWARD, request);
    }

    //接收,获取等级礼包
    [NetHandler(MODULE_OPACTIVITY.CMD_LEVEL_REWARD)]
    public void OnGetLevelReward(LvRewardRes res)
    {       
        UIOpActivity uiOpActivity = UIMgr.instance.Get<UIOpActivity>();
        for (int i = 0; i < uiOpActivity.m_opActivityGroup.Count; i++)
        {
            UIOpActivityItem item = uiOpActivity.m_opActivityGroup.Get<UIOpActivityItem>(i);
            item.UpdateTip();
        }
        UILevelReward uiLevelReward = uiOpActivity.UILevelReward;
        uiLevelReward.getLevelReward(res.levelId);
        uiLevelReward.Init();
    }

    //发送，获取vip礼包
    public void SendGetVipGift(int vipLv)
    {
        VipGiftReq request = new VipGiftReq();
        request.vipLv = vipLv;
        NetMgr.instance.Send(MODULE.MODULE_OPACTIVITY, MODULE_OPACTIVITY.CMD_VIP_GIFT, request);
    }

    //接收,获取vip礼包
    [NetHandler(MODULE_OPACTIVITY.CMD_VIP_GIFT)]
    public void OnGetVipGift(VipGiftRes res)
    {        
        UIVip uiVip = UIMgr.instance.Get<UIVipMain>().uiVip;
        uiVip.getVipGift(res.vipLv);
        uiVip.RefreshVipDes();     
    }

    public void SendDrawLotteryReq(int type, int subType)
    {
        var req = new DrawLotteryReq();
        req.type = type;
        req.subType = subType;
        NetMgr.instance.Send(MODULE.MODULE_OPACTIVITY, MODULE_OPACTIVITY.CMD_DRAW_LOTTERY, req);
    }

    [NetHandler(MODULE_OPACTIVITY.CMD_DRAW_LOTTERY)]
    public void OnDrawLotteryRes(DrawLotteryRes res)
    {
        UIMgr.instance.Get<UILotteryOpen>().ShowResultOnList(res);
    }
}
