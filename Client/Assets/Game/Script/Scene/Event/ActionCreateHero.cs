
using UnityEngine;
using System.Collections;

public class ActionCreateHero : SceneAction
{

    public ActionCfg_CreateHero mActionCfg;

    bool bCreated;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_CreateHero;
    }

    public override void OnAction()
    {
        SceneCfg.SceneData mSceneData = SceneMgr.instance.SceneData;
        if (mSceneData.mBornList.Count <= 0 || SceneMgr.instance.PrevSceneIdx >= mSceneData.mBornList.Count)
        {
            Debug.LogError("有出生事件 没有配出生点 或者");
            return;
        }
        
        SceneCfg.BornInfo bornInfo = mSceneData.mBornList[SceneMgr.instance.PrevSceneIdx];
        //出生上下文
        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.OnClear();
        mSceneData.GetBornPosAndEuler(bornInfo, out cxt.pos, out cxt.euler);
        cxt.bornAniId = bornInfo.mBornTypeId;
        cxt.deadAniId = bornInfo.mDeadTypeId;
        cxt.groundDeadAniId = bornInfo.mGroundDeadTypeId;
        cxt.hate.CopyFrom(bornInfo.hate);

        //关卡里没有配置出生死亡则取表里默认的配置
        RoleCfg cfg = RoleMgr.instance.Hero.Cfg;
        if (string.IsNullOrEmpty(bornInfo.mBornTypeId))
            cxt.bornAniId = cfg.bornType;
        if (string.IsNullOrEmpty(bornInfo.mDeadTypeId))
            cxt.deadAniId = cfg.deadType;
        if (string.IsNullOrEmpty(bornInfo.mGroundDeadTypeId))
            cxt.groundDeadAniId = cfg.groundDeadType;

        Role hero = RoleMgr.instance.Hero;
        //如果有主角，就是切换时没有销毁
        if (LevelMgr.instance.CurLevel.IsHaveRole(hero))
            LevelMgr.instance.ResetHero(cxt, mActionCfg.changeCam);
        else
            Room.instance.StartCoroutine(LevelMgr.instance.CreateHero(cxt, mActionCfg.changeCam));

    }
}
