using UnityEngine;
using System.Collections;

public class ActionStartTeach : SceneAction
{
    public ActionCfg_StartTeach mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_StartTeach;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("开始引导");

        if (string.IsNullOrEmpty(mActionCfg.teachId))
            Debug.LogError("没有配置引导id");
        else
            TeachMgr.instance.PlayTeach(mActionCfg.teachId);
    }
}