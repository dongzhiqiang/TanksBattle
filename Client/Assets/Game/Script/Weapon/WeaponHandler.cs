#region Header
/**
 * 名称：登录
 
 * 日期：2015.9.21
 * 描述：登录
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Encrypt;
using System.Net.Sockets;

[NetModule(MODULE.MODULE_WEAPON)]
public class WeaponHandler 
{
    #region Fields

    #endregion


    #region Properties

    #endregion

    #region Constructors
    public WeaponHandler()
    {
        UIMainCity.AddClick(enSystem.weapon,()=> UIMgr.instance.Open<UIWeapon>());
    }
    #endregion

    #region Net

    #endregion

    #region Private Methods

    #endregion
    //接收,换武器
    [NetHandler(MODULE_WEAPON.CMD_CHANGE_WEAPON)]
    public void OnChangeWeapon(WeaponChangeRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        weaponPart.CurWeaponIdx = res.weapon;

        //刷新战斗属性
        hero.PropPart.FreshBaseProp();

        UIWeapon ui = UIMgr.instance.Get<UIWeapon>();
        if (ui.IsOpen)
        {
            ui.Refresh();
        }
        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(true);        
    }

    //发送,换武器
    public void SendChangeWeapon(enEquipPos pos)
    {
        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        WeaponChangeReq request = new WeaponChangeReq();
        request.weapon = (int)pos-(int)enEquipPos.minWeapon;
        NetMgr.instance.Send(MODULE.MODULE_WEAPON, MODULE_WEAPON.CMD_CHANGE_WEAPON, request);
    }

    //接收,技能升级
    [NetHandler(MODULE_WEAPON.CMD_SKILL_LEVEL_UP)]
    public void OnSkillUp(WeaponSkillLevelUpRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        WeaponSkill skill = weaponPart.GetWeapon(res.weapon).GetSkill(res.skill);
        skill.lv = res.lv;

        //升级特效
        //UIFxPanel.ShowFx("fx_ui_zhuangbei_shengjichengong");

        //界面刷新
        UIWeapon ui = UIMgr.instance.Get<UIWeapon>();
        if (ui.IsOpen)
        {
            UIWeaponSkillItem item = ((UIWeaponPageSkill)ui.m_pages[0]).GetSkillItem(skill);
            if(item != null )
            {
                item.StartFx();
            }
            else
            {
                ui.Fresh();
            }
        }
            
        UIWeaponSkillUp uiSkillUp = UIMgr.instance.Get<UIWeaponSkillUp>();
        if (uiSkillUp.IsOpen)
            uiSkillUp.Refresh();

        hero.Fire(MSG_ROLE.WEAPON_SKILL_CHANGE);

        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(false);
    }

    //发送,技能升级
    public void SendSkillUp(WeaponSkill skill)
    {
        WeaponSkillLevelUpReq request = new WeaponSkillLevelUpReq();
        request.weapon = skill.Parent.Idx;
        request.skill = skill.Idx;
        request.lv = skill.lv + 1;
        
        NetMgr.instance.Send(MODULE.MODULE_WEAPON, MODULE_WEAPON.CMD_SKILL_LEVEL_UP, request);
    }

    //接收,技能升级
    [NetHandler(MODULE_WEAPON.CMD_TALENT_LEVEL_UP)]
    public void OnTalentUp(WeaponSkillTalentUpRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        weaponPart.GetWeapon(res.weapon).GetSkill(res.skill).GetTalent(res.talent).lv = res.lv;

        //升级特效
        UIFxPanel.ShowFx("fx_ui_zhuangbei_shengjichengong");

        //界面刷新
        UIWeapon ui = UIMgr.instance.Get<UIWeapon>();
        if (ui.IsOpen)
            ui.Refresh();

        UIWeaponTalentUp uiTalentUp = UIMgr.instance.Get<UIWeaponTalentUp>();
        if (uiTalentUp.IsOpen)
            uiTalentUp.Refresh();

        hero.Fire(MSG_ROLE.WEAPON_TALENT_CHANGE);
        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(false);
    }

    //发送,铭文升级，注意idx是装备位减enEquipPos.minWeapon
    public void SendTalentUp(WeaponSkillTalent talent)
    {
        WeaponSkillTalentUpReq request = new WeaponSkillTalentUpReq();
        request.weapon = talent.Parent.Parent.Idx;
        request.skill = talent.Parent.Idx;
        request.talent = talent.Idx;
        request.lv = talent.lv + 1;

        NetMgr.instance.Send(MODULE.MODULE_WEAPON, MODULE_WEAPON.CMD_TALENT_LEVEL_UP, request);
    }

    //接收,技能升级
    [NetHandler(MODULE_WEAPON.CMD_CHANGE_ELEMENT)]
    public void OnElementChange(WeaponElementChangeRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        weaponPart.GetWeapon(res.weapon).elements = res.elements;
        
        //界面刷新
        UIWeapon ui = UIMgr.instance.Get<UIWeapon>();
        if (ui.IsOpen)
            ui.Refresh();
    }

    //发送,切换武器元素属性
    public void SendElementChange(int element)
    {
        if (element <= (int)enElement.none || element > (int)enElement.max)
            return;

        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        var weapon = weaponPart.CurWeapon;
        WeaponElementChangeReq request = new WeaponElementChangeReq();
        request.weapon = weapon.Idx;
        request.idx = weapon.elements.IndexOf(element);

        if (request.idx == 0)
            return;

        NetMgr.instance.Send(MODULE.MODULE_WEAPON, MODULE_WEAPON.CMD_CHANGE_ELEMENT, request);
    }


    //发送,切换武器元素属性
    public void SendElementChange(Weapon weapon,int idx)
    {
        WeaponElementChangeReq request = new WeaponElementChangeReq();
        request.weapon = weapon.Idx;
        request.idx = idx;
        
        NetMgr.instance.Send(MODULE.MODULE_WEAPON, MODULE_WEAPON.CMD_CHANGE_ELEMENT, request);
    }
}
