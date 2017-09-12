using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UILevelStarReward : UIPanel {
    #region Fields
    public StateHandle[] mStateHandles;
    public StateGroup[] mRewardGroups;
    public StateHandle[] mGetHandles;

    RoomNodeCfg mNodeCfg;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        mGetHandles[0].AddClick(OnClickStar1);
        mGetHandles[1].AddClick(OnClickStar2);
        mGetHandles[2].AddClick(OnClickStar3);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        mNodeCfg = (RoomNodeCfg)param;
        Role hero = RoleMgr.instance.Hero;
        int curStarsNum = hero.LevelsPart.GetStarsByNodeId(mNodeCfg.id);
        int allStarsNum = 0;
        if (RoomCfg.mRoomDict.ContainsKey(mNodeCfg.id))
            allStarsNum = RoomCfg.mRoomDict[mNodeCfg.id].Count * 3;

        //mStarNum.text = string.Format("{0}/{1}", curStarsNum, allStarsNum);
        Dictionary<string, int> stars;
        hero.LevelsPart.StarsReward.TryGetValue(mNodeCfg.id, out stars);
        if (stars == null)
            stars = new Dictionary<string, int>();
        for (int i = 0; i < mStateHandles.Length; i++)
        {
            int num = (i + 1) * 10;
            int state = 0;
            stars.TryGetValue(num.ToString(), out state);
            if (state != 1)
            {
                if (curStarsNum >= num)
                {
                    mStateHandles[i].SetState(1);
                }
                else
                {
                    mStateHandles[i].SetState(0);
                }
            }
            else
            {
                mStateHandles[i].SetState(2);
            }
        }

        StarRewardCfg cfg = StarRewardCfg.Get(mNodeCfg.id);
        for (int i = 0; i < mRewardGroups.Length; i++)
        {
            int rewardid = cfg.normalReward[i][0];
            List<RewardItem> itemList = RewardCfg.GetRewardsDefinite(rewardid);
            StateGroup rewardGroup = mRewardGroups[i];
            rewardGroup.SetCount(itemList.Count);
            for (int j = 0; j < itemList.Count; j++)
            {
                rewardGroup.Get<UIItemIcon>(j).Init(itemList[j].itemId, itemList[j].itemNum);
                rewardGroup.Get<UIItemIcon>(j).isSimpleTip = true;
            }
        }
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
    void OnClickStar1()
    {
        SendGet(10);
    }
    void OnClickStar2()
    {
        SendGet(20);
    }
    void OnClickStar3()
    {
        SendGet(30);
    }

    void SendGet(int starNum)
    {
        LevelStarReqVo request = new LevelStarReqVo();
        request.nodeId = mNodeCfg.id;
        request.starNum = starNum;
        NetMgr.instance.LevelHandler.SendStar(request);
    }
    #endregion

    public void SetOpen(int idx)
    {
        if (idx > 2)
            return;
        mStateHandles[idx].SetState(2);
    }
}
