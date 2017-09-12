using UnityEngine;
using System.Collections;

public class UILevelAreaNotice : UILevelArea
{

    #region Fields
    public GameObject m_topGo;
    public GameObject m_bottomGo;
    public TextEx m_topNotice;
    public TextEx m_bottomNotice;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.notice; } }
    public override bool IsOpenOnStart { get { return false; } }

    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        m_topGo.gameObject.SetActive(false);
        m_bottomGo.gameObject.SetActive(false);
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        m_topGo.gameObject.SetActive(false);
        m_bottomGo.gameObject.SetActive(false);
    }

    protected override void OnUpdateArea()
    {

    }

    //关闭
    protected override void OnCloseArea()
    {
     
    }

    protected override void OnRoleBorn()
    {

    }

    public void SetTopNotice(string desc)
    {
        if(!this.m_parent.IsOpen)
        {
            Debuger.LogError("战斗界面没有显示的时候要设置显示关卡提示");
            return;
        }
        if (!this.IsOpen)
            this.OpenArea();
        m_topGo.gameObject.SetActive(true);
        m_topNotice.text = desc;
        if (string.IsNullOrEmpty(m_bottomNotice.text))
            m_bottomGo.gameObject.SetActive(false);

    }

    public void SetBottomNotice(string desc)
    {
        if (!this.m_parent.IsOpen)
        {
            Debuger.LogError("战斗界面没有显示的时候要设置显示独白");
            return;
        }
        if (!this.IsOpen)
            this.OpenArea();

        m_bottomGo.gameObject.SetActive(true);
        m_bottomNotice.text = desc;

        if (string.IsNullOrEmpty(m_topNotice.text))
            m_topGo.gameObject.SetActive(false);
    }

    public void CloseTopNotice()
    {
        if (!this.m_parent.IsOpen || !this.IsOpen)
            return;
        m_topNotice.text = "";
        m_topGo.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(m_bottomNotice.text))
        {
            m_bottomGo.gameObject.SetActive(false);
            CloseArea();
        }
    }

    public void CloseBottomNotice()
    {
        if (!this.m_parent.IsOpen || !this.IsOpen)
            return;
        m_bottomNotice.text = "";
        m_bottomGo.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(m_topNotice.text))
        {
            m_topGo.gameObject.SetActive(false);
            CloseArea();
        }
    }
    #endregion

    #region Private Methods

    #endregion


}
