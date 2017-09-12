using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPet : UIPanel
{
    #region SerializeFields
    //public BowListView m_petListView;
    public BowListView m_equipListView;
    public StateGroup m_top;
    public StateGroup m_equipNav;
    public StateGroup m_petNav;
    public UIPetPageAttribute m_pageAttribute;
    public UIPetPageEquipUpgrade m_pageEquipUpgrade;
    public UIPetPageEquipRouse m_pageEquipRouse;
    public UIPetPageAdvance m_pageAdvance;
    public UIPetPageUpstar m_pageUpstar;
    public UIPetPageUpgrade m_pageUpgrade;
    public UIPetPageSkill m_pageSkill;
    public UIPetPageTalent m_pageTalent;
    public StateHandle m_arrow;
    public StateHandle m_arrowUp;
    public List<ImageEx> m_starCovers;
    public Text m_textLevel;
    public Text m_textName;
    public UI3DView m_petMod;
    public StateHandle m_petDown;
    public StateHandle m_petUp;
    public StateHandle m_petLeft;
    public StateHandle m_petRight;
    //public StateHandle m_allPet;
    public GameObject m_equipOpUpgrade;
    public GameObject m_equipOpRouse;
    public GameObject m_petOpUpgrade;
    public GameObject m_petOpAdvance;
    public GameObject m_petOpUpstar;
    public GameObject m_petOpSkill;
    public GameObject m_petOpTalent;
    public GameObject m_petOp;
    public GameObject m_equipOp;
    public GameObject m_allPetOp;
    public List<GameObject> m_starFxs;
    public FlyIconFx m_flyIconFx;
    public FlyIconFx m_flyIconFxPet;
    public GameObject m_upgradeFx;
    public GameObject m_advanceFx;
    public GameObject m_upstarFx;
    public GameObject m_equipFx;
    public GameObject m_updateArrowFx;
    public GameObject m_updateShineFx;
    public Text m_power;
    public GameObject m_upgradeOnceOp;
    public StateHandle m_petAdvance;
    public StateHandle m_petTalent;
    public Text m_gold;
    public StateHandle m_addGold;
    #endregion

    private Role m_pet;
    private List<Role> m_petList;
    private int m_obId;

    public Role CurPet
    {
        get { return m_pet; }
    }

    //初始化时调用
    public override void OnInitPanel()
    {
        m_top.AddSel(OnTopSel);
        m_equipNav.AddSel(OnEquipNavSel);
        m_petNav.AddSel(OnPetNavSel);
        m_pageAttribute.OnInitPage(this);
        m_pageEquipUpgrade.OnInitPage(this);
        m_pageEquipRouse.OnInitPage(this);
        m_pageAdvance.OnInitPage(this);
        m_pageUpstar.OnInitPage(this);
        m_pageSkill.OnInitPage(this);
        m_pageTalent.OnInitPage(this);
        m_equipListView.AddChangeCallback(OnEquipListChange);
        //m_petListView.AddChangeCallback(OnPetListChange);
        m_arrow.AddClick(OnArrowDownClick);
        m_arrowUp.AddClick(OnArrowUpClick);
        m_petDown.AddClick(OnPetDownClick);
        m_petUp.AddClick(OnPetUpClick);
        m_petLeft.AddClick(OnPetLeftClick);
        m_petRight.AddClick(OnPetRightClick);
        //m_allPet.AddClick(OnAllPetClick);
        m_addGold.AddClick(OnAddGoldClick);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        // 初始化宠物列表
        UpdatePetInfo();
        object pet = param;
        if(pet == null)
        {
            foreach (Role role in RoleMgr.instance.Hero.PetsPart.Pets.Values)
            {
                if(role.PetsPart.CanOperateAndIsBattle())
                {
                    pet = role;
                    break;
                }
            }
        }
        
        if(pet != null)
        {
            //m_petListView.SelectItem(pet);
            m_pet = (Role)pet;
        }
        else
        {
            m_pet = m_petList[0];
        }
        
        OnPetListChange(true);
        m_top.SetSel(0);
        m_petMod.gameObject.SetActive(false);

        string errMsg;
        if(SystemMgr.instance.IsEnabled(enSystem.petAdvance, out errMsg))
        {
            m_petAdvance.enabled = true;
            foreach (ImageEx image in m_petAdvance.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(false);
            }
        }
        else
        {
            m_petAdvance.enabled = false;
            foreach (ImageEx image in m_petAdvance.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(true);
            }
        }
        if (SystemMgr.instance.IsEnabled(enSystem.petTalent, out errMsg))
        {
            m_petTalent.enabled = true;
            foreach (ImageEx image in m_petTalent.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(false);
            }
        }
        else
        {
            m_petTalent.enabled = false;
            foreach (ImageEx image in m_petTalent.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(true);
            }
        }

        m_obId = RoleMgr.instance.Hero.Add(MSG_ROLE.GET_REWARD, Refresh);

        GetComponent<ShowUpController>().Prepare();
    }

    public override void OnClosePanel()
    {
        m_top.SetSel(0,false);
        EventMgr.Remove(m_obId);
        base.OnClosePanel();
    }

    public void Refresh()
    {
        Refresh2(true);
    }

    public void Refresh2(bool fullReflesh = true)
    {
        if (m_pet == null)
        {
            return;
        }
        UpdatePetInfo();
        OnPetListChange(false, fullReflesh);
    }

    public void UpdatePetInfo()
    {
        m_petList = new List<Role>();
        foreach(Role pet in RoleMgr.instance.Hero.PetsPart.Pets.Values)
        {
            m_petList.Add(pet);
        }

        //m_petListView.SetData(petList);
        m_gold.text = RoleMgr.instance.Hero.ItemsPart.GetGold().ToString();
    }
    
    public void UpdateEquipInfo(bool resetEquip)
    {
        if(m_pet == null)
        {
            return;
        }
        List<object> equipList = new List<object>();

        EquipsPart equipsPart = m_pet.EquipsPart;
        Equip curEquip = null;
        Equip equip = equipsPart.GetEquip(enEquipPos.minWeapon);
        if (equip.CanOperate) curEquip = equip;
        if (equip != null)
        {
            equipList.Add(equip);
            if (curEquip == null && equip.CanOperate) curEquip = equip;
        }
        for (enEquipPos i = enEquipPos.minNormal; i <= enEquipPos.maxNormal; ++i)
        {
            equip = equipsPart.GetEquip(i);
            if (equip != null)
            {
                equipList.Add(equip);
                if (curEquip == null && equip.CanOperate) curEquip = equip;
            }      
        }

        m_equipListView.SetData(equipList);

        if (curEquip == null)
        {
            curEquip = equipsPart.GetEquip(enEquipPos.minWeapon); ;
        }

        if(resetEquip)m_equipListView.SelectItem(curEquip);

        m_equipOp.SetActive(equipsPart.HasEquipCanOperate());
    }

    void OnAddGoldClick()
    {
        UIMessage.Show("该功能未实现，敬请期待!");
    }

    void FreshTop(int idx, bool reset, bool fullReflesh = true)
    {
        EquipsPart equipsPart = m_pet.EquipsPart;
        m_equipOp.SetActive(equipsPart.HasEquipCanOperate());
        m_petOp.SetActive(m_pet != null && m_pet.PetsPart.CanOperateAndIsBattle());
        //m_allPet.gameObject.SetActive(false);
        if (idx == 0)
        {
            //m_allPet.gameObject.SetActive(true);
            m_pageAttribute.OnOpenPage(m_pet);
            if (reset) m_pageAttribute.ResetShow();
            m_arrow.gameObject.SetActive(false);
            m_arrowUp.gameObject.SetActive(false);
        }
        else if (idx == 1)
        {
            m_petOp.SetActive(false);
            if (reset) m_petNav.SetSel(0);
            OnPetNavSelB(null, m_petNav.CurIdx, fullReflesh);
            m_arrow.gameObject.SetActive(false);
            m_arrowUp.gameObject.SetActive(false);
        }
        else if (idx == 2)
        {
            m_equipOp.SetActive(false);
            if (reset) m_equipNav.SetSel(0);
            OnEquipNavSel(null, m_equipNav.CurIdx);
            m_arrow.gameObject.SetActive(true);
            m_arrowUp.gameObject.SetActive(true);
        }
    }

    void OnTopSel(StateHandle s, int idx)
    {
        FreshTop(idx, true);
    }

    void OnEquipNavSel(StateHandle s, int idx)
    {
        UpdateEquipOp();
        if (idx == 0)
        {
            m_pageEquipUpgrade.OnOpenPage(m_pet, m_equipListView.GetCurSel<Equip>());
        }
        else
        {
            m_pageEquipRouse.OnOpenPage(m_pet, m_equipListView.GetCurSel<Equip>());
        }
        
    }

    void OnPetNavSel(StateHandle s, int idx)
    {
        OnPetNavSelB(s, idx, true);
    }

    void OnPetNavSelB(StateHandle s, int idx, bool fullReflesh = true)
    {
        if (idx == 0)
        {
            m_pageUpgrade.OnOpenPage(m_pet, fullReflesh);
        }
        else if(idx == 1)
        {
            m_pageAdvance.OnOpenPage(m_pet);
        }
        else if(idx == 2)
        {
            m_pageUpstar.OnOpenPage(m_pet);
        }
        else if(idx == 3)
        {
            m_pageSkill.OnOpenPage(m_pet);
        }
        else if(idx == 4)
        {
            m_pageTalent.OnOpenPage(m_pet);
        }
    }

    void UpdateEquipOp()
    {
        Equip equip = m_equipListView.GetCurSel<Equip>();

        if (equip != null && !equip.IsNotEquipedWeapon() && (equip.CanUpgrade() || equip.CanAdvance()))
        {
            m_equipOpUpgrade.SetActive(true);
        }
        else
        {
            m_equipOpUpgrade.SetActive(false);
        }
        if (equip != null && !equip.IsNotEquipedWeapon() && equip.CanRouse())
        {
            m_equipOpRouse.SetActive(true);
        }
        else
        {
            m_equipOpRouse.SetActive(false);
        }
        if (m_pet!=null && m_pet.EquipsPart.HasEquipCanUpgradeOnce())
        {
            m_upgradeOnceOp.SetActive(true);
        }
        else
        {
            m_upgradeOnceOp.SetActive(false);
        }
    }

    void UpdatePetOp()
    {
        if (RoleMgr.instance.Hero.PetsPart.HasBattlePetCanOperate() || RoleMgr.instance.Hero.PetsPart.HasRecruitPet())
        {
            m_allPetOp.SetActive(true);
        }
        else
        {
            m_allPetOp.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.IsBattle() && m_pet.PetsPart.CanUpgrade())
        {
            m_petOpUpgrade.SetActive(true);
        }
        else
        {
            m_petOpUpgrade.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.IsBattle() && m_pet.PetsPart.CanAdvance())
        {
            m_petOpAdvance.SetActive(true);
        }
        else
        {
            m_petOpAdvance.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.IsBattle() && m_pet.PetsPart.CanUpstar())
        {
            m_petOpUpstar.SetActive(true);
        }
        else
        {
            m_petOpUpstar.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.IsBattle() && m_pet.PetsPart.CanUpgradeSkill())
        {
            m_petOpSkill.SetActive(true);
        }
        else
        {
            m_petOpSkill.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.IsBattle() && m_pet.PetsPart.CanUpgradeTalent())
        {
            m_petOpTalent.SetActive(true);
        }
        else
        {
            m_petOpTalent.SetActive(false);
        }
        if (m_pet != null && m_pet.PetsPart.CanOperateAndIsBattle())
        {
            m_petOp.SetActive(true);
        }
        else
        {
            m_petOp.SetActive(false);
        }
    }

    void OnEquipListChange()
    {
        if (m_top.CurIdx == 2)
        {
            OnEquipNavSel(null, m_equipNav.CurIdx);
        }
    }

    void OnPetListChange(bool resetEquip, bool fullReflesh = true)
    {
        //m_pet = m_petListView.GetCurSel<Role>();
        UpdatePet();
        UpdateEquipInfo(resetEquip);
        FreshTop(m_top.CurIdx, false, fullReflesh);
    }

    void UpdatePet()
    {
        if (m_pet == null) return;
        for (int i = 0; i < m_starCovers.Count; i++)
        {
            if (i < m_pet.PropPart.GetInt(enProp.star) )
            {
                m_starCovers[i].gameObject.SetActive(false);
            }
            else
            {
                m_starCovers[i].gameObject.SetActive(true);
            }
        }
        m_textLevel.text = "Lv." + m_pet.PropPart.GetInt(enProp.level);
        m_textName.text = m_pet.Cfg.name;
        PetAdvLvPropRateCfg cfg = PetAdvLvPropRateCfg.m_cfgs[m_pet.PropPart.GetInt(enProp.advLv)];
        m_textName.color = QualityCfg.GetColor(cfg.quality);
        m_petMod.SetModel(m_pet.Cfg.mod, m_pet.Cfg.uiModScale);
        m_power.text = m_pet.GetInt(enProp.power).ToString();
        UpdatePetOp();
    }

    void OnArrowUpClick()
    {
        m_equipListView.IncSelection();
    }

    void OnArrowDownClick()
    {
        m_equipListView.DecSelection();
    }

    public override void OnOpenPanelEnd()
    {
        m_petMod.gameObject.SetActive(true);
        m_petMod.SetModel(m_pet.Cfg.mod, m_pet.Cfg.uiModScale, true);
        GetComponent<ShowUpController>().Start();
    }

    void OnPetDownClick()
    {   
        //m_petListView.IncSelection();
    }

    void OnPetUpClick()
    {
        //m_petListView.DecSelection();
    }

    void OnPetLeftClick()
    {
        int index = m_petList.IndexOf(m_pet);
        index--;
        if (index < 0) index = m_petList.Count - 1;
        m_pet = m_petList[index];
        OnPetListChange(true);
    }

    void OnPetRightClick()
    {
        int index = m_petList.IndexOf(m_pet);
        index++;
        if (index >= m_petList.Count) index = 0;
        m_pet = m_petList[index];
        OnPetListChange(true);
    }

    void OnAllPetClick()
    {
        UIMgr.instance.Open<UIChoosePet>();
    }

    bool OnTeachCheck(string arg)
    {
        switch (arg)
        {
            case "hasPetLvReach5":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return false;
                    var pets = hero.PetsPart.Pets;
                    foreach (var pet in pets.Values)
                    {
                        var lv = pet.GetInt(enProp.level);
                        if (lv >= 5)
                            return true;
                    }
                    return false;
                }
        }

        return true;
    }

   
}