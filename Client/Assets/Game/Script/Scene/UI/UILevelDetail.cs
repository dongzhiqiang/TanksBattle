using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UILevelDetail : UIPanel
{
    public StateHandle mSweepLevel;
    public StateHandle mEnterBtn;

    public StateHandle mBtnPreLevel;
    public StateHandle mBtnNextLevel;

    public Text mTitle;
    public TextEx mChallengeNum;
    public TextEx mCostNum;
    public TextEx mFightNum;
    public TextEx mLevelDesc;
    public List<ImageEx> mStars = new List<ImageEx>();
    public List<ImageEx> mStars2 = new List<ImageEx>();
    public TextEx mStory;
    public ImageEx mImage;
    public ImageEx mStarDi;
    public TextEx mCurIdx;
    public List<TextEx> mConditions = new List<TextEx>();

    public StateGroup mRewardGroup;

    public RoomCfg m_cfg;
    public LevelInfo m_info;

    public int m_curIdx;

    LevelsPart m_part;
    Role m_hero;
    public override void OnInitPanel()
    {
        mSweepLevel.AddClick(OnSweepLevel);
        mEnterBtn.AddClick(EnterRoom);

        mBtnPreLevel.AddClick(ShowPrevLevel);
        mBtnNextLevel.AddClick(ShowNextLevel);
        
    }

    public void OnSweepLevel()
    {
        if (m_part.CanEnterLevel(m_info))
        {
            UIMgr.instance.Open<UISweepLevel>(m_cfg.id);
        }
    }
    
    public void ShowPrevLevel()
    {
        RoomCfg cfg = m_part.GetPrevLevel(m_cfg);
        if (cfg == null)
            return;
        m_cfg = cfg;
        UpdateDetail();
        mBtnNextLevel.gameObject.SetActive(true);
    }

    public void ShowNextLevel()
    {
        RoomCfg cfg = m_part.GetNextLevel(m_cfg);
        if (cfg == null)
        {
            UIMessage.Show(string.Format("请先通关{0}", m_cfg.roomName));
            return;
        }
        m_cfg = cfg;
        UpdateDetail();
        mBtnPreLevel.gameObject.SetActive(true);
        
    }
    
    private bool CheckPower()
    {
        if (!m_cfg.needPowerTip)
            return true;

        var ratio = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        var diff = PowerFactorCfg2.Get(ratio);
        if (diff.difficulty >= PowerDifficulty.danger)
        {
            var tipMsg = LanguageCfg.Get("level_power_tip");
            UIMessageBox.Open(tipMsg, () =>
            {
                m_part.EnterLevel(m_info);
            }, () =>
            {
            }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
            return false;
        }

        return true;
    }

    public void EnterRoom()
    {

        if (!CheckPower())
            return;

        m_part.EnterLevel(m_info);
    }

    public void UpdateDetail()
    {
        m_info = m_part.GetLevelInfoById(m_cfg.id);
        int n = int.Parse(m_cfg.id);
        m_curIdx = n - (n / 10 * 10);
        m_curIdx = m_curIdx == 0 ? 10 : m_curIdx;

        mTitle.text = m_cfg.roomName;
        int times = m_part.GetEnterNum(m_info);
        mChallengeNum.text = string.Format("{0}/{1}", m_cfg.maxChallengeNum - times, m_cfg.maxChallengeNum);
        mCostNum.text = string.Format("{0}/{1}", m_hero.GetStamina(), m_cfg.staminaCost);
        float power = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        mFightNum.text = string.Format(PowerFactorCfg2.Get(power).desc, m_hero.GetInt(enProp.powerTotal), m_cfg.powerNum);

        mStory.text = m_cfg.roomStory;
        mImage.Set(m_cfg.roomSprite);
        mCurIdx.text = m_curIdx.ToString();
        
        mRewardGroup.SetCount(m_cfg.rewardShow.Length);
        for (int i = 0; i < m_cfg.rewardShow.Length; i++)
        {
            mRewardGroup.Get<UIItemIcon>(i).Init(m_cfg.rewardShow[i][0], m_cfg.rewardShow[i][1]);
            mRewardGroup.Get<UIItemIcon>(i).isSimpleTip = true;
        }

        List<int> taskIds = m_cfg.GetTaskIdList();
        if (taskIds.Count != 3)
            Debuger.LogError("配置的通关条件不是3个");
        else
        {
            for (int i = 0; i < mConditions.Count; i++)
            {
                mConditions[i].text = "";
                RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);
                int num = 0;
                if (m_info.starsInfo.TryGetValue(conditionCfg.id.ToString() , out num) && num > 0)
                {
                    mStars2[i].gameObject.SetActive(true);
                    mConditions[i].text = string.Format("<color=green>{0}</color>", conditionCfg.endDesc);
                }
                else
                {
                    mConditions[i].text = conditionCfg.endDesc;
                    mStars2[i].gameObject.SetActive(false);
                }
            }
        }

        if (m_curIdx <= 1)
        {
            mBtnPreLevel.gameObject.SetActive(false);
            mBtnNextLevel.gameObject.SetActive(true);
        }
        else if (m_curIdx >= 10)
        {
            mBtnPreLevel.gameObject.SetActive(true);
            mBtnNextLevel.gameObject.SetActive(false);
        }
        else
        {
            mBtnPreLevel.gameObject.SetActive(true);
            mBtnNextLevel.gameObject.SetActive(true);
        }

        mLevelDesc.text = "容易";

    }

    public override void OnOpenPanel(object param)
    {
        m_cfg = param as RoomCfg;
        if (m_cfg == null)
        {
            Debug.LogError("打开错误 传入关卡配置为空");
            return;
        }
        
        m_hero = RoleMgr.instance.Hero;
        if (m_hero == null)
            return;

        m_part = m_hero.LevelsPart;
        m_info = m_part.GetLevelInfoById(m_cfg.id);
        UpdateDetail();
      
    }

    public override void OnClosePanel()
    {
    }
    
}
