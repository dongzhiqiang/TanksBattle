using UnityEngine;
using System.Collections;

public class ActionMsg : SceneAction
{

    public ActionCfg_Msg mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Msg;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("发送消息");

        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.EVENTMSG, mActionCfg.id, mActionCfg.content);
    }
}