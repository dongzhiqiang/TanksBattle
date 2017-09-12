using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LitJson;

public class ActionCfg : CheckCfgBase
{
    public SceneCfg.ActionType mType { get; set; }

    public int _idx;    //触发顺序
    public float _delay;  //延时时间
    public ActionCfg()
    {

    }
}

public class ActionCfg_UseSkill : ActionCfg
{

    public string skillID;             //需要用到的字段变量 要与字段名字相对应
    public string flagIds = "";
    public ActionCfg_UseSkill()
    {
        mType = SceneCfg.ActionType.Skill;

#if UNITY_EDITOR
        TypeDesc = "使用技能";      //行为在编辑器里的描述
        ParamDesc = new string[] { "技能ID", "刷新点id(空表主角)" };       //字段的描述 要与定义的字段相对应
#endif

    }
}

public class ActionCfg_RemoveNpc : ActionCfg
{
    public string npcFlag;

    public ActionCfg_RemoveNpc()
    {
        mType = SceneCfg.ActionType.RemoveNpcId;

#if UNITY_EDITOR
        TypeDesc = "杀死NPC";
        ParamDesc = new string[] { "npcFlag" };
#endif
    }
}

public class ActionCfg_ActivateRefresh : ActionCfg
{

    public string refreshID;

    public ActionCfg_ActivateRefresh()
    {
        mType = SceneCfg.ActionType.ActivaRefresh;

#if UNITY_EDITOR
        TypeDesc = "激活刷新点";
        ParamDesc = new string[] { RefreshFlagDesc };
#endif

    }
}

public class ActionCfg_Win : ActionCfg
{
    public ActionCfg_Win()
    {
        mType = SceneCfg.ActionType.Win;
#if UNITY_EDITOR
        TypeDesc = "胜利";
#endif
    }
}

public class ActionCfg_Lose : ActionCfg
{
    public ActionCfg_Lose()
    {
        mType = SceneCfg.ActionType.Lose;
#if UNITY_EDITOR
        TypeDesc = "失败";
#endif
    }
}

public class ActionCfg_CreateHero : ActionCfg
{
    public string changeCam;
    public ActionCfg_CreateHero()
    {
        mType = SceneCfg.ActionType.CreateHero;
#if UNITY_EDITOR
        TypeDesc = "创建主角";
        ParamDesc = new string[] { "切换镜头" };
#endif
    }
}


public class ActionCfg_ActivateDangban : ActionCfg
{
    public string flagId = "";
    public ActionCfg_ActivateDangban()
    {
        mType = SceneCfg.ActionType.ActivateDangban;

#if UNITY_EDITOR
        TypeDesc = "激活挡板";
        ParamDesc = new string[] { DangbanDesc };
#endif

    }
}

public class ActionCfg_HideDangban : ActionCfg
{
    public string flagId = "";
    public ActionCfg_HideDangban()
    {
        mType = SceneCfg.ActionType.HideDangban;

#if UNITY_EDITOR
        TypeDesc = "隐藏挡板";
        ParamDesc = new string[] { DangbanDesc };
#endif

    }
}

public class ActionCfg_HideDir : ActionCfg
{
    public ActionCfg_HideDir()
    {
        mType = SceneCfg.ActionType.HideDir;

#if UNITY_EDITOR
        TypeDesc = "隐藏指向";
#endif

    }
}

public class ActionCfg_ShowDir : ActionCfg
{
    public Vector3 findPos = Vector3.zero;
    public ActionCfg_ShowDir()
    {
        mType = SceneCfg.ActionType.ShowDir;

#if UNITY_EDITOR
        TypeDesc = "激活指向";
        ParamDesc = new string[] { FindPosition };
#endif

    }
}

public class ActionCfg_Story : ActionCfg
{
    public string storyId = "";
    public ActionCfg_Story()
    {
        mType = SceneCfg.ActionType.Story;

#if UNITY_EDITOR
        TypeDesc = "触发剧情";
        ParamDesc = new string[] { StoryDesc };
#endif

    }
}

public class ActionCfg_Buff : ActionCfg
{
    public bool bHaveHero = false;
    public string FlagList = "";
    public int buffId = 0;
    public ActionCfg_Buff()
    {
        mType = SceneCfg.ActionType.Buff;

#if UNITY_EDITOR
        TypeDesc = "添加状态";
        ParamDesc = new string[] { "是否包括主角", "FlagId列表,分割", "BuffID" };
#endif

    }
}

public class ActionCfg_ShowWave : ActionCfg
{
    public int maxWave;
    public string waveDesc = "怪物波数：";
    public ActionCfg_ShowWave()
    {
        mType = SceneCfg.ActionType.ShowWave;

#if UNITY_EDITOR
        TypeDesc = "显示波数";
        ParamDesc = new string[] { "最大波数", "波次描述" };
#endif

    }
}

public class ActionCfg_AddWave : ActionCfg
{
    public ActionCfg_AddWave()
    {
        mType = SceneCfg.ActionType.AddWave;

#if UNITY_EDITOR
        TypeDesc = "增加波数";
#endif

    }
}

public class ActionCfg_HideWave : ActionCfg
{
    public ActionCfg_HideWave()
    {
        mType = SceneCfg.ActionType.HideWave;

#if UNITY_EDITOR
        TypeDesc = "隐藏波数";
#endif

    }
}

public class ActionCfg_ChangeScene : ActionCfg
{
    public string sceneName;
    public ActionCfg_ChangeScene()
    {
        mType = SceneCfg.ActionType.ChangeScene;

#if UNITY_EDITOR
        TypeDesc = "切换场景";
        ParamDesc = new string[] { SceneFileDesc };
#endif

    }
}

public class ActionCfg_ShowTarget : ActionCfg
{
    public ActionCfg_ShowTarget()
    {
        mType = SceneCfg.ActionType.ShowTarget;

#if UNITY_EDITOR
        TypeDesc = "显示目标";
#endif

    }
}

public class ActionCfg_Camera : ActionCfg
{
    public string cameraFlag;       //镜头标记

    public ActionCfg_Camera()
    {
        mType = SceneCfg.ActionType.Camera;

#if UNITY_EDITOR
        TypeDesc = "镜头转向";
        ParamDesc = new string[] { CameraFlagDesc };
#endif

    }
}

public class ActionCfg_HideAllRole : ActionCfg
{
    public ActionCfg_HideAllRole()
    {
        mType = SceneCfg.ActionType.HideAllRole;

#if UNITY_EDITOR
        TypeDesc = "隐藏怪物";
#endif

    }
}

public class ActionCfg_ShowAllRole : ActionCfg
{
    public ActionCfg_ShowAllRole()
    {
        mType = SceneCfg.ActionType.ShowAllRole;

#if UNITY_EDITOR
        TypeDesc = "显示怪物";
#endif

    }
}

public class ActionCfg_StartTeach : ActionCfg
{
    public string teachId = "";
    public ActionCfg_StartTeach()
    {
        mType = SceneCfg.ActionType.StartTeach;

#if UNITY_EDITOR
        TypeDesc = "开始引导";
        ParamDesc = new string[] { "引导ID" };
#endif

    }
}

public class ActionCfg_NextTeach : ActionCfg
{
    public string eventType = "level";
    public string eventParam = "";
    public ActionCfg_NextTeach()
    {
        mType = SceneCfg.ActionType.NextTeach;

#if UNITY_EDITOR
        TypeDesc = "推进引导";
        ParamDesc = new string[] { "事件类型", "事件参数" };
#endif

    }
}

public class ActionCfg_KillAllMonster : ActionCfg
{
    public ActionCfg_KillAllMonster()
    {
        mType = SceneCfg.ActionType.KillAllMonster;

#if UNITY_EDITOR
        TypeDesc = "杀死所有怪";
#endif
    }
}
public class ActionCfg_PauseRefresh : ActionCfg
{
    public string refreshGroup = "";
    public ActionCfg_PauseRefresh()
    {
        mType = SceneCfg.ActionType.PauseRefresh;

#if UNITY_EDITOR
        TypeDesc = "暂停刷新组";
        ParamDesc = new string[] { "组id,分割空所有" };
#endif
    }
}

public class ActionCfg_RestartRefresh : ActionCfg
{
    public string refreshGroup = "";
    public ActionCfg_RestartRefresh()
    {
        mType = SceneCfg.ActionType.RestartRefresh;

#if UNITY_EDITOR
        TypeDesc = "重启刷新组";
        ParamDesc = new string[] { "组id,分割空所有" };
#endif
    }
}

public class ActionCfg_EnterFight : ActionCfg
{
    public string cameraName = "";
    public float disRate = 1;
    public float roundDis = 7;
    public ActionCfg_EnterFight()
    {
        mType = SceneCfg.ActionType.EnterFightCamera;

#if UNITY_EDITOR
        TypeDesc = "进入战斗状态";
        ParamDesc = new string[] { "镜头", "拉近比例", "范围距离" };
#endif
    }
}

public class ActionCfg_LeaveFight : ActionCfg
{
    public ActionCfg_LeaveFight()
    {
        mType = SceneCfg.ActionType.LeaveFightCamera;

#if UNITY_EDITOR
        TypeDesc = "离开战斗状态";
#endif
    }
}

public class ActionCfg_ShowIdea : ActionCfg
{
    public string desc = "";
    public ActionCfg_ShowIdea()
    {
        mType = SceneCfg.ActionType.ShowIdea;

#if UNITY_EDITOR
        TypeDesc = "显示独白";
        ParamDesc = new string[] { "内容" };
#endif
    }
}


public class ActionCfg_HideIdea : ActionCfg
{
    public ActionCfg_HideIdea()
    {
        mType = SceneCfg.ActionType.HideIdea;

#if UNITY_EDITOR
        TypeDesc = "隐藏独白";
#endif
    }
}


public class ActionCfg_ShowTips : ActionCfg
{
    public string desc = "";

    public ActionCfg_ShowTips()
    {
        mType = SceneCfg.ActionType.ShowTips;

#if UNITY_EDITOR
        TypeDesc = "显示提示";
        ParamDesc = new string[] { "内容" };
#endif
    }
}


public class ActionCfg_HideTips : ActionCfg
{
    public ActionCfg_HideTips()
    {
        mType = SceneCfg.ActionType.HideTips;

#if UNITY_EDITOR
        TypeDesc = "隐藏提示";
#endif
    }
}

public class ActionCfg_GlobalFly : ActionCfg
{
    public string flyer = "";
    public FxCreateCfg createCfg = new FxCreateCfg();


    public ActionCfg_GlobalFly()
    {
        mType = SceneCfg.ActionType.GlobalFly;

#if UNITY_EDITOR
        TypeDesc = "飞出物";
        ParamDesc = new string[] { "飞出物id" };
#endif

    }

#if UNITY_EDITOR
    public override void OnDraw()
    {
        createCfg.name = EditorGUILayout.TextField("特效", createCfg.name);
        if (GUILayout.Button("打开特效编辑器"))
        {
            Transform source = null;
            if (Application.isPlaying && RoleMgr.instance != null && RoleMgr.instance.GlobalEnemy != null)
                source = RoleMgr.instance.GlobalEnemy.transform;
            

            EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FX_EDITOR, "特效", createCfg, source);
        }
            
        if (GUILayout.Button("飞出物编辑器"))
        {
            FlyerCfg flyerCfg = string.IsNullOrEmpty(flyer) ? null : FlyerCfg.Get(flyer);
            Action<string> onSel = (string flyerId) => flyer = flyerId;
            EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FLYER_EDITOR, flyerCfg, onSel);
        }
    }
#endif
}
public class ActionCfg_BuffRemove : ActionCfg
{
    public bool bHaveHero = false;
    public string FlagList = "";
    public int buffId = 0;
    public ActionCfg_BuffRemove()
    {
        mType = SceneCfg.ActionType.BuffRemove;

#if UNITY_EDITOR
        TypeDesc = "移除状态";
        ParamDesc = new string[] { "是否包括主角", "FlagId列表,分割", "BuffID" };
#endif

    }
}

public class ActionCfg_ChangeAI : ActionCfg
{
    public string groupId = "";
    public string ai = "";
    public ActionCfg_ChangeAI()
    {
        mType = SceneCfg.ActionType.ChangeAI;

#if UNITY_EDITOR
        TypeDesc = "改变AI";
        ParamDesc = new string[] { "刷新组标记", "AI",};
#endif

    }
}
public class ActionCfg_TimeScale : ActionCfg
{
    public float ratio = 1;
    public float duration = 0;
    public ActionCfg_TimeScale()
    {
        mType = SceneCfg.ActionType.TimeScale;

#if UNITY_EDITOR
        TypeDesc = "时间缩放";
        ParamDesc = new string[] { "缩放比例", "持续时间", };
#endif

    }
}

public class ActionCfg_Msg : ActionCfg
{
    public string id = "";
    public string content = "";
    public ActionCfg_Msg()
    {
        mType = SceneCfg.ActionType.Msg;

#if UNITY_EDITOR
        TypeDesc = "发出消息";
        ParamDesc = new string[] { "消息ID", "内容", };
#endif

    }
}

public class ActionCfg_EventGroup : ActionCfg
{
    public string eventGroupId;

    public ActionCfg_EventGroup()
    {
        mType = SceneCfg.ActionType.EventGroup;

#if UNITY_EDITOR
        TypeDesc = "事件组";
        ParamDesc = new string[] { "事件组id" };
#endif

    }
#if UNITY_EDITOR
    public override void OnDraw()
    {
        
        if (GUILayout.Button("打开事件组编辑器"))
        {
            EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.EVENT_GROUP_EDITOR);
        }
    }
#endif
}

public class ActionCfg_ActivateArea : ActionCfg
{
    public string flagId = "";
    public ActionCfg_ActivateArea()
    {
        mType = SceneCfg.ActionType.ActivateArea;

#if UNITY_EDITOR
        TypeDesc = "激活区域";
        ParamDesc = new string[] { AreaFlagDesc };
#endif

    }
}

public class ActionCfg_KillMonster : ActionCfg
{
    public bool bHaveHero = false;
    public string flagIds = "";
    public bool bHeroKill = false;
    public ActionCfg_KillMonster()
    {
        mType = SceneCfg.ActionType.KillMonster;

#if UNITY_EDITOR
        TypeDesc = "杀死怪物";
        ParamDesc = new string[] { "是否包括主角", "FlagId列表,分割", "算主角杀死"};
#endif
    }
}

public class ActionCfg_FireEvent : ActionCfg
{
    public string eventId = "";
    public float delayTime = 0;
    public ActionCfg_FireEvent()
    {
        mType = SceneCfg.ActionType.FireEvent;

#if UNITY_EDITOR
        TypeDesc = "触发事件";
        ParamDesc = new string[] { EventFlagDesc, "间隔延时" };
#endif
    }
}

public class ActionCfg_BgMusic : ActionCfg
{
    public int bgmId = 0;
    public ActionCfg_BgMusic()
    {
        mType = SceneCfg.ActionType.BGMusic;

#if UNITY_EDITOR
        TypeDesc = "切换背景音";
        ParamDesc = new string[] { "id" };
#endif
    }
}


public class ActionCfg_RemoveEvent : ActionCfg
{
    public string eventId = "";
    public ActionCfg_RemoveEvent()
    {
        mType = SceneCfg.ActionType.RemoveEvent;

#if UNITY_EDITOR
        TypeDesc = "移除事件";
        ParamDesc = new string[] { EventFlagDesc };
#endif
    }
}


public class ActionCfg_None : ActionCfg
{
    public ActionCfg_None()
    {
        mType = SceneCfg.ActionType.None;

#if UNITY_EDITOR
        TypeDesc = "无行为";
#endif
    }
}

public class ActionCfg_RandomEvent : ActionCfg
{
    public string eventIds;
    public string rates;
    public ActionCfg_RandomEvent()
    {
        mType = SceneCfg.ActionType.RandomEvent;

#if UNITY_EDITOR
        TypeDesc = "随机激活";
        ParamDesc = new string[] { "事件id(,分割)", "概率(匹配id)" };
#endif

    }
}

public class ActionCfgFactory : Singleton<ActionCfgFactory>
{

    public static Dictionary<string, ActionCfg> cfgMap = new Dictionary<string, ActionCfg>();

    public ActionCfg GetActionCfg(SceneCfg.ActionType cfgType, string content = "")
    {
        ActionCfg actionCfg = null;
        switch (cfgType)
        {
            case SceneCfg.ActionType.Skill:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_UseSkill();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_UseSkill>(content);
                break;
            case SceneCfg.ActionType.RemoveNpcId:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_RemoveNpc();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_RemoveNpc>(content);
                break;
            case SceneCfg.ActionType.ActivaRefresh:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ActivateRefresh();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ActivateRefresh>(content);
                break;
            case SceneCfg.ActionType.Win:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Win();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Win>(content);
                break;
            case SceneCfg.ActionType.Lose:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Lose();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Lose>(content);
                break;
            case SceneCfg.ActionType.CreateHero:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_CreateHero();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_CreateHero>(content);
                break;
            case SceneCfg.ActionType.ActivateDangban:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ActivateDangban();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ActivateDangban>(content);
                break;
            case SceneCfg.ActionType.HideDangban:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideDangban();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideDangban>(content);
                break;
            case SceneCfg.ActionType.ShowDir:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowDir();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowDir>(content);
                break;
            case SceneCfg.ActionType.HideDir:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideDir();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideDir>(content);
                break;
            case SceneCfg.ActionType.Story:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Story();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Story>(content);
                break;
            case SceneCfg.ActionType.Buff:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Buff();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Buff>(content);
                break;
            case SceneCfg.ActionType.ShowWave:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowWave();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowWave>(content);
                break;
            case SceneCfg.ActionType.AddWave:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_AddWave();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_AddWave>(content);
                break;
            case SceneCfg.ActionType.HideWave:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideWave();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideWave>(content);
                break;
            case SceneCfg.ActionType.ChangeScene:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ChangeScene();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ChangeScene>(content);
                break;
            case SceneCfg.ActionType.ShowTarget:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowTarget();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowTarget>(content);
                break;
            case SceneCfg.ActionType.Camera:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Camera();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Camera>(content);
                break;
            case SceneCfg.ActionType.HideAllRole:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideAllRole();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideAllRole>(content);
                break;
            case SceneCfg.ActionType.ShowAllRole:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowAllRole();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowAllRole>(content);
                break;
            case SceneCfg.ActionType.StartTeach:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_StartTeach();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_StartTeach>(content);
                break;
            case SceneCfg.ActionType.NextTeach:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_NextTeach();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_NextTeach>(content);
                break;
            case SceneCfg.ActionType.KillAllMonster:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_KillAllMonster();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_KillAllMonster>(content);
                break;
            case SceneCfg.ActionType.PauseRefresh:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_PauseRefresh();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_PauseRefresh>(content);
                break;
            case SceneCfg.ActionType.RestartRefresh:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_RestartRefresh();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_RestartRefresh>(content);
                break;
            case SceneCfg.ActionType.EnterFightCamera:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_EnterFight();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_EnterFight>(content);
                break;
            case SceneCfg.ActionType.LeaveFightCamera:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_LeaveFight();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_LeaveFight>(content);
                break;
            case SceneCfg.ActionType.ShowIdea:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowIdea();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowIdea>(content);
                break;
            case SceneCfg.ActionType.HideIdea:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideIdea();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideIdea>(content);
                break;
            case SceneCfg.ActionType.ShowTips:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ShowTips();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ShowTips>(content);
                break;
            case SceneCfg.ActionType.HideTips:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_HideTips();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_HideTips>(content);
                break;
            case SceneCfg.ActionType.GlobalFly:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_GlobalFly();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_GlobalFly>(content);
                break;
            case SceneCfg.ActionType.BuffRemove:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_BuffRemove();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_BuffRemove>(content);
                break;
            case SceneCfg.ActionType.ChangeAI:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ChangeAI();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ChangeAI>(content);
                break;
            case SceneCfg.ActionType.TimeScale:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_TimeScale();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_TimeScale>(content);
                break;
            case SceneCfg.ActionType.EventGroup:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_EventGroup();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_EventGroup>(content);
                break;
            case SceneCfg.ActionType.Msg:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_Msg();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_Msg>(content);
                break;
            case SceneCfg.ActionType.ActivateArea:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_ActivateArea();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_ActivateArea>(content);
                break;
            case SceneCfg.ActionType.KillMonster:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_KillMonster();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_KillMonster>(content);
                break;
            case SceneCfg.ActionType.FireEvent:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_FireEvent();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_FireEvent>(content);
                break;
            case SceneCfg.ActionType.BGMusic:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_BgMusic();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_BgMusic>(content);
                break;
            case SceneCfg.ActionType.RemoveEvent:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_RemoveEvent();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_RemoveEvent>(content);
                break;
            case SceneCfg.ActionType.None:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_None();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_None>(content);
                break;
            case SceneCfg.ActionType.RandomEvent:
                if (string.IsNullOrEmpty(content))
                    actionCfg = new ActionCfg_RandomEvent();
                else
                    actionCfg = JsonMapper.ToObject<ActionCfg_RandomEvent>(content);
                break;
        }
        return actionCfg;
    }

    public Dictionary<string, ActionCfg> GetAllActionCfg()
    {
        if (cfgMap.Count > 0)
            return cfgMap;

        Type actionType = typeof(SceneCfg.ActionType);
        foreach (int cfgType in Enum.GetValues(actionType))
        {
            ActionCfg cfg = GetActionCfg((SceneCfg.ActionType)cfgType);
            if (cfg != null)
            {
                cfgMap[cfg.GetTypeDesc()] = cfg;
            }
            else
            {
                //Debug.LogError(string.Format("有定义的行为类型 {0}，没有创建成功", cfgType));
            }
        }

        return cfgMap;
    }

}
