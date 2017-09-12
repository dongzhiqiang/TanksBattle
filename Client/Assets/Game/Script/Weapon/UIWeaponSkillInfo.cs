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


public class UIWeaponSkillInfo : UIPanel
{

    #region Fields
    public ImageEx m_icon;
    public Text m_skillName;
    public TextEx m_comboLimit;
    public TextEx m_desc;
    public TextEx m_lv;
    //public TextEx m_nextLv;
    //public TextEx m_cost;

    WeaponSkill m_skill;
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {

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
            m_skillName.text = roleSkillCfg.name;
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
        m_lv.text = string.Format("Lv.{0}", m_skill.lv  );
        /*
        if (isMax)
            m_nextLv.text = string.Format("已满级");
        else
            m_nextLv.text = string.Format("Lv.{0}", m_skill.lv + 1);


        m_cost.text = costGold.ToString();
         */
    }
}
