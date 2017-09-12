using UnityEngine;
using System.Collections;

public class UIFriendRecommendItem : MonoBehaviour {
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
    //添加按钮
    public StateHandle m_addBtn;
    //点击可打开菜单
    public StateHandle m_select;
    //待审核状态
    public StateHandle m_auditState;

    //记录heroId
    int m_heroId;
    bool isInit = false;
    Friend m_data;
    public int OnSetData(Friend data)
    {
        if (!isInit)
            OnInit();
        m_data = data;
        m_roleName.text = m_data.name;
        m_power.text = m_data.powerTotal.ToString();
        m_lv.text = m_data.level.ToString();
        m_heroId = m_data.heroId;
        m_vip.text = "VIP " + m_data.vipLv;

        int state = RoleMgr.instance.Hero.SocialPart.addReqs.Contains(m_data.name) ? 1 : 0;  //1：待审核
        m_auditState.SetState(state);
        return state;

    }

    void OnInit()
    {
        //添加好友
        m_addBtn.AddClick(OnAddBtn);
        //点击头像
        m_select.AddClick(OnSelect);

        isInit = true;
    }

    void OnAddBtn()
    {
        NetMgr.instance.SocialHandler.AddFriend(m_data.name);
        m_auditState.SetState(1);
    }

    void OnSelect()
    {
        if (m_data.heroId != RoleMgr.instance.Hero.GetInt(enProp.heroId))  //不是自己才可以弹出
            UIHeroMenu.Show(m_data.name, m_data.heroId, m_data.powerTotal, m_data.level);
    }
}
