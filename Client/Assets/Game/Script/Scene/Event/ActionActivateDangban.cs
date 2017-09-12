using UnityEngine;
using System.Collections;

public class ActionActivateDangban : SceneAction
{
    public ActionCfg_ActivateDangban mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ActivateDangban;
    }

    public override void OnAction()
    {

        GameObject go = SceneMgr.instance.GetDangban(mActionCfg.flagId);
        if (go == null)
        {
            Debuger.LogError("找不到挡板id：" + mActionCfg.flagId);
            return;
        }

        go.gameObject.SetActive(true);
    }
}
