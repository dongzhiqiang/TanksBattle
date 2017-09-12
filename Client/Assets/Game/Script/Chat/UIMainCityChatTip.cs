using UnityEngine;
using System.Collections;

public class UIMainCityChatTip : MonoBehaviour
{
    public TextEx m_msgContent;
    public TextEx m_heroName;
    public ImageEx m_heroHead;
    public TextEx m_roleLv;
    public ImageEx m_vipBg;
    public TextEx m_vipLv;
    public StateHandle m_headState;
    public float m_hideTime = 0;

    private TimeMgr.Timer m_timer;
    private ChatChannel m_channel;
    private int m_heroId;

    public void Init()
    {
        GetComponent<StateHandle>().AddClick(OnClick);
    }

    void OnClick()
    {
        UIMgr.instance.Get<UIChat>().ShowChatMsgList(m_channel, m_heroId);
    }

    public void Show(ChatChannel channel, int heroId, string msg, string name, string roleId, int roleLv, int vipLv)
    {
        m_channel = channel;
        m_heroId = heroId;
        m_msgContent.text = msg;
        m_heroName.text = name;
        m_heroHead.Set(RoleCfg.GetHeadIcon(roleId, channel != ChatChannel.system));
        m_roleLv.text = roleLv.ToString();
        m_vipBg.gameObject.SetActive(vipLv > 0);
        m_vipLv.text = vipLv.ToString();
        m_headState.SetState(channel != ChatChannel.system ? 0 : 1);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (m_timer != null)
        {
            m_timer.Release();
            m_timer = null;
        }

        if (m_hideTime > 0)
            m_timer = TimeMgr.instance.AddTimer(m_hideTime, HideMe);
    }

    public void HideMe()
    {
        if (m_timer != null)
        {
            m_timer.Release();
            m_timer = null; 
        }

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public static UIMainCityChatTip GetInstance()
    {
        return UIMgr.instance.Get<UIMainCity>().ChatTip;
    }
}
