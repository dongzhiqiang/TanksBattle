using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_EQUIP)]
public class EquipHandler
{
    #region Fields
    #endregion


    #region Properties
    #endregion


    #region Net
    //接收，同步
    [NetHandler(MODULE_EQUIP.PUSH_ADD_OR_UPDATE_EQUIP)]
    public void OnPushAddOrUpdateEquip(AddOrUpdateEquipVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        EquipsPart equipsPart = role.EquipsPart;
        EquipCfg equipCfg = EquipCfg.m_cfgs[info.equip.equipId];

        Equip equip = equipsPart.GetEquip(equipCfg.posIndex);
        if (equip == null)
        {
            equip = new Equip();
            equip.Owner = role;
            equipsPart.Equips.Add(equipCfg.posIndex, equip);
        }

        equip.LoadFromVo(info.equip);

        role.PropPart.FreshBaseProp();
        role.Fire(MSG_ROLE.EQUIP_CHANGE);

    }

    [NetHandler(MODULE_EQUIP.PUSH_REMOVE_EQUIP)]
    public void OnRemoveEquip(RemoveEquipVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        EquipsPart equipsPart = role.EquipsPart;
        Equip equip = equipsPart.GetEquip(info.index);
        if (equip != null)
        {
            equip.Owner = null;
            equipsPart.Equips.Remove(info.index);

            role.PropPart.FreshBaseProp();
            if (role == RoleMgr.instance.Hero)
            {
                if (UIMgr.instance.Get<UIEquip>() != null)
                {
                    UIMgr.instance.Get<UIEquip>().Refresh();
                }
            }
        }
    }

    //发送,升级
    public void SendUpgrade(string roleGUID, enEquipPos equipPos)
    {
        UpgradeEquipRequestVo request = new UpgradeEquipRequestVo();
        request.ownerGUID = roleGUID;
        request.equipPosIndex = (int)(equipPos);
        NetMgr.instance.Send(MODULE.MODULE_EQUIP, MODULE_EQUIP.CMD_UPGRADE, request); 
    }

    //接收，升级
    [NetHandler(MODULE_EQUIP.CMD_UPGRADE)]
    public void OnUpgrade(GrowEquipResultVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        if (role == RoleMgr.instance.Hero)
        {
            if (UIMgr.instance.Get<UIEquip>() != null)
            {
                UIMgr.instance.Get<UIEquip>().m_pageUpgrade.StartUpgradeFx(Equip.CreateFromVo(info.oldEquip), Equip.CreateFromVo(info.newEquip));
            }
        }
    }

    //发送,一键升级
    public void SendUpgradeOnce(string roleGUID, enEquipPos equipPos)
    {
        UpgradeOnceEquipRequestVo request = new UpgradeOnceEquipRequestVo();
        request.ownerGUID = roleGUID;
        request.equipPosIndex = (int)(equipPos);
        NetMgr.instance.Send(MODULE.MODULE_EQUIP, MODULE_EQUIP.CMD_UPGRADE_ONCE, request); 
    }

    //接收，一键升级
    [NetHandler(MODULE_EQUIP.CMD_UPGRADE_ONCE)]
    public void OnUpgradeOnce(GrowEquipResultVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        if (info.newEquip.advLv == info.oldEquip.advLv)
        {
            if (role == RoleMgr.instance.Hero)
            {
                if (UIMgr.instance.Get<UIEquip>() != null)
                {
                    UIMgr.instance.Get<UIEquip>().m_pageUpgrade.StartUpgradeFx(Equip.CreateFromVo(info.oldEquip), Equip.CreateFromVo(info.newEquip));
                }
            }
        }
        else
        {
            if (role == RoleMgr.instance.Hero)
            {
                if (UIMgr.instance.Get<UIEquip>() != null)
                {
                    UIMgr.instance.Get<UIEquip>().m_pageUpgrade.StartAdvanceFx(Equip.CreateFromVo(info.oldEquip), Equip.CreateFromVo(info.newEquip));
                }
            }
        }
    }

    //发送,全部升级
    public void SendUpgradeAll(string roleGUID)
    {
        UpgradeAllEquipRequestVo request = new UpgradeAllEquipRequestVo();
        request.ownerGUID = roleGUID;
        NetMgr.instance.Send(MODULE.MODULE_EQUIP, MODULE_EQUIP.CMD_UPGRADE_ALL, request); 
    }

    //接收，全部升级
    [NetHandler(MODULE_EQUIP.CMD_UPGRADE_ALL)]
    public void OnUpgradeAll(UpgradeAllEquipResultVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;
        if (info.equipList.Count==0)
        {
            UIMessage.Show("没有可升级的装备");
            if (UIMgr.instance.Get<UIEquip>() != null)
            {
                UIMgr.instance.Get<UIEquip>().m_pageUpgrade.ClearLock();
            }
            return;
        }
        if (role == RoleMgr.instance.Hero)
        {
            if (UIMgr.instance.Get<UIEquip>() != null)
            {
                UIMgr.instance.Get<UIEquip>().Refresh();
            }
        }
        UIMgr.instance.Open<UIEquipUpgradeResult>(info.equipList);
        UIPowerUp.SaveNewProp(role);
        UIPowerUp.ShowPowerUp(true);
    }

    //发送,升品
    public void SendAdvance(string roleGUID, enEquipPos equipPos)
    {
        AdvanceEquipRequestVo request = new AdvanceEquipRequestVo();
        request.ownerGUID = roleGUID;
        request.equipPosIndex = (int)(equipPos);
        NetMgr.instance.Send(MODULE.MODULE_EQUIP, MODULE_EQUIP.CMD_ADVANCE, request); 
    }

    //接收，升品
    [NetHandler(MODULE_EQUIP.CMD_ADVANCE)]
    public void OnAdvance(GrowEquipResultVo info)
    {
        //UIFxPanel.ShowFx("fx_ui_jinjiechenggong");
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        if (role == RoleMgr.instance.Hero)
        {
            if (UIMgr.instance.Get<UIEquip>() != null)
            {
                UIMgr.instance.Get<UIEquip>().m_pageUpgrade.StartAdvanceFx(Equip.CreateFromVo(info.oldEquip), Equip.CreateFromVo(info.newEquip));
            }
        }
    }

    //发送,觉醒
    public void SendRouse(string roleGUID, enEquipPos equipPos)
    {
        RouseEquipRequestVo request = new RouseEquipRequestVo();
        request.ownerGUID = roleGUID;
        request.equipPosIndex = (int)(equipPos);
        NetMgr.instance.Send(MODULE.MODULE_EQUIP, MODULE_EQUIP.CMD_ROUSE, request); 
    }

    //接收，觉醒
    [NetHandler(MODULE_EQUIP.CMD_ROUSE)]
    public void OnRouse(GrowEquipResultVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        if (role == RoleMgr.instance.Hero)
        {
            if (UIMgr.instance.Get<UIEquip>() != null)
            {
                UIMgr.instance.Get<UIEquip>().m_pageRouse.StartRouseFx(Equip.CreateFromVo(info.oldEquip), Equip.CreateFromVo(info.newEquip));
            }
        }
    }

   

   
    #endregion



}
