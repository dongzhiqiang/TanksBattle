using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HeroUpgradeInfo
{
    public int oldLevel;
    public int newLevel;
    public int oldStamina;
    public int newStamina;
}

public class UIHeroUpgrade : UIPanel
{
    #region SerializeFields
    public Text m_level;
    public Text m_oldLevel;
    public Text m_newLevel;
    public Text m_oldMaxStamina;
    public Text m_newMaxStamina;
    public Text m_oldStamina;
    public Text m_newStamina;
    public GameObject m_upgradeFx;
    #endregion

    static HeroUpgradeInfo m_upgradeInfo;
    static bool m_needOpenUI = false;
    public static void SetUpgradeInfo(HeroUpgradeInfo upgradeInfo)
    {
        m_upgradeInfo = upgradeInfo;
        m_needOpenUI = true;
    }

    public static bool CheckOpen()
    {
        if(m_needOpenUI)
        {
            m_needOpenUI = false;
            UIMgr.instance.Open<UIHeroUpgrade>();
            return true;
        }
        return false;
    }


    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        if (m_upgradeInfo == null)
        {
            Debuger.LogError("没有设置升级信息");
            return;
        }
        this.GetComponent<UIPanelBase>().m_btnClose.enabled = false;
        m_level.text = m_upgradeInfo.newLevel.ToString();
        m_oldLevel.text = m_upgradeInfo.oldLevel.ToString();
        m_newLevel.text = m_upgradeInfo.newLevel.ToString();
        m_oldMaxStamina.text = RoleLvExpCfg.Get(m_upgradeInfo.oldLevel).maxStamina.ToString();
        m_newMaxStamina.text = RoleLvExpCfg.Get(m_upgradeInfo.newLevel).maxStamina.ToString();
        int addStamina = 0;
        for (int i = m_upgradeInfo.oldLevel; i < m_upgradeInfo.newLevel; i++ )
        {
            addStamina += RoleLvExpCfg.Get(i).upgradeStamina;
        }
        m_oldStamina.text = (m_upgradeInfo.newStamina-addStamina).ToString();
        m_newStamina.text = m_upgradeInfo.newStamina.ToString();
        m_upgradeFx.SetActive(false);
        TimeMgr.instance.AddTimer(1, () =>
        {
            m_upgradeFx.SetActive(true);

        });
        TimeMgr.instance.AddTimer(2, () =>
        {
            this.GetComponent<UIPanelBase>().m_btnClose.enabled = true;
        });
    }

    public override void OnClosePanel()
    {
        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(true);
    }

}