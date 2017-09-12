using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEquip : UIPanel
{
    #region SerializeFields
    public BowListView m_equipListView;
    public StateGroup m_top;
    public StateGroup m_right;
    public UIEquipPageAttribute m_pageAttribute;
    public UIEquipPageUpgrade m_pageUpgrade;
    public UIEquipPageRouse m_pageRouse;
    public StateHandle m_arrow;
    public StateHandle m_arrowUp;
    public UI3DView m_weapon;
    public GameObject m_equipOpUpgrade;
    public GameObject m_equipOpRouse;
    public GameObject m_equipFx;
    public FlyIconFx m_flyIconFx;
    public GameObject m_updateArrowFx;
    public GameObject m_updateShineFx;
    public List<GameObject> m_elementFxs;
    public GameObject m_equipOp;
    public StateHandle m_close;
    public GameObject m_upgradeOnceOp;
    public Text m_gold;
    public StateHandle m_addGold;
    #endregion

    private int m_obId;
    //初始化时调用
    public override void OnInitPanel()
    {
        m_top.AddSel(OnTopSel);
        m_right.AddSel(OnRightSel);
        m_pageAttribute.OnInitPage(this);
        m_pageUpgrade.OnInitPage(this);
        m_pageRouse.OnInitPage(this);
        m_equipListView.AddChangeCallback(OnListChange);
        m_arrow.AddClick(OnArrowDownClick);
        m_arrowUp.AddClick(OnArrowUpClick);
        m_addGold.AddClick(OnAddGoldClick);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_top.SetSel(0);
        // 初始化装备列表
        UpdateEquipInfo(true);
        UpdateWeaponInfo();
        FreshTop(m_top.CurIdx, true);
        m_weapon.gameObject.SetActive(false);

        m_obId = RoleMgr.instance.Hero.Add(MSG_ROLE.GET_REWARD, Refresh);

        GetComponent<ShowUpController>().Prepare();
    }

    public override void OnClosePanel()
    {
        EventMgr.Remove(m_obId);
        base.OnClosePanel();
    }


    public void Refresh()
    {
        UpdateEquipInfo(false);
        UpdateWeaponInfo();
        FreshTop(m_top.CurIdx);
    }

    public void UpdateEquipInfo(bool resetEquip)
    {
        Role hero =RoleMgr.instance.Hero;
        EquipsPart equipsPart = hero.EquipsPart;
        WeaponPart weaponPart = hero.WeaponPart;
        Equip weapon = weaponPart.CurWeapon.Equip;
        List<object> equipList = new List<object>();

        if (weapon != null)
            equipList.Add(weapon);

        Equip equip;        
        Equip curEquip = null;
        if (hero.WeaponPart.CurWeapon.Equip.CanOperate)
        {
            curEquip = hero.WeaponPart.CurWeapon.Equip;
        }
        for (enEquipPos i = enEquipPos.minNormal; i <= enEquipPos.maxNormal; ++i)
        {
            equip = equipsPart.GetEquip(i);
            if (equip != null)
            {
                equipList.Add(equip);
                if (curEquip == null && equip.CanOperate && !equip.IsNotEquipedWeapon()) curEquip = equip;
            }
                
        }
        for (enEquipPos i = enEquipPos.minWeapon; i <= enEquipPos.maxWeapon; ++i)
        {
            int uiPos = EquipPosCfg.Get((int)i).ui;  //调整一下显示顺序
            equip = equipsPart.GetEquip((enEquipPos)uiPos);
            if (equip != null && weapon!= equip)
            { 
                equipList.Add(equip);
                if (curEquip == null && equip.CanOperate && !equip.IsNotEquipedWeapon()) curEquip = equip;
            }
        }

        m_equipListView.SetData(equipList);
        if (curEquip==null)
        {
            curEquip = hero.WeaponPart.CurWeapon.Equip;
        }

        if (resetEquip) m_equipListView.SelectItem(curEquip);

        m_equipOp.SetActive(equipsPart.HasEquipCanOperate());
        UpdateWeaponEnabled(false);

        m_gold.text = RoleMgr.instance.Hero.ItemsPart.GetGold().ToString();
    }

    public void UpdateWeaponInfo()
    {
        string weaponMod = null;

        Role hero = RoleMgr.instance.Hero;
        EquipsPart equipsPart = hero.EquipsPart;
        WeaponPart weaponPart = hero.WeaponPart;
        Equip weapon = weaponPart.CurWeapon.Equip;
        if (weapon != null && weapon.Cfg.weaponId != 0)
        {
            WeaponCfg weaponCfg = WeaponCfg.Get(weapon.Cfg.weaponId);
            weaponMod = weaponCfg.uiMod;
        }

        m_weapon.SetModel(weaponMod);

        //元素属性对应的火盆特效
        int curFx = (int)weaponPart.CurWeapon.CurElementType - 1 ;
        for(int i=0;i<m_elementFxs.Count;++i)
        {
            m_elementFxs[i].SetActive(i == curFx);
        }
        
    }

    public void SelectWeapon()
    {
        m_equipListView.SetSelectedIndex(0);
        OnListChange();
    }

    void FreshTop(int idx, bool reset = false)
    {
        Role hero = RoleMgr.instance.Hero;
        EquipsPart equipsPart = hero.EquipsPart;
        m_equipOp.SetActive(equipsPart.HasEquipCanOperate());
        if (idx == 0)
        {
            m_pageAttribute.OnOpenPage();
            if (reset) m_pageAttribute.ResetShow();
            m_arrow.gameObject.SetActive(false);
            m_arrowUp.gameObject.SetActive(false);
            UpdateWeaponEnabled(false);
        }
        else if (idx == 1)
        {
            m_equipOp.SetActive(false);
            if(reset)m_right.SetSel(0);
            OnRightSel(null, m_right.CurIdx);
            m_arrow.gameObject.SetActive(true);
            m_arrowUp.gameObject.SetActive(true);
        }
    }

    //跳转到装备升级页
    public void SelectEquipUpGrade()
    {
        TimeMgr.instance.AddTimer(0.7f, () => { m_top.SetSel(1); });     
    }    

    void OnTopSel(StateHandle s, int idx)
    {
        FreshTop(idx, true);
    }

    void UpdateEquipOp()
    {
        Equip equip = m_equipListView.GetCurSel<Equip>();
        
        if(equip != null &&!equip.IsNotEquipedWeapon()&& !equip.IsLockedWeapon() &&  (equip.CanUpgrade()||equip.CanAdvance()))
        {
            m_equipOpUpgrade.SetActive(true);
        }
        else
        {
            m_equipOpUpgrade.SetActive(false);
        }
        if (equip != null && !equip.IsNotEquipedWeapon() && !equip.IsLockedWeapon() && equip.CanRouse())
        {
            m_equipOpRouse.SetActive(true);
        }
        else
        {
            m_equipOpRouse.SetActive(false);
        }
        if (RoleMgr.instance.Hero.EquipsPart.HasEquipCanUpgradeOnce() && !equip.IsLockedWeapon())
        {
            m_upgradeOnceOp.SetActive(true);
        }
        else
        {
            m_upgradeOnceOp.SetActive(false);
        }
    }

    void OnRightSel(StateHandle s, int idx)
    {
        UpdateEquipOp();
        if (idx == 0)
        {
            m_pageUpgrade.OnOpenPage(m_equipListView.GetCurSel<Equip>());
        }
        else
        {
            m_pageRouse.OnOpenPage(m_equipListView.GetCurSel<Equip>());
        }
        UpdateWeaponEnabled(false);
    }

    override public void OnOpenPanelEnd()
    {
        //m_weapon.gameObject.SetActive(true);
        GetComponent<ShowUpController>().Start();
    }

    void OnAddGoldClick()
    {
        UIMessage.Show("该功能未实现，敬请期待!");
    }

    void OnListChange()
    {
        if (m_top.CurIdx == 1)
        {
            OnRightSel(null, m_right.CurIdx);
        }

        UpdateWeaponEnabled(true);
    }

    void OnArrowUpClick()
    {
        m_equipListView.IncSelection();
    }

    void OnArrowDownClick()
    {
        m_equipListView.DecSelection();
    }

    bool IsOpButton(StateHandle button)
    {
        return button != m_close && button != m_arrow && button != m_arrowUp
            && (button.m_ctrlType == StateHandle.CtrlType.button || button.m_ctrlType == StateHandle.CtrlType.toggle);
    }

    void UpdateWeaponEnabled(bool showMsg)
    {
        Equip equip = m_equipListView.GetCurSel<Equip>();
        if (equip == null) return;
        if(equip.IsLockedWeapon())
        {
            if(showMsg)
            {
                UIMessage.Show("该武器尚未解锁");
            }
            foreach(StateHandle button in this.GetComponentsInChildren<StateHandle>())
            {
                if (IsOpButton(button))
                {
                    button.enabled = false;
                    foreach(ImageEx image in button.GetComponentsInChildren<ImageEx>())
                    {
                        image.SetGrey(true);
                    }
                }
            }
        }
        else
        {
            foreach (StateHandle button in this.GetComponentsInChildren<StateHandle>())
            {
                if (IsOpButton(button))
                {
                    button.enabled = true;
                    foreach (ImageEx image in button.GetComponentsInChildren<ImageEx>())
                    {
                        image.SetGrey(false);
                    }
                }
            }
        }
    }
}