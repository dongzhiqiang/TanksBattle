#region Header
/**
 * 名称：UILogin
 * 作者：XiaoLizhi
 * 日期：2015.12.22
 * 描述：
 **/
#endregion

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UIRegister : UIPanel
{
    #region SerializeFields
    public InputField m_username;
    public InputField m_password;
    public StateHandle m_register;
    #endregion

    #region Fields

    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_register.AddClick(OnEnter);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_username.text = "";
        m_password.text = "";
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
    void OnEnter()
    {
        if (NetMgr.instance.AccountHandler.IsWaitServer)
            return;

        if (string.IsNullOrEmpty(m_username.text))
        {
            UIMessageBox.Open(LanguageCfg.Get("enter_account"), () => { });
            return;
        }

        if (string.IsNullOrEmpty(m_password.text))
        {
            UIMessageBox.Open(LanguageCfg.Get("enter_psw"), () => { });
            return;
        }

        //尝试账号登录
        NetMgr.instance.AccountHandler.RegisterUser(m_username.text, m_password.text);
    }
    #endregion
}