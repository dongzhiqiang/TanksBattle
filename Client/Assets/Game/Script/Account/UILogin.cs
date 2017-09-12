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

public class UILogin : UIPanel
{
    #region SerializeFields
    public InputField m_username;
    public StateHandle m_loginNow;

    
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
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_username.text = PlayerPrefs.GetString("lastUsername", "");

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

    #region Public Methods
    public void SetUsernamePassword(string username, string password)
    {
        //修改控件文件
        m_username.text = username;
        //   m_password.text = password;
        //保存账号信息
        PlayerPrefs.SetString("lastUsername", username);
        PlayerPrefs.SetString("lastPassword", password);
    }

 
    #endregion

    #region Private Methods
    void OnEnter()
    {
        if (NetMgr.instance.AccountHandler.IsWaitServer)
            return;

        if (string.IsNullOrEmpty(m_username.text))
        {
            UIMessageBox.Open(LanguageCfg.Get("enter_account"), () => { });
            return;
        }
        //保存账号信息
        PlayerPrefs.SetString("lastUsername", m_username.text);

        //尝试账号登录
          NetMgr.instance.AccountHandler.CheckAccount(m_username.text, "123456");
        
    }
    
    #endregion
}
