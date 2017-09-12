using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_CHAT
{
    public const int CMD_SEND_CHAT_MSG      = 1;
    public const int CMD_CREATE_WHISPER     = 2;
    public const int CMD_REQ_ROLE_ONLINE    = 3;
    public const int PUSH_RECV_CHAT_MSG     = -1;
}

public class RESULT_CODE_CHAT : RESULT_CODE
{
    public const int TARGET_NOT_ONLINE = 1;     //对方不在线
    public const int HORN_NUM_LACK = 2;         //喇叭数不足
    public const int MUST_JOIN_CORPS = 3;       //必须加入一个公会
    public const int MSG_CANNOT_EMPTY = 4;      //聊天内容不能为空
    public const int ROLE_LEVEL_LOW = 5;        //角色等级不够
    public const int MUST_JOIN_TEAM = 6;        //必须加入一个队伍
}

public class ChatMsgItem
{
    public int      heroId;
    public string   msg;
    public string   name;
    public long     time;
    public string   roleId;
    public int      rolelv;
    public int      viplv;
}

public enum ChatChannel
{
    world,
    corps,
    team,
    system,
    whisper,
}

public class SendChatMsgReq
{
    public ChatChannel  channel;
    public int          target;
    public string       content;
}

public class SendChatMsgRes
{
    public string cxt;
}

public class RecvChatMsgRes
{
    public ChatChannel  channel;
    public int          target;
    public ChatMsgItem  msg;
}

public class CreateWhisperReq
{
    public int heroId;
}

public class WhisperRoleInfo
{
    public int heroId;
    public string name;
    public string roleId;
    public int rolelv;
    public int viplv;
    public bool online;
}

public class RoleOnlineStateReq
{
    public List<int> heroIds;
}

public class RoleOnlineStateRes
{
    public Dictionary<string, int> states;
}