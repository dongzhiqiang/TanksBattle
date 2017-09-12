using UnityEngine;
using System.Collections;

public class ActionActivateArea : SceneAction
{
    public ActionCfg_ActivateArea mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ActivateArea;
    }

    public override void OnAction()
    {

        AreaTrigger area = SceneEventMgr.instance.GetAreaTriggerByFlag(mActionCfg.flagId);
        if (area == null)
        {
            return;
        }
        area.gameObject.SetActive(true);
    }
}
