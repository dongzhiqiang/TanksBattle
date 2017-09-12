using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MailPart : RolePart 
{
    #region Fields
    //邮件信息列表，排序后的
    List<Mail> m_mailList = new List<Mail>();

    #endregion

    #region Properties
    public override enPart Type { get { return enPart.mail; } }

    public List<Mail> mailList { get { return m_mailList; } }
    #endregion


    #region Mono Frame
    #endregion

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.mails != null)
            UpdateMailsInfo(vo.mails);
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_mailList.Count > 0)
            m_mailList.Clear();
    }
    //更新邮件  isAdd是否新的邮件
    public void UpdateMailInfo(Mail mail, bool isAdd, List<Mail> delMails)
    {
        if(delMails != null && delMails.Count > 0)  //超出上限的邮件自动删除 更新
        {
            for(int i=0;i<delMails.Count;i++)
            {
                DeleteMail(delMails[i].mailId, false);
            }
        }

        SortAttachQuality(mail);
        if (isAdd)
        {
            m_mailList.Add(mail);
            Sort();   //不是添加的不更改顺序了，保留已阅读的邮件位置
        }
        else
        {
            int len = m_mailList.Count;
            for (int i = 0; i < len; i++)
            {
                if (m_mailList[i].mailId == mail.mailId)
                {
                    m_mailList[i] = mail;
                    break;
                }
            }
        }
     
        ChangeMailIconTip();

    }
    //全部更新邮件信息
    public void UpdateMailsInfo(List<Mail> mails)
    {
        if(m_mailList.Count > 0)
            m_mailList.Clear();
        m_mailList = mails;
        for (int i = 0; i < m_mailList.Count; i++)
            SortAttachQuality(m_mailList[i]);
        Sort();
        ChangeMailIconTip();

    }

    public void DeleteMail(string mailId, bool needFire = true)
    {
        if (string.IsNullOrEmpty(mailId))  //检查下
            return;
        int len = m_mailList.Count;
        int i = -1;
        for (i = 0; i < len; i++)
        {
            if (m_mailList[i].mailId == mailId)
                break;
        }
        if (i == -1)//没找到
            return;
        m_mailList.RemoveAt(i);
        if (needFire)
            ChangeMailIconTip();
    }

    public void DeleteMailsByIds(List<string> ids)
    {
        int len = ids.Count;
        for (int i = 0;i < len; i++)
        {
            DeleteMail(ids[i], false);
        }
        ChangeMailIconTip();
    }

    //删除所有邮件
    public void ClearMails()
    {
        if(m_mailList.Count != 0)
            m_mailList.Clear();
    }
    //检测是否有附件可以领取，返回可领的所有邮件id
    public List<string> CheckMailsCanReward()
    {
        int count = m_mailList.Count;
        List<string> ids = new List<string>();
        for(int i=0; i<count; i++)
        {
            if (m_mailList[i].attach.Count > 0)
                ids.Add(m_mailList[i].mailId);
        }
        return ids;
    }

    //对邮件进行排序
    public void Sort()
    {
        m_mailList.SortEx((Mail a, Mail b) =>
        {
            //先按未读状态，再按时间
            int value = a.status.CompareTo(b.status);
            if (value == 0)
                value = b.sendTime.CompareTo(a.sendTime);
            return value;
        });
    }

    #region Private Methods

    void ChangeMailIconTip()
    {
        bool needTip = false;
        for(int i = 0,count = m_mailList.Count; i<count; i++)
        {
            if (m_mailList[i].status == 0 || m_mailList[i].attach.Count > 0)
            {
                needTip = true;
                break;
            } 
        }
        if(needTip)
            SystemMgr.instance.SetTip(enSystem.mail, true);
        else
            SystemMgr.instance.SetTip(enSystem.mail, false);
    }

    //对品质进行排序
    void SortAttachQuality(Mail mail)
    {
        List<MailItemQualitySortVo> attach = mail.attach;
        ItemCfg itemcfg;
        int count = attach.Count;
        for (int i = 0;i< count; i++)
        {
            itemcfg = ItemCfg.m_cfgs[attach[i].itemId];
            attach[i].quality = itemcfg.quality * 10+ itemcfg.qualityLevel;
        }

        attach.SortEx((MailItemQualitySortVo a, MailItemQualitySortVo b) =>
        {
            int value = b.quality.CompareTo(a.quality);
            if(value == 0)
                value = b.itemId.CompareTo(a.itemId);
            return value;
        });
    }

    #endregion

}
