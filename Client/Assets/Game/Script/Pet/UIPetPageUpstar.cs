
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageUpstar : MonoBehaviour
{
    #region SerializeFields
    public StateGroup m_attributes;
    public StateHandle m_btnUpstar;
    public StateGroup m_costItems;
    public GameObject m_goldPart;
    public Text m_gold;
    public GameObject m_needText;
    public Text m_maxText;
    public List<ImageEx> m_starCovers;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    Color m_oldStateColor;
    bool m_buttonLock = false;
    Color m_colorGoldNormal;
    Color m_colorRed = QualityCfg.ToColor("CE3535");
    GrowFxMgr m_upstarFxMgr = new GrowFxMgr();
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
        m_btnUpstar.AddClick(OnUpstar);
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
        bool isMaxStar = star >= roleCfg.maxStar;

        //计算升星前后的属性用于显示
        roleCfg.GetBaseProp(baseProp, lv, advLv, star);
        if (!isMaxStar)
        {
            roleCfg.GetBaseProp(upProp, lv, advLv, star+1);
        }

        for (int i = 0; i < m_starCovers.Count; i++)
        {
            if(i<star)
            {
                m_starCovers[i].color = Color.clear;
            }
            else
            {
                m_starCovers[i].color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        m_upstarFxMgr.m_attrAnis.Clear();

        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for (enProp i = enProp.minFightProp + 1; i <= enProp.damage; i++)
        {
            float value = baseProp.GetFloat(i);
            float addValue = 0;
            if (!isMaxStar)
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
                if (!isMaxStar)
                {
                    attrItem.addValue = "(+"+String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (!isMaxStar)
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
            m_upstarFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
        }

        // 消耗
        
        int costGold = 0;

        if (!isMaxStar)
        { 
            List<CostItem> sourceCostItems;
            sourceCostItems = PetUpstarCostCfg.GetCost(roleCfg.upstarCostId, star);
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
            m_upstarFxMgr.m_flyIconSources.Clear();
            m_costItems.gameObject.SetActive(true);
            m_costItems.SetCount(costItems.Count);
            for (int i = 0; i < costItems.Count;i++ )
            {
                m_costItems.Get<UIEquipCostItem>(i).Init(costItems[i]);
                m_upstarFxMgr.m_flyIconSources.Add(m_costItems.Get<UIEquipCostItem>(i).m_icon);
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
        if (!isMaxStar)
        {
            m_btnUpstar.gameObject.SetActive(true);
        }
        else
        {
            m_btnUpstar.gameObject.SetActive(false);
        }
    }

    public void StartUpstarFx(int newStar)
    {
        UIPowerUp.SaveNewProp(m_pet);
        m_upstarFxMgr.m_newStar = newStar;
        m_upstarFxMgr.Start();
    }

    #endregion

    #region Private Methods

    void Awake()
    {
        m_colorGoldNormal = m_gold.color;

        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFlyIcon());
        List<GrowAct> actList2 = new List<GrowAct>();
        actList2.Add(new GrowActStarPlay());
        List<GrowAct> actList3 = new List<GrowAct>();
        actList3.Add(new GrowActAttrTextShake());
        actList3.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_upstarFx));
        List<GrowAct> actList4 = new List<GrowAct>();
        actList4.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh()));
        actList4.Add(new GrowActFx("fx_ui_shensi_shengxinchenggong", new Vector3(0, 200, 0)));
        actList4.Add(new GrowActPowerUp(true, 3f));
        actList4.Add(new GrowActPlaySound(112));
        //actList2.Add(new GrowActEquipChangeFloat());
        m_upstarFxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        m_upstarFxMgr.m_actList.Add(new GrowActBatch(actList2, 0.2f));
        m_upstarFxMgr.m_actList.Add(new GrowActBatch(actList3, 0.2f));
        m_upstarFxMgr.m_actList.Add(new GrowActBatch(actList4, 0.5f));
        m_upstarFxMgr.m_stars = GetComponentInParent<UIPet>().m_starFxs;
        m_upstarFxMgr.m_flyIconFx = GetComponentInParent<UIPet>().m_flyIconFxPet;
        //m_upstarFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    void OnUpstar()
    {
        if (m_buttonLock)
        {
            return;
        }
        RoleCfg roleCfg = RoleCfg.Get(m_pet.GetString(enProp.roleId));
        // check items
        List<CostItem> costItems = PetUpstarCostCfg.GetCost(roleCfg.upstarCostId, m_pet.PropPart.GetInt(enProp.star));
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
        NetMgr.instance.PetHandler.SendUpstarPet(m_pet.GetString(enProp.guid));
        m_buttonLock = true;
    }


    #endregion

}
