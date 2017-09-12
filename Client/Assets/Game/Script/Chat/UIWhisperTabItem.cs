using UnityEngine;
using System;

public class UIWhisperTabItem : MonoBehaviour
{
    public TextEx m_heroName;
    public ImageEx m_heroHead;
    public TextEx m_roleLv;
    public ImageEx m_vipBg;
    public TextEx m_vipLv;
    public TextEx m_online;
    public TextEx m_unread;

    [NonSerialized]
    public int m_heroId;

    public void Init(int heroId, string heroName, string roleId, int roleLv, int vipLv, bool online)
    {
        m_heroId = heroId;
        m_heroName.text = heroName;
        m_heroHead.Set(RoleCfg.GetHeadIcon(roleId));
        m_roleLv.text = roleLv.ToString();
        m_vipBg.gameObject.SetActive(vipLv > 0);
        m_vipLv.text = vipLv.ToString();
        m_online.text = online ? "在线" : "离线";

        UpdateUnread();
    }

    public void Clear()
    {
        m_heroId = 0;
        m_heroName.text = "";
        m_heroHead.Set(null);
        m_roleLv.text = "";
        m_vipBg.gameObject.SetActive(false);
        m_vipLv.text = "";
        m_online.text = "";
        m_unread.text = "";
        m_unread.transform.parent.gameObject.SetActive(false);
    }

    public void UpdateUnread()
    {
        var unread = NetMgr.instance.ChatHandler.GetWhisperUnread(m_heroId);
        m_unread.text = unread.ToString();
        if (unread <= 0 && m_unread.transform.parent.gameObject.activeSelf)
            m_unread.transform.parent.gameObject.SetActive(false);
        else if (unread > 0 && !m_unread.transform.parent.gameObject.activeSelf)
            m_unread.transform.parent.gameObject.SetActive(true);
    }

    public void RefreshOnlineState()
    {
        var whisperMap = NetMgr.instance.ChatHandler.WhisperMsgMap;
        WhisperData info;
        if (whisperMap.TryGetValue(m_heroId, out info))
            m_online.text = info.roleInfo.online ? "在线" : "离线";
    }
}