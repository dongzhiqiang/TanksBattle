using UnityEngine;
using System.Collections;

public class ActionTimeScale : SceneAction
{

    public ActionCfg_TimeScale mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_TimeScale;
    }

    public override void OnAction()
    {
        TimeMgr.TimeScaleHandle timeHandle = TimeMgr.instance.AddTimeScale(mActionCfg.ratio, mActionCfg.duration, 100);
    }
}
