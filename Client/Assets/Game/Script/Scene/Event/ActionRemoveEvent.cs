using UnityEngine;
using System.Collections;

public class ActionRemoveEvent : SceneAction
{

    public ActionCfg_RemoveEvent mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_RemoveEvent;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("移除事件");

        SceneTrigger trigger = SceneEventMgr.instance.GetTrigger(mActionCfg.eventId);
        if (trigger != null && !trigger.IsTriggering)
            SceneEventMgr.instance.RemoveEvent(mActionCfg.eventId);
    }
}
