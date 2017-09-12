using UnityEngine;
using System.Collections;

public class ActionHideWave : SceneAction
{
    public ActionCfg_HideWave mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideWave;
    }

    public override void OnAction()
    {
        UILevelAreaWave uiAreaWave = UIMgr.instance.Get<UILevel>().Get<UILevelAreaWave>();
        if (uiAreaWave.IsOpen)
            uiAreaWave.CloseArea();
        else
            Debuger.LogError("当前没有显示波数界面 却又隐藏波数事件");
    }
}
