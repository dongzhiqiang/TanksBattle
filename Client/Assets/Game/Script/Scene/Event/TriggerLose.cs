using UnityEngine;
using System.Collections;

public class TriggerLose : SceneTrigger
{
    public EventCfg_Lose m_eventCfg;
    public bool bAchieve;
    int ob;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_Lose;
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.LOSE, OnLevelLose);
    }

    void OnLevelLose()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("关卡失败事件");

        SceneEventMgr.instance.FireAction(mFlag);
        
    }

    public override void OnRelease() 
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
    }

}
