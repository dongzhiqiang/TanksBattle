using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
///  好友界面 
///  1.好友信息打开界面的时候会请求，好友信息筛选uptime有变动的才更新；申请信息只用登录时发的，不会每次打开界面请求。   
///  2.有新的好友和申请时 服务端会推送，两个都会更新。
/// </summary>
public class UIFriend : UIPanel
{
    //页面状态控制
    public StateHandle m_pageState;
    //页签按钮
    public StateGroup m_tab;
    //好友列表
    public FriendsPage m_friendsPage;
    //申请列表
    public FriReqPage m_reqPage;
    //叹号提示
    public ImageEx m_reqTip;

    #region Fields
    //是否初始化了
    bool isFirst;
    #endregion

    #region Mono Frame
    #endregion

    #region Private Methods
    void OnSelectTab(StateHandle select, int idx)
    {
        m_pageState.SetState(idx);

    }
    #endregion

    public override void OnInitPanel()
    {
        m_tab.AddSel(OnSelectTab);
        m_friendsPage.OnInit();
        m_reqPage.OnInit();

    }

    public override void OnOpenPanel(object param)
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        NetMgr.instance.SocialHandler.SendReqFriendData();

        UpdateReqPage();//请求界面直接设置

        m_tab.SetSel(0);
        m_pageState.SetState(0);
        m_friendsPage.ResetView();
        m_reqPage.ResetView();
    }

    public override void OnClosePanel()
    {
    }
    
    //更新好友界面
    public void UpdateFriendPage()
    {
        m_friendsPage.OnUpdateData();

    }

    public void UpdateReqPage()
    {
        m_reqPage.OnUpdateData();
    }

    public void SetReqTip(bool isShow)
    {
        m_reqTip.gameObject.SetActive(isShow);
    }

}
