using UnityEngine;
using System.Collections;

public class ActionLeaveFightCamera : SceneAction
{
    public ActionCfg_LeaveFight mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_LeaveFight;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("离开战斗状态镜头");

        if (SceneMgr.instance.FightCamera == null)
        {
            Debug.LogError("战斗镜头已经不存在");
            return;
        }

        SceneMgr.instance.FightCamera.Release();
        SceneMgr.instance.FightCamera = null;
    }
}
