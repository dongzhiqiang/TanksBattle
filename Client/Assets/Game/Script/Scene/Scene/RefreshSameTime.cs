using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshSameTime : RefreshBase
{

    public override void Start()
    {
        mState = RefreshState.RUN;
        startTime = TimeMgr.instance.logicTime;
        waveNum = 0;
        runTime = 0;

        foreach (RefreshPoint refPoint in refPointList)
        {
            refPoint.Init(startTime);
        }
    }

    public override void Update()
    {
        base.Update();
        if (mState != RefreshState.RUN)
            return;

        bool isFinish = true;
        foreach (RefreshPoint refPoint in refPointList)
        {
            refPoint.Update();
            if (!refPoint.isFinish)
                isFinish = false;
        }

        //刷新点都刷完 停止刷新组
        if (isFinish)
        {
            //连续刷新下一波
            if (!string.IsNullOrEmpty(mGroupCfg.nextGroupFlag))
            {
                Room.instance.StartCoroutine(CoStartRefresh(mGroupCfg.nextGroupFlag, mGroupCfg.nextWaveDelay));
            }
            Stop();
        }
       
    }

}
