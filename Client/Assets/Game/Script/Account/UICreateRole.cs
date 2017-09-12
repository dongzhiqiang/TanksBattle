#region Header
/**
 * 名称：UICreateRole
 * 作者：XiaoLizhi
 * 日期：2015.2.1
 * 描述：
 **/
#endregion

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UICreateRole : UIPanel
{
    #region SerializeFields
    public InputField m_roleName;
    public StateHandle m_createNow;
    //3D模型
    public UI3DView m_roleMod;
    #endregion

    #region Fields

    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_createNow.AddClick(OnEnter);
    //    PanelBase.m_btnClose.AddClick(OnBtnClose);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_roleName.text = "勇士" + Random.Range(0, 1000000);
        m_roleMod.gameObject.SetActive(false);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
    }

    public override void OnOpenPanelEnd()
    {
        m_roleMod.gameObject.SetActive(true);
        m_roleMod.SetModel("mod_kratos_02", 1, true);
    }
    #endregion

    #region Private Methods
    void OnEnter()
    {
        if (NetMgr.instance.AccountHandler.IsWaitServer)
            return;

        if (string.IsNullOrEmpty(m_roleName.text))
        {
            UIMessageBox.Open(LanguageCfg.Get("enter_hero_name"), () => { });
            return;
        }

        string badWords;
        if (BadWordsCfg.HasBadNickNameWords(m_roleName.text, out badWords))
        {
            UIMessageBox.Open(string.Format(LanguageCfg.Get("enter_name_error"), badWords), () => { });
            return;
        }

        NetMgr.instance.AccountHandler.SendCreateRole(AccountHandler.DEF_HERO_ROLEID, m_roleName.text);
    }

    //void OnBtnClose()
    //{
    //    PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer);
    //}
    #endregion
}