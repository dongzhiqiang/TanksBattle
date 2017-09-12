
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageAttribute : MonoBehaviour
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
    public Text m_positioning;
    public Text m_quality;
    public StateGroup m_bonds;
    public StateHandle m_btnShow;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    static PropertyTable addProp = new PropertyTable();
    #endregion

    #region Properties

    #endregion

    #region Frame
    void SetQuality(Text text, PetAdvLvPropRateCfg cfg)
    {
        text.text = GetQualityText(cfg.quality, cfg.qualityLevel);
        text.color = QualityCfg.GetColor(cfg.quality);
    }

    string GetQualityText(int quality, int qualityLevel)
    {
        string text = QualityCfg.m_cfgs[quality].name;
        if (qualityLevel > 0)
        {
            text = text + "+" + qualityLevel;
        }
        return text;
    }

    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
    }

    //显示
    public void OnOpenPage(Role pet)
    {
        m_pet = pet;
        // 刷新属性
        RefleshAttribute();
    }

    void RefleshAttribute()
    {
        PropPart propPart = m_pet.PropPart;
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
        m_positioning.text = m_pet.Cfg.positioning;
        SetQuality(m_quality, PetAdvLvPropRateCfg.m_cfgs[propPart.GetInt(enProp.advLv)]);
        RefleshBonds();
    }

    void RefleshBonds()
    {
        RoleCfg roleCfg = RoleCfg.Get(m_pet.GetString(enProp.roleId));
        m_bonds.SetCount(roleCfg.petBonds.Count);
        for(int i=0;i<roleCfg.petBonds.Count;i++)
        {
            m_bonds.Get<UIPetBondItem>(i).Init(roleCfg.petBonds[i], m_pet.GetString(enProp.roleId));
        }
    }

    public void ResetShow()
    {
        if (m_btnShow != null)
        {
            m_btnShow.SetState(0);
        }
    }

    #endregion

    #region Private Methods


    #endregion

}
