using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 好友界面
/// </summary>
public class FriendsPage : MonoBehaviour {

    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //添加好友
    public StateHandle m_addFriendBtn;
    //一键领取
    public StateHandle m_onekeyGetBtn;
    //推荐好友
    public StateHandle m_recommendBtn;
    //更多的箭头
    public ImageEx m_moreArrow;
    //好友数量
    public TextEx m_friendNum;
    //今日剩余领取次数
    public TextEx m_staminaNum;

    #region Fields

    #endregion

    public void OnInit()
    {
        //添加好友
        m_addFriendBtn.AddClick(OnAddFriendBtn);
        //一键领取体力
        m_onekeyGetBtn.AddClick(OnOneKeyGetBtn);
        //推荐好友
        m_recommendBtn.AddClick(OnRecommendBtn);
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    void OnAddFriendBtn()
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        UIInputBox.Show("输入想要添加好友的名字", "", 10, (string input) =>
        {
            //input就是输入的的名字
            if (string.IsNullOrEmpty(input))   //输入为空
                UIMessage.ShowFlowTip("friend_name_null");
            else if (input == RoleMgr.instance.Hero.GetString(enProp.name))   //输入的是自己的名字
                UIMessage.ShowFlowTip("friend_add_self");
            else
                NetMgr.instance.SocialHandler.AddFriend(input);

            return false;
        });
    }

    void OnOneKeyGetBtn()
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        int count = part.unColStms.Count;
        if (count == 0)
            return;
        List<int> ids = new List<int>();
        for (int i = 0; i < count; ++i)
            ids.Add(part.unColStms[i].heroId);

        //超出上限时截掉一些
        int remain = ConfigValue.GetInt("maxGetFriendStam") - part.colStms.Count;
        if (ids.Count > remain)
            ids = ids.GetRange(0, remain);

        NetMgr.instance.SocialHandler.OnekeyStamina(ids);
    }

    void OnRecommendBtn()
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        UIMgr.instance.Open<UIFriendRecommend>();
        
    }

    //更新显示数据
    public void OnUpdateData()
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        int count = socialPart.friends.Count;
        m_GridGroup.SetCount(count);
        
        m_moreArrow.gameObject.SetActive(count >= 5 ? true : false);

        for (int i=0; i< count; i++)
        {
            UIFriendSelectItem sel = m_GridGroup.Get<UIFriendSelectItem>(i);
            sel.OnSetData(socialPart.friends[i], socialPart);

        }
        FriendMaxCfg cfg = FriendMaxCfg.Get(RoleMgr.instance.Hero.GetInt(enProp.level));
        m_friendNum.text = count + "/" + cfg.maxFriend;
        int total = ConfigValue.GetInt("maxGetFriendStam");
        int remain = total - socialPart.colStms.Count;
        m_staminaNum.text = remain + "";    //今日剩余可领体力
        if (remain > 0 && socialPart.unColStms.Count > 0)
            m_onekeyGetBtn.gameObject.SetActive(true);
        else
            m_onekeyGetBtn.gameObject.SetActive(false);
    }

    public void ResetView()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    void OnScrollChanged(Vector2 v)
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        m_moreArrow.gameObject.SetActive(socialPart.friends.Count >= 5 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }

}
