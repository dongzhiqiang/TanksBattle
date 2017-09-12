using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITriedCell : MonoBehaviour
{
    //试炼名
    public TextEx m_triedName;
    //介绍
    public TextEx m_intro;
    //星级
    public StateHandle m_start;
    //金币奖励
    public TextEx m_gold;
    //金苹果奖励
    public TextEx m_special;
    //物品格子
    public UIItemIcon m_item;
    //前往按钮
    public StateHandle m_goToBtn;
    //已领取按钮
    public StateHandle m_getBtn;
    //试炼完成状态
    public StateHandle m_completeState;

    bool m_isInit;
    WarriorTriedLevel m_levelInfo;
    int m_index;
    const int MedalItemId = 30013;

    public void OnSetData(WarriorTriedLevel levelInfo, int index)
    {
        if (!m_isInit)
            OnInit();
        m_index = index;
        m_levelInfo = levelInfo;
        TriedLevelConfig lvCfg =  TriedLevelCfg.Get(levelInfo.room);
        RoomCfg room = RoomCfg.GetRoomCfgByID(lvCfg.roomId);
        m_triedName.text = room.roomName;
        m_start.SetState(levelInfo.star - 1);
        RoomConditionCfg condition = RoomConditionCfg.GetCfg(lvCfg.passId);
        m_intro.text = condition.endDesc;
        
        for(int i =0,len =levelInfo.rewards.Count; i<len;++i )
        {
            if (levelInfo.rewards[i].itemId == ITEM_ID.EXP)
                m_gold.text = levelInfo.rewards[i].num.ToString();
            else if (levelInfo.rewards[i].itemId == MedalItemId)
                m_special.text = levelInfo.rewards[i].num.ToString();
            else
            {
                m_item.Init(levelInfo.rewards[i].itemId, levelInfo.rewards[i].num);  //道具奖励  
                m_item.isSimpleTip = true;
            }
        }

        //完成状态
         TimeMgr.instance.AddTimer(0.1f,()=> { m_completeState.SetState(levelInfo.status); });   //加一个延迟
    }

    public void OnInit()
    {
        //前往
        m_goToBtn.AddClick(OnBtnGoto);
        //领取奖励
        m_getBtn.AddClick(OnBtnGetReward);
        m_isInit = true;
    } 

    void OnBtnGoto()
    {
        if (CheckAccess())
            NetMgr.instance.ActivityHandler.SendEnterWarrior(m_index);
    }
    void OnBtnGetReward()
    {
        NetMgr.instance.ActivityHandler.SendWarriorGetReward(m_index);
    }

    //检查是否可以进入副本 还未实现
    bool CheckAccess()
    {
        //检查系统是否开启
        string errMsg;
        if(!SystemMgr.instance.IsEnabled(enSystem.warriorTried, out errMsg))
        {
            UIMessage.Show("勇士试炼系统未开启");
            return false;
        }
        //检查完成状态
        if(m_levelInfo.status != 0)
        {
            UIMessage.Show("试炼已完成");
            return false;
        }
        //检查剩余试炼次数
        if(RoleMgr.instance.Hero.ActivityPart.warriorTried.remainTried == 0)
        {
            UIMessage.Show("试炼次数不足");
            return false;
        }
        return true;
    }
}
