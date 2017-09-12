using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
public class UICheckIn : MonoBehaviour {

    public StateGroup m_checkInGroup;
    public ScrollRect m_CheckInScroll;
    public TextEx m_title;
    int m_checkInNums;
    bool m_isCheckIn;
    long m_lastCheckInTime;
    CheckInRewardCfg checkInReward;
    bool isDouble = false;


    public void Init()
    {
        m_checkInGroup.AddSel(OnSelectCheckInItem);
        LoadCheckIn();
    }

    void OnSelectCheckInItem(StateHandle s, int idx)
    {
        UICheckInItem item = s.GetComponent<UICheckInItem>();
        if (item.state != UICheckInItem.CheckInItemState.CanCheckIn)
        {
            UIMgr.instance.Open<UIItemTip>(item.itemCfg.id);
        }
        else
        {                      
            if (idx == m_checkInNums)
            {                
                NetMgr.instance.OpActivityHandler.SendCheckIn();
            }
        }
    }
    public void GetCheckInReward(int checkInRewardId)
    {
        CheckInRewardCfg checkInRewardCfg = CheckInRewardCfg.m_cfgs[checkInRewardId];
        Role hero = RoleMgr.instance.Hero;
        int vipLv = hero.GetInt(enProp.vipLv);
        isDouble = vipLv >= checkInRewardCfg.vipLevel && checkInRewardCfg.vipLevel != 0;     
        RewardItem rewardItem = new RewardItem();
        rewardItem.itemId = checkInRewardCfg.itemId;
        rewardItem.itemNum = isDouble ? checkInRewardCfg.itemNums * 2 : checkInRewardCfg.itemNums;
        List<RewardItem> rewardItems = new List<RewardItem>();
        rewardItems.Add(rewardItem);
        UIMgr.instance.Open<UIReward>(rewardItems);
    }



 
    public  void LoadCheckIn()
    {
        //读取签到数据
        Role hero = RoleMgr.instance.Hero;
        OpActivityPart opActivityPart = hero.OpActivityPart;
        m_lastCheckInTime = opActivityPart.GetLong(enOpActProp.lastCheckIn);      
        if (TimeMgr.instance.IsToday(m_lastCheckInTime))
        {
            m_checkInNums = opActivityPart.GetInt(enOpActProp.checkInNums);
            m_isCheckIn = true;
        }
        else
        {
            m_isCheckIn = false;
            if (TimeMgr.instance.IsThisMonth(m_lastCheckInTime))
            {
                m_checkInNums = opActivityPart.GetInt(enOpActProp.checkInNums);               
            }
            else
            {
                m_checkInNums = 0;
                //opActivityPart.CurCheckInNums = 0;
            }
        }

        //Debug.Log("上次签到时间：" + TimeMgr.instance.TimestampToServerDateTime(m_lastCheckInTime));
        m_title.text = "本月累积签到:" + m_checkInNums + "天";
        DateTime curTime = TimeMgr.instance.GetServerDateTime();
        int curMonth = curTime.Month;
        int cfgLength = CheckInRewardCfg.GetLengthByMonth(curMonth);
        int dayInCurMonth = DateTime.DaysInMonth(curTime.Year, curMonth);
        if(cfgLength< dayInCurMonth)
        {
            Debug.LogError("签到配置表有误，填的天数小于当月天数");
            return;
        }
        m_checkInGroup.SetCount(dayInCurMonth);
        for (int i = 0; i < m_checkInGroup.Count; i++)
        {
            UICheckInItem checkinItem = m_checkInGroup.Get<UICheckInItem>(i);
            CheckInRewardCfg checkInrewardCfg = CheckInRewardCfg.m_cfgs[CheckInRewardCfg.GetFirstIdByMonth(curMonth) + i];
            if (i < m_checkInNums)
            {
                checkinItem.HasCheckedInInit(checkInrewardCfg);
            }
            else if (i == m_checkInNums)
            {
                if (!m_isCheckIn)
                {
                    checkinItem.CanCheckInInit(checkInrewardCfg);
                }
                else
                {
                    checkinItem.NotCheckInInit(checkInrewardCfg);
                }
            }
            else if (i > m_checkInNums)
            {
                checkinItem.NotCheckInInit(checkInrewardCfg);
            }
        }
        if(m_checkInNums<=9)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_CheckInScroll, 0); });
        }
        else if (m_checkInNums > 9 && m_checkInNums < 15)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_CheckInScroll, 15); });
        }
        else if (m_checkInNums >= 15)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_CheckInScroll, 20); });
        }
    }
}
