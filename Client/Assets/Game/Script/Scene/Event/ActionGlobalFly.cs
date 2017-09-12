using UnityEngine;
using System.Collections;

public class ActionGlobalFly : SceneAction
{
    public ActionCfg_GlobalFly mActionCfg;


    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_GlobalFly;

        //预加载
        mActionCfg.createCfg.PreLoad();
        if (!string.IsNullOrEmpty(mActionCfg.flyer))
            FlyerCfg.PreLoad(mActionCfg.flyer);
    }

    public override void OnAction()
    {
        Role globalEnemy = RoleMgr.instance.GlobalEnemy;

        if (globalEnemy == null)
        {
            Debuger.LogError("全局敌人不存在关卡不能放出飞出物");
            return;
        }
        mActionCfg.createCfg.Create(globalEnemy, null, globalEnemy.transform.position,
            enElement.none
            , OnLoad, new object[] { globalEnemy, null, null });

    }

    void OnLoad(GameObject go, object param)
    {
        object[] pp = (object[])param;
        Role source = (Role)pp[0];
        Role target = (Role)pp[1];
        Skill parentSkill = (Skill)pp[2];
        Flyer.Add(go, mActionCfg.flyer, source, target, parentSkill);

        //如果飞出物上没有任何销毁的脚本，那么提示下
        if (!FxDestroy.HasDelay(go))
        {
            Debuger.LogError("特效上没有绑销毁脚本，特效事件的特效也没有指定销毁时间.特效名:{0}", go.name);
        }
    }
}
