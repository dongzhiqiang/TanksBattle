using UnityEngine;
using System.Collections;

public class ActionShowTips : SceneAction
{
    public ActionCfg_ShowTips mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowTips;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("显示关卡提示");

        UILevel uiLevel = UIMgr.instance.Get<UILevel>();
        if (uiLevel.IsOpen)
        {
            uiLevel.Get<UILevelAreaNotice>().SetTopNotice(mActionCfg.desc);
        }
        else
        {
            Debug.LogError("关卡界面隐藏时出现提示");
        }
    }
}
