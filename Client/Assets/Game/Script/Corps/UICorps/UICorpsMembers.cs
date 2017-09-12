using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
//公户成员信息界面
public class UICorpsMembers : MonoBehaviour
{
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;
    //弹劾按钮
    public StateHandle m_impeachBtn;
    //入会设置按钮
    public StateHandle m_joinSetBtn;
    //退出公会按钮
    public StateHandle m_quitBtn;
    ////等级排序
    public StateHandle m_levelSort;
    //职位排序
    public StateHandle m_posSort;
    //恭喜度排序
    public StateHandle m_contriSort;
    //最后登录时间排序
    public StateHandle m_lastoutSort;
    //排序高亮显示
    public StateHandle m_sortShow;
    //弹劾按钮上的文本
    public TextEx m_impeachTxt;
    //弹劾按钮上的叹号
    public ImageEx m_impTip;

    public StateHandle m_preState;

    public void OnInit()
    {
        //弹劾会长
        m_impeachBtn.AddClick(OnImpeachBtn);
        //入会设置
        m_joinSetBtn.AddClick(OnJoinSetBtn);
        //退出公会
        m_quitBtn.AddClick(OnQuitBtn);
        //等级排序
        m_levelSort.AddClick(OnLevelSortBtn);
        //职务排序
        m_posSort.AddClick(OnPosSort);
        //贡献排序
        m_contriSort.AddClick(OnContriSort);
        //离线时间排序
        m_lastoutSort.AddClick(OnLastOutSort);

        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    void OnImpeachBtn()
    {
        UIMgr.instance.Open<UICorpsImpeach>();
        m_impTip.gameObject.SetActive(false);
    }

    void OnJoinSetBtn()
    {
        UIMgr.instance.Open<UICorpsJoinSet>();
    }

    void OnQuitBtn()
    {
        if (RoleMgr.instance.Hero.CorpsPart.personalInfo.pos == (int)CorpsPosEnum.President)//会长
        {
            if (RoleMgr.instance.Hero.CorpsPart.corpsInfo.members.Count > 1)//公会里还有其他人
                UIMessage.Show(LanguageCfg.Get("quit_corps_havepeople_desc"));
            else
            {
                UIMessageBox.Open(LanguageCfg.Get("quit_corps_time_desc"), () =>
                {
                    QuitCorps();
                }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
            }
        }
        else
        {
            UIMessageBox.Open(LanguageCfg.Get("quit_corps_time_desc2"), () =>
            {
                QuitCorps();
            }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
        }
    }

    void OnLevelSortBtn()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.SortMembersByLevel();
        OnUpdateData();
        m_sortShow.SetState(0);
    }

    void OnPosSort()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.SortMembersByPos();
        OnUpdateData();
        m_sortShow.SetState(1);
    }

    void OnContriSort()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.SortMembersByContribute();
        OnUpdateData();
        m_sortShow.SetState(2);
    }

    void OnLastOutSort()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.SortMembersByLastOut();
        OnUpdateData();
        m_sortShow.SetState(3);
    }

    //退出公会
    void QuitCorps()
    {
        int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
        int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
        NetMgr.instance.CorpsHandler.QuitCorps(corpsId, heroId);
    }
    public void ResetView()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }
    void OnScrollChanged(Vector2 v)
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        m_moreArrow.gameObject.SetActive(part.corpsInfo.members.Count >= 5 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }

    public void OnUpdateData(bool defaultSort = false)
    {
        if(defaultSort)
            m_sortShow.SetState(1);
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        int count = part.corpsInfo.members.Count;
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(count >= 5 ? true : false);
        for (int i = 0; i < count; i++)
        {
            CorpsMemberItem sel = m_GridGroup.Get<CorpsMemberItem>(i);
            sel.OnSetData(part.corpsInfo.members[i]);
        }
        m_preState.SetState(part.personalInfo.pos == (int) CorpsPosEnum.President ? 0 : 1);
        m_joinSetBtn.gameObject.SetActive(part.personalInfo.pos <= (int)CorpsPosEnum.Elder ? true : false);

        m_impeachTxt.text = part.corpsInfo.impeach.initiateId > 0 ? LanguageCfg.Get("impeach_progress") : LanguageCfg.Get("impeach_crops");
        CheckTip();   //检测叹号
    }
    //检测叹号
    void CheckTip()
    {
        m_impTip.gameObject.SetActive(false);
        CorpsInfo info = RoleMgr.instance.Hero.CorpsPart.corpsInfo;
        if (RoleMgr.instance.Hero.CorpsPart.personalInfo.contribution < CorpsBaseCfg.Get().supportContribute)  //达不到贡献要求
            return;
        if(info.impeach.initiateId > 0)
        {
            int myId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
            bool has = false;
            //找一下自己赞成过没有
            for(int i = 0,len = info.impeach.agree.Count; i < len; ++i)
            {
                if (info.impeach.agree[i] == myId)
                {
                    has = true;
                    break;
                }
            }
            m_impTip.gameObject.SetActive(!has);
        }
    }
}
