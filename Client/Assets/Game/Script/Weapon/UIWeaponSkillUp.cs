#region Header
/**
 * 名称：UISkillInfo
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UIWeaponSkillUp : UIPanel
{

    #region Fields
    public ImageEx m_icon;
    public TextEx m_skillName;
    public TextEx m_comboLimit;
    public TextEx m_desc;
    public TextEx m_nextLv;
    public TextEx m_cost;
    public StateHandle m_cancel;
    public StateHandle m_btnUp;
    public float m_btnTimeInterval = 1f;

    WeaponSkill m_skill;
    float m_lastClickTime = -1;
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_cancel.AddClick(Close);
        m_btnUp.AddClick(OnLevelUp);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_skill = (WeaponSkill)param;

        Refresh();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Private Methods
    void OnLevelUp()
    {
        if (m_lastClickTime != -1 && TimeMgr.instance.realTime - m_lastClickTime < m_btnTimeInterval)
            return;
        m_lastClickTime = TimeMgr.instance.realTime;
        //已经满级
        if (m_skill.lv >= ConfigValue.GetInt("maxSkillLevel"))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.LEVEL_MAX);
            return;
        }

        //不能超过角色等级
        if (m_skill.lv >= RoleMgr.instance.Hero.GetInt(enProp.level))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.ROLE_LEVEL_LIMIT);
            return;
        }

        //钱不够
        var roleSkillCfg = m_skill.RoleSkillCfg;
        List<CostItem> costItems = SkillLvCostCfg.GetCost(roleSkillCfg.levelCostId, m_skill.lv);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.NO_ENOUGH_GOLD);
            return;
        }

        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        NetMgr.instance.WeaponHandler.SendSkillUp(m_skill);
    }

    #endregion

    public void Refresh()
    {
        var roleSkillCfg = m_skill.RoleSkillCfg;
        var skillCfg = m_skill.SkillCfg;
        if (roleSkillCfg != null && skillCfg != null)
        {
            //计算下连击限制
            int comboLimitCount;
            int limitLv;
            m_skill.GetComboLimit(out comboLimitCount, out limitLv);

            m_icon.Set(skillCfg == null ? null : skillCfg.icon);
            m_skillName.text = string.Format("{0} Lv.{1}", roleSkillCfg.name, m_skill.lv);
            m_comboLimit.text = limitLv == -1 ? "" : string.Format("Lv.{0}解锁{1}连击", limitLv, comboLimitCount + 1);
            m_desc.text = LvValue.ParseText(roleSkillCfg.description, m_skill.lv);
        }
        else
        {
            m_icon.Set(null);
            m_skillName.text = "";
            m_comboLimit.text = "";
            m_desc.text = "";
        }
        
        bool isMax = m_skill.lv>= ConfigValue.GetInt("maxSkillLevel") ;
        int costGold = isMax ||roleSkillCfg == null ? 0 : SkillLvCostCfg.GetCostGold(roleSkillCfg.levelCostId, m_skill.lv);
        if (isMax)
            m_nextLv.text = string.Format("已满级");
        else
            m_nextLv.text = string.Format("Lv.{0}", m_skill.lv + 1);


        m_cost.text = costGold.ToString();
    }
}
