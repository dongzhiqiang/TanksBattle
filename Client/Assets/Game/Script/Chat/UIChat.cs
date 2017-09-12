using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIChat : UIPanel
{
    #region Fields
    public StateGroup m_topTab;
    public StateGroup m_leftTabNormal;
    public StateGroup m_leftTabWhisper;

    public UIChatMsgList m_msgList1;
    public UIChatMsgList m_msgList2;
    public InputField m_inputField1;
    public TextEx m_txtHornNum;
    public InputField m_inputField1_2;
    public InputField m_inputField2;
    public StateHandle m_btnSend1;
    public StateHandle m_btnSend1_2;
    public StateHandle m_btnSend2;

    public TextEx m_unreadTopTabNormal;
    public TextEx m_unreadTopTabWhisper;
    public TextEx m_unreadWorldChannel;
    public TextEx m_unreadCorpsChannel;
    public TextEx m_unreadTeamChannel;
    public TextEx m_unreadSystemChannel;

    private bool m_inited = false;
    private bool m_inSwitch = false;
    private bool m_addingfirstWhisper = false;

    private ChatChannel m_leftCurChannel = ChatChannel.world;
    private int m_rightCurHeroId = 0;

    private int m_hornNumObId = 0;

    private TimeMgr.Timer m_timerRefreshOnline = null;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnSend1.AddClick(() => { OnSendChatMsg(m_leftCurChannel, 0, m_inputField1.text); m_inputField1.text = ""; });
        m_btnSend1_2.AddClick(() => { OnSendChatMsg(m_leftCurChannel, 0, m_inputField1_2.text); m_inputField1_2.text = ""; });
        m_btnSend2.AddClick(() => { OnSendChatMsg(ChatChannel.whisper, m_rightCurHeroId, m_inputField2.text); m_inputField2.text = ""; });

        m_topTab.AddSel(OnTopTabSel);
        m_leftTabNormal.AddSel(OnLeftTabNormal);
        m_leftTabWhisper.AddSel(OnLeftTabWhisper);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        if (!m_inited)
        {
            Clear();
        }

        var myHero = RoleMgr.instance.Hero;
        EventObserver.OnFire updateHornNum = () => {
            m_txtHornNum.text = myHero.GetInt(enProp.hornNum).ToString();
        };
        m_hornNumObId = myHero.AddPropChange(enProp.hornNum, updateHornNum);
        updateHornNum();

        RefreshMsgList();

        m_timerRefreshOnline = TimeMgr.instance.AddTimer(ChatHandler.REQ_ONLINE_TIMER_INV, ()=> {
            //是私聊面板才更新
            if (m_topTab.CurIdx == 1)
                NetMgr.instance.ChatHandler.RequestRoleOnline();
        }, 0, -1);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        EventMgr.Remove(m_hornNumObId);

        if (m_timerRefreshOnline != null)
        {
            m_timerRefreshOnline.Release();
            m_timerRefreshOnline = null;
        }
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private void OnSendChatMsg(ChatChannel channel, int targetHeroId, string msg)
    {
        msg = msg == null ? "" : msg.Trim();
        if (string.IsNullOrEmpty(msg))
        {
            UIMessage.Show("不能发空白内容");
            return;
        }
        switch (channel)
        {
            case ChatChannel.whisper:
                {
                    //私聊必须对方的主角ID是有效的
                    if (targetHeroId == 0)
                    {
                        UIMessage.Show("必须有一个私聊对象");
                        return;
                    }
                }
                break;
            case ChatChannel.corps:
                {
                    //必须有公会
                    var myHero = RoleMgr.instance.Hero;
                    var corpsId = myHero.GetInt(enProp.corpsId);                    
                    if (corpsId == 0)
                    {
                        UIMessage.Show("必须加入一个公会才能使用这个频道");
                        return;
                    }
                }
                break;
        }        
        msg = BadWordsCfg.ReplaceBadWords(msg);
        NetMgr.instance.ChatHandler.SendChatMsg(channel, targetHeroId, msg);
    }

    private int GetTabItemIndexByHero(int heroId)
    {
        for (var i = 0; i < m_leftTabWhisper.Count; ++i)
        {
            var uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(i);
            if (uiItem.m_heroId == heroId)
                return i;
        }
        return -1;
    }
    private UIWhisperTabItem GetTabItemByHero(int heroId)
    {
        for (var i = 0; i < m_leftTabWhisper.Count; ++i)
        {
            var uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(i);
            if (uiItem.m_heroId == heroId)
                return uiItem;
        }
        return null;
    }

    private void OnTopTabSel(StateHandle stateHandle, int idx)
    {
        if (m_inSwitch)
            return;

        RefreshMsgList();

        //是私聊面板才更新
        if (m_topTab.CurIdx == 1)
            NetMgr.instance.ChatHandler.RequestRoleOnline();
    }

    private void OnLeftTabNormal(StateHandle stateHandle, int idx)
    {
        if (m_inSwitch)
            return;

        switch (idx)
        {
            case 0:
                SwitchToChannel(ChatChannel.world, 0);
                break;
            case 1:
                SwitchToChannel(ChatChannel.corps, 0);
                break;
            case 2:
                SwitchToChannel(ChatChannel.team, 0);
                break;
            case 3:
                SwitchToChannel(ChatChannel.system, 0);
                break;
        }
    }

    private void OnLeftTabWhisper(StateHandle stateHandle, int idx)
    {
        if (m_inSwitch)
            return;

        var heroId = stateHandle.Get<UIWhisperTabItem>().m_heroId;
        SwitchToChannel(ChatChannel.whisper, heroId);
    }

    private void SwitchToChannel(ChatChannel channel, int heroId)
    {
        m_inSwitch = true;

        switch (channel)
        {
            case ChatChannel.world:
            case ChatChannel.corps:
            case ChatChannel.team:
            case ChatChannel.system:
                {
                    m_leftCurChannel = channel;
                    if (m_topTab.CurIdx != 0)
                        m_topTab.SetSel(0);
                    var idx = (int)channel;
                    if (m_leftTabNormal.CurIdx != idx)
                        m_leftTabNormal.SetSel(idx);
                    var data = NetMgr.instance.ChatHandler.ChatMsgMap.GetNewIfNo(channel);

                    switch (channel)
                    {
                        case ChatChannel.system:
                            m_msgList1.SetItemList(data.msgList, false, true);
                            break;
                        default:
                            m_msgList1.SetItemList(data.msgList, true, true);
                            break;
                    }

                    data.lastReadIndex = data.msgList.Count - 1;
                    UpdateChannelUnread(channel);
                    UpdateTopTabNormalUnread();
                }
                break;
            case ChatChannel.whisper:
                {
                    var idx = GetTabItemIndexByHero(heroId);
                    if (idx >= 0)
                    {
                        m_rightCurHeroId = heroId;
                        if (m_topTab.CurIdx != 1 && !m_addingfirstWhisper)
                            m_topTab.SetSel(1);
                        if (m_leftTabWhisper.CurIdx != idx)
                            m_leftTabWhisper.SetSel(idx);
                        var data = NetMgr.instance.ChatHandler.WhisperMsgMap.GetNewIfNo(heroId);
                        m_msgList2.SetItemList(data.msgList, false, false);
                        if (!m_addingfirstWhisper)
                            data.lastReadIndex = data.msgList.Count - 1;
                        UpdateWhisperUnread(heroId);
                        UpdateTopTabWhisperUnread();
                    }
                }
                break;
        }

        m_inSwitch = false;
    }

    private void RefreshMsgList()
    {
        switch (m_topTab.CurIdx)
        {
            case 0:
                {
                    m_msgList1.ScrollToBottom();
                    var data = NetMgr.instance.ChatHandler.ChatMsgMap.GetNewIfNo(m_leftCurChannel);
                    data.lastReadIndex = data.msgList.Count - 1;
                    UpdateChannelUnread(m_leftCurChannel);
                    UpdateTopTabNormalUnread();
                }                
                break;
            case 1:
                {
                    m_msgList2.ScrollToBottom();
                    var data = NetMgr.instance.ChatHandler.WhisperMsgMap.GetNewIfNo(m_rightCurHeroId);
                    data.lastReadIndex = data.msgList.Count - 1;
                    UpdateWhisperUnread(m_rightCurHeroId);
                    UpdateTopTabWhisperUnread();
                }                
                break;
        }
    }
    #endregion

    public void Clear()
    {
        m_inited = true;

        m_msgList1.Clear();
        m_msgList2.Clear();
        m_inputField1.text = "";
        m_inputField1_2.text = "";
        m_inputField2.text = "";
        m_leftCurChannel = ChatChannel.world;
        m_rightCurHeroId = 0;

        m_topTab.SetSel(0);
        m_leftTabNormal.SetSel(0);
        m_leftTabWhisper.SetCount(0);
        var uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(0);
        if (uiItem != null)
            uiItem.Clear();

        UpdateAllUnread();
    }

    private void UpdateAllUnread()
    {
        UpdateTopTabNormalUnread();
        UpdateTopTabWhisperUnread();
        foreach (var e in Enum.GetValues(typeof(ChatChannel)))
        {
            var channel = (ChatChannel)e;
            if (channel != ChatChannel.whisper)
                UpdateChannelUnread(channel);
        }
    }

    public void AddOrUpdateWhisperTabItem(int heroId)
    {
        WhisperData data;
        if (!NetMgr.instance.ChatHandler.WhisperMsgMap.TryGetValue(heroId, out data))
            return;

        //如果私聊的用户头像还没有，就添加一个
        UIWhisperTabItem uiItem = null;
        var nItem = GetTabItemIndexByHero(data.roleInfo.heroId);
        if (nItem >= 0)
        {
            uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(nItem);
        }
        else
        {
            if (m_leftTabWhisper.Count == 1 && m_leftTabWhisper.Get<UIWhisperTabItem>(0).m_heroId == 0)
            {
                m_leftTabWhisper.SetCount(1);
                uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(0);
                nItem = 0;
            }
            else
            {
                m_leftTabWhisper.SetCount(m_leftTabWhisper.Count + 1);
                uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(m_leftTabWhisper.Count - 1);
                nItem = m_leftTabWhisper.Count - 1;
            }
        }

        var roleInfo = data.roleInfo;
        uiItem.Init(roleInfo.heroId, roleInfo.name, roleInfo.roleId, roleInfo.rolelv, roleInfo.viplv, true);

        //把这个条目移到首位
        m_leftTabWhisper.MoveChildToFirst(nItem);

        if (m_rightCurHeroId == 0)
        {
            //防止切换顶部标签
            m_addingfirstWhisper = true;
            m_leftTabWhisper.SetSel(0);
            m_addingfirstWhisper = false;
        }            
    }

    private TextEx GetUnreadTextUI(ChatChannel channel)
    {
        TextEx uiItem = null;
        switch (channel)
        {
            case ChatChannel.world:
                uiItem = m_unreadWorldChannel;
                break;
            case ChatChannel.corps:
                uiItem = m_unreadCorpsChannel;
                break;
            case ChatChannel.team:
                uiItem = m_unreadTeamChannel;
                break;
            case ChatChannel.system:
                uiItem = m_unreadSystemChannel;
                break;
        }
        return uiItem;
    }

    private void UpdateTopTabNormalUnread()
    {
        var unread = NetMgr.instance.ChatHandler.AllNormalChatUnread;
        m_unreadTopTabNormal.text = unread.ToString();
        if (unread <= 0 && m_unreadTopTabNormal.transform.parent.gameObject.activeSelf)
            m_unreadTopTabNormal.transform.parent.gameObject.SetActive(false);
        else if (unread > 0 && !m_unreadTopTabNormal.transform.parent.gameObject.activeSelf)
            m_unreadTopTabNormal.transform.parent.gameObject.SetActive(true);
    }

    private void UpdateTopTabWhisperUnread()
    {
        var unread = NetMgr.instance.ChatHandler.AllWhisperChatUnread;
        m_unreadTopTabWhisper.text = unread.ToString();
        if (unread <= 0 && m_unreadTopTabWhisper.transform.parent.gameObject.activeSelf)
            m_unreadTopTabWhisper.transform.parent.gameObject.SetActive(false);
        else if (unread > 0 && !m_unreadTopTabWhisper.transform.parent.gameObject.activeSelf)
            m_unreadTopTabWhisper.transform.parent.gameObject.SetActive(true);
    }

    private void UpdateChannelUnread(ChatChannel channel)
    {
        var uiItem = GetUnreadTextUI(channel);
        if (uiItem != null)
        {
            var unread = NetMgr.instance.ChatHandler.GetNormalUnread(channel);
            uiItem.text = unread.ToString();
            if (unread <= 0 && uiItem.transform.parent.gameObject.activeSelf)
                uiItem.transform.parent.gameObject.SetActive(false);
            else if (unread > 0 && !uiItem.transform.parent.gameObject.activeSelf)
                uiItem.transform.parent.gameObject.SetActive(true);
        }            
    }

    private void UpdateWhisperUnread(int heroId)
    {
        var uiItem = GetTabItemByHero(heroId);
        if (uiItem != null)
            uiItem.UpdateUnread();
    }

    public void RefreshWhisperRoleOnlineState()
    {        
        for (var i = 0; i < m_leftTabWhisper.Count; ++i)
        {
            var uiItem = m_leftTabWhisper.Get<UIWhisperTabItem>(i);
            uiItem.RefreshOnlineState();
        }
    }

    public void OnRecvNewChatMsg(RecvChatMsgRes res)
    {
        var myHero = RoleMgr.instance.Hero;
        var myHeroId = myHero.GetInt(enProp.heroId);

        switch (res.channel)
        {
            case ChatChannel.whisper:
                {
                    //这个res.msg可能是别人发给自己的，也可能是自己发给别人的
                    //如果是自己发给别人的，就要取别人的主角ID来插入聊天数据
                    //chatHeroId就是对方的主角ID
                    var chatHeroId = res.msg.heroId == myHeroId && res.target != myHeroId ? res.target : res.msg.heroId;
                    //如果是自己发给别人的消息的回收，对方的信息并没有修改，不用更新私聊头像
                    if (chatHeroId == res.msg.heroId)
                    {
                        AddOrUpdateWhisperTabItem(chatHeroId);
                    }
                    //不用更新内容，但要前移
                    else
                    {
                        var nItem = GetTabItemIndexByHero(chatHeroId);
                        m_leftTabWhisper.MoveChildToFirst(nItem);
                    }

                    //如果窗口打开中、切到了私聊界面，选中了这个主角的头像，那就刷新聊天消息列表
                    if (IsOpenEx && m_topTab.CurIdx == 1 && m_rightCurHeroId == chatHeroId)
                    {
                        RefreshMsgList();
                    }
                    else if (myHeroId != res.msg.heroId) //自己发的就不用提示了
                    {
                        //如果本窗口没打开，要在主城界面提示
                        if (!IsOpenEx)
                        {
                            var msgObj = res.msg;
                            UIMainCityChatTip.GetInstance().Show(res.channel, msgObj.heroId, msgObj.msg, msgObj.name, msgObj.roleId, msgObj.rolelv, msgObj.viplv);
                        }

                        //上面的切换按钮更新角标
                        UpdateTopTabWhisperUnread();

                        //左边的切换按钮更新角标
                        UpdateWhisperUnread(chatHeroId);
                    }
                }
                break;
            default:
                {
                    //如果窗口打开中、切到了综合界面，选中这个频道的标签，那就刷新聊天消息列表
                    if (IsOpenEx && m_topTab.CurIdx == 0 && m_leftCurChannel == res.channel)
                    {
                        RefreshMsgList();
                    }
                    else if (myHeroId != res.msg.heroId) //自己发的就不用提示了
                    {
                        //窗口有没有打开，都要在主城界面提示
                        if (res.channel == ChatChannel.system)
                        {
                            UIMessage.Show(res.msg.msg);
                        }
                        else
                        {
                            var showTip = false;
                            var teamId = 0; //myHero.GetInt(enProp.teamId);
                            var corpsId = myHero.GetInt(enProp.corpsId);
                            
                            //优先提示队伍消息
                            if (teamId != 0)
                            {
                                if (res.channel == ChatChannel.team)
                                    showTip = true;
                            }
                            //再是公会消息
                            else if (corpsId != 0)
                            {
                                if (res.channel == ChatChannel.corps)
                                    showTip = true;
                            }
                            //没队伍、公会，就可以提示世界频道消息（会有公会、队伍消息么？）                
                            else
                            {
                                showTip = true;
                            }

                            if (showTip)
                            {
                                var msgObj = res.msg;
                                UIMainCityChatTip.GetInstance().Show(res.channel, msgObj.heroId, msgObj.msg, msgObj.name, msgObj.roleId, msgObj.rolelv, msgObj.viplv);
                            }
                        }

                        //上面的切换按钮更新角标
                        UpdateTopTabNormalUnread();

                        //左边的切换按钮更新角标
                        UpdateChannelUnread(res.channel);
                    }
                }
                break;
        }        
    }

    public void ShowChatMsgList(ChatChannel channel, int heroId)
    {
        if (!IsOpenEx)
            Open(null);
        SwitchToChannel(channel, heroId);
    }
}