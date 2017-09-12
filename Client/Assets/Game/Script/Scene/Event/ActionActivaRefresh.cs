using UnityEngine;
using System.Collections;

public class ActionActivaRefresh : SceneAction
{
    public ActionCfg_ActivateRefresh mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ActivateRefresh;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
          Debug.Log(string.Format("激活刷新点 {0}", mActionCfg.refreshID));

        RefreshBase refresh = SceneMgr.instance.GetRefreshNpcByFlag(mActionCfg.refreshID);
        if (refresh != null)
        {
            refresh.Start();
        }
    }
}