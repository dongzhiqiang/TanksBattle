using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIFriendRecommend : UIPanel {

    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;
    //刷新按钮
    public StateHandle m_refreshBtn;
    //一键添加按钮
    public StateHandle m_onekeyBtn;
    //倒计时
    public TextEx m_timeTxt;
    //推荐界面显示状态
    public StateHandle m_showState;

    float remainTime;
    List<string> adds;

    public override void OnInitPanel()
    {
        //刷新
        m_refreshBtn.AddClick(() =>
        {
            SocialPart part = RoleMgr.instance.Hero.SocialPart;
            if (TimeMgr.instance.GetTimestamp() - part.recommendUptime > ConfigValue.GetInt("friendRecRefresh"))
                NetMgr.instance.SocialHandler.ReqRefreshRecommend();
             else
                UIMessage.Show("刷新时间冷却中");
        });
        //一键申请
        m_onekeyBtn.AddClick(() =>
        {
            SocialPart part = RoleMgr.instance.Hero.SocialPart;
            int addNum = FriendMaxCfg.Get(RoleMgr.instance.Hero.GetInt(enProp.level)).maxFriend - part.friends.Count;    //剩余可以添加的数量
            if (addNum <= 0)
                UIMessage.ShowFlowTip("friend_list_full");
            else
            {
                int minNum = addNum > part.recommends.Count ? part.recommends.Count : addNum;
                adds = new List<string>();
                for(int i = 0; i < minNum; ++i)
                    adds.Add(part.recommends[i].name);

                Role role = RoleMgr.instance.Hero;
                Friend ownInfo = part.MakeFriendDataByRole(role);
                NetMgr.instance.SocialHandler.OneKeyAddFriend(adds, ownInfo);
            }
        });
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    public override void OnOpenPanel(object param)
    {
        // UpdatePanel();
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        NetMgr.instance.SocialHandler.ReqRecommendFriend(part.isRecFirst);
    }
    public override void OnUpdatePanel()
    {
        remainTime -= Time.unscaledDeltaTime;
        if (remainTime > 0)
        {
            m_refreshBtn.GetComponent<ImageEx>().SetGrey(true);
            m_refreshBtn.GetComponent<StateHandle>().enabled = false;
            m_timeTxt.text = StringUtil.FormatTimeSpan((long)remainTime);
        }
        else
        {
            remainTime = 0;
            m_refreshBtn.GetComponent<ImageEx>().SetGrey(false);
            m_refreshBtn.GetComponent<StateHandle>().enabled = true;
            m_timeTxt.text = "";
        }
    }

    //更新日志界面数据
    public void UpdatePanel()
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        int count = part.recommends.Count;
        m_showState.SetState(count > 0 ? 0 : 1);
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(count > 6? true : false);
        int countCanAdd = 0;
        for (int i = 0; i < count; i++)
        {
            UIFriendRecommendItem sel = m_GridGroup.Get<UIFriendRecommendItem>(i);
            int state = sel.OnSetData(part.recommends[i]);
            if (state == 0)
                countCanAdd++;
        }
        m_onekeyBtn.gameObject.SetActive(countCanAdd > 0 ? true : false);   //有一个以上才可以显示一键添加
        remainTime = part.recommendUptime + ConfigValue.GetInt("friendRecRefresh") - TimeMgr.instance.GetTimestamp();   //剩下时间

    }
    public override void OnClosePanel()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    void OnScrollChanged(Vector2 v)
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        m_moreArrow.gameObject.SetActive(part.recommends.Count > 6 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }
    
}
