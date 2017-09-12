using UnityEngine;
using System.Collections;

public class ActionEnterFightCamera : SceneAction
{
    public ActionCfg_EnterFight mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_EnterFight;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("进入战斗状态镜头");

        CameraTriggerGroup group = CameraTriggerMgr.instance.CurGroup;
        if (group == null)
        {
            Debug.LogError("战斗状态镜头组没找到");
            return;
        }

        CameraTrigger trigger = group.GetTriggerByName(mActionCfg.cameraName);
        if ( trigger == null)
        {
            Debug.LogError("战斗状态触发镜头没找到");
            return;
        }

        if (SceneMgr.instance.FightCamera != null)
            SceneMgr.instance.FightCamera.Release();

        SceneMgr.instance.FightCamera = CameraMgr.instance.Add(trigger.m_info);
        SceneMgr.instance.SetFightCamera(mActionCfg.disRate, mActionCfg.roundDis);
    }
}