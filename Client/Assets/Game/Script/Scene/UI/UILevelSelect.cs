using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UILevelSelect : UIPanel
{
    public StateGroup mNodeGroup;
    public StateGroup mLevelGroup;

    public ScrollRect mNodeScroll;
    public ScrollRect mLevelScroll;

    public Text mTitle;

    public StateHandle mBtnPreLevel;
    public StateHandle mBtnNextLevel;

    public StateHandle mStarReardBtn;
    public TextEx mStarNum;

    public static UILevelSelect instance;

    public List<RoomCfg> mCurNodeRoomList = new List<RoomCfg>();

    int m_curSelect = 0;

    LevelsPart m_part;
    public override void OnInitPanel()
    {
        mNodeGroup.AddSel(OnSelectNode);
        mLevelGroup.AddSel(OnSelectLevel);
        mStarReardBtn.AddClick(OnOpenStarReward);
    }

    public override void OnOpenPanel(object param)
    {
        instance = this;

        mNodeGroup.SetCount(RoomNodeCfg.mRoomNodeList.Count);

        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        m_part = hero.LevelsPart;
        string lastNodeID = m_part.GetLastNodeId();
        for (int i = 0; i < RoomNodeCfg.mRoomNodeList.Count; i++)
        {
            RoomNodeCfg nodeCfg = RoomNodeCfg.mRoomNodeList[i];

            UILevelSelectNodeItem node = mNodeGroup.Get<UILevelSelectNodeItem>(i);
            if (int.Parse(nodeCfg.id) >= 100)
                node.gameObject.SetActive(false);
            else
                node.Init(nodeCfg);

            //判断隐藏后面的链子
            if (int.Parse(nodeCfg.id) >= RoomNodeCfg.mRoomNodeList.Count)
                node.linkImg.gameObject.SetActive(false);
            if (!node.gameObject.activeSelf && i > 0)
                mNodeGroup.Get<UILevelSelectNodeItem>(i - 1).linkImg.gameObject.SetActive(false);

            if (lastNodeID == nodeCfg.id)
                m_curSelect = i;
        }


        mNodeGroup.SetSel(m_curSelect);

        mBtnPreLevel.gameObject.SetActive(false);
        mBtnNextLevel.gameObject.SetActive(false);

        GetComponent<ShowUpController>().Prepare();
    }

    //选择章节
    void OnSelectNode(StateHandle s, int idx)
    {
        string nodeId = RoomNodeCfg.mRoomNodeList[idx].id;
        UILevelSelectNodeItem item = s.GetComponent<UILevelSelectNodeItem>();

        if (!Main.instance.isSingle) //非单机模式 判断是否能打开
        {
            if (!m_part.CanOpenNode(item.m_cfg.id))
            {
                string curNodeId = m_part.CurNodeId;
                if (curNodeId != "0")
                {
                    if (RoleMgr.instance.Hero.GetInt(enProp.level) < RoomNodeCfg.mRoomNodeList[idx].openLevel)
                        UIMessage.Show(string.Format("等级达到{0}级才能开启", RoomNodeCfg.mRoomNodeList[idx].openLevel));
                    else
                    {
                        //最后一关赢了 并且这一章节的关卡都打过了 则提示下一章
                        LevelInfo info = m_part.GetLastLevelByNodeId(curNodeId);
                        if (info != null && info.isWin && m_part.GetLevelNumByNodeId(curNodeId) == RoomCfg.mRoomDict[curNodeId].Count)
                        {
                            //并且不是最后一章节
                            if (RoomNodeCfg.mRoomNodeList[RoomNodeCfg.mRoomNodeList.Count - 1].id != curNodeId)
                                curNodeId = (int.Parse(curNodeId) + 1) + "";
                        }
                        RoomNodeCfg cfg = RoomNodeCfg.Get(curNodeId);
                        UIMessage.Show(string.Format("请先完成{0}", cfg.mapName));
                    }
                }
                else
                {
                    UIMessage.Show(string.Format("请先完成{0}", RoomNodeCfg.mRoomNodeList[0].mapName));
                }

                if (m_curSelect != idx)
                    mNodeGroup.SetSel(m_curSelect);
                return;
            }
            for (int i = mNodeGroup.Count - 1; i >= 0; --i)
            {
                var node = mNodeGroup.Get<UILevelSelectNodeItem>(i);
                node.selIcon.gameObject.SetActive(false);
            }
            item.selIcon.gameObject.SetActive(true);
        }

        mTitle.text = RoomNodeCfg.mRoomNodeList[idx].mapName;

        if (RoomCfg.mRoomDict.ContainsKey(nodeId))
        {
            mCurNodeRoomList = RoomCfg.mRoomDict[nodeId];
            mLevelGroup.SetCount(mCurNodeRoomList.Count);

            int lastIdx = 0;
            for (int i = 0; i < mCurNodeRoomList.Count; i++)
            {
                RoomCfg cfg = mCurNodeRoomList[i];
                UILevelSelectItem level = mLevelGroup.Get<UILevelSelectItem>(i);
                level.Init(cfg);
                level.levelNum.text = string.Format("{0}", i + 1);
                if (m_part.CanOpenLevel(cfg.id) && lastIdx < i)
                    lastIdx = i;
            }


            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(mLevelScroll, lastIdx); });
        }

        m_curSelect = idx;

        RoomNodeCfg curNodeCfg = RoomNodeCfg.mRoomNodeList[m_curSelect];
        int curStarsNum = m_part.GetStarsByNodeId(curNodeCfg.id);
        int allStarsNum = 0;
        if (RoomCfg.mRoomDict.ContainsKey(curNodeCfg.id))
            allStarsNum = RoomCfg.mRoomDict[curNodeCfg.id].Count * 3;

        mStarNum.text = string.Format("{0}/{1}", curStarsNum, allStarsNum);
        UpdateStarsBtn(curNodeCfg.id);
    }

    void OnSelectLevel(StateHandle s, int idx)
    {
        UILevelSelectItem item = s.GetComponent<UILevelSelectItem>();
        if (!Main.instance.isSingle) //非单机模式 判断是否能打开
        {
            if (!m_part.CanOpenLevel(item.m_cfg))
            {
                UIMessage.Show(string.Format("请先通关{0}", m_part.GetCurChallengeLevel().roomName));
                return;
            }

        }

        if (idx < mCurNodeRoomList.Count)
            m_part.OpenLevel(mCurNodeRoomList[idx]);
        else
            Debug.LogError("此关卡不存在");
    }

    void OnOpenStarReward()
    {
        RoomNodeCfg curNodeCfg = RoomNodeCfg.mRoomNodeList[m_curSelect];
        UIMgr.instance.Open<UILevelStarReward>(curNodeCfg);        
    }


    void OnTeachAction(string arg)
    {
        switch (arg)
        {
            case "selectMaxLevel":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return;

                    for (int i = mLevelGroup.Count - 1; i >= 0; --i)
                    {
                        var level = mLevelGroup.Get<UILevelSelectItem>(i);
                        if (m_part.CanOpenLevel(level.m_cfg))
                        {
                            TeachMgr.instance.SetNextStepUIObjParam(level.transform as RectTransform);
                            break;
                        }
                    }
                }
                break;
            case "selectMaxNode":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return;

                    for (int i = mNodeGroup.Count - 1; i >= 0; --i)
                    {
                        var node = mNodeGroup.Get<UILevelSelectNodeItem>(i);
                        if (node.m_cfg != null && m_part.CanOpenNode(node.m_cfg.id))
                        {
                            TeachMgr.instance.SetNextStepUIObjParam(node.transform as RectTransform);
                            break;
                        }
                    }
                }
                break;
        }
    }

    bool OnTeachCheck(string arg)
    {
        switch (arg)
        {
            case "curMaxNode":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return false;

                    for (int i = mNodeGroup.Count - 1; i >= 0; --i)
                    {
                        var node = mNodeGroup.Get<UILevelSelectNodeItem>(i);
                        if (node.m_cfg != null && m_part.CanOpenNode(node.m_cfg.id))
                            return mNodeGroup.CurIdx == i;
                    }

                    return false;
                }
            case "isCurLevelNotDanger":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return false;
                    RoomCfg roomCfg = hero.LevelsPart.GetCurChallengeLevel();
                    if (roomCfg == null)
                        return false;
                    var ratio = (float)hero.GetInt(enProp.powerTotal) / roomCfg.powerNum;
                    var diff = PowerFactorCfg2.Get(ratio);

                    return diff.difficulty < PowerDifficulty.danger;
                }
        }

        return true;
    }

    public void RefreshLevelItem(string roomId)
    {
        for (var i = 0; i < mCurNodeRoomList.Count; ++i)
        {
            if (mCurNodeRoomList[i].id == roomId)
            {
                var dataItem = mCurNodeRoomList[i];
                var uiItem = mLevelGroup.Get<UILevelSelectItem>(i);
                uiItem.Init(dataItem);
                break;
            }
        }
    }

    public void ShowStarReward(string nodeId, int starNum)
    {
        Role hero = RoleMgr.instance.Hero;

        int idx = 0;
        if (starNum == 10)
            idx = 0;
        else if (starNum == 20)
            idx = 1;
        else if (starNum == 30)
            idx = 2;
        StarRewardCfg cfg = StarRewardCfg.Get(nodeId);
        int rewardid = cfg.normalReward[idx][0];

        List<RewardItem> itemList = RewardCfg.GetRewardsDefinite(rewardid);
        UIReward tips = UIMgr.instance.Open<UIReward>(itemList);
        Dictionary<string, int> stars;
        m_part.StarsReward.TryGetValue(nodeId, out stars);
        if (stars == null)
            stars = new Dictionary<string, int>();
        stars[starNum.ToString()] = 1;
        if (m_part.StarsReward.ContainsKey(nodeId))
            m_part.StarsReward[nodeId] = stars;
        else
            m_part.StarsReward.Add(nodeId, stars);
        UIMgr.instance.Get<UILevelStarReward>().SetOpen(idx);
        UpdateStarsBtn(nodeId);
    }

    public void UpdateStarsBtn(string nodeId)
    {
        int curStarsNum = m_part.GetStarsByNodeId(nodeId);
        Dictionary<string, int> stars;
        m_part.StarsReward.TryGetValue(nodeId, out stars);
        if (stars == null)
            stars = new Dictionary<string, int>();
        //判断宝箱应该显示的状态
        int getNum = 0;
        foreach (var starInfo in stars)
        {
            getNum += starInfo.Value;
        }
        if (getNum >= 3)
            mStarReardBtn.SetState(2);
        else
        {
            bool canGet = false;
            for (int i = 1; i <= 3; i++)
            {
                int st = (i * 10);
                if (!stars.ContainsKey(st.ToString()) && curStarsNum >= st)
                    canGet = true;
            }
            if (canGet)
                mStarReardBtn.SetState(1);
            else
                mStarReardBtn.SetState(0);
        }

    }

    override public void OnOpenPanelEnd()
    {
        GetComponent<ShowUpController>().Start();
    }
}