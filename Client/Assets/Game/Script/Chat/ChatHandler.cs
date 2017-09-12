using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WhisperData
{
    public WhisperRoleInfo roleInfo = new WhisperRoleInfo();
    public long lastUpdateTime = 0; //角色信息更新时间（有可能是只更新online值）
    public List<ChatMsgItem> msgList = new List<ChatMsgItem>();
    public int lastReadIndex = -1;
}

public class NormalChatData
{
    public List<ChatMsgItem> msgList = new List<ChatMsgItem>();
    public int lastReadIndex = -1;
}

[NetModule(MODULE.MODULE_CHAT)]
public class ChatHandler
{
    /// <summary>
    /// 每个频道（私聊就是每个人）最大保留多少条消息
    /// </summary>
    public const int MAX_MSG_NUM_PER_CHANNEL   = 10000;
    /// <summary>
    /// 多少秒后，某个私聊的角色的在线状态可以被查询
    /// </summary>
    public const int REQ_ONLINE_STATE_WAIT     = 30;
    /// <summary>
    /// 打开私聊窗口时，多少秒提求刷新一下在线状态
    /// </summary>
    public const int REQ_ONLINE_TIMER_INV      = 10;


    private Dictionary<ChatChannel, NormalChatData> m_chatMsgMap = new Dictionary<ChatChannel, NormalChatData>();
    private Dictionary<int, WhisperData> m_whisperMsgMap = new Dictionary<int, WhisperData>();

    public Dictionary<ChatChannel, NormalChatData> ChatMsgMap { get { return m_chatMsgMap; } }
    public Dictionary<int, WhisperData> WhisperMsgMap { get { return m_whisperMsgMap; } }
    public int AllNormalChatUnread {
        get {
            var sum = 0;
            foreach (var item in m_chatMsgMap.Values)
                sum += item.msgList.Count - item.lastReadIndex - 1;
            return sum;
        }
    }
    public int AllWhisperChatUnread {
        get {
            var sum = 0;
            foreach (var item in m_whisperMsgMap.Values)
                sum += item.msgList.Count - item.lastReadIndex - 1;
            return sum;
        }
    }
    public int GetNormalUnread(ChatChannel channel)
    {
        NormalChatData item;
        if (m_chatMsgMap.TryGetValue(channel, out item))
            return item.msgList.Count - item.lastReadIndex - 1;
        return 0;
    }
    public int GetWhisperUnread(int unread)
    {
        WhisperData item;
        if (m_whisperMsgMap.TryGetValue(unread, out item))
            return item.msgList.Count - item.lastReadIndex - 1;
        return 0;
    }

    public ChatHandler()
    {
        UIMainCity.AddClick(enSystem.chat, () => {
            UIMgr.instance.Open<UIChat>();
        });

        //主角创建后再收集触发器
        EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.HERO_CREATED, () => {
            m_chatMsgMap.Clear();
            m_whisperMsgMap.Clear();
            UIMainCityChatTip.GetInstance().HideMe();
            UIMgr.instance.Get<UIChat>().Clear();
        });
    }

    public void SendChatMsg(ChatChannel channel, int target, string content)
    {
        var req = new SendChatMsgReq();
        req.channel = channel;
        req.target = target;
        req.content = content;
        NetMgr.instance.Send(MODULE.MODULE_CHAT, MODULE_CHAT.CMD_SEND_CHAT_MSG, req);
    }

    [NetHandler(MODULE_CHAT.CMD_SEND_CHAT_MSG, true)]
    public void OnSendChatMsg(int errorCode, string errorMsg, SendChatMsgRes res)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_CHAT, errorCode, errorMsg);
            UIMessage.Show(errorMsg);
            Debuger.LogError("Module:{0}, Command:{1}, errorCode:{2}, errorMsg:{3}", MODULE.MODULE_CHAT, MODULE_CHAT.CMD_SEND_CHAT_MSG, errorCode, errorMsg);

            switch (errorCode)
            {
                case RESULT_CODE_CHAT.TARGET_NOT_ONLINE:
                    {
                        var heroId = StringUtil.ToInt(res.cxt);
                        WhisperData info;
                        if (m_whisperMsgMap.TryGetValue(heroId, out info))
                        {
                            info.roleInfo.online = false;
                            info.lastUpdateTime = TimeMgr.instance.GetTimestamp();
                            UIMgr.instance.Get<UIChat>().RefreshWhisperRoleOnlineState();
                        }
                    }
                    break;
            }

            return;
        }
    }

    public void SendCreateWhisper(int heroId)
    {
        var req = new CreateWhisperReq();
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CHAT, MODULE_CHAT.CMD_CREATE_WHISPER, req);
    }

    [NetHandler(MODULE_CHAT.CMD_CREATE_WHISPER)]
    public void OnCreateWhisper(WhisperRoleInfo info)
    {
        var data = m_whisperMsgMap.GetNewIfNo(info.heroId);
        data.roleInfo = info;
        data.lastUpdateTime = TimeMgr.instance.GetTimestamp();
        UIMgr.instance.Get<UIChat>().AddOrUpdateWhisperTabItem(info.heroId);
        UIMgr.instance.Get<UIChat>().ShowChatMsgList(ChatChannel.whisper, info.heroId);
    }

    [NetHandler(MODULE_CHAT.PUSH_RECV_CHAT_MSG)]
    public void OnRecvChatMsg(RecvChatMsgRes res)
    {
        switch (res.channel)
        {
            case ChatChannel.whisper:
                {
                    var myHero = RoleMgr.instance.Hero;
                    var myHeroId = myHero.GetInt(enProp.heroId);

                    //这个res.msg可能是别人发给自己的，也可能是自己发给别人的
                    //如果是自己发给别人的，就要取别人的主角ID来插入聊天数据
                    //chatHeroId就是对方的主角ID
                    var chatHeroId = res.msg.heroId == myHeroId && res.target != myHeroId ? res.target : res.msg.heroId;
                    var data = m_whisperMsgMap.GetNewIfNo(chatHeroId);
                    //如果是自己发给别人的回发消息，data就是对方的，而res.msg是自己的，不能更新RoleInfo
                    if (chatHeroId == res.msg.heroId)
                    {
                        data.roleInfo.heroId = res.msg.heroId;
                        data.roleInfo.name = res.msg.name;
                        data.roleInfo.roleId = res.msg.roleId;
                        data.roleInfo.rolelv = res.msg.rolelv;
                        data.roleInfo.viplv = res.msg.viplv;
                        data.roleInfo.online = true;
                        data.lastUpdateTime = TimeMgr.instance.GetTimestamp();
                    }                    
                    data.msgList.Add(res.msg);
                    if (data.msgList.Count > MAX_MSG_NUM_PER_CHANNEL)
                    {
                        var removeCount = data.msgList.Count - MAX_MSG_NUM_PER_CHANNEL;
                        data.lastReadIndex = data.lastReadIndex < removeCount ? -1 : data.lastReadIndex - removeCount;
                        data.msgList.RemoveRange(0, removeCount);
                    }
                    UIMgr.instance.Get<UIChat>().OnRecvNewChatMsg(res);
                }                
                break;
            default:
                {
                    var data = m_chatMsgMap.GetNewIfNo(res.channel);
                    data.msgList.Add(res.msg);
                    if (data.msgList.Count > MAX_MSG_NUM_PER_CHANNEL)
                    {
                        var removeCount = data.msgList.Count - MAX_MSG_NUM_PER_CHANNEL;
                        data.lastReadIndex = data.lastReadIndex < removeCount ? -1 : data.lastReadIndex - removeCount;
                        data.msgList.RemoveRange(0, removeCount);
                    }
                    UIMgr.instance.Get<UIChat>().OnRecvNewChatMsg(res);
                }                
                break;
        }
    }

    /// <summary>
    /// 请求私聊角色的在线状况
    /// </summary>
    public void RequestRoleOnline()
    {
        var req = new RoleOnlineStateReq();
        req.heroIds = new List<int>();

        var curTime = TimeMgr.instance.GetTimestamp();
        foreach (var e in m_whisperMsgMap.Values)
        {
            if (e.roleInfo.heroId != 0 && curTime - e.lastUpdateTime > REQ_ONLINE_STATE_WAIT)
                req.heroIds.Add(e.roleInfo.heroId);
        }

        if (req.heroIds.Count > 0)
            NetMgr.instance.Send(MODULE.MODULE_CHAT, MODULE_CHAT.CMD_REQ_ROLE_ONLINE, req);
    }

    [NetHandler(MODULE_CHAT.CMD_REQ_ROLE_ONLINE)]
    public void OnRoleOnlineRes(RoleOnlineStateRes res)
    {
        var curTime = TimeMgr.instance.GetTimestamp();
        foreach (var e in res.states)
        {
            WhisperData info;
            if (m_whisperMsgMap.TryGetValue(StringUtil.ToInt(e.Key), out info))
            {
                info.roleInfo.online = e.Value == 0 ? false : true;
                info.lastUpdateTime = curTime;
            }                
        }

        UIMgr.instance.Get<UIChat>().RefreshWhisperRoleOnlineState();
    }
}