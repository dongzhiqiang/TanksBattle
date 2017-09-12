
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageEquipRouse : MonoBehaviour
{
    #region SerializeFields
    public Text m_name;
    public Text m_addText;
    public StateGroup m_attributes;
    public StateHandle m_btnRouse;
    public StateGroup m_costItems;
    //public GameObject m_goldPart;
    //public Text m_gold;
    public Text m_needText;
    public Text m_maxText;
    public List<ImageEx> m_oldStars;
    public List<ImageEx> m_newStars;
    public GameObject m_newStarsObject;
    public ImageEx m_arrow;
    public Text m_rouseDesciption;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    Equip m_equip;
    bool m_rouseButtonLock = false;
    GrowFxMgr m_rouseFxMgr = new GrowFxMgr();
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
        m_btnRouse.AddClick(OnRouse);
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

    //显示
    public void OnOpenPage(Role pet, Equip equip)
    {
        m_rouseButtonLock = false;
        m_pet = pet;
        m_equip = equip;
        if(equip == null)
        {
            return;
        }
        EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
        string name = equipCfg.name;
        SetName(m_name, equipCfg, equip.AdvLv);
        int rouseTarget = 0;
        rouseTarget = equipCfg.rouseEquipId;
        if (EquipCfg.m_cfgs.ContainsKey(rouseTarget))
        {
            EquipCfg targetCfg = EquipCfg.m_cfgs[rouseTarget];
            if (targetCfg.posIndex != equipCfg.posIndex)
            {
                Debug.LogError("觉醒装备填写错误:" + equipCfg.id + "->" + targetCfg.id);
                rouseTarget = 0;
            }
        }
        else
        {
            rouseTarget = 0;
        }
        for (int i = 0; i < m_oldStars.Count; i++)
        {
            if (equipCfg.star > i)
            {
                m_oldStars[i].gameObject.SetActive(true);
            }
            else
            {
                m_oldStars[i].gameObject.SetActive(false);
            }
        }
        if (rouseTarget != 0)
        {
            EquipCfg equipRouseCfg = EquipCfg.m_cfgs[equipCfg.rouseEquipId];
            m_addText.gameObject.SetActive(true);
            SetName(m_addText, equipRouseCfg, equip.AdvLv);
            m_newStarsObject.SetActive(true);
            m_arrow.gameObject.SetActive(true);
            for (int i = 0; i < m_newStars.Count; i++)
            {
                if (equipRouseCfg.star > i)
                {
                    m_newStars[i].gameObject.SetActive(true);
                }
                else
                {
                    m_newStars[i].gameObject.SetActive(false);
                }
            }
            //m_rouseDesciption.gameObject.SetActive(true);
            m_rouseDesciption.text = equipCfg.rouseDescription;
        }
        else
        {
            m_addText.gameObject.SetActive(false);
            m_newStarsObject.SetActive(false);
            m_arrow.gameObject.SetActive(false);
            //m_rouseDesciption.gameObject.SetActive(false);
            m_rouseDesciption.text = equipCfg.rouseDescription;
        }
        equip.GetBaseProp(baseProp);

        if (rouseTarget != 0)
        {
            Equip.GetEquipBaseProp(lvUpProp, equipCfg, equip.Level, equip.AdvLv, equipCfg.star + 1);
        }

        m_rouseFxMgr.m_attrAnis.Clear();

        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for(enProp i=enProp.minFightProp+1; i<enProp.maxFightProp; i++)
        {
            float value = baseProp.GetFloat(i);
            float addValue = 0;
            if (rouseTarget != 0)
            {
                addValue = lvUpProp.GetFloat(i) - value;
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
                if (rouseTarget != 0)
                {
                    attrItem.addValue = "(+" + String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (rouseTarget != 0)
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
            m_rouseFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
        }

        // 消耗

        int costGold = 0;

        if (rouseTarget != 0)
        {
            List<CostItem> sourceCostItems;
            sourceCostItems = EquipRouseCostCfg.GetCost(equipCfg.rouseCostId);

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
            m_rouseFxMgr.m_flyIconSources.Clear();
            m_costItems.gameObject.SetActive(true);
            m_costItems.SetCount(costItems.Count);
            for (int i = 0; i < costItems.Count; i++)
            {
                m_costItems.Get<UIEquipCostItem>(i).Init(costItems[i]);
                m_rouseFxMgr.m_flyIconSources.Add(m_costItems.Get<UIEquipCostItem>(i).m_icon);
            }
            /*
            if(costGold>0)
            {
                m_goldPart.SetActive(true);
                m_gold.text = "" + costGold;
            }
            else
            {
                m_goldPart.SetActive(false);
            }
             */
            m_needText.gameObject.SetActive(true);
            m_maxText.gameObject.SetActive(false);
        }
        else
        {
            m_costItems.gameObject.SetActive(false);
            //m_goldPart.SetActive(false);
            m_needText.gameObject.SetActive(false);
            m_maxText.gameObject.SetActive(true);
        }

        if (rouseTarget != 0)
        {
            m_btnRouse.gameObject.SetActive(true);
        }
        else
        {
            m_btnRouse.gameObject.SetActive(false);
        }
    }

    public void StartRouseFx(Equip oldEquip, Equip newEquip)
    {
        UIPowerUp.SaveNewProp(m_pet);
        m_rouseFxMgr.m_oldEquipData = oldEquip;
        m_rouseFxMgr.m_newEquipData = newEquip;
        m_rouseFxMgr.Start();
    }


    #endregion

    #region Private Methods


    void OnRouse()
    {
        if (m_rouseButtonLock)
        {
            return;
        }
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equip.EquipId];
        // check items
        List<CostItem> costItems = EquipRouseCostCfg.GetCost(equipCfg.rouseCostId);
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
        NetMgr.instance.EquipHandler.SendRouse(m_pet.EquipsPart.EquipRoleGUID, equipCfg.posIndex);
        m_rouseButtonLock = true;
    }

    void Awake()
    {
        List<GrowAct> rouseActList1 = new List<GrowAct>();
        rouseActList1.Add(new GrowActFlyIcon());
        List<GrowAct> rouseActList2 = new List<GrowAct>();
        rouseActList2.Add(new GrowActAttrTextShake());
        rouseActList2.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_equipFx));

        List<GrowAct> rouseActList3 = new List<GrowAct>();
        rouseActList3.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh()));
        rouseActList3.Add(new GrowActFx("fx_ui_zhuangbei_jinjie"));

        List<GrowAct> rouseActList4 = new List<GrowAct>();
        rouseActList4.Add(new GrowActGrowUI(true));

        List<GrowAct> rouseActList5 = new List<GrowAct>();
        rouseActList5.Add(new GrowActFx("fx_ui_zhuangbei_juexingchenggong", new Vector3(0, 200, 0)));
        rouseActList5.Add(new GrowActPowerUp(true, 3f));
        rouseActList5.Add(new GrowActPlaySound(112));
        //List<GrowAct> advActList3 = new List<GrowAct>();
        //advActList3.Add(new GrowActEquipChangeFloat());
        m_rouseFxMgr.m_actList.Add(new GrowActBatch(rouseActList1, 0.2f));
        m_rouseFxMgr.m_actList.Add(new GrowActBatch(rouseActList2, 0.2f));
        m_rouseFxMgr.m_actList.Add(new GrowActBatch(rouseActList3, 2.5f));
        m_rouseFxMgr.m_actList.Add(new GrowActBatch(rouseActList4, 0.5f));
        m_rouseFxMgr.m_actList.Add(new GrowActBatch(rouseActList5, 0.5f));
        //m_advanceFxMgr.m_actList.Add(new GrowActBatch(advActList3, 0.5f));
        m_rouseFxMgr.m_flyIconFx = GetComponentInParent<UIPet>().m_flyIconFx;
        //m_rouseFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    #endregion

}
