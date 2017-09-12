using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionKillAllMonster : SceneAction
{
    public ActionCfg_KillAllMonster mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_KillAllMonster;
    }

    public override void OnAction()
    {
        List<Role> pets = new List<Role>();
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.State == Role.enState.alive)
            pets = hero.PetsPart.GetMainPets();

        List<Role> rs = new List<Role>(RoleMgr.instance.Roles);
        foreach (Role r in rs)
        {
            if (r.Cfg.roleType == enRoleType.box || r.Cfg.roleType == enRoleType.trap || r.GetCamp()== enCamp.neutral)
                continue;

            if (r != hero && r.State == Role.enState.alive )
            {
                if (!pets.Contains(r))
                    r.DeadPart.Handle(false);
            }

        }
    }
}
