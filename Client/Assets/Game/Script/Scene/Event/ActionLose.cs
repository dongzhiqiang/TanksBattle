using UnityEngine;
using System.Collections;

public class ActionLose : SceneAction {

    public ActionCfg_Lose mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Lose;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("设置失败");

        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.LOSE, true);
        LevelMgr.instance.SetLose();

    }
}
