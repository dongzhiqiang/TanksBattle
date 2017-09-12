#region Header
/**
 * 名称：武器技能升级页
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class UIWeaponPageSkill : UIPage
{

    #region Fields
    public StateGroup m_group;
    #endregion

    #region Properties
    
    #endregion


    #region Frame
    //初始化
    protected override void OnInitPage() {
        
    }

    //显示
    protected override void OnOpenPage() {
        Weapon weapon = this.GetParent<UIWeapon>().CurWeapon;
        int len = (int)enSkillPos.max;

        m_group.SetCount(len);
        for(int i = 0; i < len; ++i)
        {
            UIWeaponSkillItem skillItem =m_group.Get<UIWeaponSkillItem>(i);
            WeaponSkill skill = weapon.GetSkill(i);
            skillItem.Init(skill);
        }

        
    }

    public UIWeaponSkillItem GetSkillItem(WeaponSkill skill)
    {
        for (int i = 0; i < m_group.Count; ++i)
        {
            UIWeaponSkillItem skillItem = m_group.Get<UIWeaponSkillItem>(i);
            if(skillItem.GetSkill() == skill)
            {
                return skillItem;
            }
        }
        return null;
    }

    #endregion

    #region Private Methods

    #endregion



}
