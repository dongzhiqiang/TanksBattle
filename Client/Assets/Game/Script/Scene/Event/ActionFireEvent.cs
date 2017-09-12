using UnityEngine;
using System.Collections;

public class ActionFireEvent : SceneAction
{
    public ActionCfg_FireEvent mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_FireEvent;
    }

    public override void OnAction()
    {
        Room.instance.StartCoroutine(StarFire());
    }

    IEnumerator StarFire()
    {
        SceneTrigger trigger = SceneEventMgr.instance.GetTrigger(mActionCfg.eventId);
        if (trigger == null)
        {
            Debug.LogError("没有找到事件" + mActionCfg.eventId);
            yield break;
        }
        for (int i = 0; i < trigger.mTriggerTimes; i++)
        {
            SceneEventMgr.instance.FireAction(mActionCfg.eventId);
            yield return new WaitForSeconds(mActionCfg.delayTime);
        }
        yield return 0;
    }
}
