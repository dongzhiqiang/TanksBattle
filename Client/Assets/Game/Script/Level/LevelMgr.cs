using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Simple.BehaviorTree;


//关卡战斗方式
public enum LevelBattleType
{
    Custom_AutoBattle = 1,        //自己控制是否挂机
    Cant_AutoBattle = 2,        //不可挂机
    Must_AutoBattle = 3,        //只能挂机
}

//初始挂机状态
public enum LevelAutoBattleState
{
    AutoBattle = 1,            //挂机
    NOAutoBattle = 2,            //不挂机
    LastAutoBattle = 3,            //使用上次挂机
}

public class LevelMgr : Singleton<LevelMgr>
{
    #region Fields
    
    public static string MainRoomID = "100000";
    public static string AutoBattleFlag = "isAutoBattle";

    public bool bEditorLoaded = false;
    public LevelBase m_curLevel = null;

    //关卡战斗方式
    public LevelBattleType levelBattleType = LevelBattleType.Cant_AutoBattle;
    //初始挂机状态
    public LevelAutoBattleState levelBattleState = LevelAutoBattleState.LastAutoBattle;

    //关卡中战斗力修正系数
    float m_powerHitFactor = 1;
    float m_powerBeHitFactor = 1;

    List<string> m_prevRoomIds = new List<string>();

    //关卡中的ai相关标记
    public Dictionary<string, int> LevelFlag = new Dictionary<string, int>();
    #endregion

    #region Properties
    //public int curProgress { get; set; }
    public LevelBase CurLevel { get { return m_curLevel; } set { m_curLevel = value; } }
    public string PrevRoomId { get { if (m_prevRoomIds.Count <= 0) return MainRoomID; else  return m_prevRoomIds[m_prevRoomIds.Count - 1];} }
    public float PowerHitFactor { get { return m_powerHitFactor; } set { m_powerHitFactor = value; } }
    public float PowerBeHitFactor { get { return m_powerBeHitFactor; } set { m_powerBeHitFactor = value; } }

    #endregion

    public System.Action<Role> OnRoleDeadCallback;
    public System.Action<Role> OnRoleDeadStartCallback;
    public System.Action<Role> OnRoleDeadEndCallback;
    public System.Action<Role> OnRoleBornCallback;

    #region Private Methods
    IEnumerator CoChangeLevel(string roomID, object param = null)
    {
        //yield return DebugUI.Step(1);
        //重置战力修正系数
        PowerHitFactor = 1;
        PowerBeHitFactor = 1;
        RoomCfg cfg = RoomCfg.GetRoomCfgByID(roomID);
        if (cfg == null)
        {
            Debuger.LogError("没有房间ID{0}", roomID);
            yield break;
        }

        if (CurLevel != null)
            m_prevRoomIds.Add(CurLevel.roomCfg.id);

        string log = "";
        TimeCheck check = new TimeCheck();

        OnRoleDeadCallback = null;
        OnRoleDeadStartCallback = null;
        OnRoleDeadEndCallback = null;
        OnRoleBornCallback = null;

        TimeMgr.instance.AddPause();
        // 界面销毁
        UIMgr.instance.CloseAll();
        UILoading uiLoding = UIMgr.instance.Open<UILoading>();
        {
            //yield return DebugUI.Step(2);
            if (CameraMgr.instance!= null)
                CameraMgr.instance.CurCamera.enabled = false;
            uiLoding.tipsIdList = cfg.GetLoadingTipsId();
            uiLoding.SetBgImg(cfg.loadingBg);
            uiLoding.SetProgress(0, 10);
            yield return 0;

            //yield return DebugUI.Step(3);
            // 关卡逻辑销毁
            SceneMgr.instance.OnExit();
            if (CurLevel != null)
            {
                CurLevel.OnExit();
                CurLevel.mRoleDic.Clear();
            }
            yield return 0;

            //yield return DebugUI.Step(4);
            TimeCheck checkStep0 = new TimeCheck();
            // 角色逻辑销毁
            CombatMgr.instance.ClearFlyers();
            CombatMgr.instance.ClearEventGroup();
            RoleMgr.instance.DestroyAllRole(true,false);
            yield return 3;//等待多几帧，因为对象池有可能要隔多帧才能完全回收对象
            //string log = string.Format("主角剩余的监听\n{0}", RoleMgr.instance.Hero.Notifier.LogObservers()); //检查下监听会不会剩下什么
            FxDestroy.Clear();
            BehaviorTreeMgr.instance.Clear();
            BehaviorTreeMgr.instance.Lock();
            CombatMgr.instance.ClearCombatRecord();//清空战斗记录
            yield return 3;
            //SoundMgr.instance.Clear();
            PoolMgr.instance.Clear();//清空对象池,对象的销毁反而会造成mono的增大
            log += string.Format("清空操作耗时:{0}秒\n", checkStep0.delay);
            yield return 3;

            //yield return DebugUI.Step(5);
            uiLoding.SetProgress(5, 10);
            TimeCheck checkStepDeleteOld = new TimeCheck();
            //先手动删掉老场景，不然异步加载的过程中会存在两个场景的资源，内存占用比较大
            var scene = SceneManager.GetActiveScene();
            GameObject[] rootGos = scene.GetRootGameObjects();
            foreach (var go2 in rootGos)
            {
                if (go2.name == "Main" || go2.name == "UIRoot" || go2.name == "DontDestroyOnLoad")
                    continue;

                GameObject.Destroy(go2);
            }
            GameObject go = new GameObject("temp audio listener", typeof(AudioListener));
            log += string.Format("卸载老场景耗时:{0}秒\n", checkStepDeleteOld.delay);
            yield return 2;

            uiLoding.SetProgress(10, 10);
            //yield return DebugUI.Step(6);
            // 资源回收
            TimeCheck checkStep1 = new TimeCheck();
            Resources.UnloadUnusedAssets();//卸载没有用的资源
            log += string.Format("UnloadUnusedAssets耗时:{0}秒\n", checkStep1.delay);
            yield return 2;
            TimeCheck checkStep2 = new TimeCheck();
            PoolMgr.instance.GCCollect();//垃圾回收下
            log += string.Format("垃圾回收耗时:{0}秒\n", checkStep2.delay);
            yield return 2;

            //yield return DebugUI.Step(7);
            // 切换场景
            TimeCheck checkStep3 = new TimeCheck();
            uiLoding.SetProgress(10, 60);
            AsyncOperation op = SceneManager.LoadSceneAsync(cfg.sceneId[0]);
            log += string.Format("加载场景卡住耗时:{0}秒\n", checkStep3.delay);
            TimeCheck checkStep33 = new TimeCheck();
            while (!op.isDone)
            {
                uiLoding.m_progress = 10f + 60f * op.progress;
                yield return 1;
            }
            log += string.Format("加载等待场景耗时:{0}秒\n", checkStep33.delay);
            BehaviorTreeMgr.instance.Unlock();
            yield return 0;

            //yield return DebugUI.Step(8);
            TimeCheck checkStep4 = new TimeCheck();
            uiLoding.SetProgress(60, 100);
            CurLevel = SceneFactory.GetScene(cfg);
            CurLevel.mParam = param;
            CurLevel.roomCfg = cfg;
            LevelFlag.Clear();
            foreach(string flag in cfg.levelFlag)
            {
                LevelFlag.Add(flag, 1);
            }
            CurLevel.mRoleDic.Clear();
            Main.instance.StartCoroutine(SceneMgr.instance.CoLoad(cfg));
            while (SceneMgr.LoadingProgress < 100)
            {
                uiLoding.m_progress = 60f + 40f * (SceneMgr.LoadingProgress / 100);
                yield return 0;
            }
            log += string.Format("预加载耗时:{0}秒\n", checkStep4.delay);

            //根据推荐战力计算关卡伤害系数
            if (cfg.powerNum != -1)
            {
                float power1 = (float)RoleMgr.instance.Hero.GetInt(enProp.powerTotal) - cfg.powerNum;
                float power2 = (float)RoleMgr.instance.Hero.GetInt(enProp.powerTotal) / cfg.powerNum;
                PowerHitFactor = PowerFactorCfg1.GetHitFactor(power1) + PowerFactorCfg2.GetHitFactor(power2);
                PowerBeHitFactor = PowerFactorCfg1.GetBeHitFactor(power1) + PowerFactorCfg2.GetBeHitFactor(power2);
            }

            bEditorLoaded = false;
        }

        //yield return DebugUI.Step(9);
        TimeCheck checkStep5 = new TimeCheck();
        uiLoding.SetProgress(100, 100);
        while (uiLoding.CurProgress < 100)
            yield return 0;
        TimeMgr.instance.ResetPause();
        CurLevel.OnLoadFinish();
        CurLevel.State = LevelState.Runing;
        while (!CurLevel.IsCanStart())//等待开始关卡逻辑
            yield return 0;
        SceneMgr.instance.OnStart();
        //yield return DebugUI.Step(10);
        PoolMgr.instance.GCCollect();//垃圾回收下
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.START, cfg.id);
        log += string.Format("初始化时的逻辑耗时:{0}秒\n", checkStep5.delay);
        
        
        uiLoding.Close();
        yield return 1;//等两帧，场景相机看到场景后也会有可马上清理的内存的增长
        TimeCheck checkStep6 = new TimeCheck();
        PoolMgr.instance.GCCollect();//垃圾回收下
        log += string.Format("看到场景后的垃圾回收:{0}秒\n", checkStep6.delay);
        log = string.Format("切换关卡总耗时{0}秒\n{1}", check.delay, log);
        Debuger.Log(log);
    }


    void OnRoleDead(object param, object param2, object param3, EventObserver observer)
    {

        Role role = observer.GetParent<Role>();
        if (role == null)
            return;

        bool isNow = (bool)param;

        CurLevel.OnRoleDead(role, isNow);

        if (isNow)
        {
            RemoveRole(role);
            //马上死亡时 不会触发DEAD_END事件 这里要调一下
            if (OnRoleDeadEndCallback != null)
                OnRoleDeadEndCallback(role);
        }
        //无论是不是马上死忘 都调用dead start
        if (OnRoleDeadStartCallback != null)
            OnRoleDeadStartCallback(role);

        return;
    }

    //死亡消息不一定飞魂值 但有死亡效果播完后肯定飞魂
    void OnRoleDeadEnd(object param, object param2, object param3, EventObserver observer)
    {
        Role role = observer.GetParent<Role>();
        if (role == null)
            return;

        RemoveRole(role);
        CurLevel.OnRoleDeadEnd(role);

        if (OnRoleDeadEndCallback != null)
            OnRoleDeadEndCallback(role);

    }

    void AddRole(Role role)
    {
        CurLevel.mRoleDic.Add(role.Id, role);
        CurLevel.OnRoleEnter(role);
        if (OnRoleBornCallback != null)
            OnRoleBornCallback(role);
        role.Add(MSG_ROLE.DEAD, OnRoleDead);
        role.Add(MSG_ROLE.DEAD_END, OnRoleDeadEnd);

        SceneMgr.instance.UpdateCameraOffset();
    }

    void RemoveRole(Role role)
    {
        CurLevel.RemoveRole(role);
        //调用一些事件监听的回调
        if (OnRoleDeadCallback != null)
            OnRoleDeadCallback(role);

        SceneMgr.instance.UpdateCameraOffset();
    }

    #endregion

    public void Init()
    {
        UIMainCity.AddClick(enSystem.scene, () =>
        {
            //打开关卡界面
            UIMgr.instance.Open<UILevelSelect>();
        });
    }
 
    //关卡里面创建主角 会一起创建宠物设置相机
    public IEnumerator CreateHero(RoleBornCxt cxt, string camera = null)
    {
        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
        {
            Debuger.LogError("主角不存在，无法创建");
            yield break;
        }

        //获取镜头参数

        if (cxt == null)
        {
            Debug.Log("主角信息传入错误");
            yield break;
        }

        //创建主角
        hero.Load(cxt);
        while (hero.State != Role.enState.alive)
        {
            yield return 0;
        }

        //设置相机镜头
        if (string.IsNullOrEmpty(camera))
            CameraMgr.instance.SetFollow(hero.transform, true);//如果没有别的镜头，那么重新设置下跟随就可以了
        else
        {
            CameraMgr.instance.SetFollow(hero.transform);
            CameraTrigger trigger = CameraTriggerMgr.instance.CurGroup.GetTriggerByName(camera);
            if (trigger != null)
            {
                CameraMgr.instance.Add(trigger.m_info);
            }
            else
                Debuger.LogError("英雄切换镜头失败:{0}", camera);
        }

        SetAutoBattle();


        //主角也加入角色列表里
        AddRole(hero);
        CurLevel.OnHeroEnter(hero);
        //广播创建消息
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, hero);

        //主角进入后再显示触发区域 不然可能出现bug
        Room.instance.mAreaGroup.gameObject.SetActive(true);
    }

    //同一关卡切换场景时恢复角色
    public void ResetHero(RoleBornCxt cxt, string camera = null)
    {
        //之前没有销毁的角色要显示
        
        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
        {
            Debuger.LogError("主角不存在，无法创建");
            return;
        }

        //获取镜头参数

        if (cxt == null)
        {
            Debug.Log("主角信息传入错误");
            return;
        }

        //if(hero.State != Role.enState.alive)
        //{
        //    Debug.Log("主角死了");
        //    return;
        //}

        //出生点出生方向
        hero.TranPart.SetPos(PosUtil.CaleByTerrains(cxt.pos));
        hero.TranPart.SetDir(cxt.euler);

        hero.RoleModel.Show(true);
        hero.RSM.GotoState(enRoleState.born, new RoleStateBornCxt(cxt.bornAniId,true));
        
        //设置相机镜头
        if (string.IsNullOrEmpty(camera))
            CameraMgr.instance.SetFollow(hero.transform, true);//如果没有别的镜头，那么重新设置下跟随就可以了
        else
        {
            CameraMgr.instance.SetFollow(hero.transform);
            CameraTrigger trigger = CameraTriggerMgr.instance.CurGroup.GetTriggerByName(camera);
            if (trigger != null)
            {
                CameraMgr.instance.Add(trigger.m_info);
            }
            else
                Debuger.LogError("英雄切换镜头失败:{0}", camera);
        }
        
        //主角进入后再显示触发区域 不然可能出现bug
        Room.instance.mAreaGroup.gameObject.SetActive(true);
    }

    public IEnumerator CreateNetRole(FullRoleInfoVo vo, RoleBornCxt cxt, RoleBornCxt cxt1, RoleBornCxt cxt2)
    {
        Role role = RoleMgr.instance.CreateNetRole(vo, true, cxt);
        while (role.State != Role.enState.alive)
            yield return 0;
        
        //主角也加入角色列表里
        AddRole(role);
        //广播创建消息
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, role);
    }

    public Role CreateRole(RoleBornCxt cxt, string groupFlag = "", string pointFlag = "")
    {
        Role role = RoleMgr.instance.CreateRole(cxt);
        if (!string.IsNullOrEmpty(groupFlag))
            role.SetFlag(groupFlag);
        if (!string.IsNullOrEmpty(pointFlag))
            role.SetFlag(pointFlag);

        AddRole(role);

        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, role);

        return role;
    }

    public void SetWin()
    {
        if (CurLevel.State == LevelState.End)
        {
            Debug.LogError("重复设置输赢结果");
            return;
        }
        PoolMgr.instance.GCCollect();//垃圾回收下
        CurLevel.State = LevelState.End;
        

        SceneMgr.instance.IsRefreshNpc = false;
        SceneEventMgr.instance.SetAllEventRunOrStop(false);
        CurLevel.SendResult(true);
    }
    public void SetLose()
    {
        if (CurLevel.State == LevelState.End)
        {
            Debug.LogError("重复设置输赢结果");
            return;
        }
        PoolMgr.instance.GCCollect();//垃圾回收下
        CurLevel.State = LevelState.End;

        CurLevel.SendResult(false);
    }

    public void GotoMaincity()
    {
        ChangeLevel(MainRoomID);
    }

    public void ChangeLevel(string roomId, object param = null)
    {
        Main.instance.StartCoroutine(CoChangeLevel(roomId, param));
    }

    public bool IsMainCity()
    {
        return Room.instance != null && Room.instance.roomCfg.id == MainRoomID;
    }

    //设置战斗状态
    public void SetAutoBattle()
    {
        if (CurLevel.roomCfg.battleState.Length != 2)
            return;

        levelBattleType = (LevelBattleType)CurLevel.roomCfg.battleState[0];
        levelBattleState = (LevelAutoBattleState)CurLevel.roomCfg.battleState[1];

        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        //可自由选择是否挂机
        if (levelBattleType == LevelBattleType.Custom_AutoBattle)
        {
            if (levelBattleState == LevelAutoBattleState.AutoBattle)
                hero.AIPart.Play(AIPart.HeroAI);
            else if (levelBattleState == LevelAutoBattleState.NOAutoBattle)
                hero.AIPart.Stop();
            else if (levelBattleState == LevelAutoBattleState.LastAutoBattle)
            {
                int autoBattle = PlayerPrefs.GetInt(AutoBattleFlag, 0);
                if (autoBattle == 0)
                    hero.AIPart.Stop();
                else
                    hero.AIPart.Play(AIPart.HeroAI);
            }
        }
        else if (levelBattleType == LevelBattleType.Cant_AutoBattle)
        {
            hero.AIPart.Stop();
        }
        else if (levelBattleType == LevelBattleType.Must_AutoBattle)
        {
            hero.AIPart.Play(AIPart.HeroAI);
        }
    }
}