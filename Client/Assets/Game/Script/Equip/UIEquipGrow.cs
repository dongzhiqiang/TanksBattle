using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UIEquipGrowParam
{
    public bool m_isRouse;
    public Equip m_oldEquip;
    public Equip m_newEquip;
    public UIEquipGrowParam(bool isRouse, Equip oldEquip, Equip newEquip)
    {
        m_isRouse = isRouse;
        m_oldEquip = oldEquip;
        m_newEquip = newEquip;
    }
}

public class UIEquipGrow : UIPanel
{
    #region SerializeFields
    public GameObject m_oldStarObj;
    public GameObject m_newStarObj;
    public ImageEx m_oldBg;
    public ImageEx m_oldIcon;
    public TextEx m_oldName;
    public List<ImageEx> m_oldStars;
    public ImageEx m_newBg;
    public ImageEx m_newIcon;
    public TextEx m_newName;
    public List<ImageEx> m_newStars;
    public TextEx m_description;
    public StateGroup m_attributes;
    #endregion

    static PropertyTable m_oldProps = new PropertyTable();
    static PropertyTable m_newProps = new PropertyTable();

    class EquipAttributeItem
    {
        public string attributeName;
        public string attributeValue;
        public string addValue;
    }

        //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        UIEquipGrowParam uparam = (UIEquipGrowParam)param;
        Show(uparam.m_isRouse, uparam.m_oldEquip, uparam.m_newEquip);
    }

    void SetEquip(Equip equip, ImageEx icon, ImageEx bg, TextEx text, List<ImageEx> stars)
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
        icon.Set(equipCfg.icon);
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[equip.AdvLv];
        QualityCfg qualityCfg = QualityCfg.m_cfgs[advCfg.quality];
        bg.Set(qualityCfg.backgroundSquare);
        string name = equipCfg.name;
        if(advCfg.qualityLv>0)
        {
            name += "+" + advCfg.qualityLv;
        }
        text.text = name;
        text.color = QualityCfg.GetColor(advCfg.quality);
        for (int i = 0; i < stars.Count; i++)
        {
            if (equipCfg.star > i)
            {
                stars[i].gameObject.SetActive(true);
            }
            else
            {
                stars[i].gameObject.SetActive(false);
            }
        }
    }

    public void Show(bool isRouse, Equip oldEquip, Equip newEquip)
    {
        if(isRouse)
        {
            m_oldStarObj.SetActive(true);
            m_newStarObj.SetActive(true);
        }
        else
        {
            m_oldStarObj.SetActive(false);
            m_newStarObj.SetActive(false);
        }

        SetEquip(oldEquip, m_oldIcon, m_oldBg, m_oldName, m_oldStars);
        SetEquip(newEquip, m_newIcon, m_newBg, m_newName, m_newStars);

        oldEquip.GetBaseProp(m_oldProps);
        newEquip.GetBaseProp(m_newProps);


        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        if(oldEquip.Level!=newEquip.Level)
        {
            EquipAttributeItem attrItem = new EquipAttributeItem();
            attrItem.attributeName = "Lv."+oldEquip.Level;
            attrItem.attributeValue = "";
            attrItem.addValue = "Lv." + newEquip.Level;
            attrs.Add(attrItem);
        }
        for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
        {
            float value = m_oldProps.GetFloat(i);
            float addValue = 0;
            addValue = m_newProps.GetFloat(i) - value;
            if (value < Mathf.Epsilon && addValue <= Mathf.Epsilon)
            {
                continue;
            }
            PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
            EquipAttributeItem attrItem = new EquipAttributeItem();
            attrItem.attributeName = propTypeCfg.name;
            if (propTypeCfg.format == enPropFormat.FloatRate)
            {
                attrItem.attributeValue = String.Format("{0:F}", value * 100) + "%";
                attrItem.addValue = "(+" + String.Format("{0:F}", addValue * 100) + "%)";
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                attrItem.addValue = "(+" + Mathf.RoundToInt(addValue) + ")";
            }
            attrs.Add(attrItem);
        }
        m_attributes.SetCount(attrs.Count);
        for (int i = 0; i < attrs.Count; ++i)
        {
            m_attributes.Get<UIEquipAttributeItem>(i).Init(attrs[i].attributeName, attrs[i].attributeValue, attrs[i].addValue);
        }

        if (isRouse)
        {
            m_description.gameObject.SetActive(true);
            EquipCfg equipCfg = EquipCfg.m_cfgs[oldEquip.EquipId];
            m_description.text = equipCfg.rouseDescription;
        }
        else
        {
            m_description.gameObject.SetActive(false);
        }
    }
}