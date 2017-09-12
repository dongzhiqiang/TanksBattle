using UnityEngine;
using System.Collections;

public class UIFriendSelectItem : MonoBehaviour {
    //名字
    public TextEx m_roleName;
    //战力
    public TextEx m_power;
    //等级
    public TextEx m_lv;
    //头像
    public ImageEx m_head;
    //点击可打开菜单
    public StateHandle m_select;
    //vip
    public TextEx m_vip;
    //领取按钮
    public StateHandle m_getBtn;
    //赠送按钮
    public StateHandle m_sendBtn;
    //离线时间
    public TextEx m_offlineTime;
    //领取状态
    public StateHandle m_getState;
    //赠送状态
    public StateHandle m_sendState;
    //在线状态
    public StateHandle m_onlineState;

    Friend friendData;
    //UI是否已经初始化
    bool isInit;

    public void OnSetData(Friend data, SocialPart part)
    {
        friendData = data;
        if (isInit == false)
            OnInit();
        m_roleName.text = data.name;
        m_power.text = data.powerTotal.ToString();
        m_lv.text = data.level.ToString();
        m_vip.text = "VIP " + data.vipLv;
        if (data.lastLogout > 0)//不在线
        {
            m_onlineState.SetState(1);
            m_offlineTime.text = StringUtil.FormatTimeSpan2(data.lastLogout, 30, 10)+"离线";
        }
        else
        {
            m_onlineState.SetState(0);
            m_offlineTime.text = "";
        }

        //这里对领取按钮的状态判断
        //判断是否领过体力 0不可领 1可领 2已领取
        int getMark = 0 ;
        int count = part.colStms.Count;
        for (int i = 0; i< count; i++)
        {
            if(friendData.heroId ==  part.colStms[i])//已经领过
            {
                getMark = 2;
                break;
            }
        }
        if(getMark == 0)
        {
            for(int m=0; m<part.unColStms.Count; m++)
            {
                if(friendData.heroId == part.unColStms[m].heroId)//有可领的
                {
                    getMark = 1;
                    break;
                }
            }
        }
        m_getState.SetState(getMark);

        //对赠送按钮判断 0可赠送 1已赠送 2不可赠送
        int sendMark = 0;
        if (part.sendStms.Count >= ConfigValue.GetInt("maxSendFriendStam"))
            sendMark = 2;
        for (int i = 0; i < part.sendStms.Count; ++i)
        {
            if (friendData.heroId == part.sendStms[i])  //已经赠送过体力
            {
                sendMark = 1;
                break;
            }
        }
        m_sendState.SetState(sendMark);

    }

    public void OnInit()
    {
        //领取体力按钮
        m_getBtn.AddClick(OnGetBtn);
        //赠送体力按钮
        m_sendBtn.AddClick(OnSendBtn);
        //点击头像
        m_select.AddClick(OnSelect);
        isInit = true;
    }

    void OnGetBtn()
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        if (ConfigValue.GetInt("maxGetFriendStam") - part.colStms.Count <= 0)
        {
            UIMessage.ShowFlowTip("get_friend_stam_max");
            return;
        }
        NetMgr.instance.SocialHandler.GetFriendStamina(friendData.heroId);
    }

    void OnSendBtn()
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        if (ConfigValue.GetInt("maxSendFriendStam") - part.sendStms.Count <= 0)
        {
            UIMessage.ShowFlowTip("send_friend_stam_max");
            return;
        }
        NetMgr.instance.SocialHandler.SendFriendStamina(friendData.heroId);
    }

    void OnSelect()
    {
        if (friendData.heroId != RoleMgr.instance.Hero.GetInt(enProp.heroId))  //不是自己才可以弹出
            UIHeroMenu.Show(friendData.name, friendData.heroId, friendData.powerTotal, friendData.level);
    }
}
