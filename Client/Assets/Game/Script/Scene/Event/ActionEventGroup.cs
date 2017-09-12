using UnityEngine;
using System.Collections;

public class ActionEventGroup : SceneAction
{

    public ActionCfg_EventGroup mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_EventGroup;

        //预加载
        SkillEventGroupCfg.PreLoad(mActionCfg.eventGroupId);
    }

    public override void OnAction()
    {
        Role globalEnemy = RoleMgr.instance.GlobalEnemy;

        if (globalEnemy == null)
        {
            Debuger.LogError("全局敌人不存在关卡不能使用事件组");
            return;
        }
        CombatMgr.instance.PlayEventGroup(globalEnemy, mActionCfg.eventGroupId, globalEnemy.transform.position);
    }
}
