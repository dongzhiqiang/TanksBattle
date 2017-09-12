using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UITreasureInfo : UIPanel
{
    #region SerializeFields
    public ImageEx m_icon;
    public Text m_name;
    public Text m_nameUp;
    public Text m_note;
    public Text m_desc;
    public Text m_pieceNum;
    public ImageEx m_percent;
    public UIItemIcon m_piece;
    public Text m_costGold;
    public StateHandle m_upgrade;
    public StateHandle m_battle;
    public StateHandle m_cancelBattle;
    public StateHandle m_battleState;
    public StateGroup m_attributes;
    public Text m_effect;
    public StateHandle m_treasureState;
    #endregion

    private Color m_goldColor;
    private Color m_red = QualityCfg.ToColor("CE3535");
    private int m_treasureId;


    class EquipAttributeItem
    {
        public string attributeName;
        public string attributeValue;
        public string addValue;
    }
    //初始化时调用
    public override void OnInitPanel()
    {
        m_upgrade.AddClick(OnUpgrade);
        m_battle.AddClick(OnBattle);
        m_cancelBattle.AddClick(OnCancelBattle);
        m_goldColor = m_costGold.color;
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_treasureId = (int)param;
        Reflesh();
    }

    public void Reflesh()
    {
        Role hero = RoleMgr.instance.Hero;
        Treasure treasure = hero.TreasurePart.GetTreasure(m_treasureId);
        int treasureLevel = 0;
        if(treasure != null)
        {
            treasureLevel = treasure.level;
            m_treasureState.SetState(0);
        }
        else
        {
            m_treasureState.SetState(1);
        }

        TreasureCfg treasureCfg = TreasureCfg.m_cfgs[m_treasureId];

        TreasureLevelCfg oldLevelCfg;
        if (treasureLevel > 0)
        {
            oldLevelCfg = TreasureLevelCfg.Get(m_treasureId, treasureLevel);
        }
        else
        {
            oldLevelCfg = TreasureLevelCfg.Get(m_treasureId, 1);
        }
        TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(m_treasureId, treasureLevel + 1);


        if (hero.ItemsPart.GetItemNum(treasureCfg.pieceId) < levelCfg.pieceNum)
        {
            m_upgrade.GetComponent<ImageEx>().SetGrey(true);
        }
        else
        {
            m_upgrade.GetComponent<ImageEx>().SetGrey(false);
        }

        bool isMax = false;
        if (levelCfg == null)
        {
            isMax = true;
            m_percent.fillAmount = 1;
            m_pieceNum.text = "最高";
            levelCfg = TreasureLevelCfg.Get(m_treasureId, treasureLevel);
            m_costGold.text = "0";
            m_costGold.color = m_goldColor;
            m_upgrade.gameObject.SetActive(false);
            m_nameUp.text = treasureCfg.name + " Lv." + treasureLevel;
        }
        else
        {
            float percent = (float)hero.ItemsPart.GetItemNum(treasureCfg.pieceId) / levelCfg.pieceNum;
            if (percent > 1) percent = 1;
            m_pieceNum.text = string.Format("{0}/{1}", hero.ItemsPart.GetItemNum(treasureCfg.pieceId), levelCfg.pieceNum);
            m_percent.fillAmount = percent;
            m_costGold.text = levelCfg.costGold.ToString();
            m_upgrade.gameObject.SetActive(true);
            m_nameUp.text = treasureCfg.name + " Lv." + (treasureLevel + 1);

            if (hero.ItemsPart.GetGold() < levelCfg.costGold)
            {
                m_costGold.color = m_red;
            }
            else
            {
                m_costGold.color = m_goldColor;
            }
        }

        m_icon.Set(treasureCfg.icon);
        m_name.text = treasureCfg.name + " Lv." + treasureLevel;
        m_note.text = treasureCfg.note;
        m_desc.text = treasureCfg.desc;
        m_piece.Init(treasureCfg.pieceId, 1);

        
        if(hero.TreasurePart.BattleTreasures.IndexOf(m_treasureId)>=0)
        {
            m_battleState.SetState(1);
        }
        else
        {
            if (hero.TreasurePart.BattleTreasures.Count>=3)
            {
                m_battleState.SetState(2);
            }
            else
            {
                m_battleState.SetState(0);
            }
         
        }

        m_effect.text = levelCfg.description;

        PropValueCfg valueCfg = PropValueCfg.Get(oldLevelCfg.attributeId);
        PropValueCfg newValueCfg = PropValueCfg.Get(levelCfg.attributeId);


        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for (enProp i = enProp.minFightProp + 1; i <= enProp.damage; i++)
        {
            float value = valueCfg.props.GetFloat(i);
            float addValue = 0;
            if (!isMax)
            {
                addValue = newValueCfg.props.GetFloat(i) - value;
            }
         
            if( value<Mathf.Epsilon && addValue <=Mathf.Epsilon)
            {
                continue;
            }
            PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
            EquipAttributeItem attrItem = new EquipAttributeItem();
            attrItem.attributeName = propTypeCfg.name;
            if (propTypeCfg.format == enPropFormat.FloatRate)
            {
                attrItem.attributeValue = String.Format("{0:F}", value * 100) + "%";
                if (!isMax && treasureLevel>0)
                {
                    attrItem.addValue = "(+" + String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (!isMax && treasureLevel > 0)
                {
                    attrItem.addValue = "(+" + Mathf.RoundToInt(addValue) + ")";
                }
            }
            attrs.Add(attrItem);
        }
        m_attributes.SetCount(attrs.Count);
        for (int i = 0; i < attrs.Count; ++i)
        {
            m_attributes.Get<UIEquipAttributeItem>(i).Init(attrs[i].attributeName, attrs[i].attributeValue, attrs[i].addValue);
        }
    }

    void OnUpgrade()
    {
        Role hero = RoleMgr.instance.Hero;
        Treasure treasure = hero.TreasurePart.GetTreasure(m_treasureId);
        int treasureLevel = 0;
        if (treasure != null)
        {
            treasureLevel = treasure.level;
        }

        TreasureCfg treasureCfg = TreasureCfg.m_cfgs[m_treasureId];

        TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(m_treasureId, treasureLevel + 1);

        if( hero.ItemsPart.GetItemNum(treasureCfg.pieceId) < levelCfg.pieceNum)
        {
            UIMessage.Show("神器碎片不足");
            return;
        }

       if( hero.ItemsPart.GetGold() < levelCfg.costGold )
       {
           UIMessage.Show("金币不足");
           return;
       }

       NetMgr.instance.TreasureHandler.SendUpgradeTreasure(m_treasureId);
    }

    void OnBattle()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.TreasurePart.SetBattle(m_treasureId);
    }

    void OnCancelBattle()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.TreasurePart.CancelBattle(m_treasureId);
    }
}