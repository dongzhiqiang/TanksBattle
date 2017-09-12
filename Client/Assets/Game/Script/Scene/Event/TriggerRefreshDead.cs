using UnityEngine;
using System.Collections;

public class TriggerRefreshDead : SceneTrigger
{
    public EventCfg_RefreshDead m_eventCfg;

    bool bReached = false;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_RefreshDead;
        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback += OnNpcDeadStart;
        else
            LevelMgr.instance.OnRoleDeadEndCallback += OnNpcDeadEnd;
    }

    public override bool bReach()
    {
        return bReached;
    }

    public void OnNpcDeadStart(Role role)
    {
        if (m_eventCfg.bDeadStart)
            OnNpcDead(role);
    }

    public void OnNpcDeadEnd(Role role)
    {
        if (!m_eventCfg.bDeadStart)
            OnNpcDead(role);
    }

    public void OnNpcDead(Role role)
    {
        if (!bReached && mRun)
        {
            RefreshBase refreshNpc = SceneMgr.instance.GetRefreshNpcByFlag(m_eventCfg.flagId);
            if (refreshNpc == null )
            {
                Debug.LogError("没有找到刷新组id ：" + m_eventCfg.flagId);
                return;
            }
            if (refreshNpc.mState != RefreshBase.RefreshState.END)
            {
                bReached = false;
                return;
            }

            foreach (Role r in LevelMgr.instance.CurLevel.mRoleDic.Values)
            {
                //死亡效果播放前就进来了 角色在列表里没删除
                if (r != role && r.State == Role.enState.alive && r.GetFlag(m_eventCfg.flagId) > 0)
                {
                    bReached = false;
                    return;
                }
            }

            bReached = true;
            SceneEventMgr.instance.FireAction(mFlag);
        }

    }

    public override void Reset()
    {
        bReached = false;
    }

    public override void OnRelease()
    {
        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback -= OnNpcDeadStart;
        else
            LevelMgr.instance.OnRoleDeadEndCallback -= OnNpcDeadEnd;
    }

}
