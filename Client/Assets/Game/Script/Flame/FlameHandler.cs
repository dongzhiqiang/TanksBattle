using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[NetModule(MODULE.MODULE_FLAME)]
public class FlameHandler
{
    //发送,升级
    public void SendUpgradeFlame(int flameId, List<ItemVo> items)
    {
        UpgradeFlameRequestVo request = new UpgradeFlameRequestVo();
        request.flameId = flameId;
        request.items = items;
        NetMgr.instance.Send(MODULE.MODULE_FLAME, MODULE_FLAME.CMD_UPGRADE_FLAME, request);
    }

    //接收，升级
    [NetHandler(MODULE_FLAME.CMD_UPGRADE_FLAME)]
    public void OnUpgradeFlame(UpgradeFlameResultVo info)
    {
        UIMgr.instance.Close<UIFlameMaterial>();

        UIFxPanel.ShowFx("fx_ui_zhuangbei_shengjichengong", new Vector3(0, 200, 0));
        if(info.levelAdd>0)
        {
            if (UIMgr.instance.Get<UIFlame>() != null)
            {
                UIMgr.instance.Get<UIFlame>().PlayUpgrade();
            }
            UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
            UIPowerUp.ShowPowerUp(true);
        }
        else
        {
            if (UIMgr.instance.Get<UIFlame>() != null)
            {
                UIMgr.instance.Get<UIFlame>().Reflesh();
            }
        }
    }

    [NetHandler(MODULE_FLAME.PUSH_ADD_OR_UPDATE_FLAME)]
    public void OnAddOrUpdateFlame(AddOrUpdateFlameVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.FlamesPart.AddOrUpdateFlame(info.flame);
        role.PropPart.FreshBaseProp();
    }

}