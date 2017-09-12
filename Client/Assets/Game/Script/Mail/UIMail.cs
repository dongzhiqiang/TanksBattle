using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 邮件系统界面
/// </summary>
public class UIMail : UIPanel 
{
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //一键领取
    public StateHandle m_OnekeyCollect;
    //没有邮件
    public ImageEx m_emptyText;
    //邮件详细内容
    public UIMailDetails m_details;
    //是否有邮件
    public StateHandle m_hasMail;

    #region Fields
    List<string> m_attachIds = new List<string>();
    GameObject lastSelImg;

    #endregion

    #region Private Methods

    void OnSelectMail(StateHandle select, int idx)
    {
        UIMailSelectItem item = select.GetComponent<UIMailSelectItem>();
        MailPart mailPart = RoleMgr.instance.Hero.MailPart;
        if (idx < mailPart.mailList.Count)
        {
            if (lastSelImg != null)
                lastSelImg.SetActive(false);
            item.m_selectImg.gameObject.SetActive(true);
            lastSelImg = item.m_selectImg.gameObject;
            m_details.ShowPage(idx);
        }
        else
            Debug.LogError("邮件不存在");

    }
    //一键领取
    void OnOneKeyCollect()
    {
        NetMgr.instance.MailHandler.SendOneKeyItems(m_attachIds);
    } 
    //检查是否有可领的附件
    void CheckCollectStatus()
    {
        MailPart mailPart = RoleMgr.instance.Hero.MailPart;
        m_attachIds = mailPart.CheckMailsCanReward();
        if (m_attachIds.Count > 0)
        {
            m_OnekeyCollect.GetComponent<ImageEx>().SetGrey(false);
            m_OnekeyCollect.GetComponent<StateHandle>().enabled = true;
        }
        else
        {
            //不可领取的状态
            m_OnekeyCollect.GetComponent<ImageEx>().SetGrey(true);
            m_OnekeyCollect.GetComponent<StateHandle>().enabled = false;
        }
    }

    #endregion

    //刷新邮件
    /**
     * 刷新面板 
     * @param needFirst 是否需要选中第一个
     * */
    public void UpdateMailPanel(bool needFirst = false)
    {
        MailPart mailPart = RoleMgr.instance.Hero.MailPart;
        int count = mailPart.mailList.Count;
        if (count > 0)
        {
            m_hasMail.SetState(0);
            m_emptyText.gameObject.SetActive(false);
            m_OnekeyCollect.gameObject.SetActive(true);
            if (needFirst)
                m_GridGroup.SetSel(0); //自动打开第一封邮件
        }
        else
        {
            m_hasMail.SetState(1);
            m_emptyText.gameObject.SetActive(true);
            m_OnekeyCollect.gameObject.SetActive(false);
        }

        m_GridGroup.SetCount(count);
        
        for(int i = 0; i<count; i++)
        {
            UIMailSelectItem item = m_GridGroup.Get<UIMailSelectItem>(i);
            item.SetData(mailPart.mailList[i]);
        }
        CheckCollectStatus();//检测一键领取状态
    }
    
    public override void OnInitPanel()
    {
        m_GridGroup.AddSel(OnSelectMail);
		m_OnekeyCollect.AddClick (OnOneKeyCollect);
        m_details.InitPage();
    }
    
    public override void OnOpenPanel(object param)
    {
        UpdateMailPanel(true);
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });

    }

    public override void OnClosePanel()
    {
        RoleMgr.instance.Hero.MailPart.Sort();//重排一下阅读状态排序，注意需求打开邮件阅读时顺序不用修改。

        if (lastSelImg != null)
            lastSelImg.SetActive(false);
        lastSelImg = null;
    }
    
}
