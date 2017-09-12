using UnityEngine;
using System.Collections;

public class CorpsReqItem : MonoBehaviour {
    //角色名
    public TextEx m_name;
    //等级
    public TextEx m_level;
    //战力
    public TextEx m_power;
    //最后离线
    public TextEx m_lastout;
    //头像点击
    public StateHandle m_head;
    //同意按钮
    public StateHandle m_agreeBtn;
    //拒绝按钮
    public StateHandle m_refuseBtn;

    //会员信息
    CorpsMember m_memberInfo;
    //UI是否已经初始化
    bool isInit;

    public void OnSetData(CorpsMember data)
    {
        m_memberInfo = data;
        if (isInit == false)
            OnInit();

        m_name.text = data.name;
        m_level.text = data.level.ToString();
        m_power.text = data.powerTotal.ToString();

        if (data.lastLogout > 0)//不在线
            m_lastout.text = StringUtil.FormatTimeSpan2(data.lastLogout, 30, 10) + "离线";
        else
            m_lastout.text = "在线";
    }

    public void OnInit()
    {
        //点击头像
        m_head.AddClick(OnHeadClick);
        m_agreeBtn.AddClick(OnAgreeClick);
        m_refuseBtn.AddClick(OnRefuseClick);

        isInit = true;
    }

    private void OnHeadClick()
    {
        if (m_memberInfo.heroId != RoleMgr.instance.Hero.GetInt(enProp.heroId))  //不是自己才可以弹出
            UIHeroMenu.Show(m_memberInfo.name, m_memberInfo.heroId, m_memberInfo.powerTotal, m_memberInfo.level);
    }

    private void OnAgreeClick()
    {
        int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
        int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);

        if (m_memberInfo.heroId > 0)
            NetMgr.instance.CorpsHandler.HandleMember(corpsId, heroId, m_memberInfo.heroId, m_memberInfo.name, 1, 0);
    }

    private void OnRefuseClick()
    {
        int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
        int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);

        if (m_memberInfo.heroId > 0)
            NetMgr.instance.CorpsHandler.HandleMember(corpsId, heroId, m_memberInfo.heroId, m_memberInfo.name, 2, 0);
    }
}
