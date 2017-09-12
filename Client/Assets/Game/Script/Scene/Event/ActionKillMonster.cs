using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionKillMonster : SceneAction
{
    public ActionCfg_KillMonster mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_KillMonster;
    }

    public override void OnAction()
    {
        if (mActionCfg.bHaveHero)   //包含主角 添加状态
        {
            Role hero = RoleMgr.instance.Hero;
            if (hero != null && hero.BuffPart != null)
                hero.DeadPart.Handle(true, false, mActionCfg.bHeroKill);
        }

        //获取到标记的角色 添加buff 不包括主角
        if (!string.IsNullOrEmpty(mActionCfg.flagIds))
        {
            string[] flagArr = mActionCfg.flagIds.Split(',');
            List<Role> rs = new List<Role>(RoleMgr.instance.Roles);
            foreach (Role role in rs)
            {
                for (int i = 0; i < flagArr.Length; i++)
                {
                    if (role.GetFlag(flagArr[i]) > 0)
                    {
                        role.DeadPart.Handle(true, false, mActionCfg.bHeroKill);
                        LevelMgr.instance.CurLevel.RemoveRole(role);
                        break;
                    }
                }
            }
        }
    }
}
