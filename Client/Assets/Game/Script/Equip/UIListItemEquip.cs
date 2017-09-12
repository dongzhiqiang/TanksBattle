using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIListItemEquip : MonoBehaviour
{
    public enum enType
    {
        equip,//装备界面上的item
        weapon,//武器界面上的item
    }

    public enType m_type = enType.equip;
    public ImageEx m_equipIcon;
    public List<ImageEx> m_stars;
    public ImageEx m_selImage;
    public ImageEx m_operation;
    public ImageEx m_equipBg;
    public Text m_level;
    public SimpleHandle m_isCurWeapon;
    public GameObject m_lock;

    bool m_cache = false;
    bool m_isFirstSetData = true;
    void Awake()
    {
        Cache();
    }

    public void Cache()
    {
        if (m_cache) return;
        m_cache = true;

        BowListItem listItem = GetComponent<BowListItem>();
        listItem.SetSetDataAction(SetData);
        listItem.SetSetSelectedAction(SetSelected);
    }

    public void SetData(object data)
    {
        if(!(data is Equip))
        {
            return;
        }
        Equip equip = (Equip)data;
        EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
        //Debug.Log(equipCfg.icon);
        m_equipIcon.Set(equipCfg.icon);
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[equip.AdvLv];
        QualityCfg qualityCfg = QualityCfg.m_cfgs[advCfg.quality];
        m_equipBg.Set(qualityCfg.backgroundSquare);
        for(int i=0; i<m_stars.Count; i++)
        {
            if(equipCfg.star>i)
            {
                m_stars[i].gameObject.SetActive(true);
            }
            else
            {
                m_stars[i].gameObject.SetActive(false);
            }
        }
        m_level.text = "" + equip.Level;

        
        if (m_isCurWeapon != null)
        {
            Equip curWeapon = RoleMgr.instance.Hero.WeaponPart.CurWeapon.Equip;
            bool activeBefore = m_isCurWeapon.gameObject.activeSelf;
            bool active = curWeapon != null && equip == curWeapon;
            m_isCurWeapon.gameObject.SetActive(active);
            if(!m_isFirstSetData&&!activeBefore&& active)//换武器的时候抖一下
            {
                m_isCurWeapon.ResetPlay();
            }
        }

        if(m_lock != null)
        {
            if(equip.IsLockedWeapon())
            {
                m_lock.SetActive(true);
            }
            else
            {
                m_lock.SetActive(false);
            }
        }

        if (m_type == enType.equip)
        {
            m_operation.gameObject.SetActive(equip.CanOperate && !equip.IsNotEquipedWeapon());
        }
        else
        {
            //技能升级和铭文的相关提示，还没有实现
            Weapon weapon = RoleMgr.instance.Hero.WeaponPart.GetWeapon((int)equip.Cfg.posIndex - (int)enEquipPos.minWeapon);
            m_operation.gameObject.SetActive(weapon.CanOperate() && !equip.IsLockedWeapon() && !equip.IsNotEquipedWeapon());
        }
            


        m_isFirstSetData = false;
    }

    public void SetSelected(bool selected)
    {
        m_selImage.gameObject.SetActive(selected);
    }


}