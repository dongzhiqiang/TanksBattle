using UnityEngine;
using System.Collections;

public class TriggerWin : SceneTrigger
{
    public EventCfg_Win m_eventCfg;
    public bool bAchieve;
    int ob;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_Win;
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.WIN, OnLevelWin);
    }

    void OnLevelWin(object param)
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("关卡成功事件");

        SceneEventMgr.instance.FireAction(mFlag);
        
    }

    public override void OnRelease()
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
    }
}
