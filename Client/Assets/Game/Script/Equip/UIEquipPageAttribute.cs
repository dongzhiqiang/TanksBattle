
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIEquipPageAttribute : MonoBehaviour
{
    #region SerializeFields
    public Text m_hp;
    public Text m_atk;
    public Text m_def;
    public Text m_damageDef;
    public Text m_damage;
    public Text m_critical;
    public Text m_criticalDef;
    public Text m_criticalDamage;
    public Text m_fire;
    public Text m_fireDef;
    public Text m_ice;
    public Text m_iceDef;
    public Text m_thunder;
    public Text m_thunderDef;
    public Text m_dark;
    public Text m_darkDef;
    public Text m_level;
    public Text m_expText;
    public Text m_element;
    public Text m_rouseState;
    public ImageEx m_expBar;
    public StateHandle m_btnShow;
    public Text m_weaponName;
    public Text m_hitProp;

    #endregion

    #region Fields
    UIEquip m_parent;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIEquip parent)
    {
        m_parent = parent;
    }

    //显示
    public void OnOpenPage()
    {
        // 刷新属性
        RefleshAttribute();
    }

    void RefleshAttribute()
    {
        PropPart propPart = RoleMgr.instance.Hero.PropPart;
        m_hp.text = ""+Mathf.RoundToInt(propPart.GetFloat(enProp.hpMax));
        m_atk.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.atk));
        m_def.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.def));
        m_damageDef.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.damageDef));
        m_damage.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.damage));
        m_critical.text = String.Format("{0:F}", propPart.GetFloat(enProp.critical) * 100) + "%";
        m_criticalDef.text = String.Format("{0:F}", propPart.GetFloat(enProp.criticalDef) * 100) + "%";
        m_criticalDamage.text = String.Format("{0:F}", propPart.GetFloat(enProp.criticalDamage) * 100) + "%";
        m_fire.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.fire));
        m_fireDef.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.fireDef));
        m_ice.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.ice));
        m_iceDef.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.iceDef));
        m_thunder.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.thunder));
        m_thunderDef.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.thunderDef));
        m_dark.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.dark));
        m_darkDef.text = "" + Mathf.RoundToInt(propPart.GetFloat(enProp.darkDef));
        m_level.text = "" + propPart.GetInt(enProp.level);
        int needExp;
        float expRate;
        RoleCfg roleCfg = RoleCfg.Get(propPart.GetString(enProp.roleId));
        if (propPart.GetInt(enProp.level) < ConfigValue.GetInt("maxPetLevel"))
        {
            needExp = RoleLvExpCfg.GetNeedExp(propPart.GetInt(enProp.level));
            expRate = ((float)propPart.GetInt(enProp.exp)) / needExp;
            if (expRate > 1) expRate = 1;
        }
        else
        {
            needExp = 0;
            expRate = 0;
        }
        m_expBar.fillAmount = expRate;
        m_expText.text = propPart.GetInt(enProp.exp) + "/" + needExp;

        Weapon curWeapon = RoleMgr.instance.Hero.WeaponPart.CurWeapon;
        ElementCfg elementCfg = curWeapon.CurElementCfg;
        m_element.text = string.Format("<color=#C7994C>伤害类型</color>　{0}", elementCfg.name);

        EquipCfg equipCfg = curWeapon.Equip.Cfg;
        string rouseStateText = "<color=#C7994C>觉醒效果</color>　";
        rouseStateText += equipCfg.rouseDescription;
        /*
        if (equipCfg.stateId != null && equipCfg.stateId.Length > 0)
        {
            rouseStateText += "觉醒<color=#37c21b>" + equipCfg.name + "</color>";
        }

        foreach (int state in equipCfg.stateId)
        {
            BuffCfg buffCfg = BuffCfg.Get(state);
            rouseStateText += "，" + buffCfg.desc;
        }

        */
        if (equipCfg.stateId != null && equipCfg.stateId.Length > 0)
        {
            rouseStateText += "　<color=#37c21b>(已激活)</color>";
        }
        else
        {
            rouseStateText += "　<color=#cfcfcf>(未激活)</color>";
        }
        m_rouseState.text = rouseStateText;

        m_weaponName.text = string.Format("{0} 武器特性", curWeapon.Cfg.name);
        var hitPropCfg = curWeapon.Cfg.HitPropCfg;
        m_hitProp.text = string.Format("<color=#C7994C>打击属性</color>　{0} {1}", hitPropCfg!=null? hitPropCfg.name:"",hitPropCfg!=null? hitPropCfg.desc:"");

    }

    public void ResetShow()
    {
        if(m_btnShow!=null)
        {
            m_btnShow.SetState(0);
        }
    }

    #endregion

    #region Private Methods


    #endregion

}
