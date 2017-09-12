using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionUseSkill : SceneAction
{
    public ActionCfg_UseSkill mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_UseSkill;
    }

    public override void OnAction()
    {
        if (string.IsNullOrEmpty(mActionCfg.flagIds))
        {
            Role r = RoleMgr.instance.Hero;
            if (r == null || !(r.State == Role.enState.alive)) return;
            if (string.IsNullOrEmpty(mActionCfg.skillID))
                r.CombatPart.Stop();
            else
                r.CombatPart.Play(mActionCfg.skillID);
        }
        else
        {
            List<Role> rls = LevelMgr.instance.CurLevel.GetRoleByFlag(mActionCfg.flagIds);
            if (rls.Count <= 0)
                return;
            Role r = rls[0];
            if (r == null || !(r.State == Role.enState.alive)) return;
            if (string.IsNullOrEmpty(mActionCfg.skillID))
            {
                r.CombatPart.Stop();
            }
            else
            {
                r.CombatPart.Play(mActionCfg.skillID);
            }
        }
        
        if (SceneMgr.SceneDebug)
            Debug.Log(string.Format("使用技能 {0}", mActionCfg.skillID));
    }
}
