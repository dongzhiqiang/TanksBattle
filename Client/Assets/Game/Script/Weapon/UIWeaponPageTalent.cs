#region Header
/**
 * 名称：武器铭文页
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIWeaponPageTalent : UIPage
{

    #region Fields
    public StateGroup m_group;
    #endregion

    #region Properties

    #endregion


    #region Frame
    //初始化
    protected override void OnInitPage() { }

    //显示
    protected override void OnOpenPage() {
        Weapon weapon = this.GetParent<UIWeapon>().CurWeapon;
        List<WeaponSkill> l = weapon.GetSkillsHaveTelent();
        m_group.SetCount(l.Count);
        for (int i = 0; i < l.Count; ++i)
        {
            UIWeaponTalentRow item = m_group.Get<UIWeaponTalentRow>(i);
            WeaponSkill skill = l[i];
            item.Init(skill);
        }
    }

    #endregion

    #region Private Methods

    #endregion



}
