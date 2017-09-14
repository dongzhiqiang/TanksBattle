#region Header
/**
 * 名称：主逻辑
 
 * 日期：2015.9.16
 * 描述：主要负责进入游戏时各个系统初始化，以及跳转到登录态
 **/
#endregion
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;



public class Main : MonoBehaviour {
    public static Main instance;
    public bool isSingle = false;
    public Material[] preLoadMats ;//预加载的材质球

    void Awake()
    {
        instance = this;

        //阻止手机进入休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        

        
        //可能品质设置有问题，简单检查下
        if(QualitySettings.antiAliasing !=0)
        {
            Debuger.LogError("抗锯齿没有关闭，可能quality设置有问题");
            QualitySettings.antiAliasing = 0;
        }

        //Main永远都不销毁
        DontDestroyOnLoad(this.gameObject);//过场景保留
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(CoInit());
	}
	
	// Update is called once per frame
	void Update () 
    {
#if UNITY_EDITOR
        //录屏
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (CaptureAvi.instance.IsCapturing())
                CaptureAvi.instance.StopCapture();
            else
                CaptureAvi.instance.StartCapture();
        }
#endif
    }


    IEnumerator CoInit()
    {
        float beginTime = Time.realtimeSinceStartup;

#if ART_DEBUG
        Debuger.LogError("ART_DEBUG 宏没有去掉，不能运行游戏");
        yield break;
#endif
        
        //本地日志，先启动,内部会监听所有的Debug.log
        LogUtil.Init();
        QualityMgr.instance.Init();
        var asyncOpUI = UIMgr.instance.Init();
        var asyncOpCfg = CfgMgr.instance.Init();
        UILoading uiLoding = UIMgr.instance.Open<UILoading>();
        const int uiTotalProgress = 80;
        uiLoding.SetProgress(0, uiTotalProgress);
        while (!asyncOpUI.isDone || !asyncOpCfg.isDone)
        {
            uiLoding.SetProgress((asyncOpUI.progress+ asyncOpCfg.progress)*0.5f* uiTotalProgress, uiTotalProgress);
            yield return 0;
        }
        uiLoding.SetProgress(uiTotalProgress);
        yield return 0;
        
        //管理器初始化
        DebugUI.instance.Init();
        uiLoding.ChangeTips();
        uiLoding.SetProgress(85);
        yield return 0;
        LevelMgr.instance.Init();
        ActivityMgr.instance.Init();
        RankMgr.instance.Init();
        NetMgr.instance.Init();
        SoundMgr.instance.Init();
        TeachMgr.instance.Init();

        uiLoding.SetProgress(100);
        while (uiLoding.CurProgress < 100)
            yield return 0;

        Debuger.Log(string.Format("初始化耗时{0:F2}秒", Time.realtimeSinceStartup - beginTime));

        if (!isSingle)
        {
            //进入登录态
            PlayerStateMachine.Instance.GotoState(enPlayerState.login);
        }
        else
        {
#region 单机测试
            FullRoleInfoVo roleVo    = new FullRoleInfoVo();

            var props = new Dictionary<string, Property>();
            props["guid"] = new Property(Util.GenerateGUID());
            props["roleId"] = new Property("kratos");
            props["name"] = new Property("单机测试");
            props["level"] = new Property(35);
            props["stamina"] = new Property(0);
            roleVo.props = props;

            var equips = new List<EquipVo>();
            equips.Add(new EquipVo(11000, 1, 1));
            equips.Add(new EquipVo(11100, 1, 1));
            equips.Add(new EquipVo(11200, 1, 1));
            equips.Add(new EquipVo(11300, 1, 1));
            equips.Add(new EquipVo(12000, 1, 1));
            equips.Add(new EquipVo(13000, 1, 1));
            equips.Add(new EquipVo(14000, 1, 1));
            equips.Add(new EquipVo(15000, 1, 1));
            equips.Add(new EquipVo(16000, 1, 1));
            equips.Add(new EquipVo(17000, 1, 1));
            roleVo.equips = equips;

            var weapons = new WeaponInfoVo();
            roleVo.weapons = weapons;

            ////创建宠物
            FullRoleInfoVo petVo = new FullRoleInfoVo();

            props = new Dictionary<string, Property>();
            props["guid"] = new Property(Util.GenerateGUID());
            props["roleId"] = new Property("cw_1");
            props["name"] = new Property("单机测试");
            props["level"] = new Property(1);
            props["advLv"] = new Property(1);
            props["star"] = new Property(1);
            petVo.props = props;

            equips = new List<EquipVo>();
            equips.Add(new EquipVo(21000, 1, 1));
            equips.Add(null);
            equips.Add(null);
            equips.Add(null);
            equips.Add(new EquipVo(22000, 1, 1));
            equips.Add(new EquipVo(23000, 1, 1));
            equips.Add(new EquipVo(24000, 1, 1));
            equips.Add(new EquipVo(25000, 1, 1));
            equips.Add(new EquipVo(26000, 1, 1));
            equips.Add(new EquipVo(27000, 1, 1));
            petVo.equips = equips;

            //var pets = new List<FullRoleInfoVo>(new FullRoleInfoVo[] { petVo });
            //roleVo.pets = pets;

            //创建英雄
            RoleMgr.instance.CreateHero(roleVo);
            if (isSingle)
            {
            }
            //进入主城
            LevelMgr.instance.GotoMaincity();
#endregion
        }
        uiLoding.Close();
    }

    
    void OnDisable()
    {
        instance = null;
    }

    void OnApplicationQuit()
    {
        NetMgr.instance.Dispose();
        LogUtil.Close();
    }

    void Test(Action a)
    {
        var t1 = DateTime.Now;
        a();
        var t2 = DateTime.Now;
        Debuger.Log("耗时:{0:f0} ", (t2 - t1).TotalMilliseconds);
        
    }

    void Test2(int i)
    {

    }

    //public int count = 50000;
    //public float t = 100;

    [ContextMenu("Test1")]
    void Test1()
    {
       
        Test(()=> {
            
            Action<int> a = Test2;
            Debuger.Log(Util.GetDelegateName(a));
            Action<int> b=(int param)=> { };
            Debuger.Log(Util.GetDelegateName(b));
        });
        
    }
    
    
}
