using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshRandomNum : RefreshBase
{
    List<RefreshPoint> curPointList = new List<RefreshPoint>();
    List<int> idxList = new List<int>();

    public override void Start()
    {
        mState = RefreshState.RUN;
        startTime = TimeMgr.instance.logicTime;
        runTime = 0;

        if (mGroupCfg.pointNum >= refPointList.Count)
        {
            Debuger.LogError("脑子有病么？ 总共就{0}个刷新点 还要随机｛1｝个", refPointList.Count, mGroupCfg.pointNum);
        }

        //初始化所有刷新点
        for (int i = 0; i < refPointList.Count; i++)
        {
            refPointList[i].Init(startTime);
            refPointList[i].isFinish = true;   //初始化后不马上刷新
            refPointList[i].mRefreshNum = 1;   //将没个刷新点的波数设为一 在刷新组里控制波数 因为刷新点每次出现的会不一样
            idxList.Add(i);     //保存所有下标
        }

        //随机取出下标
        List<int> curPointIdxList = Util.GetRandomElements<int>(idxList, mGroupCfg.pointNum);
        curPointList.Clear();
        //激活随机个数的刷新点
        for (int i = 0; i < curPointIdxList.Count; i++ )
        {
            refPointList[curPointIdxList[i]].isFinish = false;
            curPointList.Add(refPointList[curPointIdxList[i]]);
        }
        //设为第一波
        waveNum = 1;
    }

    public override void Update()
    {
        base.Update();
        if (mState != RefreshState.RUN)
            return;

        bool isFinish = true;
        bool isRefreshed = true;
        foreach (RefreshPoint refPoint in curPointList)
        {
            refPoint.Update();
            if (!refPoint.isFinish || refPoint.roleDic.Count > 0)     //刷新点刷完或者有怪没有死
                isFinish = false;
            if (!refPoint.isFinish)
                isRefreshed = false;
        }

        if (isRefreshed && waveNum >= mGroupCfg.refreshNum)
        {
            Stop();

            //连续刷新下一波
            if (!string.IsNullOrEmpty(mGroupCfg.nextGroupFlag))
            {
                Room.instance.StartCoroutine(CoStartRefresh(mGroupCfg.nextGroupFlag, mGroupCfg.nextWaveDelay));
            }
        }

        //刷新点怪全死  
        if(isFinish)
        {
            //随机下一波
            List<int> curPointIdxList = Util.GetRandomElements<int>(idxList, mGroupCfg.pointNum);
            curPointList.Clear();
            //激活随机个数的刷新点
            for (int i = 0; i < curPointIdxList.Count; i++)
            {
                refPointList[curPointIdxList[i]].isFinish = false;
                curPointList.Add(refPointList[curPointIdxList[i]]);
            }
            waveNum++;
        }

    }
}
