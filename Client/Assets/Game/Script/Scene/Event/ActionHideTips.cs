using UnityEngine;
using System.Collections;

public class ActionHideTips : SceneAction
{
    public ActionCfg_HideTips mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideTips;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("隐藏关卡提示");

        UIMgr.instance.Get<UILevel>().Get<UILevelAreaNotice>().CloseTopNotice();
    }
}
