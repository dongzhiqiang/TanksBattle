#region Header
/**
 * 名称：UISelectServer
 
 * 日期：2015.11.27
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UISelectServer : UIPanel
{
    #region SerializeFields
    public StateHandle m_btnClose;
    public StateGroup m_left;
    public UISelectServerPageMy m_pageMy;
    public UISelectServerPageRecommend m_pageRecommend;
    public UISelectServerPageArea m_pageArea;
    #endregion

    #region Fields    
    #endregion

    #region Properties    
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnClose.AddClick(OnBtnClose);
        m_left.AddSel(OnSel);
        m_pageMy.OnInitPage(this);
        m_pageRecommend.OnInitPage(this);
        m_pageArea.OnInitPage(this);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        //悄悄获取服务器列表
        //NetMgr.instance.AccountHandler.FetchServerList();
        UpdateUI();
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
    void OnBtnClose()
    {
        LoginInfo loginInfo = NetMgr.instance.AccountHandler.LoginInfo;
        if (loginInfo.roleList.Count == 0)//没有角色信息
            PlayerStateMachine.Instance.GotoState(enPlayerState.login);
        else
            UIMgr.instance.Close<UISelectServer>();

    }
    void OnSel(StateHandle s, int idx)
    {
        if (idx == 0)
            m_pageMy.OnOpenPage();
        else if (idx == 1)
            m_pageRecommend.OnOpenPage();
        else
        {
            UIServerAreaItem item = s.Get<UIServerAreaItem>();
            m_pageArea.OnOpenPage(item.areaName.text);
        }
    }
    #endregion

    #region Public Methods
    public void UpdateUI()
    {
        LoginInfo loginInfo = NetMgr.instance.AccountHandler.LoginInfo;

        //初始化左边的列表
        int i = 2;
        m_left.SetCount(i + loginInfo.serversByArea.Count);
        foreach (string areaName in loginInfo.serversByArea.Keys)
        {
            m_left.Get<UIServerAreaItem>(i).areaName.text = areaName;
            ++i;
        }

        if (loginInfo.serverList.Count <= 0)
            m_left.SetSel(0);
        //有上次登录的服务器就选上次登录的
        else if (loginInfo.roleList.Count > 0)
            m_left.SetSel(0);
        //有推荐服，选推荐服页
        else if (loginInfo.serversByRecommend.Count != 0)
            m_left.SetSel(1);
        else
            m_left.SetSel(2);
    }
    #endregion
}