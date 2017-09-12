
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageEquipUpgrade : MonoBehaviour
{
    #region SerializeFields
    public Text m_name;
    public Text m_addText;
    public StateGroup m_attributes;
    public StateHandle m_btnUpgrade;
    public StateHandle m_btnUpgradeOnce;
    public StateHandle m_btnDress;
    public StateHandle m_btnAdvance;
    public StateHandle m_btnUpgradeAll;
    public StateGroup m_costItems;
    public GameObject m_goldPart;
    public Text m_gold;
    public Text m_needText;
    public Text m_maxText;
    public Text m_oldName;
    public Text m_oldState;
    public Text m_newState;
    public GameObject m_newStateObject;
    public ImageEx m_arrow;
    #endregion

    #region Fields
    UIPet m_parent;
    Equip m_equip;
    Role m_pet;
    Color m_oldStateColor;
    bool m_updateButtonLock = false;
    Color m_colorGoldNormal;
    Color m_colorRed = QualityCfg.ToColor("CE3535");
    GrowFxMgr m_upgradeFxMgr = new GrowFxMgr();
    GrowFxMgr m_advanceFxMgr = new GrowFxMgr();
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
        m_btnUpgrade.AddClick(OnUpgrade);
        m_btnUpgradeOnce.AddClick(OnUpgradeOnce);
        m_btnAdvance.AddClick(OnAdvance);
        m_btnUpgradeAll.AddClick(OnUpgradeAll);
    }

    class EquipAttributeItem
    {
        public string attributeName;
        public string attributeValue;
        public string addValue;
    }

    static PropertyTable baseProp = new PropertyTable();
    static PropertyTable lvUpProp = new PropertyTable();
    static PropertyTable lvProp = new PropertyTable();

    void SetName(Text text, EquipCfg equipCfg, int advLv)
    {
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[advLv];
        string name = equipCfg.name;
        if (advCfg.qualityLv > 0)
        {
            name += "+" + advCfg.qualityLv;
        }
        text.text = name;
        text.color = QualityCfg.GetColor(advCfg.quality);
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

    //显示
    public void OnOpenPage(Role pet, Equip equip)
    {
        m_updateButtonLock = false;
        m_pet = pet;
        m_equip = equip;
        if(equip == null)
        {
            return;
        }
        EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[equip.AdvLv];
        string name = equipCfg.name;
        bool isMax = equip.Level >= advCfg.maxLv;
        bool canAdv = equip.Level >= advCfg.needLv;
        //m_name.text = name;
        if (!isMax || canAdv)
        {
            m_addText.gameObject.SetActive(true);
            if (!isMax)
            {
                m_name.gameObject.SetActive(true);
                m_addText.gameObject.SetActive(false);
                m_oldName.gameObject.SetActive(false);
                SetName(m_name, equipCfg, equip.AdvLv);
                m_oldState.text = "Lv." + equip.Level;
                m_oldState.color = m_oldStateColor;
                m_newState.text = "Lv."+(equip.Level + 1);
                m_newState.color = m_oldStateColor;
                m_newStateObject.SetActive(true);
                m_arrow.gameObject.SetActive(true);
            }
            else
            {
                m_name.gameObject.SetActive(false);
                m_addText.gameObject.SetActive(true);
                m_oldName.gameObject.SetActive(true);
                SetName(m_oldName, equipCfg, equip.AdvLv);
                SetName(m_addText, equipCfg, equip.AdvLv + 1);
                m_oldState.text = GetQualityText(advCfg.quality, advCfg.qualityLv);
                m_oldState.color = QualityCfg.GetColor(advCfg.quality);
                EquipAdvanceRateCfg advUpCfg = EquipAdvanceRateCfg.m_cfgs[equip.AdvLv + 1];
                m_newState.text = GetQualityText(advUpCfg.quality, advUpCfg.qualityLv);
                m_newState.color = QualityCfg.GetColor(advUpCfg.quality);
                m_newStateObject.SetActive(true);
                m_arrow.gameObject.SetActive(true);
            }

        }
        else
        {
            m_name.gameObject.SetActive(true);
            m_addText.gameObject.SetActive(false);
            m_oldName.gameObject.SetActive(false);
            SetName(m_name, equipCfg, equip.AdvLv);
            m_oldState.text = "Lv." + equip.Level;
            m_oldState.color = m_oldStateColor;
            m_newStateObject.SetActive(false);
            m_arrow.gameObject.SetActive(false);
        }
        equip.GetBaseProp(baseProp);

        if (!isMax)
        {
            Equip.GetEquipBaseProp(lvUpProp, equipCfg, equip.Level + 1, equip.AdvLv, equipCfg.star);
        }
        else if (canAdv)
        {
            Equip.GetEquipBaseProp(lvUpProp, equipCfg, equip.Level, equip.AdvLv + 1, equipCfg.star);
        }

        m_upgradeFxMgr.m_attrAnis.Clear();
        m_advanceFxMgr.m_attrAnis.Clear();

        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
        {
            float value = baseProp.GetFloat(i);
            float addValue = 0;
            if (!isMax || canAdv)
            {
                addValue = lvUpProp.GetFloat(i) - value;
            }
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
                if (!isMax || canAdv)
                {
                    attrItem.addValue = "(+" + String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (!isMax || canAdv)
                {
                    attrItem.addValue = "(+" + (Mathf.RoundToInt(lvUpProp.GetFloat(i)) - Mathf.RoundToInt(value)) + ")";
                }
            }
            attrs.Add(attrItem);
        }
        m_attributes.SetCount(attrs.Count);
        for (int i = 0; i < attrs.Count; ++i)
        {
            m_attributes.Get<UIEquipAttributeItem>(i).Init(attrs[i].attributeName, attrs[i].attributeValue, attrs[i].addValue);
            m_upgradeFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
            m_advanceFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
        }

        // 消耗

        int costGold = 0;

        if (!isMax || canAdv)
        {
            List<CostItem> sourceCostItems;
            if (!isMax)
            {
                sourceCostItems = EquipUpgradeCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (equip.Level));
            }
            else
            {
                sourceCostItems = EquipAdvanceCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (equip.AdvLv), true);
            }
            List<CostItem> costItems = new List<CostItem>();
            // 提取出金币物品
            foreach (CostItem costItem in sourceCostItems)
            {
                if (costItem.itemId == ITEM_ID.GOLD)
                {
                    costGold = costItem.num;
                }
                else
                {
                    costItems.Add(costItem);
                }
            }
            m_advanceFxMgr.m_flyIconSources.Clear();
            m_costItems.gameObject.SetActive(true);
            m_costItems.SetCount(costItems.Count);
            for (int i = 0; i < costItems.Count; i++)
            {
                m_costItems.Get<UIEquipCostItem>(i).Init(costItems[i]);
                m_advanceFxMgr.m_flyIconSources.Add(m_costItems.Get<UIEquipCostItem>(i).m_icon);
            }
            if (costGold > 0)
            {
                m_goldPart.SetActive(true);
                m_gold.text = "" + costGold;
                m_gold.color = costGold > RoleMgr.instance.Hero.ItemsPart.GetGold() ? m_colorRed : m_colorGoldNormal;
            }
            else
            {
                m_goldPart.SetActive(false);
            }
            m_needText.gameObject.SetActive(true);
            m_maxText.gameObject.SetActive(false);
        }
        else
        {
            m_costItems.gameObject.SetActive(false);
            m_goldPart.SetActive(false);
            m_needText.gameObject.SetActive(false);
            m_maxText.gameObject.SetActive(true);
        }

        // 装备按钮
        if (true)//equipCfg.posIndex < enEquipPos.minWeapon || RoleMgr.instance.Hero.EquipsPart.CurrentWeapon == equipCfg.posIndex)
        {
            m_btnDress.gameObject.SetActive(false);
            if (!isMax)
            {
                m_btnUpgrade.gameObject.SetActive(true);
                m_btnUpgradeOnce.gameObject.SetActive(true);
                m_btnAdvance.gameObject.SetActive(false);
            }
            else if (canAdv)
            {
                m_btnUpgrade.gameObject.SetActive(false);
                m_btnUpgradeOnce.gameObject.SetActive(true);
                m_btnAdvance.gameObject.SetActive(true);
            }
            else
            {
                m_btnUpgrade.gameObject.SetActive(false);
                m_btnUpgradeOnce.gameObject.SetActive(false);
                m_btnAdvance.gameObject.SetActive(false);
            }
        }
            /*
        else
        {
            m_btnDress.gameObject.SetActive(true);
            m_btnUpgrade.gameObject.SetActive(false);
            m_btnUpgradeOnce.gameObject.SetActive(false);
            m_btnAdvance.gameObject.SetActive(false);
        }*/
    }

    public void StartUpgradeFx(Equip oldEquip, Equip newEquip)
    {
        UIPowerUp.SaveNewProp(m_pet);
        m_upgradeFxMgr.m_oldEquipData = oldEquip;
        m_upgradeFxMgr.m_newEquipData = newEquip;
        m_upgradeFxMgr.Start();
    }

    public void StartAdvanceFx(Equip oldEquip, Equip newEquip)
    {
        UIPowerUp.SaveNewProp(m_pet);
        m_advanceFxMgr.m_oldEquipData = oldEquip;
        m_advanceFxMgr.m_newEquipData = newEquip;
        m_advanceFxMgr.Start();
    }

    #endregion

    #region Private Methods

    void Awake()
    {
        m_oldStateColor = m_oldState.color;
        m_colorGoldNormal = m_gold.color;
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_updateArrowFx, true));
        List<GrowAct> actList2 = new List<GrowAct>();
        actList2.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_updateShineFx));
        List<GrowAct> actList3 = new List<GrowAct>();
        actList3.Add(new GrowActAttrTextShake());
        actList3.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_equipFx));
        List<GrowAct> actList4 = new List<GrowAct>();
        actList4.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh()));
        actList4.Add(new GrowActFx("fx_ui_zhuangbei_shengjichengong", new Vector3(0, 200, 0)));
        actList4.Add(new GrowActPowerUp(true, 3f));
        //actList2.Add(new GrowActEquipChangeFloat());
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList2, 0.2f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList3, 0.2f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList4, 0.5f));
        //m_upgradeFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();


        List<GrowAct> advActList1 = new List<GrowAct>();
        advActList1.Add(new GrowActFlyIcon());
        List<GrowAct> advActList2 = new List<GrowAct>();
        advActList2.Add(new GrowActAttrTextShake());
        advActList2.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_equipFx));

        List<GrowAct> advActList3 = new List<GrowAct>();
        advActList3.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh()));
        advActList3.Add(new GrowActFx("fx_ui_zhuangbei_jinjie"));

        List<GrowAct> advActList4 = new List<GrowAct>();
        advActList4.Add(new GrowActGrowUI(false));

        List<GrowAct> advActList5 = new List<GrowAct>();
        advActList5.Add(new GrowActFx("fx_ui_zhuangbei_jinjiechenggong", new Vector3(0, 200, 0)));
        advActList5.Add(new GrowActPowerUp(true, 3f));
        advActList5.Add(new GrowActPlaySound(113));

        //List<GrowAct> advActList3 = new List<GrowAct>();
        //advActList3.Add(new GrowActEquipChangeFloat());
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList1, 0.2f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList2, 0.2f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList3, 2.5f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList4, 0.5f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList5, 0.5f));
        m_advanceFxMgr.m_flyIconFx = GetComponentInParent<UIPet>().m_flyIconFx;
        //m_advanceFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    void OnUpgrade()
    {
        if (m_updateButtonLock)
        {
            return;
        }
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equip.EquipId];
        if(m_equip.Level >= m_pet.PropPart.GetInt(enProp.level))
        {
            UIMessage.Show("神侍等级不足");
            return;
        }
        // check items
        List<CostItem> costItems = EquipUpgradeCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_equip.Level));
        int needItemId;
        if(!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            ItemCfg itemCfg = ItemCfg.m_cfgs[needItemId];
            if(itemCfg.type == ITEM_TYPE.ABSTRACT_ITEM)
            {
                UIMessage.Show("所需" + itemCfg.name + "不足");
            }
            else
            {
                UIMessage.Show("所需材料" + itemCfg.name + "不足");
            }
            return;
        }
        UIPowerUp.SaveOldProp(m_pet);
        NetMgr.instance.EquipHandler.SendUpgrade(m_pet.EquipsPart.EquipRoleGUID, equipCfg.posIndex);
        m_updateButtonLock = true;
    }

    void OnUpgradeOnce()
    {
        if (m_updateButtonLock)
        {
            return;
        }
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[m_equip.AdvLv];
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equip.EquipId];
        // check items
        List<CostItem> costItems;
        bool canAdv = m_equip.Level >= advCfg.needLv;
        if (canAdv)
        {
            // check items
            costItems = EquipAdvanceCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_equip.AdvLv), true);
        }
        else
        {
            if (m_equip.Level >= m_pet.PropPart.GetInt(enProp.level))
            {
                UIMessage.Show("神侍等级不足");
                return;
            }
            // check items
            costItems = EquipUpgradeCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_equip.Level));

        }
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            ItemCfg itemCfg = ItemCfg.m_cfgs[needItemId];
            if (itemCfg.type == ITEM_TYPE.ABSTRACT_ITEM)
            {
                UIMessage.Show("所需" + itemCfg.name + "不足");
            }
            else
            {
                UIMessage.Show("所需材料" + itemCfg.name + "不足");
            }
            return;
        }
        UIPowerUp.SaveOldProp(m_pet);
        NetMgr.instance.EquipHandler.SendUpgradeOnce(m_pet.EquipsPart.EquipRoleGUID, equipCfg.posIndex);
        m_updateButtonLock = true;
    }

    void OnAdvance()
    {
        if (m_updateButtonLock)
        {
            return;
        }

        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equip.EquipId];
        // check items
        List<CostItem> costItems = EquipAdvanceCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_equip.AdvLv), true);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            ItemCfg itemCfg = ItemCfg.m_cfgs[needItemId];
            if (itemCfg.type == ITEM_TYPE.ABSTRACT_ITEM)
            {
                UIMessage.Show("所需" + itemCfg.name + "不足");
            }
            else
            {
                UIMessage.Show("所需材料" + itemCfg.name + "不足");
            }
            return;
        }
        UIPowerUp.SaveOldProp(m_pet);
        NetMgr.instance.EquipHandler.SendAdvance(m_pet.EquipsPart.EquipRoleGUID, equipCfg.posIndex);
        m_updateButtonLock = true;
    }

    public void ClearLock()
    {
        m_updateButtonLock = false;
    }

    void OnUpgradeAll()
    {
        if (m_updateButtonLock)
        {
            return;
        }
        UIPowerUp.SaveOldProp(m_pet);
        NetMgr.instance.EquipHandler.SendUpgradeAll(m_pet.EquipsPart.EquipRoleGUID);
        m_updateButtonLock = true;
    }

    #endregion

}
