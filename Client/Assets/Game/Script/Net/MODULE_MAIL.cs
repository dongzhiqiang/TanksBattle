using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MailStatus
{
    Unread,
    Readed
}

public enum MailReadType
{
    Open = 1,  //阅读
    Close = 2,  //关闭
    Reward = 3  //领取附件
}

public class MODULE_MAIL
{
    //打开邮件、关闭邮件、领取附件
    public const int CMD_READ_OR_REWARD_MAIL = 1;
    //一键领奖励
    public const int CMD_ONEKEY_REWARDS = 2;

    //新邮件
    public const int PUSH_ADD_OR_UPDATE_MAIL = -1;
    //批量更新邮件
    public const int PUSH_ADD_OR_UPDATE_MAILS = -2;
    //邮件被删除
    public const int PUSH_DELETE_MAIL = -3;
    //多封邮件被删除
    public const int PUSH_DELETE_MAILS = -4;

}

public class Mail
{
    public string mailId;  //邮件id
    public string title;   //标题
    public string sender;     //发送者
    public int status;     //阅读状态
    public int sendTime;     //发送时间
    public string content;   //邮件内容
    public List<MailItemQualitySortVo> attach;    //附件
}

public class MailItemQualitySortVo
{
    public int itemId;  //物品配置ID
    public int num;     //物品数量
    public int quality;
}


/// 邮件批量更新
/// @param isAdd 是：添加  否：更新
public class AddOrUpdateMailsRes
{
    public List<Mail> mails;
    public bool isAdd;
}

/// 邮件更新
/// @param isAdd 是：添加  否：更新
public class AddOrUpdateMailRes
{
    public Mail mail;
    public bool isAdd;
    public List<Mail> delMails;   //删除的邮件
}


public class ReadOrRewardMailReq
{
    public string mailId;
    public int type;   //1打开邮件 2关闭无附件邮件 3领取附件邮件
}

public class ReadOrRewardMailRes
{
    public string mailId;
    public int type;   //1打开邮件 2关闭无附件邮件 3领取附件邮件
}

public class DeleteMailRes
{
    public string mailId;
}

public class DeleteMailsRes
{
    public List<string> mailIds;
}

public class OneKeyItemsReq
{
    public List<string> mailIds;  //有附件的id
}

public class OneKeyItemsRes
{

}