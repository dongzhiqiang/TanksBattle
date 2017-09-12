using UnityEngine;
using System.Collections;

public class UIFriendReqItem : MonoBehaviour {
    //名字
    public TextEx m_roleName;
    //战力
    public TextEx m_power;
    //等级
    public TextEx m_lv;
    //头像
    public ImageEx m_head;
    //vip
    public TextEx m_vip;
    //拒绝按钮
    public StateHandle m_refuseBtn;
    //同意按钮
    public StateHandle m_agreeBtn;
    //记录heroId
    int m_heroId;
    bool isInit = false;
    public void OnSetData(Friend data)
    {
        if (!isInit)
            OnInit();
        m_roleName.text = data.name;
        m_power.text = data.powerTotal.ToString();
        m_lv.text = data.level.ToString();
        m_heroId = data.heroId;

    }

    void OnInit()
    {
        //同意好友
        m_agreeBtn.AddClick(OnAgreeBtn);
        //拒绝好友
        m_refuseBtn.AddClick(OnRefuseBtn);
        isInit = true;
    }

    void OnAgreeBtn()
    {
        Role role = RoleMgr.instance.Hero;
        //先判断自己的好友是否满了
        int level = role.GetInt(enProp.level);
        FriendMaxCfg cfg = FriendMaxCfg.Get(level);
        if (cfg == null)
        {
            Debug.LogError("找不到对应等级的好友上限配置！");
            return;
        }
        if (role.SocialPart.friends.Count >= cfg.maxFriend)
        {
            UIMessage.ShowFlowTip("friend_list_full");
            return;
        }

        NetMgr.instance.SocialHandler.SendHandleFriend(role.GetString(enProp.name), m_heroId, (int)HandleFriendType.Agree);
    }

    void OnRefuseBtn()
    {
        Role role = RoleMgr.instance.Hero;
        NetMgr.instance.SocialHandler.SendHandleFriend(role.GetString(enProp.name), m_heroId, (int)HandleFriendType.Refuse);
    }
}
