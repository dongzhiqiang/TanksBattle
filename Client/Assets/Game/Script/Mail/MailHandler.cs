using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_MAIL)]
public class MailHandler
{
/******************************************************* 请求消息 ***************************************************************/
    //查看邮件、关闭无附件邮件、领取附件邮件
    public void SendReadRewardMail(string mailId, int type)
    {
        ReadOrRewardMailReq request = new ReadOrRewardMailReq();
        request.mailId = mailId;
        request.type = type;
        NetMgr.instance.Send(MODULE.MODULE_MAIL, MODULE_MAIL.CMD_READ_OR_REWARD_MAIL, request);
    }

    //一键领取附件
    public void SendOneKeyItems(List<string> ids)
    {
        OneKeyItemsReq req = new OneKeyItemsReq();
        req.mailIds = ids;
        NetMgr.instance.Send(MODULE.MODULE_MAIL, MODULE_MAIL.CMD_ONEKEY_REWARDS, req);
    } 


/******************************************************* 数据返回 ***************************************************************/

    /// 更新邮件
    /// info.mail  更新的邮件
    /// info.isAdd 是：添加  否：更新
    /// info.delMails 删除的邮件
    [NetHandler(MODULE_MAIL.PUSH_ADD_OR_UPDATE_MAIL)]
    public void OnAddOrUpdateMail(AddOrUpdateMailRes info)
    {
        Role role = RoleMgr.instance.Hero;
        MailPart part = role.MailPart;
        part.UpdateMailInfo(info.mail, info.isAdd, info.delMails);

        //打开时刷新界面
        UIMail ui = UIMgr.instance.Get<UIMail>();
        if (ui.IsOpen)
            ui.UpdateMailPanel(info.isAdd);

    }

    /// 更新所有邮件
    /// info.mails  更新的邮件
    /// info.isAdd 是：部分添加  否：全部更新
    [NetHandler(MODULE_MAIL.PUSH_ADD_OR_UPDATE_MAILS)]
    public void OnAddOrUpdateMails(AddOrUpdateMailsRes info)
    {
        Role role = RoleMgr.instance.Hero;
        MailPart part = role.MailPart;

        if(info.isAdd)
        {
            foreach (Mail mail in info.mails)
                part.UpdateMailInfo(mail, info.isAdd, null);
        }
        else
            part.UpdateMailsInfo(info.mails);

        //打开时刷新界面
        UIMail ui = UIMgr.instance.Get<UIMail>();
        if (ui.IsOpen)
            ui.UpdateMailPanel(true);
    }
    //阅读邮件、关闭邮件、领取附件
    [NetHandler(MODULE_MAIL.CMD_READ_OR_REWARD_MAIL)]
    public void OnReadOrRewardMail(ReadOrRewardMailRes info)
    {
        Role role = RoleMgr.instance.Hero;
        MailPart part = role.MailPart;
        if(info.type == (int)MailReadType.Reward) //领取附件成功
            UIMessage.ShowFlowTip("mail_get_attach");
        
    }
    //删除邮件返回
    [NetHandler(MODULE_MAIL.PUSH_DELETE_MAIL)]
    public void OnDeleteMailUpdate(DeleteMailRes info)
    {
        Role role = RoleMgr.instance.Hero;
        MailPart part = role.MailPart;
        part.DeleteMail(info.mailId);

        //打开时刷新界面
        UIMail ui = UIMgr.instance.Get<UIMail>();
        if (ui.IsOpen)
            ui.UpdateMailPanel(true);
    }
    //删除多封邮件返回
    [NetHandler(MODULE_MAIL.PUSH_DELETE_MAILS)]
    public void OnDeleteMailsUpdate(DeleteMailsRes info)
    {
        Role role = RoleMgr.instance.Hero;
        MailPart part = role.MailPart;
        part.DeleteMailsByIds(info.mailIds);

        //打开时刷新界面
        UIMail ui = UIMgr.instance.Get<UIMail>();
        if (ui.IsOpen)
            ui.UpdateMailPanel(true);
    }

    //一键领取返回
    [NetHandler(MODULE_MAIL.CMD_ONEKEY_REWARDS)]
    public void OnOneKeyItensRes(OneKeyItemsRes info)
    {
        UIMessage.ShowFlowTip("mail_onekey_attach");

    }
    
}
