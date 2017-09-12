using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionChangeAI : SceneAction
{
    public ActionCfg_ChangeAI mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ChangeAI;

        //预加载
        Simple.BehaviorTree.BehaviorTreeMgrCfg.PreLoad(mActionCfg.ai);
    }

    public override void OnAction()
    {
        LevelMgr.instance.OnRoleBornCallback += OnCreateRole;
        Dictionary<int, Role> roleDict = LevelMgr.instance.CurLevel.mRoleDic;
        foreach (Role role in roleDict.Values)
        {
            if (role.GetFlag(mActionCfg.groupId) > 0)
                role.AIPart.Play(mActionCfg.ai);
        }

    }

    void OnCreateRole(Role role)
    {
        if (role == null) return;
        if (string.IsNullOrEmpty(mActionCfg.groupId) || string.IsNullOrEmpty(mActionCfg.ai)) return;
        if (role.GetFlag(mActionCfg.groupId) > 0)
            role.AIPart.Play(mActionCfg.ai);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        LevelMgr.instance.OnRoleBornCallback -= OnCreateRole;
    }
}