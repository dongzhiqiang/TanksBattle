
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageAdvance : MonoBehaviour
{
    #region SerializeFields
    public Text m_addText;
    public StateGroup m_attributes;
    public StateHandle m_btnAdvance;
    public StateGroup m_costItems;
    public GameObject m_goldPart;
    public Text m_gold;
    public GameObject m_needText;
    public Text m_maxText;
    public Text m_oldName;
    public ImageEx m_arrow;
    public Text m_needLv;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    Color m_oldStateColor;
    bool m_buttonLock = false;
    Color m_colorGoldNormal;
    Color m_colorRed = QualityCfg.ToColor("CE3535");
    GrowFxMgr m_advanceFxMgr = new GrowFxMgr();
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
        m_btnAdvance.AddClick(OnAdvance);
    }

    class EquipAttributeItem
    {
        public string attributeName;
        public string attributeValue;
        public string addValue;
    }

    static PropertyTable baseProp = new PropertyTable();
    static PropertyTable upProp = new PropertyTable();

    void SetQuality(Text text, PetAdvLvPropRateCfg cfg)
    {
        text.text = GetQualityText(cfg.quality, cfg.qualityLevel);
        text.color = QualityCfg.GetColor(cfg.quality);
    }

    string GetQualityText(int quality, int qualityLevel)
    {
        string text = QualityCfg.m_cfgs[quality].name;
        if( qualityLevel > 0 )
        {
            text = text + "+" + qualityLevel;
        }
        return text;
    }

    //显示
    public void OnOpenPage(Role pet)
    {
        m_buttonLock = false;
        m_pet = pet;
        if (pet == null)
        {
            return;
        }
        RoleCfg roleCfg = pet.Cfg;
        string name = roleCfg.name;
        int lv = pet.GetInt(enProp.level);
        int advLv = pet.GetInt(enProp.advLv);
        int star = pet.GetInt(enProp.star);
        bool isMaxAdvLv = pet.PropPart.GetInt(enProp.advLv) >= roleCfg.maxAdvanceLevel;
        

        if (!isMaxAdvLv)
        {
            m_addText.gameObject.SetActive(true);
            SetQuality(m_oldName, PetAdvLvPropRateCfg.m_cfgs[advLv]);
            SetQuality(m_addText, PetAdvLvPropRateCfg.m_cfgs[advLv + 1]);
            m_arrow.gameObject.SetActive(true);
        }
        else
        {
            m_addText.gameObject.SetActive(false);
            SetQuality(m_oldName, PetAdvLvPropRateCfg.m_cfgs[advLv]);
            m_arrow.gameObject.SetActive(false);
        }

        //计算进阶前后的属性用于显示
        roleCfg.GetBaseProp(baseProp, lv, advLv, star);
        if (!isMaxAdvLv)
        {
            roleCfg.GetBaseProp(upProp, lv, advLv + 1, star);
        }

        m_advanceFxMgr.m_attrAnis.Clear();

        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for (enProp i = enProp.minFightProp + 1; i <= enProp.damage; i++)
        {
            float value = baseProp.GetFloat(i);
            float addValue = 0;
            if (!isMaxAdvLv)
            {
                addValue = upProp.GetFloat(i) - value;
            }
            value = m_pet.PropPart.GetFloat(i);
            //if( value<Mathf.Epsilon && addValue <=Mathf.Epsilon)
            //{
            //    continue;
            //}
            PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
            EquipAttributeItem attrItem = new EquipAttributeItem();
            attrItem.attributeName = propTypeCfg.name;
            if(propTypeCfg.format==enPropFormat.FloatRate)
            {
                attrItem.attributeValue = String.Format("{0:F}", value * 100) + "%";
                if (!isMaxAdvLv)
                {
                    attrItem.addValue = "(+"+String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (!isMaxAdvLv)
                {
                    attrItem.addValue = "(+" + Mathf.RoundToInt(addValue)+")";
                }
            }
            attrs.Add(attrItem);
        }
        m_attributes.SetCount(attrs.Count);
        for (int i = 0; i < attrs.Count; ++i)
        {
            m_attributes.Get<UIEquipAttributeItem>(i).Init(attrs[i].attributeName, attrs[i].attributeValue, attrs[i].addValue);
            m_advanceFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
        }

        // 消耗
        
        int costGold = 0;

        if (!isMaxAdvLv)
        { 
            List<CostItem> sourceCostItems;
            sourceCostItems = PetAdvanceCostCfg.GetCost(roleCfg.advanceCostId, advLv);
            List<CostItem> costItems = new List<CostItem>();
            // 提取出金币物品
            foreach(CostItem costItem in sourceCostItems)
            {
                ItemCfg itemCfg = ItemCfg.m_cfgs[costItem.itemId];
                if(itemCfg.type == ITEM_TYPE.ABSTRACT_ITEM)
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
            for (int i = 0; i < costItems.Count;i++ )
            {
                m_costItems.Get<UIEquipCostItem>(i).Init(costItems[i]);
                m_advanceFxMgr.m_flyIconSources.Add(m_costItems.Get<UIEquipCostItem>(i).m_icon);
            }
            if(costGold>0)
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
        if (!isMaxAdvLv)
        {
            if (PetAdvLvPropRateCfg.m_cfgs[advLv].needLv > lv)
            {
                m_needLv.text = "需要神侍等级" + PetAdvLvPropRateCfg.m_cfgs[advLv].needLv + "级";
                m_needLv.gameObject.SetActive(true);
                m_btnAdvance.gameObject.SetActive(false);
            }
            else
            {
                m_needLv.gameObject.SetActive(false);
                m_btnAdvance.gameObject.SetActive(true);
            }
        }
        else
        {
            m_btnAdvance.gameObject.SetActive(false);
            m_needLv.gameObject.SetActive(false);
        }
    }

    public void StartAdvanceFx()
    {
        UIPowerUp.SaveNewProp(m_pet);
        m_advanceFxMgr.Start();
    }


    #endregion

    #region Private Methods

    void Awake()
    {
        m_colorGoldNormal = m_gold.color;
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFlyIcon());
        List<GrowAct> actList3 = new List<GrowAct>();
        actList3.Add(new GrowActAttrTextShake());
        actList3.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_advanceFx));
        List<GrowAct> actList4 = new List<GrowAct>();
        actList4.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh()));
        actList4.Add(new GrowActFx("fx_ui_shensi_jinjiechengong", new Vector3(0, 200, 0)));
        actList4.Add(new GrowActPowerUp(true, 3f));
        actList4.Add(new GrowActPlaySound(113));
        //actList2.Add(new GrowActEquipChangeFloat());
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(actList3, 0.2f));
        m_advanceFxMgr.m_actList.Add(new GrowActBatch(actList4, 0.5f));
        m_advanceFxMgr.m_stars = GetComponentInParent<UIPet>().m_starFxs;
        m_advanceFxMgr.m_flyIconFx = GetComponentInParent<UIPet>().m_flyIconFxPet;
        //m_advanceFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    void OnAdvance()
    {
        if (m_buttonLock)
        {
            return;
        }
        RoleCfg roleCfg = RoleCfg.Get(m_pet.GetString(enProp.roleId));
        // check items
        List<CostItem> costItems = PetAdvanceCostCfg.GetCost(roleCfg.advanceCostId, m_pet.PropPart.GetInt(enProp.advLv));
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
        NetMgr.instance.PetHandler.SendAdvancePet(m_pet.GetString(enProp.guid));
        m_buttonLock = true;
    }



    #endregion

}
