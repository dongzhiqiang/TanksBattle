using UnityEngine;
using System.Collections;

public class ActionShowWave : SceneAction
{
    public ActionCfg_ShowWave mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowWave;
    }

    public override void OnAction()
    {
        UILevelAreaWave uiAreaWave = UIMgr.instance.Get<UILevel>().Get<UILevelAreaWave>();
        if (!uiAreaWave.IsOpen)
            uiAreaWave.OpenArea();
        else
            Debuger.LogError("已经有显示波数 重复设置");

        uiAreaWave.SetDesc(mActionCfg.waveDesc);
        uiAreaWave.SetWave(0, mActionCfg.maxWave);
    }
}
