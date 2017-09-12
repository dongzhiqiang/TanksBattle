using UnityEngine;
using System.Collections;
using LitJson;

public class ActionFactory
{
    public static SceneAction GetAction(SceneCfg.CheckSaveCfg saveCfg)
    {
        SceneAction action = null;
        SceneCfg.ActionType actType = (SceneCfg.ActionType)saveCfg.eveType;
        switch (actType)
        {
            case SceneCfg.ActionType.Win:
                action = new ActionWin(); break;
            case SceneCfg.ActionType.Lose:
                action = new ActionLose(); break;
            case SceneCfg.ActionType.ActivaRefresh:
                action = new ActionActivaRefresh(); break;
            case SceneCfg.ActionType.Skill:
                action = new ActionUseSkill(); break;
            case SceneCfg.ActionType.RemoveNpcId:
                action = new ActionRemoveNpc(); break;
            case SceneCfg.ActionType.CreateHero:
                action = new ActionCreateHero(); break;
            case SceneCfg.ActionType.ActivateDangban:
                action = new ActionActivateDangban(); break;
            case SceneCfg.ActionType.HideDangban:
                action = new ActionHideDangban(); break;
            case SceneCfg.ActionType.HideDir:
                action = new ActionHideDir(); break;
            case SceneCfg.ActionType.ShowDir:
                action = new ActionShowDir(); break;
            case SceneCfg.ActionType.Story:
                action = new ActionStory(); break;
            case SceneCfg.ActionType.Buff:
                action = new ActionBuff(); break;
            case SceneCfg.ActionType.ShowWave:
                action = new ActionShowWave(); break;
            case SceneCfg.ActionType.AddWave:
                action = new ActionAddWave(); break;
            case SceneCfg.ActionType.HideWave:
                action = new ActionHideWave(); break;
            case SceneCfg.ActionType.ChangeScene:
                action = new ActionChangeScene(); break;
            case SceneCfg.ActionType.ShowTarget:
                action = new ActionShowTarget(); break;
            case SceneCfg.ActionType.Camera:
                action = new ActionCamera(); break;
            case SceneCfg.ActionType.ShowAllRole:
                action = new ActionShowAllRole(); break;
            case SceneCfg.ActionType.HideAllRole:
                action = new ActionHideAllRole(); break;
            case SceneCfg.ActionType.StartTeach:
                action = new ActionStartTeach(); break;
            case SceneCfg.ActionType.NextTeach:
                action = new ActionNextTeach(); break;
            case SceneCfg.ActionType.KillAllMonster:
                action = new ActionKillAllMonster(); break;
            case SceneCfg.ActionType.PauseRefresh:
                action = new ActionPauseRefresh(); break;
            case SceneCfg.ActionType.RestartRefresh:
                action = new ActionRestartRefresh(); break;
            case SceneCfg.ActionType.EnterFightCamera:
                action = new ActionEnterFightCamera(); break;
            case SceneCfg.ActionType.LeaveFightCamera:
                action = new ActionLeaveFightCamera(); break;
            case SceneCfg.ActionType.ShowIdea:
                action = new ActionShowIdea(); break;
            case SceneCfg.ActionType.HideIdea:
                action = new ActionHideIdea(); break;
            case SceneCfg.ActionType.ShowTips:
                action = new ActionShowTips(); break;
            case SceneCfg.ActionType.HideTips:
                action = new ActionHideTips(); break;
            case SceneCfg.ActionType.GlobalFly:
                action = new ActionGlobalFly(); break;
            case SceneCfg.ActionType.BuffRemove:
                action = new ActionBuffRemove(); break;
            case SceneCfg.ActionType.ChangeAI:
                action = new ActionChangeAI(); break;
            case SceneCfg.ActionType.TimeScale:
                action = new ActionTimeScale(); break;
            case SceneCfg.ActionType.EventGroup:
                action = new ActionEventGroup(); break;
            case SceneCfg.ActionType.Msg:
                action = new ActionMsg(); break;
            case SceneCfg.ActionType.ActivateArea:
                action = new ActionActivateArea(); break;
            case SceneCfg.ActionType.KillMonster:
                action = new ActionKillMonster(); break;
            case SceneCfg.ActionType.FireEvent:
                action = new ActionFireEvent(); break;
            case SceneCfg.ActionType.BGMusic:
                action = new ActionBgMusic(); break;
            case SceneCfg.ActionType.RemoveEvent:
                action = new ActionRemoveEvent(); break;
            case SceneCfg.ActionType.None:
                action = new ActionNone(); break;
            case SceneCfg.ActionType.RandomEvent:
                action = new ActionRandomEvent(); break;
        }

        action.Init(ActionCfgFactory.instance.GetActionCfg(actType, saveCfg.content));
        return action;
    }

}
