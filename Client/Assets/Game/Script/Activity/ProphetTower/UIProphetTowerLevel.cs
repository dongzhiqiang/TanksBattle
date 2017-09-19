using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIProphetTowerLevel : UIPanel
{

    public StateHandle mBtnTabChallenge;
    public StateHandle mBtnTabRandom;
    
    public GameObject mChallengePage;
    public GameObject mRandomPage;

    //挑战模式
    public TextEx mDesc;
    public StateGroup mReward;
    public StateHandle mBtnRank;
    public StateHandle mBtnChallenge;
    public TextEx mTopLevel;
    public TextEx mSuitFightNum;
    public TextEx mMyFightNum;
    public StateGroup mLayerGroup;

    //随机模式
    public StateGroup mBoxGroup;
    public StateHandle mBtnRandom;
    public TextEx mLevel;
    public TextEx mStage;
    public TextEx mTimes;
    public TextEx mCurRandomNum;
    public GameObject mRandomAni;
    public ImageEx mMask;
    public StateHandle m_help;

    public static ProphetTowerType mPrevEnterType = ProphetTowerType.Challenge;
    public static bool isShowAni = false;
    public static bool isFirstOpen = true;

    int m_topLevel;

    public RoomCfg m_roomCfg;
    public ProphetTowerCfg m_towerCfg;
    Role m_hero;
    ActivityPart m_actPart;
    public override void OnInitPanel()
    {
        mBtnTabChallenge.AddClick(OnOpenChallenge);
        mBtnTabRandom.AddClick(OnOpenRandom);

        mBtnRank.AddClick(OnOpenRank);
        mBtnChallenge.AddClick(OnChallenge);
        mBtnRandom.AddClick(OnRandom);

        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.prophetTower).ruleIntro);
        });

        mBoxGroup.SetCount(5);
        for(int i = 0; i < 5; i++)
        {
            mBoxGroup.Get<StateHandle>(i).AddClickEx(OnClickBox);
        }

        for(int i =0; i < 4; i ++)
        {
            mLayerGroup.Get<StateHandle>(i).AddClickEx(OnClickLayer);
        }
        isFirstOpen = true;
    }

    public override void OnOpenPanel(object param)
    {
        m_hero = RoleMgr.instance.Hero;
        m_actPart = m_hero.ActivityPart;
        if (m_hero == null)
            return;

        mLayerGroup.GetComponent<SimpleHandle>().enabled = false;

        m_topLevel = m_hero.GetInt(enProp.towerLevel);

        //int level = m_topLevel == 0 ? 1 : m_topLevel;
        int level = m_topLevel + 1;
        m_towerCfg = ProphetTowerCfg.Get(level);
        if (m_towerCfg == null)
            return;

        m_roomCfg = RoomCfg.GetRoomCfgByID(m_towerCfg.roomId);
        if (m_roomCfg == null)
            return;

        //mBtnTabChallenge.SetState(1);
        //mBtnTabRandom.SetState(0);
        if (isFirstOpen)
            OnOpenChallenge();
        isFirstOpen = false;
    }
    
    public override void OnClosePanel()
    {
    }

    void OnOpenChallenge()
    {
        if (m_topLevel == 0 || !isShowAni)
            mLayerGroup.GetComponent<SimpleHandle>().enabled = false;
        else
            mLayerGroup.GetComponent<SimpleHandle>().enabled = true;

        for (int i = 0; i < 4; i++)
        {
            int level = m_topLevel + i;
            ImageEx image = mLayerGroup.Get<ImageEx>(i);
            image.gameObject.GetComponentInChildren<TextEx>().text = level.ToString();
            if (m_topLevel == 0)
                level++;
            ProphetTowerCfg cfg = ProphetTowerCfg.Get(level);
            if (cfg.isBoss == 1)
                image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_02");
            else
                image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_04");
        }
        
        mMyFightNum.text = m_hero.GetInt(enProp.powerTotal).ToString();
        mSuitFightNum.text = m_roomCfg.powerNum.ToString();
        mDesc.text = m_roomCfg.roomStory;
        mTopLevel.text = m_topLevel.ToString()+"层";
        
        List<RewardItem> itemList = RewardCfg.GetRewardsDefinite(m_towerCfg.rewardId);
        mReward.SetCount(itemList.Count);
        for (int i = 0; i < itemList.Count; i++)
        {
            mReward.Get<UIItemIcon>(i).Init(itemList[i].itemId, itemList[i].itemNum);
            mReward.Get<UIItemIcon>(i).isSimpleTip = true;
        }
        

        if (m_topLevel != 0 && isShowAni)
            StartCoroutine(OnChangeImage());
        else
        {
            ProphetTowerCfg cfg = ProphetTowerCfg.Get(m_topLevel + 1);
            ImageEx image = mLayerGroup.Get<ImageEx>(1);
            if (cfg.isBoss == 1)
                image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_01");
            else
                image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_03");
        }
    }

    IEnumerator OnChangeImage()
    {
        yield return new WaitForSeconds(0.9f);

        ProphetTowerCfg cfg = ProphetTowerCfg.Get(m_topLevel + 1);
        ImageEx image = mLayerGroup.Get<ImageEx>(1);
        if (cfg.isBoss == 1)
            image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_01");
        else
            image.Set("ui_zhanzhen_yuyanzhe_cengjitubiao_03");
    }

    void OnOpenRandom()
    {
        mMask.gameObject.SetActive(true);
        mRandomAni.gameObject.SetActive(false);
        mCurRandomNum.gameObject.SetActive(false);
        ProphetTowerCfg towerCfg = ProphetTowerCfg.Get(m_topLevel);
        int openLevel = ConfigValue.GetInt("openTowerRandomLevel");
        if (m_topLevel < openLevel)
        {
            UIMessage.Show(string.Format(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.TOWER_CANT_OPEN_RANDOM), openLevel));
            mBtnTabChallenge.SetState(1);
            mBtnTabRandom.SetState(0);
            return;
        }

        int enterNum = GetEnterNum();
        if (!TimeMgr.instance.IsToday((long)m_hero.GetInt(enProp.towerEnterTime)))
            enterNum = 0;

        for (int i = 0; i < 5; i++)
        {
            StateHandle handle = mBoxGroup.Get<StateHandle>(i);
            if (enterNum > i)
                handle.SetState(1);
            else
                handle.SetState(0);
            TextEx text = handle.GetComponentInChildren<TextEx>();
            if (text != null)
                text.text = towerCfg.stage + "阶";

            if (m_actPart.prophetTower.getRewardState.Count >= 5)
            {
                if (m_actPart.prophetTower.getRewardState[i] == 1)
                    handle.SetState(2);
            }
        }

        mLevel.text = m_topLevel.ToString() + "层";
        mStage.text = towerCfg.stage.ToString();
        mTimes.text = enterNum.ToString() + "/" + ConfigValue.GetInt("towerRandomNum");

        int count = m_actPart.prophetTower.randomId.Count;
        if (count > 0 && enterNum < count)
        {
            int layer = m_actPart.prophetTower.randomId[enterNum];
            mCurRandomNum.text = layer.ToString();
        }
    }

    void OnOpenRank()
    {
        UIMgr.instance.Open<UIRank>(RankMgr.RANK_TYPE_PREDICTOR);
    }
    void OnChallenge()
    {
        if (!CheckPower())
            return;

        OnChallengeEnter();
    }

    void OnChallengeEnter()
    {
        mPrevEnterType = ProphetTowerType.Challenge;
        NetMgr.instance.ActivityHandler.SendTowerEnter(ProphetTowerType.Challenge);
    }


    void OnRandom()
    {
        if (GetEnterNum() >= ConfigValue.GetInt("towerRandomNum"))
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.DAY_MAX_CNT));
            return;
        }
        mPrevEnterType = ProphetTowerType.Random;
        NetMgr.instance.ActivityHandler.SendTowerEnter(ProphetTowerType.Random);
    }
    
    int GetEnterNum()
    {
        int enterNum = m_hero.GetInt(enProp.towerEnterNums);
        if (!TimeMgr.instance.IsToday((long)m_hero.GetInt(enProp.towerEnterTime)))
        {
            enterNum = 0;
            m_actPart.prophetTower.randomId.Clear();
            for(int i = 0; i <  m_actPart.prophetTower.getRewardState.Count; i++)
            {
                m_actPart.prophetTower.getRewardState[i] = 0;
            }
                
        }
        return enterNum;
    }

    bool CheckPower()
    {
        if (!m_roomCfg.needPowerTip)
            return true;

        var ratio = (float)m_hero.GetInt(enProp.powerTotal) / m_roomCfg.powerNum;
        var diff = PowerFactorCfg2.Get(ratio);
        if (diff.difficulty >= PowerDifficulty.danger)
        {
            var tipMsg = LanguageCfg.Get("level_power_tip");
            UIMessageBox.Open(tipMsg, () =>
            {
                OnChallengeEnter();
            }, () =>
            {
            }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
            return false;
        }
        return true;
    }
   
    
    public void OnEnterRes(EnterTowerRes resData)
    {
        ProphetTowerType type = (ProphetTowerType)resData.towerType;
        if (type == ProphetTowerType.Challenge)
            LevelMgr.instance.ChangeLevel(resData.roomId, resData.towerType);
        else if(type == ProphetTowerType.Random)
        {
            int num = GetEnterNum();
            m_actPart.prophetTower = resData.towerInfo;
            int level = m_actPart.prophetTower.randomId[num];
            ProphetTowerCfg cfg = ProphetTowerCfg.Get(level);
            mCurRandomNum.text = level.ToString();
            StartCoroutine(OnEnterRandom(cfg));
        }
    }

    public static bool CheckOpen()
    {
        if (LevelMgr.instance.PrevRoomId.StartsWith("prophetTower"))
        {
            if (ProphetTowerLevel.prevIsWin)
                isShowAni = true;
            UIProphetTowerLevel ui = UIMgr.instance.Open<UIProphetTowerLevel>();
            if (mPrevEnterType == ProphetTowerType.Random)
                ui.OnOpenRandom();
            else if (mPrevEnterType == ProphetTowerType.Challenge)
                ui.OnOpenChallenge();

            isShowAni = false;
            return true;
        }
        return false;
    }

    public void OnGetReward(int idx)
    {
        if (idx < 0 || idx >= 5)
        {
            Debuger.LogError("领取随机奖励的索引不对 没有第" + idx + "个");
            return;
        }
        StateHandle handle = mBoxGroup.Get<StateHandle>(idx);
        handle.SetState(2);
        ProphetTowerStageCfg cfg = ProphetTowerStageCfg.Get(m_towerCfg.stage);
        List<RewardItem> itemList = RewardCfg.GetRewardsDefinite(cfg.rewardId);
        UIReward tips = UIMgr.instance.Open<UIReward>(itemList);
        m_actPart.prophetTower.getRewardState[idx] = 1;
    }

    IEnumerator OnEnterRandom(ProphetTowerCfg cfg)
    {
        mMask.gameObject.SetActive(false);
        mCurRandomNum.gameObject.SetActive(false);
        mRandomAni.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        mRandomAni.gameObject.SetActive(false);
        mCurRandomNum.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        LevelMgr.instance.ChangeLevel(cfg.roomId, ProphetTowerType.Random);
    }

    void OnClickBox(StateHandle h)
    {
        if (h.m_curState != 1)
            return;

        int idx = 0;
        for(int i = 0; i < 5; i++)
        {
            if (mBoxGroup.Get<StateHandle>(i) == h)
            {
                idx = i;
                break;
            }
        }
        GetTowerRewardReq req = new GetTowerRewardReq();
        req.idx = idx;
        NetMgr.instance.ActivityHandler.SendGetTowerReward(req);
    }

    void OnClickLayer(StateHandle h)
    {
        int idx = 0;
        for (int i = 0; i < 5; i++)
        {
            if (mLayerGroup.Get<StateHandle>(i) == h)
            {
                idx = i;
                break;
            }
        }

        int level = m_topLevel + idx;
        ProphetTowerCfg cfg = ProphetTowerCfg.Get(level);
        List<RewardItem> itemList = RewardCfg.GetRewardsDefinite(cfg.rewardId);
        UIRewardTip tips = UIMgr.instance.Open<UIRewardTip>(itemList);
        tips.SetTitle("奖励");
    }
}
