using UnityEngine;
using System.Collections;

public class TriggerArea : SceneTrigger
{

    public EventCfg_Area m_eventCfg;
    public bool bAchieve;

    int enterObserver ;
    int leaveObserver ;

    float startTime = 0;
    bool bUpdate = false;

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_Area;
    }
    public override void Start()
    {
        base.Start();
        
        //bAchieve = !m_eventCfg.bEnter;
        bAchieve = false;
        enterObserver = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.ENTERAREA, OnEnterArea);
        leaveObserver = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.LEAVEAREA, OnLeaveArea);
    }

    public void OnEnterArea(object param)
    {
        AreaTriggerCxt cxt = param as AreaTriggerCxt;
        if (mRun && cxt.areaId == m_eventCfg.areaFlag)
        {
            //主角进入区域
            if (m_eventCfg.bHaveHero && cxt.role.IsHero)
            {
                if (m_eventCfg.bEnter)      //配置的是进入 bEnter为true
                {
                    if (SceneMgr.SceneDebug)
                        Debug.Log(string.Format("进入区域{0}", m_eventCfg.areaFlag));

                    if (m_eventCfg.timeNum > 0)     //需要延时
                    {
                        startTime = TimeMgr.instance.logicTime;
                        bUpdate = true;
                    }
                    else
                    {
                        bAchieve = true;
                        SceneEventMgr.instance.FireAction(mFlag);
                    }
                }
            }
            //配置的其他怪进入区域
            if (!string.IsNullOrEmpty(m_eventCfg.NpcIdList))
            {
                if (m_eventCfg.NpcIdList.Contains(cxt.role.GetString(enProp.roleId)))
                {
                    if (m_eventCfg.bEnter)      //配置的是进入 bEnter为true
                    {
                        if (SceneMgr.SceneDebug)
                            Debug.Log(string.Format("进入区域{0}", m_eventCfg.areaFlag));

                        if (m_eventCfg.timeNum > 0)     //需要延时
                        {
                            startTime = TimeMgr.instance.logicTime;
                            bUpdate = true;
                        }
                        else
                        {
                            bAchieve = true;
                            SceneEventMgr.instance.FireAction(mFlag);
                        }
                    }
                }
            }
        }
    }

    public void OnLeaveArea(object param)
    {
        AreaTriggerCxt cxt = param as AreaTriggerCxt;
        if (mRun && cxt.areaId == m_eventCfg.areaFlag)
        {
            //主角离开区域
            if (m_eventCfg.bHaveHero && cxt.role.IsHero)
            {
                if (!m_eventCfg.bEnter)     //配置的是离开 bEnter为false
                {
                    if (SceneMgr.SceneDebug)
                        Debug.Log(string.Format("{0}离开区域{1}", cxt.role.Cfg.name, m_eventCfg.areaFlag));
                    if (m_eventCfg.timeNum > 0)     //需要延时
                    {
                        startTime = TimeMgr.instance.logicTime;
                        bUpdate = true;
                    }
                    else
                    {
                        bAchieve = true;
                        SceneEventMgr.instance.FireAction(mFlag);
                    }
                }
            }

            //配置的其他怪离开区域
            if (!string.IsNullOrEmpty(m_eventCfg.NpcIdList))
            {
                if (m_eventCfg.NpcIdList.Contains(cxt.role.GetString(enProp.roleId)))
                {
                    if (!m_eventCfg.bEnter) //配置的是离开 bEnter为false
                    {
                        if (SceneMgr.SceneDebug)
                            Debug.Log(string.Format("{0}离开区域{1}", cxt.role.Cfg.name, m_eventCfg.areaFlag));
                        if (m_eventCfg.timeNum > 0)     //需要延时
                        {
                            startTime = TimeMgr.instance.logicTime;
                            bUpdate = true;
                        }
                        else
                        {
                            bAchieve = true;
                            SceneEventMgr.instance.FireAction(mFlag);
                        }

                    }
                }
            }
        }

    }

    public override bool bReach()
    {
        return bAchieve;
    }

    public override void Update()
    {
        if (bUpdate)    //触发之后不再更新
        {
            int overTime = (int)(TimeMgr.instance.logicTime - startTime);
            if (overTime >= m_eventCfg.timeNum)
            {
                bAchieve = true;
                SceneEventMgr.instance.FireAction(mFlag);
                bUpdate = false;
            }
        }
        
    }

    public override void OnRelease()
    {
        if (enterObserver != EventMgr.Invalid_Id) { EventMgr.Remove(enterObserver); enterObserver = EventMgr.Invalid_Id; }
        if (leaveObserver != EventMgr.Invalid_Id) { EventMgr.Remove(leaveObserver); leaveObserver = EventMgr.Invalid_Id; }
    }

}
