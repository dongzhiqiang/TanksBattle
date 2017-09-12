using UnityEngine;
using System.Collections;

public class ActionHideAllRole : SceneAction
{

    public ActionCfg_HideAllRole mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideAllRole;
    }

    public override void OnAction()
    {
        RoleMgr.instance.ShowAllRole(false, false);
    }
}