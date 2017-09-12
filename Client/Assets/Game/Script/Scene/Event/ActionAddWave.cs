using UnityEngine;
using System.Collections;

public class ActionAddWave : SceneAction
{
    public ActionCfg_AddWave mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_AddWave;
    }

    public override void OnAction()
    {
        UILevelAreaWave uiAreaWave = UIMgr.instance.Get<UILevel>().Get<UILevelAreaWave>();
        if (!uiAreaWave.IsOpen)
            Debuger.LogError("波数显示界面没有初始化");
        else
            uiAreaWave.AddWave();
    }
}
