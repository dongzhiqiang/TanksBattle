using UnityEngine;
using System.Collections;

public class ActionChangeScene : SceneAction
{
    public ActionCfg_ChangeScene mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ChangeScene;
    }

    public override void OnAction()
    {
        Main.instance.StartCoroutine(SceneMgr.instance.CoChangeScene(mActionCfg.sceneName));
    }
}