using UnityEngine;
using System.Collections;

public class ActionNextTeach : SceneAction
{
    public ActionCfg_NextTeach mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_NextTeach;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("推进引导");

        if (string.IsNullOrEmpty(mActionCfg.eventType) || string.IsNullOrEmpty(mActionCfg.eventParam))
            Debug.LogError("没有配置事件id");
        else
            TeachMgr.instance.OnDirectTeachEvent(mActionCfg.eventType, mActionCfg.eventParam);
    }
}