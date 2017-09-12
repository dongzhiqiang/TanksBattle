using UnityEngine;
using System.Collections;

public class TriggerFinishEvent : SceneTrigger
{

    public EventCfg_FinishEvent m_eventCfg;

    bool bReached = false;
    int ob;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_FinishEvent;
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EVENT_FINISH, OnFinishEvent);
    }

    void OnFinishEvent(object param)
    {
        string flag = param as string;
        if (flag == m_eventCfg.eventId)
        {
            bReached = true;
            SceneEventMgr.instance.FireAction(mFlag);
        }
    }

    public override bool bReach()
    {
        return bReached;
    }

    public override void OnRelease()
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
    }
}
