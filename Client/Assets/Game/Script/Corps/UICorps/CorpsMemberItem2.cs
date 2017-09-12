using UnityEngine;
using System.Collections;

public class CorpsMemberItem2 : MonoBehaviour
{
    //角色名
    public TextEx m_name;
    //等级
    public TextEx m_level;
    //职位
    public TextEx m_pos;
    //头像点击
    public StateHandle m_head;
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
        m_pos.text = CorpsPosFuncCfg.Get(data.pos).posName;

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
