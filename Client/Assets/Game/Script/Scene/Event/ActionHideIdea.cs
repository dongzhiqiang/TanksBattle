using UnityEngine;
using System.Collections;

public class ActionHideIdea : SceneAction
{
    public ActionCfg_HideIdea mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideIdea;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("隐藏内心独白");

        UIMgr.instance.Get<UILevel>().Get<UILevelAreaNotice>().CloseBottomNotice();
    }
}