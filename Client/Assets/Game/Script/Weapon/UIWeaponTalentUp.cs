#region Header
/**
 * 名称：UITalentInfo
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UIWeaponTalentUp : UIPanel
{

    #region Fields
    public UIWeaponTalentUpItem m_left;
    public UIWeaponTalentUpItem m_right;
    public StateGroup m_costs;
    public TextEx m_gold;
    public StateHandle m_cancel;
    public StateHandle m_btnUp;
    public float m_btnTimeInterval = 1f;

    WeaponSkillTalent m_talent;
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
        m_talent = (WeaponSkillTalent)param;
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
        if (m_talent.lv >= ConfigValue.GetInt("maxTalentLevel"))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.LEVEL_MAX);
            return;
        }
        
        //钱不够
        List<CostItem> costItems = SkillLvCostCfg.GetCost(m_talent.Cfg.levelCostId, m_talent.lv);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            if(needItemId == ITEM_ID.GOLD)
                UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.NO_ENOUGH_GOLD);
            else
                UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.NO_ENOUGH_ITEM);
            return;
        }

        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        NetMgr.instance.WeaponHandler.SendTalentUp(m_talent);
    }
    #endregion

    public void Refresh()
    {
        bool isMax = m_talent.lv >= ConfigValue.GetInt("maxTalentLevel");
        HeroTalentCfg cfg = m_talent.Cfg;
        if (!isMax)
        {
            m_left.Init(cfg, m_talent.lv,false);
            m_right.Init(cfg, m_talent.lv+1, false);

            List<CostItem> costs= SkillLvCostCfg.GetCostShow(cfg.levelCostId, m_talent.lv);
            m_costs.SetCount(costs.Count);
            for(int i = 0;i< costs.Count; ++i)
            {
                UIItemIcon item = m_costs.Get<UIItemIcon>(i);
                CostItem c = costs[i];
                item.Init(c.itemId, c.num,true);
            }
            
            int costGold = SkillLvCostCfg.GetCostGold(cfg.levelCostId, m_talent.lv);
            m_gold.text = costGold.ToString();
        }
        else
        {
            m_left.Init(cfg, m_talent.lv, false);
            m_right.Init(cfg, m_talent.lv, true);
            m_gold.text = "";
        }
    }

    
}
