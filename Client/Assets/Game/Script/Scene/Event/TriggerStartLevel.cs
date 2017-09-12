using UnityEngine;
using System.Collections;

public class TriggerStartLevel : SceneTrigger {
    public EventCfg_StartLevel m_eventCfg;
    bool bAchieve = false;
    int ob;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_StartLevel;
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.START, OnStartLevel);
    }

    void OnStartLevel()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("加载完开始关卡");

        bAchieve = true;
        SceneEventMgr.instance.FireAction(mFlag);
        
    }
    public override bool bReach()
    {
        return bAchieve;
    }

    public override void OnRelease()
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
    }

}
