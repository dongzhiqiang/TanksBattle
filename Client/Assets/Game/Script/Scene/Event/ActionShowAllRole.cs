using UnityEngine;
using System.Collections;

public class ActionShowAllRole : SceneAction
{

    public ActionCfg_ShowAllRole mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowAllRole;
    }

    public override void OnAction()
    {
        RoleMgr.instance.ShowAllRole(true, false);
    }
}