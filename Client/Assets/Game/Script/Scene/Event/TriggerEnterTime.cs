using UnityEngine;
using System.Collections;

public class TriggerEnterTime : SceneTrigger
{

    public EventCfg_EnterTime m_eventCfg;

    public float startTime = 0;
    public int second = 0;
    public bool bHaveFire = false;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_EnterTime;
    }

    public override bool bReach()
    {
        if (SceneMgr.SceneDebug)
        {
            if (second >= m_eventCfg.secNum)
                Debug.Log(string.Format("进入时间已经达到 {0}秒", m_eventCfg.secNum));
        }

        return second >= m_eventCfg.secNum;
    }

    public override void Start()
    {
        base.Start();
        startTime = TimeMgr.instance.logicTime;
    }

    public override void Update()
    {
        if (mRun)
        {
            if (bHaveFire)
            {
                return;
            }
            second = (int)(TimeMgr.instance.logicTime - startTime);
            if (second >= m_eventCfg.secNum)
            {
                bHaveFire = true;

                SceneEventMgr.instance.FireAction(mFlag);
            }
        }
    }

}
