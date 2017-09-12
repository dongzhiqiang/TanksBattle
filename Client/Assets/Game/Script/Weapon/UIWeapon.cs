#region Header
/**
 * 名称：ui类模板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UIWeapon : UIPanel
{
    #region Fields
    public UIListItemEquip m_equipItemList;
    public BowListView m_equipListView;
    public UI3DView m_weapon;
    public StateHandle m_btnChangeWeapon;
    public StateGroup m_tab;
    public List<UIPage> m_pages;
    public List<UIWeaponElementItem> m_elements;//元素属性
    public List<GameObject> m_elementFxs;
    public StateHandle m_toggle;
    public GameObject m_opSkill;
    public GameObject m_opTalent;
    public StateHandle m_arrow;
    public StateHandle m_arrowUp;
    public StateHandle m_close;
    public GameObject m_elementObj;

    public ImageEx m_hitPropIcon;
    public Text m_weaponName;
    public Text m_hitPropName;
    public Text m_desc;

    public Text m_gold;
    public StateHandle m_addGold;

    #endregion


    #region Properties
    public Weapon CurWeapon { get {
            Equip curSelWeapon = m_equipListView.GetCurSel<Equip>();
            return RoleMgr.instance.Hero.WeaponPart.GetWeapon((int)curSelWeapon.Cfg.posIndex - (int)enEquipPos.minWeapon);

        } }
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_equipItemList.Cache();
        m_equipListView.AddChangeCallback(OnListChange);
        m_btnChangeWeapon.AddClick(OnChangeWeapon);
        m_arrow.AddClick(OnArrowDownClick);
        m_arrowUp.AddClick(OnArrowUpClick);
        m_tab.AddSel(OnSelPage);
        foreach (UIPage p in m_pages)
            p.InitPage(this);

        for (int i=0;i< m_elements.Count;++i)
        {
            

            if (i != 0)
            {
                m_elements[i].btn.AddPressHold(OnTipElement);
                m_elements[i].btn.AddClickEx(OnChangeElement);
            }
            else
            {
                //如果是第一个元素，要设置多次
                var cs = m_elements[i].btn.GetComponents<StateHandle>();
                for (int j=0;j< cs.Length;++j)
                {
                    if(j == 0)
                    {
                        cs[j].AddPressHold(OnTipElement);
                    }
                    else
                        cs[j].AddPressHold(h=> { });
                }
            }
                
        }

        m_addGold.AddClick(OnAddGoldClick);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        //    m_equipListView.SetSelectedIndex(RoleMgr.instance.Hero.WeaponPart.CurWeaponIdx);
        //调整武器显示顺序
        int index = EquipPosCfg.Get(RoleMgr.instance.Hero.WeaponPart.CurWeaponIdx + (int)enEquipPos.minWeapon).ui - (int)enEquipPos.minWeapon;
        m_equipListView.SetSelectedIndex(index);
        Refresh();
        //m_tab.SetSel(m_tab.CurIdx==-1?0: m_tab.CurIdx);//再次选中当前，以初始化
        m_tab.SetSel(0);

        UpdateWeaponEnabled(false);
    }


    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        foreach (StateHandle handle in m_toggle.GetComponents<StateHandle>())
            handle.SetState(0);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Private Methods
    void OnListChange()
    {
        UpdateWeaponInfo();
        UpdateElementInfo();
        UpdateWeaponEnabled(true);
    }

    void OnChangeWeapon()
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        Equip curSelWeapon = m_equipListView.GetCurSel<Equip>();

        if(curSelWeapon == null || weaponPart.CurWeapon.Equip== curSelWeapon)
        {
            Debuger.LogError("逻辑错误，当前武器和要切换的武器一样或者当前武器为空");
            return;
        }

        NetMgr.instance.WeaponHandler.SendChangeWeapon((enEquipPos)curSelWeapon.Cfg.posIndex);
    }

    void OnSelPage(StateHandle handle, int idx)
    {
        m_pages[idx].OpenPage();
        UpdateWeaponEnabled(false);
    }

    void UpdateEquipInfo()
    {
        List<object> equipList = new List<object>();
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        EquipsPart equipsPart = RoleMgr.instance.Hero.EquipsPart;
        
        
        Equip equip;
        for (enEquipPos i = enEquipPos.minWeapon; i <= enEquipPos.maxWeapon; ++i)
        {
            int uiPos = EquipPosCfg.Get((int)i).ui;   //调整一下显示顺序
            equip = equipsPart.GetEquip((enEquipPos)uiPos);
            if (equip != null )
                equipList.Add(equip);
        }
        m_equipListView.SetData(equipList);
    }
    void UpdateWeaponInfo()
    {
        Role hero = RoleMgr.instance.Hero;
        WeaponPart weaponPart = hero.WeaponPart;
        Equip curSelWeapon =m_equipListView.GetCurSel<Equip>();
        Weapon w =hero.WeaponPart.GetWeapon((int)curSelWeapon.Cfg.posIndex - (int)enEquipPos.minWeapon);
        var weaponCfg = w.Cfg;

        //模型
        string weaponMod = weaponCfg!=null? weaponCfg.uiMod:null;
        m_weapon.SetModel(weaponMod);
        
        //元素属性对应的火盆特效
        int curFx = (int)w.CurElementType - 1;
        for (int i = 0; i < m_elementFxs.Count; ++i)
        {
            m_elementFxs[i].SetActive(i == curFx);
        }

        //换武器按钮
        m_btnChangeWeapon.gameObject.SetActive(curSelWeapon != weaponPart.CurWeapon.Equip);

        //页面刷新
        m_tab.SetSel(m_tab.CurIdx == -1 ? 0 : m_tab.CurIdx);//再次选中当前，以刷新

        UpdateWeaponOp(w);

        //武器描述和打击属性
        if (weaponCfg != null)
        {
            var hitPropCfg = weaponCfg.HitPropCfg;
            m_hitPropIcon.Set(hitPropCfg != null? hitPropCfg.icon:null);
            m_weaponName.text = weaponCfg.name;
            m_hitPropName.text = hitPropCfg != null ? hitPropCfg.name : "";
            m_desc.text = weaponCfg.desc;
        }
        else
        {
            m_hitPropIcon.Set(null);
            m_weaponName.text = "";
            m_hitPropName.text = "";
            m_desc.text = "";
        }
    }

    void UpdateWeaponOp(Weapon weapon)
    {
        if (weapon != null && weapon.CanUpgradeSkill())
        {
            m_opSkill.SetActive(true);
        }
        else
        {
            m_opSkill.SetActive(false);
        }
        if (weapon != null && weapon.CanUpgradeTalent())
        {
            m_opTalent.SetActive(true);
        }
        else
        {
            m_opTalent.SetActive(false);
        }
    }

    void UpdateElementInfo()
    {
        string outMsg;
        if (!SystemMgr.instance.IsEnabled(enSystem.element, out outMsg))
        {
            m_elementObj.SetActive(false);
            return;
        }
        else
        {
            m_elementObj.SetActive(true);
        }

        //元素属性
        for (int i = 0; i < m_elements.Count; ++i)
        {
            UIWeaponElementItem item = m_elements[i];
            enElement elementType = CurWeapon.GetElementType(i);
            item.icon.Set(ElementCfg.Element_Icons[(int)elementType]);
        }
    }

    void OnTipElement(StateHandle s)
    {
        UIWeaponElementItem item = s.Get<UIWeaponElementItem>();
        int idx = m_elements.IndexOf(item);
        var cfg =CurWeapon.GetElementCfg(idx);
        
        UITip.Show(cfg.desc, s.GetComponent<RectTransform>(), UITip.enCloseType.pointerUp);
    }

    

    void OnChangeElement(StateHandle s)
    {
        UIWeaponElementItem item = s.Get<UIWeaponElementItem>();
        int idx = m_elements.IndexOf(item);
        NetMgr.instance.WeaponHandler.SendElementChange(CurWeapon,idx);
        switch (CurWeapon.GetElementType(idx))
        {
            case enElement.fire:
                SoundMgr.instance.Play2DSoundAutoChannel(114);
                break;
            case enElement.ice:
                SoundMgr.instance.Play2DSoundAutoChannel(115);
                break;
            case enElement.thunder:
                SoundMgr.instance.Play2DSoundAutoChannel(116);
                break;
            case enElement.dark:
                SoundMgr.instance.Play2DSoundAutoChannel(117);
                break;
        }
    }
    #endregion

    public void Refresh()
    {
        UpdateEquipInfo();
        UpdateWeaponInfo();
        UpdateElementInfo();
        m_gold.text = RoleMgr.instance.Hero.ItemsPart.GetGold().ToString();
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

    void OnAddGoldClick()
    {
        UIMessage.Show("该功能未实现，敬请期待!");
    }

    void UpdateWeaponEnabled(bool showMsg)
    {
        Equip equip = m_equipListView.GetCurSel<Equip>();
        if (equip == null) return;
        if (equip.IsLockedWeapon())
        {
            if (showMsg)
            {
                UIMessage.Show("该武器尚未解锁");
            }
            foreach (StateHandle button in this.GetComponentsInChildren<StateHandle>())
            {
                if (IsOpButton(button))
                {
                    button.enabled = false;
                    foreach (ImageEx image in button.GetComponentsInChildren<ImageEx>())
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

    void OnTeachAction(string arg)
    {
        switch (arg)
        {
            case "findDarkElemBtn":
                {
                    var darkItem = m_elements[m_elements.Count - 1];
                    for (int i = 1; i < m_elements.Count; ++i)
                    {
                        UIWeaponElementItem item = m_elements[i];
                        enElement elemType = CurWeapon.GetElementType(i);
                        if (elemType == enElement.dark)
                        {
                            darkItem = item;
                            break;
                        }
                    }
                    TeachMgr.instance.SetNextStepUIObjParam(darkItem.btn.transform as RectTransform);
                }                
                break;
        }
    }
}
