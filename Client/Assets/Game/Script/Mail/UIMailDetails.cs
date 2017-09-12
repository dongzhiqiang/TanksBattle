using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 邮件详情
/// </summary>
public class UIMailDetails : MonoBehaviour
{
    public TextEx m_title;
    public TextEx m_content;
    public TextEx m_sender;
    //领取按钮
    public StateHandle m_rewardBtn;
    //有无附件
    public StateHandle m_attachState;
    //物品切换栏
    public StateHandle m_iconBarState;
    //物品格子们
    public UIItemIcon[] m_itemIcons;
    //左切页
    public StateHandle m_leftHandle;
    //右切页
    public StateHandle m_rightHandle;
    
    string m_mailId;
    //附件索引
    int m_index;
    int m_itemCount;
    int m_totalPage;
    List<MailItemQualitySortVo> m_attach;
    const int GRID_MAX = 4;

    #region PrivateMethod

    void CheckPageIndex()
    {
        int r = m_index * GRID_MAX + GRID_MAX > m_itemCount ? m_itemCount - m_index * GRID_MAX : GRID_MAX;
        List<MailItemQualitySortVo> items = m_attach.GetRange(m_index * GRID_MAX, r);

        int count = items.Count;
        for (int i = 0; i < GRID_MAX; i++)
        {
            UIItemIcon icon = m_itemIcons[i];
            if (i < count)
            {
                icon.gameObject.SetActive(true);
                icon.Init(items[i].itemId, items[i].num);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }

        if (m_itemCount <= GRID_MAX)
        {
            m_iconBarState.SetState(3);
            return;
        }

        if (m_index == 0)
            m_iconBarState.SetState(0);
        else if (m_index > 0 && m_index + 1 < m_totalPage)
            m_iconBarState.SetState(1);
        else
            m_iconBarState.SetState(2);
    }
    #endregion

    public void InitPage()
    {
        m_rewardBtn.AddClick(OnRewardBtn);
        m_leftHandle.AddClick(OnLefthandle);
        m_rightHandle.AddClick(OnRightHandle);
    }

    private void OnRewardBtn()
    {
        NetMgr.instance.MailHandler.SendReadRewardMail(m_mailId, (int)MailReadType.Reward);
    }
    
    private void OnLefthandle()
    {
        if (m_index > 0) { m_index--; }
        CheckPageIndex();
    }

    private void OnRightHandle()
    {
        if (m_index < m_totalPage - 1) { m_index++; }
        CheckPageIndex();
    }

    public void ShowPage(object param)
    {
        MailPart mailPart = RoleMgr.instance.Hero.MailPart;
        Mail mail = mailPart.mailList[(int)param];
        m_mailId = mail.mailId;
        if(mail.status == (int)MailStatus.Unread && mail.attach.Count == 0)  //未读的无附件邮件才发送
            NetMgr.instance.MailHandler.SendReadRewardMail(mail.mailId, (int)MailReadType.Open);
        m_title.text = mail.title;
        m_content.text = mail.content;
        m_sender.text = mail.sender;

        if (mail.attach.Count > 0)//有附件
        {
            m_attachState.SetState(1);

            //设置物品格子数据
            m_attach = mail.attach;
            m_itemCount = mail.attach.Count;
            m_index = 0;
            m_totalPage = m_itemCount / GRID_MAX + 1;
    
            CheckPageIndex();
        }
        else
            m_attachState.SetState(0);

    }
    
}
