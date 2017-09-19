using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//主城场景
public class MainCityScene : LevelBase
{

    AniFxMgr heroAni;
    GameObject heroGo;

    static string Ani_zhuCheng_DaiJi_01 = "zhucheng_daiji01";
    static string Ani_zhuCheng_DaiJi_02 = "zhucheng_daiji02";
    static string Ani_zhuCheng_DaiJiGuoDu = "zhucheng_daijiguodu";
    static string Ani_zhuCheng_ShiNvDaiJi = "xiuxiandaiji";
    static string Ani_zhuCheng_DaiJi = "zhuchengdaiji";

    static bool isFirst = true;

    //能不能加非战斗状态
    public override bool CanAddUnaliveBuff { get { return true; } }

    public override IEnumerator OnLoad()
    {
        Role hero = RoleMgr.instance.Hero;
        //加载主角模型
        //GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(RoleMgr.instance.Hero.Cfg.mod, null, OnLoadHeroMod, false);
        yield return 0;
    }

    //场景切换完成
    public override void OnLoadFinish()
    {      
        //打开主城界面        
        UIMgr.instance.Open<UIMainCity>();

        //相机切到主角身上
        GameObject heroPos = GameObject.Find("heroPos");
        CameraTriggerMgr caTriggerMgr = CameraTriggerMgr.instance;
        CameraTriggerGroup caTriggerGroup = caTriggerMgr.CurGroup;
        CameraMgr.instance.SetFollow(heroPos.transform);
        if (CameraTriggerMgr.instance != null)
            CameraTriggerMgr.instance.CurGroup.SetGroupActive(true);

        if (isFirst)
        {
            isFirst = false;
            CameraMgr.instance.Add(CameraTriggerMgr.instance.CurGroup.Triggers[0].m_info);
        }
        else
        {
            CameraMgr.instance.Set(CameraTriggerMgr.instance.CurGroup.Triggers[0].m_info);
        }
        
        //回城弹窗
        CheckOpen();
    }
    //创建全局敌人的时候，返回全局敌人的阵营，如果不希望创建可以返回enCamp.max
    public override enCamp OnCreateGlobalEnemy() { return enCamp.max; }


    public override void OnExit()
    {

    }
    
    public override void OnUpdate()
    {
    }
    
    //检查一些回城弹窗
    void CheckOpen()
    {
        //TimeMgr.instance.AddTimer(1, CheckOpen2);
        CheckOpen2(); //策划决定不延时 有需求冲突那就是策划脑残
    }

    void CheckOpen2()
    {
      
    }
}
