using UnityEngine;
using System.Collections;

public class ActionRemoveNpc : SceneAction {

    public ActionCfg_RemoveNpc mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_RemoveNpc;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("移除npc");

        LevelMgr.instance.CurLevel.RemoveRoleByFlag(mActionCfg.npcFlag);

    }
}
