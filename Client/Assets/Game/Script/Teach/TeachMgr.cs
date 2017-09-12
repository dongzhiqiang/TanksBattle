using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Text;
using System.Reflection;

public class TeachMgr : SingletonMonoBehaviour<TeachMgr>
{
    private class HeroLevelTriggerInfo
    {
        public HeroLevelTriggerInfo(string teachName, int minLv, int maxLv)
        {
            this.teachName = teachName;
            this.minLv = minLv;
            this.maxLv = maxLv;
        }

        public string teachName;
        public int minLv;
        public int maxLv;
    }

    private class TeachTaskInfo
    {
        public TeachTaskInfo(string teachName, int priority)
        {
            this.teachName = teachName;
            this.priority = priority;
        }

        public string teachName;
        public int priority;
    }

    #region Constant
    public const string TEACH_ACTION_FUNC = "OnTeachAction";
    public const string TEACH_CHECK_FUNC = "OnTeachCheck";
    public const int TEACH_PLAYED_FLAG = 1;
    public const int TEACH_UNFINISHED_FLAG = -1;

    private const string FX_NAME_END_POINT = "fx_zhiyin_02";
    private const string FX_NAME_PATH_POINT = "fx_zhiyin_01";    
    private const float PATH_ARROW_YOFFSET = 0.1f;
    private const float PATH_ARROW_GAP = 1.5f;    
    #endregion

    #region Fields
    private Action<int, object> m_editorCallback;
    private GameObjectPool m_gameObjPool = null;    
    private UITeach m_uiTeach = null;

    private GameObject m_endPointGameObj = null;
    private List<GameObject> m_pathPointGameObj = new List<GameObject>();

    private bool m_recordNow = false;
    private bool m_recordStateHandleState = true;
    private bool m_playNow = false;
    private TimeMgr.Timer m_timerPlayInNextFrame = null;
    private TimeMgr.Timer m_timerCancelIfPanelNotTop = null;
    private int m_curPlayIndex = -1;
    private int m_curRecordIndex = -1;
    private TeachConfig m_curTeachCfg = null;
    private TeachPauseMode m_lastPauseMode = TeachPauseMode.none;
    private TimeMgr.TimeScaleHandle m_timeScaleHandle = null;
    private bool m_lastMuteMusic = false; //最近是否做了背景音乐静音操作
    private bool m_musicOrigMute = false; //最近背景音乐静音操作时，是不是原本就是静音的
    private DateTime m_stepStartTime = DateTime.Now;
    private RectTransform m_rectFromPanelFunc = null;
    private RectTransform m_rectFromPanelFunc2 = null;
    private RectTransform m_rectFromPanelFuncForCheck = null;
    private RectTransform m_rectFromPanelFuncForCheck2 = null;
    private int m_eventIdSceneStart = EventMgr.Invalid_Id;
    private int m_eventIdSceneExit = EventMgr.Invalid_Id;
    private int m_eventIdPanelOpen = EventMgr.Invalid_Id;
    private int m_eventIdPanelClose = EventMgr.Invalid_Id;
    private int m_eventIdHeroLevel = EventMgr.Invalid_Id;
    private int m_eventIdMainCityUITop = EventMgr.Invalid_Id;
    private int m_eventIdForCurStep = EventMgr.Invalid_Id;
    private List<int> m_eventIdForCurTeach = new List<int>();
    private bool m_sortAndStartPlay = false;
    private Dictionary<string, long> m_lastPlayTimeMap = new Dictionary<string, long>();   //最近播放的时间，用于带CoolDown的引导
    private List<GameObject> m_gameObjectsToDestroy = new List<GameObject>(); //要在Update里销毁的游戏物品列表

    private List<TeachTaskInfo> m_teachQueue = new List<TeachTaskInfo>();
    private Dictionary<string, List<string>> m_triBackMainCity = new Dictionary<string, List<string>>(); //key是pre roomid
    private List<string> m_triMainCityFromAny = new List<string>();
    private List<HeroLevelTriggerInfo> m_triHeroLevel = new List<HeroLevelTriggerInfo>();
    private Dictionary<string, List<string>> m_triDirectEvent = new Dictionary<string, List<string>>(); //key是eventType,eventParam
    private Dictionary<string, List<string>> m_triOpenPanel = new Dictionary<string, List<string>>(); //key是UIPanel的Path
    private Dictionary<string, List<string>> m_triEnterScene = new Dictionary<string, List<string>>(); //key是roomid    
    private List<string> m_triMainCityUITop = new List<string>();
    private Dictionary<string, Dictionary<string, int>> m_triNormalEvent = new Dictionary<string, Dictionary<string, int>>(); //key是事件类型，事件ID
    private Dictionary<string, List<TeachAgent>> m_teachAgents = new Dictionary<string, List<TeachAgent>>();    //key是引导名，TODO 销毁模块时，要销毁它
    #endregion

    #region Properties
    public bool RecordNow { get { return m_recordNow; } }
    public bool RecordStateHandleState { get { return m_recordStateHandleState; } set { m_recordStateHandleState = value; } }
    public bool PlayNow { get { return m_playNow; } }
    public TeachConfig CurTeachConfig { get { return m_curTeachCfg; } }
    public int CurPlayIndex { get { return m_curPlayIndex; } }
    public int CurRecordIndex { get { return m_curRecordIndex; } }
    public Action<int, object> EditorCallback { get { return m_editorCallback; } set { m_editorCallback = value; } }
    #endregion

    public void Init()
    {
        m_gameObjPool = GameObjectPool.GetPool(GameObjectPool.enPool.Fx);
        m_uiTeach = UIMgr.instance.Get<UITeach>();
        m_uiTeach.AddFullScreenClick(OnStateHandleFullScreenClick);

        //一开始就要监听事件，因为有些可以充当触发条件
        AddFixedNormalEventListener();

        //收集触发器
        RegisterTriggers();
        //主角创建后再收集触发器
        EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.HERO_CREATED, RegisterTriggers);
        
    }

    private void Update()
    {
        CheckDestroyGameObjects();

        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            CheckTimeout(stepCfg);
            CheckStepObj(stepCfg);
        }
    }

    #region 场景路径指示相关
    private void ShowPathInScene(Vector3 destPos)
    {
        var hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        hero.RoleModel.RolePath.GetPath(destPos, OnGetPathNodes);
    }

    private void CheckDestroyGameObjects()
    {
        var cnt = m_gameObjectsToDestroy.Count;
        if (cnt <= 0)
            return;

        for (var i = 0; i < cnt; ++i)
            GameObject.DestroyImmediate(m_gameObjectsToDestroy[i]);

        m_gameObjectsToDestroy.Clear();
    }

    private void ClearPathIndicators()
    {
        if (m_endPointGameObj != null)
        {
            //GameObject.DestroyImmediate(m_endPointGameObj);
            m_gameObjectsToDestroy.Add(m_endPointGameObj);
            m_endPointGameObj = null;
        }

        if (m_pathPointGameObj.Count > 0)
        {
            for (var i = 0; i < m_pathPointGameObj.Count; ++i)
            {
                //GameObject.DestroyImmediate(m_pathPointGameObj[i]);
                m_gameObjectsToDestroy.Add(m_pathPointGameObj[i]);
            }                
            m_pathPointGameObj.Clear();
        }        
    }

    private void OnLoadEndPoint(GameObject obj, object cxt)
    {
        if (!m_playNow)
        {
            GameObject.DestroyImmediate(obj);
            return;
        }

        m_endPointGameObj = obj;
        List<Vector3> pathNodes = (List<Vector3>)cxt;
        var endPos = pathNodes[pathNodes.Count - 1];
        endPos.y += PATH_ARROW_YOFFSET;
        m_endPointGameObj.transform.position = endPos;
    }

    private void OnLoadPathPoint(GameObject obj, object cxt)
    {
        if (!m_playNow)
        {
            GameObject.DestroyImmediate(obj);
            return;
        }

        List<Vector3> pathNodes = (List<Vector3>)cxt;
        for (var i = 0; i < pathNodes.Count - 1; ++i)
        {
            var curNode = pathNodes[i];
            var nextNode = pathNodes[i + 1];
            var moveDir = nextNode - curNode;
            var moveDirNorm = moveDir.normalized;
            var dist = moveDir.magnitude;
            var arrowCnt = Mathf.Max(1, (int)(dist / PATH_ARROW_GAP));
            var curArrowNode = curNode;
            for (var j = 0; j < arrowCnt; ++j)
            {
                var nextArrowNode = curArrowNode + moveDirNorm * PATH_ARROW_GAP;
                var pathPtObj = GameObject.Instantiate(obj);
                pathPtObj.transform.position = new Vector3(curArrowNode.x, curArrowNode.y + PATH_ARROW_YOFFSET, curArrowNode.z);
                pathPtObj.transform.forward = nextArrowNode - curArrowNode;
                m_pathPointGameObj.Add(pathPtObj);
                curArrowNode = nextArrowNode;
            }
        }

        //这个模板可以丢了
        GameObject.DestroyImmediate(obj);
    }

    private void OnGetPathNodes(List<Vector3> pathNodes)
    {
        ClearPathIndicators();

        m_gameObjPool.Get(FX_NAME_END_POINT, pathNodes, OnLoadEndPoint, false);
        m_gameObjPool.Get(FX_NAME_PATH_POINT, pathNodes, OnLoadPathPoint, false);
    }
    #endregion

    #region 播放相关
    private bool CheckPlayCondition(TeachConfig cfg)
    {
        var hero = RoleMgr.instance.Hero;
        if (hero != null && !string.IsNullOrEmpty(cfg.dataKey))
        {
            var part = hero.SystemsPart;
            if (part != null && part.GetTeachVal(cfg.dataKey) == TEACH_PLAYED_FLAG)
            {
                Debuger.Log("引导已被播放过了：" + cfg.teachName);
                return false;
            }
        }

        if (cfg.coolDown > 0)
        {
            long lastPlayTime = 0;
            if (m_lastPlayTimeMap.TryGetValue(cfg.teachName, out lastPlayTime) && TimeMgr.instance.GetTimestamp() - lastPlayTime < cfg.coolDown)
            {
                Debuger.Log("引导还在冷却中：" + cfg.teachName);
                return false;
            }
        }

        var checkConds = cfg.checkConds;
        for (int i = 0, cnt = checkConds.Count; i < cnt; ++i)
        {
            var condData = checkConds[i];
            switch (condData.checkType)
            {
                case TeachCheckCond.inScene:
                    {
                        if (Room.instance == null || Room.instance.roomCfg.id != condData.checkParam)
                            return false;
                    }
                    break;
                case TeachCheckCond.inMainCity:
                    {
                        if (!LevelMgr.instance.IsMainCity())
                            return false;
                    }
                    break;
                case TeachCheckCond.inLevelScene:
                    {
                        //不在场景里，或者在主城场景，都不算在副本场景
                        if (Room.instance == null || LevelMgr.instance.IsMainCity())
                            return false;
                    }
                    break;
                case TeachCheckCond.heroLevel:
                    {
                        if (hero == null)
                            return false;
                        var lv = hero.GetInt(enProp.level);
                        var num1 = StringUtil.ToInt(condData.checkParam);
                        var num2 = StringUtil.ToInt(condData.checkParam2);
                        if (lv < num1 || lv > num2)
                            return false;
                    }
                    break;
                case TeachCheckCond.panelShow:
                    {
                        var obj = UITeach.FindUIPanel(condData.checkParam);
                        if (obj == null || !obj.IsTop)
                            return false;
                    }
                    break;
                case TeachCheckCond.levelId:
                    {
                        if (hero == null)
                            return false;
                        var part = hero.LevelsPart;
                        var lvId = StringUtil.ToInt(part.CurLevelId);
                        var info = part.GetLevelInfoById(part.CurLevelId);
                        //如果本关没有打通，就减一
                        if (info != null && !info.isWin)
                            lvId = Math.Max(0, lvId - 1);
                        var num1 = StringUtil.ToInt(condData.checkParam);
                        var num2 = StringUtil.ToInt(condData.checkParam2);
                        if (lvId < num1 || lvId > num2)
                            return false;
                    }
                    break;
                case TeachCheckCond.teachData:
                    {
                        if (hero == null)
                            return false;
                        var part = hero.SystemsPart;
                        var tval = part.GetTeachVal(condData.checkParam);
                        var arr = string.IsNullOrEmpty(condData.checkParam2) ? new string[] { "", "" } : condData.checkParam2.Split(new char[] { ',' });
                        var opType = arr.Length >= 1 ? StringUtil.ToInt(arr[0]) : 0;
                        var value = arr.Length >= 2 ? StringUtil.ToInt(arr[1]) : 0;
                        switch (opType)
                        {
                            case 0: //等于
                                if (tval != value)
                                    return false;
                                break;
                            case 1: //不等于
                                if (tval == value)
                                    return false;
                                break;
                        }
                    }
                    break;
                case TeachCheckCond.taskState:
                    break;
                case TeachCheckCond.uiPanelFunc:
                    {
                        if (!CallPanlCheckFunc(condData.checkParam, condData.checkParam2))
                            return false;
                    }
                    break;
                case TeachCheckCond.systemIcon:
                    {
                        if (hero == null)
                            return false;
                        var sysIcon = (enSystem)StringUtil.ToInt(condData.checkParam);
                        var enabled = StringUtil.ToInt(condData.checkParam2) == 0 ? false : true;
                        var errMsg = "";
                        if (SystemMgr.instance.IsEnabled(sysIcon, out errMsg) != enabled)
                            return false;
                    }
                    break;
            }
        }

        return true;
    }

    private bool StartPlayTeach(string teachName)
    {
        if (m_recordNow)
        {
            Debuger.LogError("录制时请求播放引导：" + teachName);
            return false;
        }

        var cfg = TeachConfig.FindConfigByName(teachName);
        if (cfg == null)
        {
            Debuger.LogError("找不到引导配置：" + teachName);
            return false;
        }

        //先停止前面的引导（如果有的话）
        StartPlay(false, 0, false);

        //看看能不能播放
        if (!CheckPlayCondition(cfg))
        {
            PlayTeachFromQueue();
            return false;
        }

        //设置当前播放引导
        var index = TeachConfig.Configs.IndexOf(cfg);
        SetCurTeachConfig(cfg);
        NotifyChangeConfig(index);
        //为了防止太多层递归，这里放到下一帧执行
        m_timerPlayInNextFrame = TimeMgr.instance.AddTimer(0, () => {
            StartPlay(true);
        });

        return true;
    }

    private void DoCancelActions(TeachConfig cfg)
    {
        var cancelActions = cfg.cancelActions;
        for (int i = 0, cnt = cancelActions.Count; i < cnt; ++i)
        {
            var actionData = cancelActions[i];
            switch (actionData.actionType)
            {
                case TeachCancelActionType.openPanel:
                case TeachCancelActionType.showUINode:
                    {
                        var go = GameObject.Find(actionData.actionParam);
                        if (go == null)
                            Debuger.LogError("找不到UI节点对象：" + actionData.actionParam);
                        else
                            m_uiTeach.ShowUINode(go);
                    }
                    break;
                case TeachCancelActionType.closePanel:
                case TeachCancelActionType.hideUINode:
                    {
                        var go = GameObject.Find(actionData.actionParam);
                        if (go == null)
                            Debuger.LogError("找不到UI节点对象：" + actionData.actionParam);
                        else
                            m_uiTeach.HideUINode(go);
                    }
                    break;
                case TeachCancelActionType.stateHandleState:
                    {
                        var go = GameObject.Find(actionData.actionParam);
                        if (go == null)
                        {
                            Debuger.LogError("找不到UI节点对象：" + actionData.actionParam);
                        }
                        else
                        {
                            var stateHandle = go.GetComponent<StateHandle>();
                            if (stateHandle == null)
                                Debuger.LogError("找不到StateHandle组件：" + actionData.actionParam);
                            else
                                stateHandle.SetState(StringUtil.ToInt(actionData.actionParam2));
                        }
                    }
                    break;
                case TeachCancelActionType.uiPanelFunc:
                    {
                        CallPanelFunc(actionData.actionParam, actionData.actionParam2);
                    }
                    break;
            }
        }
    }

    private bool CanSkipThisStep(TeachStepConfig stepCfg)
    {
        switch (stepCfg.stepSkipType)
        {
            case TeachStepSkipType.stateHandleState:
                {
                    var obj = UITeach.FindStateHandle(stepCfg.skipParam);
                    var num = StringUtil.ToInt(stepCfg.skipParam2);
                    return obj != null && obj.CurStateIdx == num;
                }
            case TeachStepSkipType.stateGroupCurSel:
                {
                    var obj = UITeach.FindStateGroup(stepCfg.skipParam);
                    var num = StringUtil.ToInt(stepCfg.skipParam2);
                    return obj != null && (num >= 0 ? obj.CurIdx == num : obj.CurIdx == obj.Count + num);
                }
            case TeachStepSkipType.uiPanelOpenTop:
                {
                    var obj = UITeach.FindUIPanel(stepCfg.skipParam);
                    return obj != null && obj.IsTop;
                }
            case TeachStepSkipType.uiPanelOpen:
                {
                    var obj = UITeach.FindUIPanel(stepCfg.skipParam);
                    return obj != null && obj.IsOpenEx;
                }
            case TeachStepSkipType.showUINode:
                {
                    var obj = UITeach.FindRectTransform(stepCfg.skipParam);
                    return obj != null && obj.gameObject.activeInHierarchy;
                }
            case TeachStepSkipType.hideUINode:
                {
                    var obj = UITeach.FindRectTransform(stepCfg.skipParam);
                    return obj == null || !obj.gameObject.activeInHierarchy;
                }
            case TeachStepSkipType.teachData:
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        break;
                    var part = hero.SystemsPart;
                    var tval = part.GetTeachVal(stepCfg.skipParam);
                    var arr = string.IsNullOrEmpty(stepCfg.skipParam2) ? new string[] { "", "" } : stepCfg.skipParam2.Split(new char[] { ',' });
                    var opType = arr.Length >= 1 ? StringUtil.ToInt(arr[0]) : 0;
                    var value = arr.Length >= 2 ? StringUtil.ToInt(arr[1]) : 0;
                    switch (opType)
                    {
                        case 0: //等于
                            if (tval == value)
                                return true;
                            break;
                        case 1: //不等于
                            if (tval != value)
                                return true;
                            break;
                    }
                }
                break;
            case TeachStepSkipType.uiPanelFunc:
                {
                    if (CallPanlCheckFunc(stepCfg.skipParam, stepCfg.skipParam2))
                        return true;
                }
                break;
        }
        return false;
    }

    private void PlayStep(TeachStepConfig stepCfg)
    {
        if (stepCfg.uiOpType != TeachUIOpType.none)
        {
            m_rectFromPanelFuncForCheck = m_rectFromPanelFunc;
            m_rectFromPanelFuncForCheck2 = m_rectFromPanelFunc2;
            m_rectFromPanelFunc = null;
            m_rectFromPanelFunc2 = null;
        }        

        if (m_lastPauseMode != stepCfg.pauseMode)
        {
            switch (m_lastPauseMode)
            {
                case TeachPauseMode.logic:
                    TimeMgr.instance.SubPause();
                    break;
                case TeachPauseMode.full:
                    if (m_timeScaleHandle != null)
                    {
                        m_timeScaleHandle.Release();
                        m_timeScaleHandle = null;
                    }
                    break;
            }

            m_lastPauseMode = stepCfg.pauseMode;

            switch (m_lastPauseMode)
            {
                case TeachPauseMode.logic:
                    TimeMgr.instance.AddPause();
                    break;
                case TeachPauseMode.full:
                    m_timeScaleHandle = TimeMgr.instance.AddTimeScale(0, -1, 100);
                    break;
            }            
        }

        if (stepCfg.stopPreSnd)
        {
            SoundMgr.instance.Stop2DSound(Sound2DType.other);
        }

        if (!m_lastMuteMusic && stepCfg.muteMusic)
        {
            m_lastMuteMusic = true;
            m_musicOrigMute = SoundMgr.instance.muteBGM;
            SoundMgr.instance.MuteBGM(true);
        }
        else if (m_lastMuteMusic && !stepCfg.muteMusic)
        {
            m_lastMuteMusic = false;
            SoundMgr.instance.MuteBGM(m_musicOrigMute);
            m_musicOrigMute = false;
        }

        m_stepStartTime = DateTime.Now;

        ClearPathIndicators();
        m_uiTeach.ClearTeachUI();
        RemoveStepNormalEventListener();

        if (CanSkipThisStep(stepCfg))
        {
            //如果设置了要用停止来代替跳过，就停止
            if (stepCfg.stopIfNeedSkip)
                StartPlay(false);
            else
            {
                TimeMgr.instance.AddTimer(0, () =>
                {
                    //当前步骤是上次要跳过的步骤才真的跳过
                    if (IsCurStepConfig(stepCfg))
                        PlayNextStep();
                });
            }            
            return;
        }

        if (!m_uiTeach.IsOpenEx)
            m_uiTeach.Open(null, true);
        AddStepNormalEventListener();

        if (stepCfg.soundId != 0)
        {
            SoundMgr.instance.Play2DSound(Sound2DType.other, stepCfg.soundId);
        }

        if (!string.IsNullOrEmpty(stepCfg.centerTip))
        {
            m_uiTeach.ShowCenterText(stepCfg.centerTip);
        }

        StringBuilder notice = new StringBuilder();

        if (stepCfg.uiOpType != TeachUIOpType.none)
        {
            notice.AppendFormat("{0}:{1}", Util.GetEnumDesc(stepCfg.uiOpType), stepCfg.uiOpParam);
        }
        
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.fullScreenImg:
            case TeachUIOpType.windowImg:
                PlayShowImg(stepCfg);
                break;
            case TeachUIOpType.playStory:
                PlayStory(stepCfg);
                break;
            case TeachUIOpType.showMainCityUI:
            case TeachUIOpType.uiPanelOpenTop:
            case TeachUIOpType.uiPanelClose:
                break;
            case TeachUIOpType.uiPanelFunc:
                CallPanelFuncWhenPlay(stepCfg);
                break;
            case TeachUIOpType.uiPanelFuncSync:
                CallPanelFuncSyncWhenPlay(stepCfg);
                break;
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                ShowHideUINode(stepCfg, m_rectFromPanelFuncForCheck, m_rectFromPanelFuncForCheck2);
                break;
            default:
                PlayUIStep(stepCfg, m_rectFromPanelFuncForCheck, m_rectFromPanelFuncForCheck2);
                break;
        }

        if (stepCfg.sceneOpType != TeachSceneOpType.none)
        {
            if (notice.Length > 0)
                notice.AppendLine();
            notice.AppendFormat("{0}:{1}", Util.GetEnumDesc(stepCfg.sceneOpType), stepCfg.sceneOpParam);
        }

        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.movePos:
                PlayMovePos(stepCfg);
                break;
            case TeachSceneOpType.fireSceneAction:
                PlayFireSceneAction(stepCfg);
                break;
            case TeachSceneOpType.enterScene:
                {
                    //如果当前没有Room或不在指定ID的Room
                    if (Room.instance == null || Room.instance.roomCfg.id != stepCfg.sceneOpParam)
                    {
                        if (stepCfg.stepObj == TeachStepObj.sceneOp)
                            PlayNextStep();

                        LevelMgr.instance.ChangeLevel(stepCfg.sceneOpParam);
                    }
                    else if (stepCfg.stepObj == TeachStepObj.sceneOp)
                    {
                        PlayNextStep();
                    }                     
                }
                break;
            case TeachSceneOpType.leaveScene:
                {
                    //如果不在主城，就退出到主城
                    if (!LevelMgr.instance.IsMainCity())
                    {
                        if (stepCfg.stepObj == TeachStepObj.sceneOp)
                            PlayNextStep();

                        LevelMgr.instance.GotoMaincity();
                    }                        
                    //如果本来就在主城，能退到哪？直接下一步吧
                    else if (stepCfg.stepObj == TeachStepObj.sceneOp)
                    {
                        PlayNextStep();
                    }
                }
                break;
            case TeachSceneOpType.teachData:
                {
                    var hero = RoleMgr.instance.Hero;
                    var part = hero == null ? null : hero.SystemsPart;
                    if (part != null)
                        part.SetTeachData(stepCfg.sceneOpParam, StringUtil.ToInt(stepCfg.sceneOpParam2));
                    PlayNextStep();
                }
                break;
            case TeachSceneOpType.enableHeroAI:
                {
                    var hero = RoleMgr.instance.Hero;
                    var part = hero == null || hero.State != Role.enState.alive ? null : hero.AIPart;

                    if (part != null)
                    {
                        var enable = StringUtil.ToInt(stepCfg.sceneOpParam) == 0 ? false : true;
                        if (enable)
                            part.Play(AIPart.HeroAI);
                        else
                            part.Stop();
                    }

                    PlayNextStep();
                }
                break;
        }

        if (notice.Length > 0)
        {
            ShowNotice(notice.ToString());
        }            
        else
        {
            ShowNotice("进入下一步");
        }
    }

    private void PlayMovePos(TeachStepConfig stepCfg)
    {
        switch (stepCfg.sceneGuideType)
        {
            case TeachSceneGuideType.fullPathGuide:
                {
                    var destPos = Vector3.zero;
                    StringUtil.TryParse(stepCfg.sceneOpParam, out destPos);
                    ShowPathInScene(destPos);
                }
                break;
        }
    }

    private void PlayFireSceneAction(TeachStepConfig stepCfg)
    {
        SceneEventMgr.instance.FireAction(stepCfg.sceneOpParam);
        if (IsCurStepConfig(stepCfg))
        {
            if (stepCfg.stepObj != TeachStepObj.sceneOp)
            {
                Debuger.LogError("当前步骤目标必须是sceneOp");
            }

            PlayNextStep();
        }            
    }

    private void PlayShowImg(TeachStepConfig stepCfg)
    {
        m_uiTeach.PlayShowImg(stepCfg);
    }

    private void PlayStory(TeachStepConfig stepCfg)
    {
        StoryMgr.instance.PlayStory(stepCfg.uiOpParam);
    }

    private void PlayUIStep(TeachStepConfig stepCfg, RectTransform rectDirect = null, RectTransform rectDirect2 = null)
    {
        m_uiTeach.PlayUIStep(stepCfg, rectDirect, rectDirect2);
    }

    private bool CallPanelFunc(string uiPath, string param)
    {
        var uiPanel = UITeach.FindUIPanel(uiPath);
        if (uiPanel == null)
        {
            Debuger.LogError("找不到UIPanel：" + uiPath);
            return false;
        }

        var methodInfo = uiPanel.GetType().GetMethod(TeachMgr.TEACH_ACTION_FUNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo == null)
        {
            Debuger.LogError("找不到" + TeachMgr.TEACH_ACTION_FUNC + "：" + uiPath);
            return false;
        }

        try
        {
            methodInfo.Invoke(uiPanel, new object[] { param });
        }
        catch (System.Exception ex)
        {
            Debuger.LogError("调用" + TeachMgr.TEACH_ACTION_FUNC + "出错，窗口路径：" + uiPath + "，参数：" + param);
            Debuger.LogError(ex.Message);
            return false;
        }

        return true;
    }

    private bool CallPanelFuncWhenPlay(TeachStepConfig stepCfg)
    {
        if (!CallPanelFunc(stepCfg.uiOpParam, stepCfg.uiOpParam2))
        {
            StartPlay(false);
            return false;
        }
        
        return true;
    }

    private void CallPanelFuncSyncWhenPlay(TeachStepConfig stepCfg)
    {
        //这个函数调用完就马上下一步
        if (CallPanelFuncWhenPlay(stepCfg) && IsCurStepConfig(stepCfg))
        {
            if (stepCfg.stepObj != TeachStepObj.uiOp)
            {
                Debuger.LogError("当前步骤目标必须是uiOp");
            }

            PlayNextStep();
        }
    }

    private bool CallPanlCheckFunc(string uiPath, string param)
    {
        var uiPanel = UITeach.FindUIPanel(uiPath);
        if (uiPanel == null)
        {
            Debuger.LogError("找不到UIPanel：" + uiPath);
            return false;
        }

        var methodInfo = uiPanel.GetType().GetMethod(TeachMgr.TEACH_CHECK_FUNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo == null)
        {
            Debuger.LogError("找不到" + TeachMgr.TEACH_CHECK_FUNC + "：" + uiPath);
            return false;
        }

        try
        {
            return (bool)methodInfo.Invoke(uiPanel, new object[] { param });
        }
        catch (System.Exception ex)
        {
            Debuger.LogError("调用" + TeachMgr.TEACH_CHECK_FUNC + "出错，窗口路径：" + uiPath + "，参数：" + param);
            Debuger.LogError(ex.StackTrace);
            return false;
        }
    }

    private void ShowHideUINode(TeachStepConfig stepCfg, RectTransform rectDirect = null, RectTransform rectDirect2 = null)
    {
        //这个函数调用完就马上下一步
        m_uiTeach.ShowHideUINode(stepCfg, rectDirect, rectDirect2);
        if (IsCurStepConfig(stepCfg))
        {
            if (stepCfg.stepObj != TeachStepObj.uiOp)
            {
                Debuger.LogError("当前步骤目标必须是uiOp");
            }

            PlayNextStep();
        }            
    }

    private void PlayNextStep()
    {
        if (m_curTeachCfg == null)
            return;

        if (!string.IsNullOrEmpty(m_curTeachCfg.dataKey) && m_curPlayIndex >= 0 && m_curPlayIndex < m_curTeachCfg.stepList.Count)
        {
            var hero = RoleMgr.instance.Hero;
            var stepCfg = m_curTeachCfg.stepList[m_curPlayIndex];
            if (hero != null && stepCfg.keyStep)
            {
                var part = hero.SystemsPart;
                if (part != null && part.GetTeachVal(m_curTeachCfg.dataKey) != TEACH_PLAYED_FLAG)
                {
                    if (!part.SetTeachData(m_curTeachCfg.dataKey, TEACH_PLAYED_FLAG))
                    {
                        Debuger.LogError("设置引导数据标记失败");
                    }
                    else
                    {
                        //播放过了，从触发器里删除
                        RemoveTriggerRegister(m_curTeachCfg.teachName);
                    }
                }
            }
        }

        ++m_curPlayIndex;
        if (m_curPlayIndex >= m_curTeachCfg.stepList.Count)
        {
            StartPlay(false);            
        }
        else
        {
            var stepCfg = m_curTeachCfg.stepList[m_curPlayIndex];
            PlayStep(stepCfg);
        }
    }

    private void PlayTeachFromQueue()
    {
        if (m_teachQueue.Count > 0)
        {
            var teachTaskInfo = m_teachQueue[0];
            m_teachQueue.RemoveAt(0);
            StartPlayTeach(teachTaskInfo.teachName);
        }
    }

    private void RenameFromPlayQueue(string oldTeachName, string newTeachName)
    {
        var idx = m_teachQueue.FindIndex((info) => { return info.teachName == oldTeachName; });
        if (idx >= 0)
            m_teachQueue[idx] = new TeachTaskInfo(newTeachName, TeachConfig.GetTeachPriority(newTeachName));
    }

    private void RenameFromAgents(string oldTeachName, string newTeachName)
    {
        List<TeachAgent> agents;
        if (m_teachAgents.TryGetValue(oldTeachName, out agents))
        {
            for (var i = 0; i < agents.Count; ++i)
                agents[i].TeachName = newTeachName;
        }            
    }

    public void RemoveFromPlayQueue(string teachName)
    {
        var idx = m_teachQueue.FindIndex((info) => { return info.teachName == teachName; });
        if (idx >= 0)
            m_teachQueue.RemoveAt(idx);
    }

    public void OnRenameTeach(string oldTeachName, string newTeachName)
    {
        RenameFromPlayQueue(oldTeachName, newTeachName);
        RenameFromAgents(oldTeachName, newTeachName);
    }

    public void ClearPlayQueue()
    {
        m_teachQueue.Clear();
    }

    public void ClearCoolDownData()
    {
        m_lastPlayTimeMap.Clear();
    }

    public void BuildUnfinishedPlayQueue()
    {
        var hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        var configs = TeachConfig.Configs;
        for (int i = 0, cnt = configs.Count; i < cnt; ++i)
        {
            TeachConfig cfg = configs[i];

            if (cfg != m_curTeachCfg && !string.IsNullOrEmpty(cfg.dataKey) && cfg.replayUnfinished)
            {
                var teachVal = hero.SystemsPart.GetTeachVal(cfg.dataKey);
                if (teachVal == TeachMgr.TEACH_UNFINISHED_FLAG)
                    PlayTeach(cfg.teachName);
            }
        }
    }

    private void TryAddUnfinishedFlag(string teachName)
    {
        var cfg = TeachConfig.FindConfigByName(teachName);
        if (cfg == null)
        {
            Debuger.LogError("找不到引导配置：{0}", teachName);
            return;
        }

        //对于触发了，但还没播放的引导，要打上标记，好让后面接着播放
        var hero = RoleMgr.instance.Hero;
        if (hero != null && !string.IsNullOrEmpty(cfg.dataKey) && cfg.replayUnfinished)
        {
            var val = hero.SystemsPart.GetTeachVal(cfg.dataKey);
            if (val == 0)
                hero.SystemsPart.SetTeachData(cfg.dataKey, TEACH_UNFINISHED_FLAG);
        }
    }

    private void RemoveTimerCancelIfPanelNotTop()
    {
        if (m_timerCancelIfPanelNotTop != null)
        {
            m_timerCancelIfPanelNotTop.Release();
            m_timerCancelIfPanelNotTop = null;
        }
    }

    private bool IsInPlaying(string teachName = null)
    {
        return m_curTeachCfg != null && (string.IsNullOrEmpty(teachName) ? true : m_curTeachCfg.teachName == teachName) && ((m_playNow || m_recordNow) || m_timerPlayInNextFrame != null);
    }
    #endregion

    #region 事件相关
    private void RegisterTriggers()
    {
        m_triBackMainCity.Clear();
        m_triMainCityFromAny.Clear();
        m_triHeroLevel.Clear();
        m_triDirectEvent.Clear();
        m_triOpenPanel.Clear();
        m_triEnterScene.Clear();
        m_triMainCityUITop.Clear();

        foreach (var m in m_triNormalEvent.Values)
        {
            foreach (var obId in m.Values)
            {
                EventMgr.Remove(obId);
            }                
        }
        m_triNormalEvent.Clear();

        foreach (var agents in m_teachAgents.Values)
        {
            for (var i = 0; i < agents.Count; ++i)
                agents[i].Release();
        }
        m_teachAgents.Clear();

        var configs = TeachConfig.Configs;
        for (int i = 0, cnt = configs.Count; i < cnt; ++i)
        {
            TeachConfig cfg = configs[i];
            AddTriggerRegister(cfg);
        }
    }

    private TeachAgent CreateTeachAgent(string teachName, TeachAgentType type)
    {
        switch (type)
        {
            case TeachAgentType.skillFrame:
                return new TeachAgentSkillFrame(teachName);
            case TeachAgentType.levelFail:
                return new TeachAgentLevelFail(teachName);
            default:
                return null;
        }        
    }

    public void AddTriggerRegister(TeachConfig cfg)
    {
        var hero = RoleMgr.instance.Hero;
        if (hero != null && !string.IsNullOrEmpty(cfg.dataKey))
        {
            var teachVal = hero.SystemsPart.GetTeachVal(cfg.dataKey);
            if (teachVal == TeachMgr.TEACH_PLAYED_FLAG)
                return;
        }

        var teachName = cfg.teachName;
        var triggerConds = cfg.triggerConds;
        for (int j = 0, cnt2 = triggerConds.Count; j < cnt2; ++j)
        {
            var triggerCond = triggerConds[j];
            switch (triggerCond.triggerType)
            {
                case TeachTriggerCond.mainCity:
                    {
                        var key = string.IsNullOrEmpty(triggerCond.triggerParam) ? LevelMgr.MainRoomID : triggerCond.triggerParam;
                        var teachList = m_triBackMainCity.GetNewIfNo(key);
                        if (!teachList.Contains(teachName))
                            teachList.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.mainCityFromAny:
                    {
                        if (!m_triMainCityFromAny.Contains(teachName))
                            m_triMainCityFromAny.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.heroLevel:
                    {
                        var minLv = StringUtil.ToInt(triggerCond.triggerParam);
                        var maxLv = StringUtil.ToInt(triggerCond.triggerParam2);
                        if (!m_triHeroLevel.Exists((info)=> { return info.teachName == teachName && info.minLv == minLv && info.maxLv == maxLv; }))
                            m_triHeroLevel.Add(new HeroLevelTriggerInfo(teachName, minLv, maxLv));
                    }
                    break;
                case TeachTriggerCond.directEvent:
                    {
                        var key = triggerCond.triggerParam + "," + triggerCond.triggerParam2;
                        var teachList = m_triDirectEvent.GetNewIfNo(key);
                        if (!teachList.Contains(teachName))
                            teachList.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.openPanel:
                    {
                        var key = triggerCond.triggerParam;
                        var teachList = m_triOpenPanel.GetNewIfNo(key);
                        if (!teachList.Contains(teachName))
                            teachList.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.enterScene:
                    {
                        var key = triggerCond.triggerParam;
                        var teachList = m_triEnterScene.GetNewIfNo(key);
                        if (!teachList.Contains(teachName))
                            teachList.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.mainCityUITop:
                    {
                        if (!m_triMainCityUITop.Contains(teachName))
                            m_triMainCityUITop.Add(teachName);
                    }
                    break;
                case TeachTriggerCond.normalEvent:
                    {
                        var key = triggerCond.triggerParam + "," + triggerCond.triggerParam2;
                        var teachList = m_triNormalEvent.GetNewIfNo(key);
                        if (!teachList.ContainsKey(teachName))
                        {
                            var msg = StringUtil.ToInt(triggerCond.triggerParam);
                            var code = StringUtil.ToInt(triggerCond.triggerParam2);
                            teachList[teachName] = EventMgr.AddAll(msg, code, OnNormalEventForTrigger);
                        }                            
                    }
                    break;
                case TeachTriggerCond.teachAgent:
                    {
                        var agents = m_teachAgents.GetNewIfNo(teachName);
                        var agentType = (TeachAgentType)StringUtil.ToInt(triggerCond.triggerParam);
                        var agentObj = CreateTeachAgent(teachName, agentType);
                        agentObj.Init(triggerCond.triggerParam2);
                        //这里不判断重复了，编辑时注意防重复
                        agents.Add(agentObj);
                    }
                    break;
            }
        }
    }

    public void RemoveTriggerRegister(string teachName)
    {
        foreach (var l in m_triBackMainCity.Values)
        {
            l.Remove(teachName);
        }

        m_triMainCityFromAny.Remove(teachName);

        m_triHeroLevel.RemoveAll((info) => { return info.teachName == teachName; });        

        foreach (var l in m_triDirectEvent.Values)
        {
            l.Remove(teachName);
        }

        foreach (var l in m_triOpenPanel.Values)
        {
            l.Remove(teachName);
        }

        foreach (var l in m_triEnterScene.Values)
        {
            l.Remove(teachName);
        }

        m_triMainCityUITop.Remove(teachName);

        foreach (var m in m_triNormalEvent.Values)
        {
            int obId;
            if (m.TryGetValue(teachName, out obId))
            {
                EventMgr.Remove(obId);
                m.Remove(teachName);
            }
        }

        List<TeachAgent> agents;
        if (m_teachAgents.TryGetValue(teachName, out agents))
        {
            for (var i = 0; i < agents.Count; ++i)
                agents[i].Release();
            m_teachAgents.Remove(teachName);
        }
    }

    public void RefreshTriggerRegister(TeachConfig cfg)
    {
        RemoveTriggerRegister(cfg.teachName);
        AddTriggerRegister(cfg);
    }

    private void OnStateHandleClick(StateHandle handle)
    {
        ProcessUIEvent(TeachUIOpType.uiClick, handle.gameObject);
    }

    private void OnStateHandleHold(StateHandle handle)
    {
        ProcessUIEvent(TeachUIOpType.uiHold, handle.gameObject);
    }

    private void OnStateHandlePtrDown(PointerEventData eventData)
    {
        //这里解释一下，如果是真实按下是lastPress不null，如果是模拟按下是pointerPress不为null
        ProcessUIEvent(TeachUIOpType.uiPtrDown, eventData.lastPress != null ? eventData.lastPress : eventData.pointerPress);
    }

    private void OnStateHandlePtrUp(PointerEventData eventData)
    {
        ProcessUIEvent(TeachUIOpType.uiPtrUp, eventData.pointerPress);
    }

    private void OnStateHandleDragBegin(PointerEventData eventData)
    {
        ProcessUIEvent(TeachUIOpType.uiDragBegin, eventData.pointerDrag);
    }

    private void OnStateHandleDragEnd(PointerEventData eventData)
    {
        ProcessUIEvent(TeachUIOpType.uiDragEnd, eventData.pointerDrag);
    }

    private void OnStateHandleState(StateHandle handle, int state)
    {
        if (!m_recordNow || m_recordStateHandleState)
            ProcessUIEvent(TeachUIOpType.stateHandle, handle.gameObject);

        //处理回退机制
        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.stateHandle:
                        {
                            var handle1 = UITeach.FindStateHandle(backCheck.checkParam);
                            var refIdx = StringUtil.ToInt(backCheck.checkParam2);
                            if (handle == handle1 && handle.CurStateIdx != refIdx)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }
                        }
                        break;
                }
            }
        }
    }

    private void OnStateGroupSel(StateGroup group, int idx)
    {
        if (!m_recordNow || m_recordStateHandleState)
            ProcessUIEvent(TeachUIOpType.stateGroup, group.gameObject);

        //处理回退机制
        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.stateGroup:
                        {
                            var group1 = UITeach.FindStateGroup(backCheck.checkParam);
                            if (group == group1)
                            {
                                var refIdx = StringUtil.ToInt(backCheck.checkParam2);
                                if (refIdx >= 0 ? group1.CurIdx != refIdx : group1.CurIdx != group1.Count + refIdx)
                                {
                                    hasAction = true;
                                    ExecuteBackAction(backCheck);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    private void ProcessUIEvent(TeachUIOpType opType, GameObject go)
    {
        if (m_curTeachCfg == null)
            return;

        var path = Util.GetGameObjectPath(go);
        if (m_recordNow)
        {
            var stepCfg = new TeachStepConfig();
            stepCfg.stepObj = TeachStepObj.uiOp;
            stepCfg.stepObjParam = UITeach.DEF_WAIT_UI_TIME.ToString();
            stepCfg.uiOpType = opType;
            stepCfg.uiOpParam = path;
            stepCfg.circleType = TeachCircleType.roundWave;
            switch (opType)
            {
                case TeachUIOpType.uiClick:
                case TeachUIOpType.uiPtrDown:
                case TeachUIOpType.uiPtrUp:
                    stepCfg.arrowType = TeachArrowType.arrow;
                    break;
                case TeachUIOpType.uiHold:
                case TeachUIOpType.uiDragBegin:
                case TeachUIOpType.uiDragEnd:
                    stepCfg.arrowType = TeachArrowType.hand;
                    break;
                case TeachUIOpType.stateHandle:
                    {
                        stepCfg.arrowType = TeachArrowType.arrow;
                        var stateHandle = go.GetComponent<StateHandle>();
                        if (stateHandle != null)
                            stepCfg.uiOpParam2 = stateHandle.CurStateIdx.ToString();
                    }            
                    break;
                case TeachUIOpType.stateGroup:
                    {
                        stepCfg.arrowType = TeachArrowType.arrow;
                        var stateGroup = go.GetComponent<StateGroup>();
                        if (stateGroup != null)
                            stepCfg.uiOpParam2 = stateGroup.CurIdx.ToString();
                    }
                    break;
            }
            m_curTeachCfg.stepList.Insert(m_curRecordIndex++, stepCfg);
            m_curTeachCfg.IsDirty = true;

            Debuger.Log(string.Format("{0}:{1}", Util.GetEnumDesc(opType), path));
            ShowNotice(string.Format("{0}:{1}", Util.GetEnumDesc(opType), path));
        }
        else if (m_playNow)
        {
            //Debuger.Log(string.Format("{0}:{1}", Util.GetEnumDesc(opType), path));

            if (m_curTeachCfg.stepList.Count <= 0)
            {
                ShowNotice("没有动作");
            }
            else
            {
                if (m_curPlayIndex >= m_curTeachCfg.stepList.Count)
                {
                    StartPlay(false);
                }
                else
                {
                    var stepCfg = m_curTeachCfg.stepList[m_curPlayIndex];
                    if (((m_rectFromPanelFuncForCheck == null && path == stepCfg.uiOpParam) || go.transform == m_rectFromPanelFuncForCheck) && (opType == stepCfg.uiOpType || (opType == TeachUIOpType.uiDragBegin && stepCfg.uiOpType == TeachUIOpType.uiDragDemo)))
                    {
                        m_rectFromPanelFuncForCheck = null;
                        m_rectFromPanelFuncForCheck2 = null;

                        if (opType == TeachUIOpType.stateHandle)
                        {
                            var stateHandle = go.GetComponent<StateHandle>();
                            if (stateHandle == null || stateHandle.CurStateIdx != StringUtil.ToInt(stepCfg.uiOpParam2))
                                return;
                        }
                        else if (opType == TeachUIOpType.stateGroup)
                        {
                            var stateGroup = go.GetComponent<StateGroup>();
                            if (stateGroup == null)
                                return;

                            var refIdx = StringUtil.ToInt(stepCfg.uiOpParam2);
                            if (refIdx >= 0 ? stateGroup.CurIdx != refIdx : stateGroup.CurIdx != stateGroup.Count + refIdx)
                                return;
                        }

                        if (stepCfg.stepObj == TeachStepObj.uiOp)
                            PlayNextStep();
                    }
                }
            }
        }
    }

    private void OnStateHandleFullScreenClick()
    {
        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            switch (stepCfg.uiOpType)
            {
                case TeachUIOpType.fullScreenImg:
                case TeachUIOpType.windowImg:
                case TeachUIOpType.fullScreenClick:
                    {
                        if (stepCfg.stepObj == TeachStepObj.uiOp)
                            PlayNextStep();
                    }
                    break;
            }
        }
    }

    private void CheckStepObj(TeachStepConfig stepCfg)
    {
        switch (stepCfg.stepObj)
        {
            case TeachStepObj.sceneOp:
                CheckSceneOp(stepCfg);
                break;
            case TeachStepObj.showUINode:
            case TeachStepObj.hideUINode:
            case TeachStepObj.uiOp:
                CheckStory(stepCfg);
                CheckUIActive(stepCfg);
                break;
            case TeachStepObj.heroLevel:
                CheckHeroLevel(stepCfg);
                break;
            case TeachStepObj.none:
                CheckObjNone(stepCfg);                
                break;
        }
    }

    private void CheckSceneOp(TeachStepConfig stepCfg)
    {
        if (stepCfg.stepObj != TeachStepObj.sceneOp)
            return;

        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.movePos:
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero != null)
                    {
                        var heroPos = hero.TranPart.Pos;
                        var destPos = Vector3.zero;
                        StringUtil.TryParse(stepCfg.sceneOpParam, out destPos);
                        var radius = StringUtil.ToFloat(stepCfg.sceneOpParam2, 1);
                        if (Vector3.Distance(destPos, heroPos) <= radius)
                        {
                            //有时弹出了一个新窗口，导致摇杆得不到弹起事件，这里让它弹起
                            UIMgr.instance.Get<UILevel>().Get<UILevelAreaJoystick>().UIJoystick.JoystickUp();
                            PlayNextStep();
                        }
                    }
                }
                break;
        }
    }

    private void CheckStory(TeachStepConfig stepCfg)
    {
        if (stepCfg.uiOpType == TeachUIOpType.playStory && (stepCfg.stepObj == TeachStepObj.uiOp || stepCfg.stepObj == TeachStepObj.hideUINode))
        {
            if (!StoryMgr.instance.IsPlaying)
                PlayNextStep();
        }
    }

    private void CheckUIActive(TeachStepConfig stepCfg)
    {
        switch (stepCfg.stepObj)
        {
            case TeachStepObj.showUINode:
                {
                    var go = GameObject.Find(stepCfg.stepObjParam);
                    if (go != null && go.activeInHierarchy)
                        PlayNextStep();
                }
                break;
            case TeachStepObj.hideUINode:
                {
                    var go = GameObject.Find(stepCfg.stepObjParam);
                    if (go == null || !go.activeInHierarchy)
                        PlayNextStep();
                }
                break;
            case TeachStepObj.uiOp:
                {
                    switch (stepCfg.uiOpType)
                    {
                        case TeachUIOpType.uiPanelOpenTop:
                            {
                                var ui = UITeach.FindUIPanel(stepCfg.uiOpParam);
                                if (ui != null && ui.IsTop)
                                    PlayNextStep();
                            }
                            break;
                        case TeachUIOpType.uiPanelClose:
                            {
                                var ui = UITeach.FindUIPanel(stepCfg.uiOpParam);
                                if (ui == null || !ui.IsOpenEx)
                                    PlayNextStep();
                            }
                            break;
                        case TeachUIOpType.showMainCityUI:
                            {
                                var ui = UIMgr.instance.Get<UIMainCity>();
                                if (ui != null && ui.IsTop)
                                    PlayNextStep();
                            }
                            break;
                    }
                }
                break;
        }
    }

    private void CheckHeroLevel(TeachStepConfig stepCfg)
    {
        if (stepCfg.stepObj == TeachStepObj.heroLevel)
        {
            var hero = RoleMgr.instance.Hero;
            if (hero != null)
            {
                var lv = StringUtil.ToInt(stepCfg.stepObjParam);
                if (hero.GetInt(enProp.level) >= lv)
                    PlayNextStep();
            }
        }
    }

    private void CheckObjNone(TeachStepConfig stepCfg)
    {
        if (stepCfg.timeoutInMS <= 0)
            PlayNextStep();
    }

    private void CheckTimeout(TeachStepConfig stepCfg)
    {
        var curTime = DateTime.Now;
        if (stepCfg.timeoutInMS <= 0 || (DateTime.Now - m_stepStartTime).TotalMilliseconds < stepCfg.timeoutInMS)
            return;

        m_stepStartTime = DateTime.Now; //为了防止频繁执行

        //////////////////////////
        if (stepCfg.timeoutStop)
        {
            StartPlay(false);
            return;
        }            

        //////////////////////////
        switch (stepCfg.stepObj)
        {
            case TeachStepObj.none:
            case TeachStepObj.directEventEx:
            case TeachStepObj.normalEventEx:
                PlayNextStep();
                break;
        }

        //////////////////////////
        //如果stepCfg不是当前步骤了，就不用继续
        if (!IsCurStepConfig(stepCfg))
            return;  

        //////////////////////////
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.playStory:
                {
                    StoryMgr.instance.EndStory();
                }                
                break;
            case TeachUIOpType.none:
            case TeachUIOpType.uiStatic:
            case TeachUIOpType.uiDragDemo:
                {
                    if (stepCfg.stepObj == TeachStepObj.uiOp)
                        PlayNextStep();
                }
                break;
            case TeachUIOpType.uiPanelFunc:
            case TeachUIOpType.uiPanelFuncSync:
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                break;
            default:
                {
                    m_uiTeach.ExecuteUIStep(stepCfg, m_rectFromPanelFuncForCheck);
                }                
                break;
        }

        //////////////////////////
        //如果stepCfg不是当前步骤了，就不用继续
        if (!IsCurStepConfig(stepCfg))
            return;

        //////////////////////////
        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.movePos:
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero != null)
                    {
                        var destPos = Vector3.zero;
                        StringUtil.TryParse(stepCfg.sceneOpParam, out destPos);
                        hero.MovePart.MovePos(destPos);
                    }
                }
                break;
            case TeachSceneOpType.none:
                {
                    if (stepCfg.stepObj == TeachStepObj.sceneOp)
                        PlayNextStep();
                }
                break;
        }
    }

    private void AddUIListener()
    {
        StateHandle.AddGlobalHook(OnStateHandleClick, OnStateHandleHold, OnStateHandlePtrDown, OnStateHandlePtrUp, OnStateHandleDragBegin, OnStateHandleDragEnd, OnStateHandleState);
        StateGroup.AddGlobalHook(OnStateGroupSel);
        DragControl.AddGlobalHook(OnStateHandleDragBegin);
    }

    private void AddBackNormalEventListener()
    {
        if (m_curTeachCfg == null)
            return;
        var backChecks = m_curTeachCfg.backChecks;
        for (var i = 0; i < backChecks.Count; ++i)
        {
            var elem = backChecks[i];
            if(elem.checkType == TeachBackCheckType.normalEvent)
            {
                var obId = EventMgr.AddAll(StringUtil.ToInt(elem.checkParam), StringUtil.ToInt(elem.checkParam2), OnNormalEventForBack);
                m_eventIdForCurTeach.Add(obId);
            }            
        }
    }

    private void AddStepNormalEventListener()
    {
        var curStepCfg = GetCurStepConfig();
        if (curStepCfg == null)
            return;
        if (curStepCfg.stepObj == TeachStepObj.normalEvent || curStepCfg.stepObj == TeachStepObj.normalEventEx)
        {
            var obId = EventMgr.AddAll(StringUtil.ToInt(curStepCfg.stepObjParam), StringUtil.ToInt(curStepCfg.stepObjParam2), OnNormalEventForStep);
            m_eventIdForCurStep = obId;
        }
    }

    private void RemoveUIListener()
    {
        StateHandle.RemoveGlobalHook(OnStateHandleClick, OnStateHandleHold, OnStateHandlePtrDown, OnStateHandlePtrUp, OnStateHandleDragBegin, OnStateHandleDragEnd, OnStateHandleState);
        StateGroup.RemoveGlobalHook(OnStateGroupSel);
        DragControl.RemoveGlobalHook(OnStateHandleDragBegin);
    }

    private void RemoveBackNormalEventListener()
    {
        if (m_eventIdForCurTeach.Count <= 0)
            return;

        for (var i = 0; i < m_eventIdForCurTeach.Count; ++i)
        {
            var obId = m_eventIdForCurTeach[i];
            EventMgr.Remove(obId);
        }
        m_eventIdForCurTeach.Clear();
    }

    private void RemoveStepNormalEventListener()
    {
        if (m_eventIdForCurStep == EventMgr.Invalid_Id)
            return;

        EventMgr.Remove(m_eventIdForCurStep);
        m_eventIdForCurStep = EventMgr.Invalid_Id;
    }

    private void AddFixedNormalEventListener()
    {
        if (m_eventIdSceneStart == EventMgr.Invalid_Id)
            m_eventIdSceneStart = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.START, OnEventSceneStart);
        if (m_eventIdSceneExit == EventMgr.Invalid_Id)
            m_eventIdSceneExit = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, OnEventSceneExit);
        if (m_eventIdPanelOpen == EventMgr.Invalid_Id)
            m_eventIdPanelOpen = EventMgr.AddAll(MSG.MSG_SYSTEM, MSG_SYSTEM.PANEL_OPEN, OnEventPanelOpen);
        if (m_eventIdPanelClose == EventMgr.Invalid_Id)
            m_eventIdPanelClose = EventMgr.AddAll(MSG.MSG_SYSTEM, MSG_SYSTEM.PANEL_CLOSE, OnEventPanelClose);
        if (m_eventIdHeroLevel == EventMgr.Invalid_Id)
            m_eventIdHeroLevel = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.PROP_CHANGE + (int)enProp.level, OnEventHeroLevel);
        if (m_eventIdMainCityUITop == EventMgr.Invalid_Id)
            m_eventIdMainCityUITop = EventMgr.AddAll(MSG.MSG_SYSTEM, MSG_SYSTEM.MAINCITY_UI_TOP, OnEventMainCityUITop);
    }

    private void RemoveFixedNormalEventListener()
    {
        if (m_eventIdSceneStart != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_eventIdSceneStart);
            m_eventIdSceneStart = EventMgr.Invalid_Id;
        }

        if (m_eventIdSceneExit != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_eventIdSceneExit);
            m_eventIdSceneExit = EventMgr.Invalid_Id;
        }

        if (m_eventIdHeroLevel != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_eventIdHeroLevel);
            m_eventIdHeroLevel = EventMgr.Invalid_Id;
        }

        if (m_eventIdMainCityUITop != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_eventIdMainCityUITop);
            m_eventIdMainCityUITop = EventMgr.Invalid_Id;
        }
    }

    private void AddToPlayQueue(List<string> teachNames)
    {
        for (int i = 0, cnt = teachNames.Count; i < cnt; ++i)
        {
            PlayTeach(teachNames[i]);
        }
    }

    private void AddToPlayQueue(Dictionary<string, int> teachNames)
    {
        foreach (var teachName in teachNames.Keys)
        {
            PlayTeach(teachName);
        }
    }

    private void OnNormalEventForTrigger(object param1, object param2, object param3, EventObserver observer)
    {
        var msg = observer.msg;
        var code = observer.code;

        //判断触发
        var key = msg + "," + code;
        Dictionary<string, int> teachList;
        if (m_triNormalEvent.TryGetValue(key, out teachList))
        {
            AddToPlayQueue(teachList);
        }
    }

    private void OnNormalEventForStep(object param1, object param2, object param3, EventObserver observer)
    {
        var msg = observer.msg;
        var code = observer.code;

        //判断推进
        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            if (stepCfg.stepObj == TeachStepObj.normalEvent || stepCfg.stepObj == TeachStepObj.normalEventEx)
            {
                if (!string.IsNullOrEmpty(stepCfg.stepObjParam))
                {
                    if (StringUtil.ToInt(stepCfg.stepObjParam) == msg && StringUtil.ToInt(stepCfg.stepObjParam2) == code)
                        PlayNextStep();
                }
            }
        }
    }

    private void OnNormalEventForBack(object param1, object param2, object param3, EventObserver observer)
    {
        var msg = observer.msg;
        var code = observer.code;

        //判断回退或取消
        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.normalEvent:
                        {
                            if (StringUtil.ToInt(backCheck.checkParam) == msg && StringUtil.ToInt(backCheck.checkParam2) == code)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }
                        }
                        break;
                }
            }
        }
    }

    private void OnEventSceneStart(object arg1)
    {
        var roomId = (string)arg1;

        if (roomId == LevelMgr.MainRoomID)
        {
            var PrevRoomId = LevelMgr.instance.PrevRoomId;
            List<string> teachList;
            if (m_triBackMainCity.TryGetValue(PrevRoomId, out teachList))
            {
                AddToPlayQueue(teachList);
            }
            
            AddToPlayQueue(m_triMainCityFromAny);

            //进入主城了，把一些以前没播完的引导也补进去
            BuildUnfinishedPlayQueue();
        }

        {
            List<string> teachList;
            if (m_triEnterScene.TryGetValue(roomId, out teachList))
            {
                AddToPlayQueue(teachList);
            }
        }        
    }

    private void OnEventSceneExit()
    {
        //停掉当前引导（并不执行后续的）
        StartPlay(false, 0, false);
        //跨场景要清引导队列
        ClearPlayQueue();
        //引导的冷却时间也清一下吧
        ClearCoolDownData();
    }

    private void OnEventPanelOpen(object arg1)
    {
        var uiPanel = (UIPanel)arg1;
        var uiPath = Util.GetGameObjectPath(uiPanel.gameObject);

        List<string> teachList;
        if (m_triOpenPanel.TryGetValue(uiPath, out teachList))
        {
            AddToPlayQueue(teachList);
        }

        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.beCovered:
                        {
                            var uiPanel1 = UITeach.FindUIPanel(backCheck.checkParam);
                            //顶层窗口不是目标窗口？
                            if (uiPanel1 != null && UIMgr.instance.TopPanel != uiPanel1)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }
                        }
                        break;
                }
            }
        }
    }

    private void OnEventPanelClose(object arg1)
    {
        var uiPanel = (UIPanel)arg1;

        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.closePanel:
                        {
                            var uiPanel1 = UITeach.FindUIPanel(backCheck.checkParam);
                            //关闭的窗口正好是目标窗口
                            if (uiPanel == uiPanel1)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }
                        }
                        break;
                    case TeachBackCheckType.beCovered:
                        {
                            var uiPanel1 = UITeach.FindUIPanel(backCheck.checkParam);
                            //顶层窗口不是目标窗口？
                            if (uiPanel1 != null && UIMgr.instance.TopPanel != uiPanel1)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }
                        }
                        break;
                }                
            }
        }
    }

    private void OnEventHeroLevel(object arg1)
    {
        var hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;
        var heroLv = hero.GetInt(enProp.level);
        for (int i = 0, cnt = m_triHeroLevel.Count; i < cnt; ++i)
        {
            var info = m_triHeroLevel[i];
            if (heroLv >= info.minLv && heroLv <= info.maxLv)
                PlayTeach(info.teachName);
        }
    }

    private void OnEventMainCityUITop()
    {
        AddToPlayQueue(m_triMainCityUITop);
    }

    private void ExecuteBackAction(TeachBackCheckData backCheck)
    {
        if (m_curTeachCfg == null || !m_playNow)
            return;

        switch (backCheck.actionType)
        {
            case TeachBackActionType.backStepTo:
                {
                    var stepNo = Mathf.Clamp(StringUtil.ToInt(backCheck.actionParam), 1, m_curTeachCfg.stepList.Count);
                    m_curPlayIndex = stepNo - 1;
                    PlayStep(m_curTeachCfg.stepList[m_curPlayIndex]);
                }
                break;
            case TeachBackActionType.cancelPlay:
                {
                    StartPlay(false);
                }
                break;
        }
    }
    #endregion

    #region 杂项函数
    private TeachStepConfig GetCurStepConfig()
    {
        if (m_curTeachCfg != null && m_curPlayIndex >= 0 && m_curPlayIndex < m_curTeachCfg.stepList.Count)
            return m_curTeachCfg.stepList[m_curPlayIndex];
        return null;
    }

    private bool IsCurStepConfig(TeachStepConfig stepCfg)
    {
        return stepCfg != null && stepCfg == GetCurStepConfig();
    }

    private void NotifyChangeConfig(int index)
    {
        if (m_editorCallback != null)
            m_editorCallback(1, index);
    }

    public void SetCurTeachConfig(TeachConfig config)
    {
        if (config != m_curTeachCfg)
        {
            m_curTeachCfg = config;
            m_curPlayIndex = -1;
        }
    }

    public void OnFindUIError(TeachStepConfig stepCfg)
    {
        m_uiTeach.Close(true);
        if (stepCfg.stepObj == TeachStepObj.uiOp)
        {
            StartPlay(false);
        }        
    }
    #endregion

    #region 常用接口
    public void PlayTeach(string teachName)
    {
        //先看看这个引导是否正在播放中
        if (!IsInPlaying(teachName))
        {
            //如果不在播放中，看看队列中有没有
            var idx = m_teachQueue.FindIndex((info) => { return info.teachName == teachName; });
            //如果不在队列中，那就尝试加入队列
            if (idx < 0)
            {
                var cfg = TeachConfig.FindConfigByName(teachName);
                if (cfg == null)
                {
                    Debuger.LogError("找不到引导配置：{0}", teachName);
                    return;
                }

                m_teachQueue.Add(new TeachTaskInfo(teachName, cfg.priority));

                TryAddUnfinishedFlag(teachName);
            }
        }        

        //如果本来就在队列里，这里还是会执行，是因为防止没有正在执行的引导，而队列一直等着
        if (!m_sortAndStartPlay)
            StartCoroutine(CoSortAndStartPlay());
    }

    public IEnumerator CoSortAndStartPlay()
    {
        m_sortAndStartPlay = true;

        //等下一帧再排序
        yield return new WaitForSeconds(0);

        m_sortAndStartPlay = false;

        m_teachQueue.SortEx((a, b) => { return a.priority.CompareTo(b.priority); });

        //没引导在播放？那就直接从队列取出来播放
        if (!IsInPlaying())
            PlayTeachFromQueue();
        else
        {
            //看看队列第一个引导是不是比正在播放的优先级更高，正在播放的引导是不是可以被抢占
            if (m_teachQueue.Count > 0)
            {
                var teachTaskInfo = m_teachQueue[0];
                //这里小于才是高优先级
                if (teachTaskInfo.priority < m_curTeachCfg.priority && m_curTeachCfg.canInterrupt)
                    PlayTeachFromQueue();   //直接播放即可，里面会中断当前播放的
            }
        }
    }
    
    public void StartRecord(bool recordNow, int startStepIndex = int.MinValue)
    {
        if (m_recordNow == recordNow)
            return;

        if (m_curTeachCfg == null)
            return;

        m_recordNow = recordNow;
        if (m_recordNow)
        {
            if (startStepIndex == int.MinValue)
            {
                //清空再追加
                m_curRecordIndex = 0;
                m_curTeachCfg.stepList.Clear();
                m_curTeachCfg.IsDirty = true;
            }
            else
            {
                //这里允许m_curTeachCfg.stepList.Count，就是最后面追加，m_curTeachCfg.stepList.Count - 1就是把最后一个往后挤了
                m_curRecordIndex = Mathf.Clamp(startStepIndex, 0, m_curTeachCfg.stepList.Count);
            }

            m_curPlayIndex = -1;
            ShowNotice("开始录制");

            AddUIListener();
        }
        else
        {
            m_curRecordIndex = -1;
            ShowNotice("停止录制");

            RemoveUIListener();
        }
    }

    public void StartPlay(bool playNow, int startStepIndex = 0, bool playSubsequent = true)
    {
        //销毁定时器
        if (m_timerPlayInNextFrame != null)
        {
            m_timerPlayInNextFrame.Release();
            m_timerPlayInNextFrame = null;
        }

        //都取消本引导或开启新引导了，那就取消定时停止引导的定时器
        RemoveTimerCancelIfPanelNotTop();

        if (m_playNow == playNow)
            return;

        if (m_curTeachCfg == null)
            return;

        //保存一份，以免中间这个成员变量被修改了
        var curTeachCfg = m_curTeachCfg;

        m_playNow = playNow;
        if (m_playNow)
        {
            var stepList = curTeachCfg.stepList;
            if (stepList.Count <= 0)
            {
                ShowNotice("没有动作");
                m_playNow = false;
            }
            else
            {
                EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.TEACH_START, curTeachCfg.teachName);

                AddUIListener();
                AddBackNormalEventListener();

                if (curTeachCfg.coolDown > 0)
                    m_lastPlayTimeMap[curTeachCfg.teachName] = TimeMgr.instance.GetTimestamp();

                m_curPlayIndex = Mathf.Clamp(startStepIndex, 0, stepList.Count - 1);
                var stepCfg = stepList[m_curPlayIndex];
                PlayStep(stepCfg);
            }
        }
        else
        {
            m_rectFromPanelFunc = null;
            m_rectFromPanelFunc2 = null;
            m_rectFromPanelFuncForCheck = null;
            m_rectFromPanelFuncForCheck2 = null;

            if (curTeachCfg.stopSoundWhenFinish)
            {
                SoundMgr.instance.Stop2DSound(Sound2DType.other);
            }

            switch (m_lastPauseMode)
            {
                case TeachPauseMode.logic:
                    TimeMgr.instance.SubPause();
                    break;
                case TeachPauseMode.full:
                    if (m_timeScaleHandle != null)
                    {
                        m_timeScaleHandle.Release();
                        m_timeScaleHandle = null;
                    }
                    break;
            }
            m_lastPauseMode = TeachPauseMode.none;

            if (m_lastMuteMusic)
            {
                m_lastMuteMusic = false;
                SoundMgr.instance.MuteBGM(m_musicOrigMute);
                m_musicOrigMute = false;
            }

            ClearPathIndicators();
            RemoveUIListener();
            RemoveBackNormalEventListener();
            RemoveStepNormalEventListener();

            var isNormalEnd = m_curPlayIndex >= curTeachCfg.stepList.Count;
            m_curPlayIndex = -1;

            if (m_uiTeach.IsOpenEx)
                m_uiTeach.Close(true);
            ShowNotice("停止播放");

            //不正常停止要调用取消行为
            if (!isNormalEnd)
                DoCancelActions(curTeachCfg);

            EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.TEACH_END, curTeachCfg.teachName, isNormalEnd);

            ///////后续引导//////////
            if (playSubsequent)
            {
                string postTeachName = curTeachCfg.postTeach;
                //如果是正常结束考虑播放后续引导
                if (isNormalEnd && !string.IsNullOrEmpty(postTeachName) && postTeachName != curTeachCfg.teachName)
                {
                    
                    if (StartPlayTeach(postTeachName))
                        TryAddUnfinishedFlag(postTeachName);
                }
                else
                {
                    PlayTeachFromQueue();
                }
            }            
        }
    }

    public void PlayNextStepFromPanelFunc()
    {
        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            if (stepCfg.uiOpType == TeachUIOpType.uiPanelFunc)
            {
                if (stepCfg.stepObj != TeachStepObj.uiOp)
                {
                    Debuger.LogError("当前步骤目标必须是uiOp");
                }

                PlayNextStep();
            }                
        }
    }

    public void PlayNextStepWithUIObjParam(RectTransform rect, RectTransform rect2 = null)
    {
        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            if (stepCfg.uiOpType == TeachUIOpType.uiPanelFunc)
            {
                if (stepCfg.stepObj != TeachStepObj.uiOp)
                {
                    Debuger.LogError("当前步骤目标必须是uiOp");
                }

                SetNextStepUIObjParam(rect, rect2);
                PlayNextStep();
            }
        }
    }

    public void SetNextStepUIObjParam(RectTransform rect, RectTransform rect2 = null)
    {
        m_rectFromPanelFunc = rect;
        m_rectFromPanelFunc2 = rect2;
        m_rectFromPanelFuncForCheck = null;
        m_rectFromPanelFuncForCheck2 = null;
    }

    public void ShowNotice(string str)
    {
        if (m_editorCallback != null)
            m_editorCallback(0, str);
    }

    public void OnDirectTeachEvent(string eventType, string eventParam)
    {
        //判断回退或取消
        if (m_playNow && m_curTeachCfg != null)
        {
            var backChecks = m_curTeachCfg.backChecks;
            var hasAction = false;
            var curStepNo = m_curPlayIndex + 1;
            for (var i = 0; i < backChecks.Count && !hasAction; ++i)
            {
                var backCheck = backChecks[i];

                if (curStepNo < backCheck.fromStep || curStepNo > backCheck.toStep)
                    continue;

                switch (backCheck.checkType)
                {
                    case TeachBackCheckType.directEvent:
                        {
                            if (backCheck.checkParam == eventType && backCheck.checkParam2 == eventParam)
                            {
                                hasAction = true;
                                ExecuteBackAction(backCheck);
                            }                            
                        }
                        break;
                }
            }
        }


        //判断推进
        var stepCfg = GetCurStepConfig();
        if (stepCfg != null)
        {
            if (stepCfg.stepObj == TeachStepObj.directEvent || stepCfg.stepObj == TeachStepObj.directEventEx)
            {
                if (!string.IsNullOrEmpty(stepCfg.stepObjParam))
                {
                    if (stepCfg.stepObjParam == eventType && stepCfg.stepObjParam2 == eventParam)
                        PlayNextStep();
                }
            }
        }

        //判断触发
        var key = eventType + "," + eventParam;
        List<string> teachList;
        if (m_triDirectEvent.TryGetValue(key, out teachList))
        {
            AddToPlayQueue(teachList);
        }
    }

    public static bool CanAutoNext(TeachStepConfig stepCfg)
    {
        switch (stepCfg.stepObj)
        {
            case TeachStepObj.showUINode:
            case TeachStepObj.hideUINode:
            case TeachStepObj.uiOp:
                {
                    switch (stepCfg.uiOpType)
                    {
                        case TeachUIOpType.uiDragBegin:
                        case TeachUIOpType.uiDragEnd:
                        case TeachUIOpType.uiPanelFunc:
                        case TeachUIOpType.uiPanelFuncSync:
                        case TeachUIOpType.showUINode:
                        case TeachUIOpType.hideUINode:
                            return false;
                        default:
                            return true;
                    }
                }
            case TeachStepObj.sceneOp:
                {
                    switch (stepCfg.sceneOpType)
                    {
                        case TeachSceneOpType.fireSceneAction:
                        case TeachSceneOpType.enterScene:
                        case TeachSceneOpType.leaveScene:
                        case TeachSceneOpType.teachData:
                        case TeachSceneOpType.enableHeroAI:
                            return false;
                        default:
                            return true;
                    }
                }
            case TeachStepObj.none:
            case TeachStepObj.directEventEx:
            case TeachStepObj.normalEventEx:
                return true;
        }
        return false;
    }

    public static bool NoShowIndicatorUIOp(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.none:
            case TeachUIOpType.playStory:
            case TeachUIOpType.fullScreenImg:
            case TeachUIOpType.windowImg:
            case TeachUIOpType.uiPanelOpenTop:
            case TeachUIOpType.uiPanelClose:
            case TeachUIOpType.showMainCityUI:
            case TeachUIOpType.uiPanelFunc:
            case TeachUIOpType.uiPanelFuncSync:
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                return true;
        }
        return false;
    }

    public static bool HasTeachActionFunc(System.Object obj)
    {
        if (obj == null)
            return false;

        var methodInfo = obj.GetType().GetMethod(TeachMgr.TEACH_ACTION_FUNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo == null)
            return false;

        var paramInfos = methodInfo.GetParameters();
        return paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(string);
    }

    public static bool HasTeachCheckFunc(System.Object obj)
    {
        if (obj == null)
            return false;

        var methodInfo = obj.GetType().GetMethod(TeachMgr.TEACH_CHECK_FUNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo == null)
            return false;

        var paramInfos = methodInfo.GetParameters();
        return paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(string) && methodInfo.ReturnType == typeof(bool);
    }
    #endregion
}