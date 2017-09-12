#region Header
/**
 * 名称：UILogin
 
 * 日期：2015.11.27
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class UILogin2 : UIPanel
{
    #region SerializeFields
    //登录按钮
    public StateHandle m_loginNow;
    //选择服务器点击
    public StateHandle m_openServer;
    //上次登录服务器里的名字
    public TextEx m_lastName;
    //切换账号
    public StateHandle m_changeAcc;
    public StateHandle m_state;
    #endregion

    #region Fields
    
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_loginNow.AddClick(OnEnter);

        m_openServer.AddClick(OnOpenServer);
        m_changeAcc.AddClick(OnChangeAccount);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_state.SetState(0);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Public Methods


    public void UpdateStateUI()
    {
        //判断一下是否已有账号
        LoginInfo loginInfo = NetMgr.instance.AccountHandler.LoginInfo;
        if (loginInfo.roleList.Count > 0)
        {
            m_lastName.text = loginInfo.roleList[0].serverInfo.name;
            m_state.SetState(1);
        }
        else
        {
            UIMgr.instance.Open<UISelectServer>();
            m_state.SetState(0);
        }

    }
    #endregion

    #region Private Methods
    void OnEnter()
    {
        if (NetMgr.instance.AccountHandler.IsWaitServer)
            return;
        
        LoginInfo loginInfo = NetMgr.instance.AccountHandler.LoginInfo;
        NetMgr.instance.AccountHandler.ConnectServer(loginInfo.roleList[0].serverInfo);
    }


    void OnOpenServer()
    {
        UIMgr.instance.Open<UISelectServer>();
    }

    //切换账号
    void OnChangeAccount()
    {
        PlayerStateMachine.Instance.GotoState(enPlayerState.login);
    }

    #endregion
}
