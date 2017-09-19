using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEliteLevel : UIPanel
{
    #region SerializeFields
    public BowListView m_eliteLevels;
    public ImageEx m_contentImage;
    public List<ImageEx> m_stars;
    public StateGroup m_rewards;
    public Text m_count;
    public Text m_stamina;
    public StateHandle m_sweep;
    public StateHandle m_enterLevel;
    public StateHandle m_reset;
    public UIEliteLevelDescription m_uiDescription;
    public UIEliteLevelStar m_uiStar;
    public UIEliteLevelFirstReward m_uiFirstReward;
    public StateHandle m_showStar;
    public StateHandle m_showDescription;
    public StateHandle m_firstReward;
    #endregion
    private int m_levelId;
    private int m_curLevelIdx;
    
    //初始化时调用
    public override void OnInitPanel()
    {
        m_sweep.AddClick(OnSweep);
        m_enterLevel.AddClick(OnEnterLevel);
        m_reset.AddClick(OnReset);
        m_eliteLevels.AddChangeCallback(OnListChange);
        m_eliteLevels.SetBorder(2);
        m_showStar.AddClick(OnShowStar);
        m_showDescription.AddClick(OnShowDescription);
        m_firstReward.AddClick(OnFirstReward);
        m_uiDescription.Init();
        m_uiStar.Init();
        m_uiFirstReward.Init();
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        UpdateEliteList();
        m_eliteLevels.SetSelectedIndex(m_curLevelIdx);
        UpdateEliteLevel();
    }


    public void Reflesh()
    {
        UpdateEliteList(false);
        UpdateEliteLevel();
    }

    void UpdateEliteList(bool reset=true)
    {
        List<int> eliteLevels = new List<int>();

        foreach (EliteLevelCfg cfg in EliteLevelCfg.m_cfgs.Values)
        {
            eliteLevels.Add(cfg.id);
        }
        eliteLevels.Sort();
        List<object> eliteLevelObjs = new List<object>();
        m_curLevelIdx = -1;
        for(int i=0; i<eliteLevels.Count; i++)
        {
            int id = eliteLevels[i];
            eliteLevelObjs.Add(id);
            EliteLevel eliteLevel = RoleMgr.instance.Hero.EliteLevelsPart.GetEliteLevel(id);
            if (/*(eliteLevel != null && eliteLevel.passed) &&*/ i + 1 < eliteLevels.Count && EliteLevel.CanOpen(EliteLevelCfg.m_cfgs[eliteLevels[i+1]]))
            {
                if (m_curLevelIdx < i)
                {
                    m_curLevelIdx = i;
                }
            }
        }
        m_curLevelIdx++;

        m_eliteLevels.SetData(eliteLevelObjs);

        if(reset)SelectEliteLevel(eliteLevels[m_curLevelIdx], true);

        if (m_curLevelIdx > eliteLevelObjs.Count - 3) m_curLevelIdx = eliteLevelObjs.Count - 3;
        if (m_curLevelIdx < 2) m_curLevelIdx = 2;
       
    }

    void UpdateEliteLevel()
    {
        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[m_levelId];
        m_contentImage.Set(eliteLevelCfg.contentImage);

        Role hero = RoleMgr.instance.Hero;
        EliteLevel eliteLevel = hero.EliteLevelsPart.GetEliteLevel(m_levelId);
        if (eliteLevel != null) eliteLevel.CheckDay();

        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(eliteLevelCfg.roomId);

        List<int> taskIds = roomCfg.GetTaskIdList();
        if (taskIds.Count != 3)
            Debuger.LogError("配置的通关条件不是3个");
        else
        {
            for (int i = 0; i < m_stars.Count; i++)
            {
                RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);
                int num = 0;
                if (eliteLevel != null && eliteLevel.GetStarState(conditionCfg.id.ToString()) == 1)
                {
                    m_stars[i].SetGrey(false);
                }
                else
                {
                    m_stars[i].SetGrey(true);
                }
            }
        }

        m_rewards.SetCount(roomCfg.rewardShow.Length);
        for (int i = 0; i < roomCfg.rewardShow.Length; i++)
        {
            m_rewards.Get<UIItemIcon>(i).Init(roomCfg.rewardShow[i][0], roomCfg.rewardShow[i][1]);
            m_rewards.Get<UIItemIcon>(i).isSimpleTip = true;
        }


        int count = 0;
        if(eliteLevel != null)
        {
            count = eliteLevel.count;
        }
        m_count.text = string.Format("{0}/{1}", EliteLevelBasicCfg.Get().dayMaxCnt-count, EliteLevelBasicCfg.Get().dayMaxCnt);
        m_stamina.text = string.Format("{0}/{1}", hero.GetStamina(), EliteLevelBasicCfg.Get().costStamina);
        
        if (!EliteLevel.CanOpen(eliteLevelCfg))
        {
            m_enterLevel.gameObject.SetActive(false);
            m_sweep.gameObject.SetActive(false);
            m_reset.gameObject.SetActive(false);
        }
        else
        {
            if(count < EliteLevelBasicCfg.Get().dayMaxCnt)
            {
                m_enterLevel.gameObject.SetActive(true);
                m_sweep.gameObject.SetActive(true);
                m_reset.gameObject.SetActive(false);
            }
            else
            {
                m_enterLevel.gameObject.SetActive(false);
                m_sweep.gameObject.SetActive(false);
                m_reset.gameObject.SetActive(true);
            }
        }
    }

    void OnSweep()
    {
        UIMgr.instance.Open<UISweepEliteLevel>(m_levelId);
    }

    void OnEnterLevel()
    {
        NetMgr.instance.EliteLevelHandler.SendEnterEliteLevel(m_levelId);
    }

    void OnListChange()
    {
        //UpdateEliteLevel();
    }

    public override void OnOpenPanelEnd()
    {
        m_eliteLevels.Reflesh();
    }

    public void SelectEliteLevel(int levelId, bool force=false)
    {
        if(!force)
        {
            EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[levelId];
            if (!EliteLevel.CanOpen(eliteLevelCfg))
            {
                UIMessage.Show(eliteLevelCfg.messageNotOpen);
                return;
            }
        }
        m_levelId = levelId;
        UpdateEliteLevel();
        m_eliteLevels.Reflesh();
    }
    void OnShowStar()
    {
        m_uiStar.Open(m_levelId);
    }

    void OnShowDescription()
    {
        m_uiDescription.Open(m_levelId);
    }

    void OnFirstReward()
    {
        m_uiFirstReward.Open(m_levelId);
    }

    public int GetLevelId()
    {
        return m_levelId;
    }

    void OnReset()
    {
        Role hero = RoleMgr.instance.Hero;
        EliteLevel eliteLevel = hero.EliteLevelsPart.GetEliteLevel(m_levelId);
        VipCfg vipCfg = VipCfg.m_cfg[hero.GetInt(enProp.vipLv)];
        if (vipCfg.specialLvlResetNum <= eliteLevel.resetCount)
        {
            UIMessage.Show(LanguageCfg.Get("reset_nums_no"));
            return;
        }
        EliteLevelResetCfg resetCfg = EliteLevelResetCfg.m_cfgs[eliteLevel.resetCount + 1];

        UIMessageBox.Open(string.Format(LanguageCfg.Get("reset_nums_cost_desc"), resetCfg.costDiamond, vipCfg.specialLvlResetNum - eliteLevel.resetCount), () =>
        {
            if (hero.GetInt(enProp.diamond) >= resetCfg.costDiamond)
            {
                NetMgr.instance.EliteLevelHandler.SendResetEliteLevel(m_levelId);
            }
            else
            {
                UIMessageBox.Open(LanguageCfg.Get("diamond_buy_desc"), () =>
                {   //打开充值界面
                }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"), LanguageCfg.Get("diamond_low"));
            }
        }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"), "reset_nums");
    }

    public override void OnClosePanel()
    {
    }
}