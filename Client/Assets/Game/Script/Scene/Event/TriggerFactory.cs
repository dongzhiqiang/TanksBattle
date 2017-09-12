using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class TriggerFactory
{
    public static SceneTrigger GetTrigger(SceneCfg.CheckSaveCfg saveCfg)
    {
        SceneTrigger trigger = null;
        SceneCfg.EventType eveType = (SceneCfg.EventType)saveCfg.eveType;
        switch (eveType)
        {
            case SceneCfg.EventType.None:
                trigger = new TriggerNone(); break;
            case SceneCfg.EventType.StartLevel:
                trigger = new TriggerStartLevel(); break;
            case SceneCfg.EventType.EnterTime:
                trigger = new TriggerEnterTime(); break;
            case SceneCfg.EventType.NpcIDDead:
                trigger = new TriggerNpcDead(); break;
            case SceneCfg.EventType.Area:
                trigger = new TriggerArea(); break;
            case SceneCfg.EventType.Win:
                trigger = new TriggerWin(); break;
            case SceneCfg.EventType.Lose:
                trigger = new TriggerLose(); break;
            case SceneCfg.EventType.RoleDead:
                trigger = new TriggerRoleDead(); break;
            case SceneCfg.EventType.RoleEnter:
                trigger = new TriggerRoleEnter(); break;
            case SceneCfg.EventType.RefreshDead:
                trigger = new TriggerRefreshDead(); break;
            case SceneCfg.EventType.RoleBlood:
                trigger = new TriggerRoleBlood(); break;
            case SceneCfg.EventType.RoleNum:
                trigger = new TriggerRoleNum(); break;
            case SceneCfg.EventType.GroupDeadNum:
                trigger = new TriggerGroupDeadNum(); break;
            case SceneCfg.EventType.FinishEvent:
                trigger = new TriggerFinishEvent(); break;
        }
        if (trigger != null)
            trigger.Init(EventCfgFactory.instance.GetEventCfg(eveType, saveCfg.content));

        return trigger;
    }

    public static SceneTrigger GetCondition(RoomConditionCfg cfg)
    {
        SceneCfg.EventType conditionType = (SceneCfg.EventType)Enum.Parse(typeof(SceneCfg.EventType), cfg.type, true);
        SceneTrigger trigger = null;

        switch (conditionType)
        {
            case SceneCfg.EventType.FINISH_LEVEL:
                trigger = new TriggerCondFinishLevel(); break;
            case SceneCfg.EventType.BLOOD_LIMIT:
                trigger = new TriggerCondBloodLimit(); break;
            case SceneCfg.EventType.TIME_LIMIT:
                trigger = new TriggerCondTimeLimit(); break;
            case SceneCfg.EventType.OPEN_BOX:
                trigger = new TriggerCondOpenBox(); break;  //宝箱也是怪 用KillMonster条件代替 这个不用了
            case SceneCfg.EventType.ROLE_DEAD_ALIVE:
                trigger = new TriggerCondRoleDeadAlive(); break;    //角色一直存活或死亡
            case SceneCfg.EventType.REACH_AREA:
                trigger = new TriggerCondReachArea(); break;
            case SceneCfg.EventType.KILL_TIMES:
                trigger = new TriggerCondKillTimes(); break;
            case SceneCfg.EventType.KILL_MONSTER:
                trigger = new TriggerCondKillMonster(); break;
            case SceneCfg.EventType.WEAPON_LIMIT:
                trigger = new TriggerCondWeaponLimit(); break;
            case SceneCfg.EventType.SKILL_USE:
                trigger = new TriggerCondSkillUse(); break;
            case SceneCfg.EventType.PET_USE:
                trigger = new TriggerCondPetUse(); break;
            case SceneCfg.EventType.STATE_TIMES:
                trigger = new TriggerCondStateTimes(); break;
            case SceneCfg.EventType.BREAK_ITEM:
                trigger = new TriggerCondBreakItem(); break;
            case SceneCfg.EventType.HURT_NUM:
                trigger = new TriggerCondHurtNum(); break;
            case SceneCfg.EventType.CONTINUE_HIT:
                trigger = new TriggerCondContinueHit(); break;
            case SceneCfg.EventType.TRAP_NUM:
                trigger = new TriggerCondTrap(); break;
            case SceneCfg.EventType.PET_ALIVE:
                trigger = new TriggerCondPetAlive(); break;
            case SceneCfg.EventType.KILL_ALL:
                trigger = new TriggerCondKillAll(); break;
            case SceneCfg.EventType.PET_USE2:
                trigger = new TriggerCondPetUse2(); break;
        }
        if (trigger != null)
            trigger.Init(cfg);
        return trigger;

    }

}
