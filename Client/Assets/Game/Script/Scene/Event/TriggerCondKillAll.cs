using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerCondKillAll : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    public int m_count;

    //int m_observer;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        //m_observer = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.WIN, OnLevelEnd);
        LevelMgr.instance.OnRoleDeadStartCallback += OnNpcDead;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = false;
    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
        //if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        LevelMgr.instance.OnRoleDeadStartCallback -= OnNpcDead;
    }

    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc, m_count);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }
    
    void OnNpcDead(Role ro)
    {
        if (!mRun)
            return;
        Dictionary<string, RefreshBase> refDict = SceneMgr.instance.refreshNpcDict;
        foreach(RefreshBase r in refDict.Values)
        {
            //没有激活的刷新组 有怪也失败
            if (r.mState == RefreshBase.RefreshState.INIT)
            {
                List<RefreshPoint> pointList = r.refPointList;
                for (int i = 0; i < pointList.Count; i++)
                {
                    RoleCfg cfg = RoleCfg.Get(pointList[i].mPointCfg.roleId);
                    if (cfg.roleType != enRoleType.box && cfg.roleType != enRoleType.trap)
                    {
                        bAchieve = false;
                        return;
                    }
                }
            }

            if (r.mState != RefreshBase.RefreshState.INIT)
            {
                List<RefreshPoint> pointList = r.refPointList;
                for (int i = 0; i < pointList.Count; i++)
                {
                    foreach(var pair in pointList[i].roleDic)
                    {
                        var role = pair.Value;
                        bool bDead = role.IsUnAlive(pair.Key);
                        if (pair.Key == ro.Id)
                            bDead = true;
                        if (!bDead && role.Cfg.roleType != enRoleType.box && role.Cfg.roleType != enRoleType.trap && role.GetCamp() != enCamp.neutral && role.GetCamp() != enCamp.camp1)
                        {
                            bAchieve = false;
                            return;
                        }
                    }
                }
            }
        }
        bAchieve = true;
    }
}