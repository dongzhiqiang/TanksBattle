
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageUpgrade : MonoBehaviour
{
    #region SerializeFields
    public Text m_addText;
    public StateGroup m_attributes;
    public StateGroup m_expItems;
    public GameObject m_needText;
    public Text m_maxText;
    public Text m_oldName;
    public ImageEx m_arrow;
    public Text m_expText;
    public ImageEx m_expBar;
    public GameObject m_upgradeFx;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    GrowFxMgr m_upgradeFxMgr = new GrowFxMgr();
    bool m_upgradeFlag = false;
    int m_virtualExp = 0;
    int m_virtualExp2 = 0;
    int m_oldExp = 0;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
    }

    class EquipAttributeItem
    {
        public string attributeName;
        public string attributeValue;
        public string addValue;
    }

    static PropertyTable baseProp = new PropertyTable();
    static PropertyTable upProp = new PropertyTable();


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
    public void OnOpenPage(Role pet, bool fullReflesh = true)
    {
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
        bool isMax = pet.PropPart.GetInt(enProp.level) >= ConfigValue.GetInt("maxPetLevel");

        PetAdvLvPropRateCfg advCfg = PetAdvLvPropRateCfg.m_cfgs[advLv];
        //m_name.text = name;

        if (!isMax)
        {
            m_addText.gameObject.SetActive(true);
            m_addText.text = "Lv." + (lv + 1);
            m_addText.color = QualityCfg.GetColor(advCfg.quality);
            m_arrow.gameObject.SetActive(true);
        }
        else
        {
            m_addText.gameObject.SetActive(false);
            m_arrow.gameObject.SetActive(false);
        }
        m_oldName.text = "Lv." + lv;
        m_oldName.color = QualityCfg.GetColor(advCfg.quality);

        //计算升星前后的属性用于显示
        roleCfg.GetBaseProp(baseProp, lv, advLv, star);
        if (!isMax)
        {
            roleCfg.GetBaseProp(upProp, lv+1, advLv, star);
        }

        m_upgradeFxMgr.m_attrAnis.Clear();

        List<EquipAttributeItem> attrs = new List<EquipAttributeItem>();
        for (enProp i = enProp.minFightProp + 1; i <= enProp.damage; i++)
        {
            float value = baseProp.GetFloat(i);
            float addValue = 0;
            if (!isMax)
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
                if (!isMax)
                {
                    attrItem.addValue = "(+"+String.Format("{0:F}", addValue * 100) + "%)";
                }
            }
            else
            {
                attrItem.attributeValue = "" + Mathf.RoundToInt(value);
                if (!isMax)
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
            m_upgradeFxMgr.m_attrAnis.Add(m_attributes.Get<UIEquipAttributeItem>(i).m_addValue.GetComponent<SimpleHandle>());
        }

        // 消耗
        UpdateExpItems(fullReflesh);

        if (m_oldExp != pet.PropPart.GetInt(enProp.exp))
        {
            m_virtualExp2 = 0;
            m_oldExp = pet.PropPart.GetInt(enProp.exp);
        }

        if (!isMax)
        {
            //Debuger.Log("a" + pet.PropPart.GetInt(enProp.exp) + "+" + m_virtualExp + "+" + m_virtualExp2);
            int needExp = PetUpgradeCostCfg.GetCostExp(roleCfg.upgradeCostId, lv);
            int newExp = pet.PropPart.GetInt(enProp.exp) + m_virtualExp + m_virtualExp2;
            float expRate = ((float)pet.PropPart.GetInt(enProp.exp)) / needExp;
            if (expRate > 1) expRate = 1;
            m_expBar.fillAmount = expRate;
            m_expText.text = pet.PropPart.GetInt(enProp.exp)+"/"+needExp;
            m_expItems.gameObject.SetActive(true);
            m_needText.gameObject.SetActive(true);
            m_maxText.gameObject.SetActive(false);
        }
        else
        {
            m_expBar.fillAmount = 0;
            m_expText.text = "0/0";
            m_expItems.gameObject.SetActive(false);
            m_needText.gameObject.SetActive(false);
            m_maxText.gameObject.SetActive(true);
        }

        
    }

    public bool CanVirtualAddExp()
    {
        if (m_pet.GetInt(enProp.level) >= m_pet.Parent.GetInt(enProp.level))
        {
            UIMessage.Show("神侍等级不能超过角色等级");
            return false;
        }
        return true;
    }

    public void VirtualAddExp(int exp)
    {
        RoleCfg roleCfg = m_pet.Cfg;

        bool isUpgrade = false;
        int newExp = m_pet.PropPart.GetInt(enProp.exp) + exp + m_virtualExp2;
        int needExp = PetUpgradeCostCfg.GetCostExp(roleCfg.upgradeCostId, m_pet.GetInt(enProp.level));
        if (newExp >= needExp) UIPowerUp.SaveOldProp(m_pet);
        while (newExp >= needExp)
        {

            
            m_pet.AddInt(enProp.level, 1);
            newExp -= needExp;
            isUpgrade = true;
            if (m_pet.GetInt(enProp.level) >= m_pet.Parent.GetInt(enProp.level))
            {
                break;
            }

            needExp = PetUpgradeCostCfg.GetCostExp(roleCfg.upgradeCostId, m_pet.GetInt(enProp.level));
        }
        m_pet.SetInt(enProp.exp, newExp);
        
        if(isUpgrade)
        {
            m_pet.PropPart.FreshBaseProp();
            var uiPet = UIMgr.instance.Get<UIPet>();
            if (uiPet != null)
            {
                uiPet.m_pageUpgrade.StartUpgradeFx();
            }       
        }
        else
        {
            //OnOpenPage(m_pet, false);
            float expRate = ((float)m_pet.PropPart.GetInt(enProp.exp)) / needExp;
            if (expRate > 1) expRate = 1;
            m_expBar.fillAmount = expRate;
            m_expText.text = m_pet.PropPart.GetInt(enProp.exp) + "/" + needExp;
        }
    }


    public void UpdateExpItems(bool fleshAll)
    {
        List<int> expItems = new List<int>();
        foreach(ItemCfg itemCfg in ItemCfg.m_cfgs.Values)
        {
            if(itemCfg.type == ITEM_TYPE.EXP_ITEM)
            {
                expItems.Add(itemCfg.id);
            }
        }
        expItems.Sort();
        m_expItems.SetCount(expItems.Count);
        for (int i = 0; i < expItems.Count; i++)
        {
            m_expItems.Get<UIPetExpItem>(i).Init(expItems[i], this, fleshAll);
        }
    }


    public bool UpgradePet( int itemId, int num )
    {


        //RoleCfg roleCfg = m_pet.Cfg;
        //int needExp = PetUpgradeCostCfg.GetCostExp(roleCfg.upgradeCostId, m_pet.GetInt(enProp.level));
        //if (needExp <= m_pet.PropPart.GetInt(enProp.exp) + m_virtualExp + m_virtualExp2)



        NetMgr.instance.PetHandler.SendUpgradePet(m_pet.GetString(enProp.guid), itemId, num);
       // m_virtualExp2 += m_virtualExp;
        //m_virtualExp = 0;
        //Debuger.Log("c" + m_pet.PropPart.GetInt(enProp.exp) + "+" + m_virtualExp + "+" + m_virtualExp2);
        return true;
    }

    /*
    public void LateUpgradeFx()
    {
        m_upgradeFlag = true;
    }

    public void CheckUpgradeFx()
    {
        if(m_upgradeFlag)
        {
            StartUpgradeFx();
        }
    }*/

    public void StartUpgradeFx()
    {
        UIPowerUp.SaveNewProp(m_pet);
        //m_expBar.fillAmount = 1;
        m_upgradeFxMgr.Start();
        m_upgradeFlag = false;
    }

    void Awake()
    {
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFxPlay(m_upgradeFx));
        //List<GrowAct> actList2 = new List<GrowAct>();
        // actList2.Add(new GrowActFxPlay(GetComponentInParent<UIEquip>().m_updateShineFx));
        List<GrowAct> actList2 = new List<GrowAct>();
        actList2.Add(new GrowActAttrTextShake());
        actList2.Add(new GrowActFxPlay(GetComponentInParent<UIPet>().m_upgradeFx));
        List<GrowAct> actList3 = new List<GrowAct>();
        actList3.Add(new GrowActFunc(() => UIMgr.instance.Get<UIPet>().Refresh2(false)));
        actList3.Add(new GrowActFx("fx_ui_shensi_shengjichenggong", new Vector3(0, 200, 0)));
        List<GrowAct> actList4 = new List<GrowAct>();
        actList4.Add(new GrowActPowerUp(true, 3f));
        //actList2.Add(new GrowActEquipChangeFloat());
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList2, 0.2f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList3, 0.5f));
        m_upgradeFxMgr.m_actList.Add(new GrowActBatch(actList4, 0.5f));

        //m_upgradeFxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    #endregion

    #region Private Methods





    #endregion

}
