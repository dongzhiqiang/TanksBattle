using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UICorps : UIPanel
{
    //页签控制
    public StateHandle m_pageHandler;
    //页签按钮
    public StateGroup m_tab;
    //总览页面
    public UICorpsOverView m_overview;
    //成员页面
    public UICorpsMembers m_members;
    //请求页面
    public UICorpsReqPage m_reqs;
    //申请人页签
    public StateHandle m_reqTab;
    //申请页签上的叹号
    public ImageEx m_reqTip;
    

    public override void OnInitPanel()
    {
        m_tab.AddSel(OnSelectTab);
        m_overview.OnInit();
        m_members.OnInit();
        m_reqs.OnInit();
    }

    public override void OnOpenPanel(object param)
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
        //请求公会基础数据
        NetMgr.instance.CorpsHandler.ReqCorpsData(corpsId, !part.hasInit);

        if (part.hasInit) //已经读过初始化会员数据
        {
            NetMgr.instance.CorpsHandler.ReqCorpsMembersReqs(corpsId, false);
        }
        else  //第一次请求会员数据
            NetMgr.instance.CorpsHandler.ReqCorpsMembersReqs(corpsId, true);
        
        //UI回复初始状态
        m_tab.SetSel(0);
        m_pageHandler.SetState(0);
        m_members.ResetView();
        m_reqs.ResetView();

    }
    public override void OnClosePanel()
    {
    }
    //更新总览界面
    public void UpdateOverViewPage()
    {
        m_overview.OnUpdateProp();
   
    }
    //更新公会成员信息、申请信息
    public void UpdateMembersPage()
    {
        m_overview.OnUpdateMember();
        m_members.OnUpdateData(true);
        m_reqs.OnUpdateData();

        CheckReqTabTip();
    }
    //检测申请页签上的叹号是否显示
    public void CheckReqTabTip()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        if (part.personalInfo.pos == (int)CorpsPosEnum.Common)
            m_reqTab.gameObject.SetActive(false);
        else
            m_reqTab.gameObject.SetActive(true);
        m_reqTip.gameObject.SetActive(part.corpsInfo.reqs.Count > 0 ? true : false);
    }





















    //点击标签
    void OnSelectTab(StateHandle select, int idx)
    {
        m_pageHandler.SetState(idx);

    }


}
