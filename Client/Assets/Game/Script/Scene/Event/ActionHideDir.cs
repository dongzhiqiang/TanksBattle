using UnityEngine;
using System.Collections;

public class ActionHideDir : SceneAction
{
    public ActionCfg_HideDir mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideDir;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("结束寻路");

        SceneMgr.instance.OverFind();
    }
}
