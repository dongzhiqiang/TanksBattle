using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[NetModule(MODULE.MODULE_TREASURE)]
public class TreasureHandler
{

    //发送,升级
    public void SendUpgradeTreasure(int treasureId)
    {
        UpgradeTreasureRequestVo request = new UpgradeTreasureRequestVo();
        request.treasureId = treasureId;
        NetMgr.instance.Send(MODULE.MODULE_TREASURE, MODULE_TREASURE.CMD_UPGRADE_TREASURE, request);
    }

    //接收，升级
    [NetHandler(MODULE_TREASURE.CMD_UPGRADE_TREASURE)]
    public void OnUpgradeTreasure()
    {
        if (UIMgr.instance.Get<UITreasure>().IsOpen )
        {
            UIMgr.instance.Get<UITreasure>().Refresh();
        }

        if (UIMgr.instance.Get<UITreasureInfo>().IsOpen)
        {
            UIMgr.instance.Get<UITreasureInfo>().Reflesh();
        }
    }

    //发送,出战
    public void SendChangeBattleTreasure(List<int> battleTreasure)
    {
        ChangeBattleTreasureRequestVo request = new ChangeBattleTreasureRequestVo();
        request.battleTreasure = battleTreasure;
        NetMgr.instance.Send(MODULE.MODULE_TREASURE, MODULE_TREASURE.CMD_CHANGE_BATTLE_TREASURE, request);
    }

    //接收，出战
    [NetHandler(MODULE_TREASURE.CMD_CHANGE_BATTLE_TREASURE)]
    public void OnChangeBattleTreasure()
    {
        if (UIMgr.instance.Get<UITreasure>().IsOpen)
        {
            UIMgr.instance.Get<UITreasure>().Refresh();
        }

        if (UIMgr.instance.Get<UITreasureInfo>().IsOpen)
        {
            UIMgr.instance.Get<UITreasureInfo>().Reflesh();
        }
    }


    [NetHandler(MODULE_TREASURE.PUSH_ADD_OR_UPDATE_TREASURE)]
    public void OnAddOrUpdateTreasure(AddOrUpdateTreasureVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.TreasurePart.AddOrUpdateTreasure(info.treasure);
        //role.PropPart.FreshBaseProp();
    }


    [NetHandler(MODULE_TREASURE.PUSH_UPDATE_BATTLE_TREASURE)]
    public void OnAddOrUpdateTreasure(UpdateBattleTreasureVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.TreasurePart.UpdateBattleTreasure(info.battleTreasure);
        //role.PropPart.FreshBaseProp();
    }
}