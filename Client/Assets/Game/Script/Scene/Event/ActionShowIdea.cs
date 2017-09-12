using UnityEngine;
using System.Collections;

public class ActionShowIdea : SceneAction
{
    public ActionCfg_ShowIdea mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowIdea;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("显示内心独白");

        UILevel uiLevel = UIMgr.instance.Get<UILevel>();
        if (uiLevel.IsOpen)
        {
            uiLevel.Get<UILevelAreaNotice>().SetBottomNotice(mActionCfg.desc);
        }
        else
        {
            Debug.LogError("关卡界面隐藏时出现内心独白");
        }
    }
}