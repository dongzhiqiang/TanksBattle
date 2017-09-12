using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionBuff : SceneAction
{
    public ActionCfg_Buff mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Buff;
    }

    public override void OnAction()
    {
        if (mActionCfg.buffId <= 0)
        {
            Debuger.LogError("buffId填写错误");
            return;
        }

        if (mActionCfg.bHaveHero)   //包含主角 添加状态
        {
            Role hero = RoleMgr.instance.Hero;
            if (hero != null && hero.BuffPart != null)
                hero.BuffPart.AddBuff(mActionCfg.buffId);
        }

        //获取到标记的角色 添加buff 不包括主角
        if (!string.IsNullOrEmpty(mActionCfg.FlagList))     
        {
            string str = mActionCfg.FlagList;
            string[] strArr = str.Split(',');
            List<Role> roleList = null;
            for(int i = 0; i < strArr.Length; i++)
            {
                roleList = LevelMgr.instance.CurLevel.GetRoleByFlag(strArr[i]);
                foreach(Role role in roleList)
                {
                    if (role != null && !role.IsHero && role.BuffPart != null)
                    {
                        role.BuffPart.AddBuff(mActionCfg.buffId);
                    }
                }
            }
        }
    }
}
