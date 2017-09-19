using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Simple.BehaviorTree;
public enum SceneState
{
    Loading,        //加载中
    LoadEnd,        //加载结束
    Playing,        //开始逻辑
    WaitContinue,   //等待复活
    Win,
    Lost,
}

public class SceneMgr : Singleton<SceneMgr>
{

    #region Fields
    SceneCfg.BornInfo mBornInfo = null;


    public SceneState mCurState = SceneState.Loading;
    
    public Dictionary<string, RefreshBase> refreshNpcDict = new Dictionary<string, RefreshBase>();
    
    public Dictionary<string, GameObject> mDangbanDict = new Dictionary<string, GameObject>();

    Dictionary<string, Dictionary<string, RefreshBase>> refreshSaveData = new Dictionary<string, Dictionary<string, RefreshBase>>();

    
    bool m_dirShow = false;
    bool m_arrowShow = false;
    bool m_isRefreshNpc = true;
    Vector3 m_findPos = Vector3.zero;
    CameraHandle m_fightCamera = null;
    float m_cameraDisRate = 1;
    float m_cameraRoundDis = 7;
    float m_lastUpdate = 0;

    public static bool SceneDebug = false;

    public SortedList<float, Role> m_autoFaceTem = new SortedList<float, Role>(new RoleMgr.CloseComparer());//用于自动朝向的临时列表

    #endregion


    #region Properties
    public SceneCfg.SceneData SceneData { get; set; }
    public bool IsDirShow { get { return m_dirShow; } }     //控制方向箭头显示
    public bool IsArrowShow { get { return m_arrowShow; } }   //控制怪物指向显示
    public Vector3 CurFindPos { get { return m_findPos; } }     //当前寻路点
    public int CurSceneIdx { get; set; }    //当前scene索引
    public int PrevSceneIdx { get; set; }    //前一个Scene索引
    public static int LoadingProgress { get; set; }    //关卡内加载的进度

    public CameraHandle FightCamera { get { return m_fightCamera; } set { m_fightCamera = value; } }   //战斗相机 方便释放

    public bool IsRefreshNpc { get { return m_isRefreshNpc; } set { m_isRefreshNpc = value; } }  //是否要更新刷新点
    #endregion


    #region Load Interface
    void LoadRole()
    {
        //加载全局敌人
        RoleCfg.PreLoad(RoleMgr.GlobalEnemyId);
        
        //加载主角模型
        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        hero.PreLoad();
        

        //预加载关卡中配置的出生死亡特效
        if (SceneData != null)
        {
            foreach(SceneCfg.BornInfo born in SceneData.mBornList)
                BornCfg.PreLoad(born.mBornTypeId, born.mDeadTypeId, born.mGroundDeadTypeId);
        }
        
        return;
    }

    void LoadEffect()
    {
        //挡板
        for (int i = 0; i < SceneCfg.DangbanName.Length; i++)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(SceneCfg.DangbanName[i]);

        //魂特效
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_hunzhi");
        //飞物品
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_fly_item");
        //红魂
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_fly_gold");
        //金币
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_fly_soul_red");

        //加载背景音乐
        if (Room.instance.roomCfg.bgmId != 0)
            SoundMgr.instance.PreLoad(Room.instance.roomCfg.bgmId);

        return;
    }

    //加载刷新点
    void LoadRefresh()
    {
        string sceneFile = "";
        if (LevelMgr.instance.CurLevel.roomCfg.sceneFileName.Count > CurSceneIdx)
            sceneFile = LevelMgr.instance.CurLevel.roomCfg.sceneFileName[CurSceneIdx];
        else
            return;

        if (refreshSaveData.TryGetValue(sceneFile, out refreshNpcDict))
        {
            foreach(RefreshBase re in refreshNpcDict.Values)
            {
                re.UnPause();
                re.OnLoadSave();
            }
        }
        else
        {

            refreshNpcDict = new Dictionary<string, RefreshBase>();
            for (int i = 0; i < SceneData.mRefGroupList.Count; i++)
            {
                SceneCfg.RefGroupCfg groupCfg = SceneData.mRefGroupList[i];

                RefreshBase refNpc = RefreshMgr.Create(groupCfg);

                refreshNpcDict.Add(groupCfg.groupFlag, refNpc);
            }

            refreshSaveData.Add(sceneFile, refreshNpcDict);
        }

        return;
    }

    public IEnumerator CoLoad(RoomCfg cfg, int sceneIdx = -1)
    {
        //先根据品质设置下
        QualityMgr.instance.OnChangeLevel();


        LoadingProgress = 0;

        Room room = new GameObject("room", typeof(Room)).GetComponent<Room>();
        room.Init(cfg);
        GameObject.DontDestroyOnLoad(Room.instance.gameObject);

        m_fightCamera = null;

        if (sceneIdx == -1)
        {

            if (cfg.sceneFileName.Count > CurSceneIdx)
                SceneData = RoomCfg.GetSceneCfg(cfg.sceneFileName[CurSceneIdx]);
            else
                SceneData = RoomCfg.GetSceneCfg("");
        }
        else
        {
            SceneData = RoomCfg.GetSceneCfg(cfg.sceneFileName[CurSceneIdx]);
        }

        //预加载
        {
            GameObjectPool.CheckPreLoading = true;//标记下，用于检查有没有漏预加载的地方

            //加载下相机的径向模糊特效
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_camera_blur_internal");
            
            //加载怪物
            LoadRole();
            //加载特效
            LoadEffect();
            //加载刷新点 刷新点里有角色id 会加载角色
            LoadRefresh();
            //加载事件
            SceneEventMgr.instance.LoadEvent(CurSceneIdx);

            //加载剧情
            yield return Main.instance.StartCoroutine(StoryMgr.instance.LoadMovie());
            
            LoadingProgress = 30;
            yield return Main.instance.StartCoroutine(LevelMgr.instance.CurLevel.OnLoad());

            //记录进度
            int resCount = GameObjectPool.GetPool(GameObjectPool.enPool.Role).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Other).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Fx).RequestCount + SoundMgr.instance.RequestCount;
            if (resCount > 0)
            {
                //预加载
                while (!GameObjectPool.GetPool(GameObjectPool.enPool.Role).IsDone || !GameObjectPool.GetPool(GameObjectPool.enPool.Other).IsDone
                    || !GameObjectPool.GetPool(GameObjectPool.enPool.Fx).IsDone || !SoundMgr.instance.IsDone)
                {
                    int curNum = GameObjectPool.GetPool(GameObjectPool.enPool.Role).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Other).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Fx).RequestCount;
                    LoadingProgress = 30 + (int)((float)(1 - curNum / resCount) * 70);
                    yield return 1;
                }
            }

            LoadingProgress = 100;
            GameObjectPool.CheckPreLoading = false;//标记下，用于检查有没有漏预加载的地方
        }

        if (Room.instance != null)
        {
            Room.instance.OnUpdateEventCallback += UpdateCameraOffset;
            Room.instance.OnUpdateEventCallback += UpdateRefreshNpc;
        }

        //播放背景音乐
        if (cfg.bgmId != 0)
            SoundMgr.instance.Play2DSound(Sound2DType.bgm, cfg.bgmId);

        //创建全局敌人
        RoleMgr.instance.CreateGlobalEnemy(LevelMgr.instance.CurLevel.OnCreateGlobalEnemy());
        
        //初始化场景相机镜头组
        if (CameraTriggerMgr.instance != null)
        {
            CameraTriggerMgr caTriggerMgr = CameraTriggerMgr.instance;
            CameraTriggerGroup caTriggerGroup = null;

            string cameraStr = "";
            if (cfg.cameraGroupName.Count > CurSceneIdx)
                cameraStr = cfg.cameraGroupName[CurSceneIdx];

            if (!string.IsNullOrEmpty(cameraStr))
                caTriggerGroup = caTriggerMgr.GetGroupByName(cameraStr);
            else
                caTriggerGroup = caTriggerMgr.CurGroup;

            CameraTriggerMgr.instance.CurGroup = caTriggerGroup;
        }
        
        mCurState = SceneState.LoadEnd;

    }

    #endregion

    //关卡内切换场景
    public IEnumerator CoChangeScene(string sceneName)
    {
        RoomCfg cfg = LevelMgr.instance.CurLevel.roomCfg;

        PrevSceneIdx = CurSceneIdx;
        CurSceneIdx = -1;
        for (int i = 0; i < cfg.sceneFileName.Count; i++)
        {
            if (cfg.sceneFileName[i] == sceneName)
                CurSceneIdx = i;
        }

        if (CurSceneIdx == -1)
        {
            CurSceneIdx = 0;
            PrevSceneIdx = 0;
            Debug.LogError("没有找到相应配置roomid:" + cfg.id + "sceneName:" + sceneName);
            yield break;
        }

        if (cfg.sceneFileName.Count < CurSceneIdx || cfg.sceneId.Count < CurSceneIdx)
        {
            Debuger.Log("配置错误，json文件或场景文件个数少了");
            yield break;
        }

        string log = "";
        TimeCheck check = new TimeCheck();
        

        TimeMgr.instance.AddPause();
        
        //处理界面显示
        UIMgr.instance.CloseAll();
        UILoading uiLoding = UIMgr.instance.Open<UILoading>();
        uiLoding.tipsIdList = cfg.GetLoadingTipsId();
        uiLoding.SetBgImg(cfg.loadingBg);
        uiLoding.SetProgress(0,10);
        yield return 0;

        //场景逻辑销毁
        OnSaveExit(CurSceneIdx);
        LevelMgr.instance.CurLevel.OnLeave();
        yield return 0;

        // 角色销毁
        CombatMgr.instance.ClearFlyers();
        CombatMgr.instance.ClearEventGroup();
        RoleMgr.instance.DestroyAllRole(false,false);
        yield return 3;
        RoleMgr.instance.ShowAllRole(false);//将没有销毁的角色隐藏
        yield return 3;
        FxDestroy.Clear();
        BehaviorTreeMgr.instance.PauseAll();//暂停ai
        BehaviorTreeMgr.instance.Lock();//检错下，防止有逻辑在切换关卡的过程中
        //检查下剩下什么
        //string log = string.Format("主角剩余的监听\n{0}", RoleMgr.instance.Hero.Notifier.LogObservers());
        //Debuger.Log(log);
        yield return 3;//等待多几帧，因为对象池有可能要隔多帧才能完全回收对象

        //有过场预置体需要释放
       // TimeCheck checkStep1 = new TimeCheck();
        Resources.UnloadUnusedAssets();
       // log += string.Format("UnloadUnusedAssets耗时:{0}秒\n", checkStep1.delay);
        yield return 0;

        //TimeCheck checkStep2 = new TimeCheck();
        PoolMgr.instance.GCCollect();//垃圾回收下
        // log += string.Format("垃圾回收耗时:{0}秒\n", checkStep2.delay);
        yield return 2;

        uiLoding.SetProgress(10, 60);
        // 场景切换
        //TimeCheck checkStep3 = new TimeCheck();
        AsyncOperation op = SceneManager.LoadSceneAsync(cfg.sceneId[CurSceneIdx]);
        if(op == null)
        {
            Debuger.LogError("场景未成功加载，请点击菜单里Tool->打包->设置需要加载的场景");
        }
        while (!op.isDone)
        {
            uiLoding.m_progress = 10f + 60f * op.progress;
            yield return 0;
        }
            
       // log += string.Format("加载场景耗时:{0}秒\n", checkStep3.delay);
        BehaviorTreeMgr.instance.Unlock();//解除检错，防止有逻辑在切换关卡的过程中
        BehaviorTreeMgr.instance.PlayAll();
        yield return 0;

        //TimeCheck checkStep4 = new TimeCheck();
        uiLoding.SetProgress(60, 100);
        Main.instance.StartCoroutine(CoLoad(cfg, CurSceneIdx));
        while (LoadingProgress < 100)
        {
            uiLoding.m_progress = 60f+ 40f * (LoadingProgress/100);
            yield return 0;
        }


        //log += string.Format("预加载耗时:{0}秒\n", checkStep4.delay);
        uiLoding.SetProgress(100, 100);
        while (uiLoding.CurProgress < 100)
            yield return 0;
        TimeCheck checkStep5 = new TimeCheck();
        TimeMgr.instance.ResetPause();
        LevelMgr.instance.CurLevel.OnEnterAgain();
        LevelMgr.instance.bEditorLoaded = false;
        OnStart();
        PoolMgr.instance.GCCollect();//垃圾回收下
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.START, cfg.id);
        //log += string.Format("初始化时的逻辑耗时:{0}秒\n", checkStep5.delay);

        log = string.Format("关卡内切换场景耗时:{0}秒", check.delay);//+log
        Debuger.Log(log);
        
        uiLoding.Close();
        
    }

    public SceneCfg.BornInfo GetNewBornInfo()
    {
        if (SceneData.mBornList.Count >= 1)
        {
            mBornInfo = SceneData.mBornList[Random.Range(0, SceneData.mBornList.Count)];
        }
        return mBornInfo;
    }

    public SceneCfg.BornInfo GetBornInfo()
    {
        return SceneData.mBornList.Count <= 0 ? null : SceneData.mBornList[0];
    }

    public void OnStart()
    {
        //开始关卡逻辑
        SceneEventMgr.instance.StartEvent();
        if (DebugUI.instance != null)
            SceneEventMgr.instance.SetAllEventRunOrStop(DebugUI.instance.bRunLogic);

    }

    #region Level Interface

    //获取到当前刷新怪物的波数  如果刷怪有延迟 会获取到怪物最大刷新的波数
    public int GetRefreshNum(string groupFlag)
    {
        RefreshBase refreshGroup = null;
        int num = 0;
        if (refreshNpcDict.TryGetValue(groupFlag, out refreshGroup))
            num = refreshGroup.waveNum;

        return num;
    }

    //获取挡板
    public GameObject GetDangban(string dangbanId)
    {
        GameObject go = null;
        if (mDangbanDict.TryGetValue(dangbanId, out go))
            return go;

        SceneCfg.AreaCfg areaCfg = GetAreaCfgById(dangbanId);
        if (areaCfg == null)
            return null;

        go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(SceneCfg.DangbanName[(int)areaCfg.dangbanType], false);
        if (go != null)
        {
            go.transform.SetParent(Room.instance.mAreaGroup.transform, false);

            if (areaCfg == null)
            {
                GameObject.Destroy(go);
            }
            else
            {
                go.transform.position = areaCfg.pos;
                go.transform.localScale = areaCfg.size;
                //go.transform.localScale = Vector3.one;
                go.transform.rotation = Quaternion.Euler(areaCfg.dir);
                LayerMgr.instance.SetLayer(go, enGameLayer.obstructCollider);
                

                BoxCollider collider = go.GetComponent<BoxCollider>();
                if (collider == null)
                    collider = go.AddComponent<BoxCollider>();
                collider.isTrigger = false;
                collider.enabled = true;

                //go.gameObject.SetActive(areaCfg.bActivate);
                go.gameObject.SetActive(false);

                mDangbanDict[dangbanId] = go;
            }

        }
        return go;
    }

    public SceneCfg.AreaCfg GetAreaCfgById(string areaId)
    {
        for (int i = 0; i < SceneData.mAreaList.Count; i++)
        {
            if (SceneData.mAreaList[i].areaFlag == areaId)
                return SceneData.mAreaList[i];
        }

        return null;
    }

    void UpdateRefreshNpc()
    {
        if (IsRefreshNpc)
        {
            foreach (RefreshBase rn in refreshNpcDict.Values)
                rn.Update();
        }
    }

    //关卡有敌人时会更新相机偏移 让相机对准敌人和主角之间
    public void UpdateCameraOffset()
    {
        if (TimeMgr.instance.IsPause) return;

        if (TimeMgr.instance.logicTime - m_lastUpdate < 2)
            return;

        m_lastUpdate = TimeMgr.instance.logicTime;
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.transform != null && m_fightCamera != null)
        {
            Vector3 heroPos = hero.transform.position;
            RoleMgr.instance.GetCloseTargets(hero, enSkillEventTargetType.enemy, ref m_autoFaceTem);
            int count = m_autoFaceTem.Count;
          
            if (count >= 1)
            {
                float dis = m_cameraRoundDis * m_cameraRoundDis;
                float dx = heroPos.x;
                float dz = heroPos.z;
                int num = 1;
                for (int i =0; i < count; i++)
                {
                    if (SceneMgr.SceneDebug)
                    {
                        if (i == 0)
                            Debug.Log("最近的怪与主角的距离" + Vector3.Distance(m_autoFaceTem.Values[i].transform.position, heroPos));
                    }
                    if (m_autoFaceTem.Keys[i] < dis)
                    {
                        dx += m_autoFaceTem.Values[i].transform.position.x;
                        dz += m_autoFaceTem.Values[i].transform.position.z;
                        num++;
                    }
                }
                CameraMgr.instance.m_lookPosOffset = (new Vector3(dx / num, heroPos.y, dz / num)) - heroPos;
                CameraMgr.instance.m_disRate = m_cameraDisRate;
                return;
            }
        }

        CameraMgr.instance.m_lookPosOffset = Vector3.zero;
        CameraMgr.instance.m_disRate = 1;
        return;

    }

    public RefreshBase GetRefreshNpcByFlag(string flag)
    {
        RefreshBase refresh = null;
        refreshNpcDict.TryGetValue(flag, out refresh);
        return refresh;
    }

    public void StartFind(Vector3 findPos)
    {
        m_findPos = findPos;
        m_dirShow = true;
    }

    public void OverFind()
    {
        m_dirShow = false;
    }
    
    public void SetFightCamera(float disRate, float roundDis)
    {
        m_cameraDisRate = disRate;
        m_cameraRoundDis = roundDis;
    }

    #endregion

    public void OnExit()
    {
        //销毁挡板特效不能放进对象池 不然会有bug  编辑关卡在保存配置文件时 有些挡板可能是隐藏状态 如果是从对象池里获取到的挡板 隐藏的位置会有问题
        foreach (string key in mDangbanDict.Keys)
            GameObject.Destroy(mDangbanDict[key]);

        mDangbanDict.Clear();

        foreach(Dictionary<string, RefreshBase> reDic in refreshSaveData.Values)
            reDic.Clear();

        refreshSaveData.Clear();
        SceneEventMgr.instance.OnExit();
        StoryMgr.instance.OnExit();

        PrevSceneIdx = 0;
        CurSceneIdx = 0;
        
        m_arrowShow = false;
        m_dirShow = false;
        IsRefreshNpc = true;

        if (Room.instance != null)
        {
            var roomId = Room.instance.roomCfg.id;
            Room.instance.OnUpdateEventCallback -= UpdateRefreshNpc;
            Room.instance.OnUpdateEventCallback -= UpdateCameraOffset;
            GameObject roomGo = Room.instance.gameObject;
            Room.instance.Exit();
            EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, roomId);
            GameObject.Destroy(roomGo);
        }
    }

    //保存关卡相关数据退出
    public void OnSaveExit(int idx)
    {
        foreach (string key in mDangbanDict.Keys)
            GameObject.Destroy(mDangbanDict[key]);

        mDangbanDict.Clear();

        foreach (RefreshBase re in refreshNpcDict.Values)
        {
            re.OnSaveExit();
            re.Pause();
        }

        SceneEventMgr.instance.OnSaveExit();
        StoryMgr.instance.OnExit();

        m_arrowShow = false;
        m_dirShow = false;

        if (Room.instance != null)
        {
            var roomId = Room.instance.roomCfg.id;
            GameObject roomGo = Room.instance.gameObject;
            Room.instance.Exit();
            EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, roomId);
            GameObject.Destroy(roomGo);
        }
    }
    
}
