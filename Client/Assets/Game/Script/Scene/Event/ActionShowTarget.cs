using UnityEngine;
using System.Collections;

public class ActionShowTarget : SceneAction
{
    public ActionCfg_ShowTarget mActionCfg;

    TimeMgr.TimeScaleHandle m_timeHandle;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowTarget;
    }

    public override void OnAction()
    {
        UIMgr.instance.Get<UILevel>().Get<UILevelAreaNum>().ClearNum();

        if (m_timeHandle != null && !m_timeHandle.IsOver)
            return;

        m_timeHandle = TimeMgr.instance.AddTimeScale(0, -1, 100);

        UIMgr.instance.Close<UILevel>();

        if (LevelMgr.instance.CurLevel == null)
            return;

        RoomCfg cfg = LevelMgr.instance.CurLevel.roomCfg;
        if (!string.IsNullOrEmpty(cfg.targetDesc))
        {
            UIMessageBox2.Open("任务目标", cfg.targetDesc, () => {
                if (m_timeHandle != null)
                {
                    m_timeHandle.Release();
                    m_timeHandle = null;
                }

                if (!UIMgr.instance.Get<UILevel>().gameObject.activeSelf)
                    UIMgr.instance.Open<UILevel>(true);
            });
        }
        else
        {
            Debuger.LogError("配置了任务目标 却没有描述 roomId : " + cfg.id);
            m_timeHandle.Release();
            m_timeHandle = null;
        }
    }
}