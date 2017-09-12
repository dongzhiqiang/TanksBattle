using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsMemberItem : MonoBehaviour
{
    //角色名
    public TextEx m_name;
    //等级
    public TextEx m_level;
    //等级
    public TextEx m_level2;
    //职位
    public TextEx m_pos;
    //贡献
    public TextEx m_contribution;
    //最后离线
    public TextEx m_lastout;
    //头像点击
    public StateHandle m_head;

    public ImageEx m_isSelf;

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
        m_level2.text = data.level.ToString();
        m_pos.text = CorpsPosFuncCfg.Get(data.pos).posName;
        m_contribution.text = data.contribution.ToString();
        if (data.heroId == RoleMgr.instance.Hero.GetInt(enProp.heroId))
            m_isSelf.gameObject.SetActive(true);
        else
            m_isSelf.gameObject.SetActive(false);
        
        if (data.lastLogout > 0)//不在线
            m_lastout.text = StringUtil.FormatTimeSpan2(data.lastLogout, 30, 10) ;
        else
            m_lastout.text = "在线";
    }

    public void OnInit()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        //点击头像
        m_head.AddClick(OnHeadClick);

        isInit = true;
    }

    private void OnHeadClick()
    {
        if (m_memberInfo.heroId != RoleMgr.instance.Hero.GetInt(enProp.heroId))  //不是自己才可以弹出
            UIHeroMenu.Show(m_memberInfo.name, m_memberInfo.heroId, m_memberInfo.powerTotal, m_memberInfo.level);
    }
}
