using UnityEngine;
using System.Collections;

public class ActionNone : SceneAction
{

    public ActionCfg_None mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_None;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("空行为");
    }
}