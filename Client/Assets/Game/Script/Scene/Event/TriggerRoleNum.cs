using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerRoleNum : SceneTrigger {


    public EventCfg_RoleNum m_eventCfg;

    bool bReached = false;

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_RoleNum;

        LevelMgr.instance.OnRoleDeadCallback += OnUpdateNum;
        LevelMgr.instance.OnRoleBornCallback += OnUpdateNum;
    }
    
    public void OnUpdateNum(object param)
    {
        if (bReached)
            return;

        Dictionary<int, Role> roleDict = LevelMgr.instance.CurLevel.mRoleDic;

        int curNum = 0;
        if (string.IsNullOrEmpty(m_eventCfg.roleId))
        {
            curNum = roleDict.Count - 1;    //去掉主角 //宠物和陷阱宝箱什么的 看以后需求
        }
        else
        {
            foreach(var pair in roleDict)
            {
                if (pair.Value.Cfg.id == m_eventCfg.roleId)
                    curNum++;
            }
        }


        if (m_eventCfg.bUnder)  //当前数量低于配置数量触发
        {
            if (curNum < m_eventCfg.roleNum)
            {
                bReached = true;
                SceneEventMgr.instance.FireAction(mFlag);
            }
            else
                bReached = false;
        }
        else
        {
            if (curNum > m_eventCfg.roleNum)    //当前数量高于配置数量触发
            {
                bReached = true;
                SceneEventMgr.instance.FireAction(mFlag);
            }
            else
                bReached = false;
        }
    }
    public override void OnRelease()
    {
        LevelMgr.instance.OnRoleDeadCallback -= OnUpdateNum;
        LevelMgr.instance.OnRoleBornCallback -= OnUpdateNum;
    }

    public override bool bReach()
    {
        return bReached;
    }
}
