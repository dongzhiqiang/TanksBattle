using UnityEngine;
using System.Collections;

public class UICorpsImpeach : UIPanel {
    //控制状态
    public StateHandle m_typeState;
    //发起弹劾界面的取消按钮
    public StateHandle m_cancelBtn1;
    //发起弹劾界面的弹劾按钮
    public StateHandle m_impeachBtn;
    //赞成弹劾界面的取消按钮
    public StateHandle m_cancelBtn2;
    //赞成弹劾界面的同意按钮
    public StateHandle m_agreeBtn;
    //自己的贡献
    public TextEx m_ownContribute;
    //弹劾未达到要求的提示
    public TextEx m_unAchieve;
    //弹劾剩余时间
    public TextEx m_remainTime;
    //同意弹劾的人数
    public TextEx m_agreeNum;
    //赞成弹劾
    public TextEx m_hasAgreed;
    //发起人
    public TextEx m_originator;

    float remainTime;
    int showType;   //0 无人弹劾  1有人弹劾中
    
    public override void OnInitPanel()
    {
        //取消
        m_cancelBtn1.AddClick(() =>
        {
            Close();
        });
        m_cancelBtn2.AddClick(() =>
        {
            Close();
        });
        //发起弹劾按钮
        m_impeachBtn.AddClick(() =>
        {
            int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
            int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
            NetMgr.instance.CorpsHandler.InitiateImpeach(corpsId, heroId);
        });
        //赞成弹劾按钮
        m_agreeBtn.AddClick(() =>
        {
            int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
            int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
            NetMgr.instance.CorpsHandler.AgreeImpeach(corpsId, heroId);
        });
    }

    public override void OnOpenPanel(object param)
    {
        NetMgr.instance.CorpsHandler.ReqImpeachInfo(RoleMgr.instance.Hero.GetInt(enProp.corpsId));
    }
    
    public override void OnUpdatePanel()
    {
        if (showType == 0)
            return;
        remainTime -= Time.unscaledDeltaTime;
        SetRemainTime();
    }
    public override void OnClosePanel()
    {
        m_agreeBtn.GetComponent<ImageEx>().SetGrey(false);
        m_impeachBtn.GetComponent<ImageEx>().SetGrey(false);
    }
    public void UpdatePanel()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        ImpeachInfo impeachInfo = part.corpsInfo.impeach;
        CorpsMember personal = part.personalInfo;
        //默认显示
        m_unAchieve.text = "";
        m_hasAgreed.text = "";
        m_remainTime.text = "";
        m_agreeBtn.gameObject.SetActive(true);
        m_impeachBtn.gameObject.SetActive(true);

        if (impeachInfo.initiateId > 0)   //已经有人发起了弹劾
        {
            showType = 1;
            m_typeState.SetState(showType);
            if (impeachInfo.initiateId == RoleMgr.instance.Hero.GetInt(enProp.heroId))  //发起弹劾的人是自己
            {
                m_agreeBtn.gameObject.SetActive(false);
                m_hasAgreed.text = "您已发起弹劾";
            }
            else
            {
                if (personal.contribution >= CorpsBaseCfg.Get().supportContribute)   //自己的贡献达到了要求
                {
                    int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
                    bool hasAgreed = false;
                    //判断是否赞成过
                    for (int i = 0, len = impeachInfo.agree.Count; i < len; ++i)
                    {
                        if (heroId == impeachInfo.agree[i])
                        {
                            hasAgreed = true;
                            break;
                        }
                    }
                    if (hasAgreed)
                    {
                        m_agreeBtn.gameObject.SetActive(false);
                        m_hasAgreed.text = "您已赞成";
                    }
                    else
                    {
                        m_agreeBtn.GetComponent<ImageEx>().SetGrey(false);
                        m_agreeBtn.GetComponent<StateHandle>().enabled = true;
                    }
                }
                else
                {
                    m_agreeBtn.GetComponent<ImageEx>().SetGrey(true);
                    m_agreeBtn.GetComponent<StateHandle>().enabled = false;
                    m_unAchieve.text = "未达到赞成弹劾条件";
                }
            }

            remainTime = CorpsBaseCfg.Get().impTime - (TimeMgr.instance.GetTimestamp() - impeachInfo.time);
            SetRemainTime();
            m_originator.text = impeachInfo.initiateName;
            m_agreeNum.text = impeachInfo.agree.Count + "/" + CorpsBaseCfg.Get().supportNum;
        }
        else
            NormalShow();
    }

    //无人弹劾的显示界面
    void NormalShow()
    {
        showType = 0;
        m_remainTime.text = "";
        m_typeState.SetState(showType);
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        CorpsMember personal = part.personalInfo;
        m_ownContribute.text = personal.contribution.ToString();
        if (TimeMgr.instance.GetTimestamp() - part.GetPresident().lastLogout >= CorpsBaseCfg.Get().CDROfftime
           && personal.contribution >= CorpsBaseCfg.Get().impContribute)   //会长不在线时间超出了规定时间并且自己的贡献达到要求
        {
            m_impeachBtn.GetComponent<ImageEx>().SetGrey(false);
            m_impeachBtn.GetComponent<StateHandle>().enabled = true;
        }
        else
        {
            m_impeachBtn.GetComponent<ImageEx>().SetGrey(true);
            m_impeachBtn.GetComponent<StateHandle>().enabled = false;
            m_unAchieve.text = "未达到弹劾条件";
        }
    }

    void SetRemainTime()
    {
        if (remainTime < 0)  //时间倒计时结束界面回归初始
        {
            remainTime = 0;
            NormalShow();
        }
        else
            m_remainTime.text = StringUtil.FormatTimeSpan((long)remainTime);
    }
}
