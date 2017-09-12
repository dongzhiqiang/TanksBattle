using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshBase
{

    public enum RefreshState
    {
        INIT = 0,
        RUN = 1,
        PAUSE = 2,
        END = 3,
    }

    public List<RefreshPoint> refPointList = new List<RefreshPoint>();  //逻辑刷新点列表

    public SceneCfg.RefGroupCfg mGroupCfg;
    public RefreshState mState = RefreshState.INIT;

    public int waveNum = 0;     //刷新波数

    protected float startTime = 0;
    protected float runTime = 0;

    private RefreshState m_prevState;

    public void Init(SceneCfg.RefGroupCfg groupCfg)
    {
        mGroupCfg = groupCfg;
        if (mGroupCfg != null)
        {
            List<SceneCfg.RefPointCfg> pList = mGroupCfg.Points;
            for (int j = 0; j < pList.Count; j++)
            {
                RoleCfg.PreLoad(pList[j].roleId);

                if (pList[j].buffId != 0)
                    BuffCfg.ProLoad(pList[j].buffId);
                //预加载出生死亡特效
                BornCfg.PreLoad(pList[j].bornTypeId);
                BornCfg.PreLoad(pList[j].deadTypeId);
                BornCfg.PreLoad(pList[j].groundDeadTypeId);


                RefreshPoint refPoint = new RefreshPoint(pList[j]);
                refPoint.mGroupFlag = mGroupCfg.groupFlag;
                refPoint.mRefreshNum = mGroupCfg.refreshNum;
                refPoint.groupRefresh = this;
                refPointList.Add(refPoint);

                //ai预加载
                Simple.BehaviorTree.BehaviorTreeMgrCfg.PreLoad(pList[j].ai);

            }
        }

        mState = RefreshState.INIT;
    }

    public virtual void Start()
    {
      
    }

    public void OnSaveExit()
    {
        for (int i = 0; i < refPointList.Count; i++)
        {
            refPointList[i].OnSaveExit();
        }
    }

    public void OnLoadSave()
    {
        for (int i = 0; i < refPointList.Count; i++)
        {
            refPointList[i].OnLoadSave();
        }
    }

    public void Pause()
    {
        m_prevState = mState;
        mState = RefreshState.PAUSE;
    }

    public void UnPause()
    {
        mState = m_prevState;
    }

    public void Stop()
    {
        mState = RefreshState.END;
    }

    public bool isGroupAllRoleDead()
    {
        for(int i = 0; i < refPointList.Count; i++)
        {
            if (refPointList[i].roleDic.Count > 0)
                return false;
        }
        return true;
    }

    public virtual void Update()
    {
        if (mState == RefreshState.RUN)
        {
            runTime = TimeMgr.instance.logicTime - startTime;
          
        }
    }

    public IEnumerator CoStartRefresh(string groupFlag, int delay)
    {
        //上一个刷新组的怪都死后才刷新下一组
        while (!isGroupAllRoleDead())
            yield return 0;

        if (delay > 0)
        {
            float time = TimeMgr.instance.logicTime;
            while (TimeMgr.instance.logicTime - time < delay)
                yield return 0;
        }

        RefreshBase refresh = SceneMgr.instance.GetRefreshNpcByFlag(mGroupCfg.nextGroupFlag);
        if (refresh != null)
        {
            refresh.Start();
        }

        yield return 0;
    }
}


public class RefreshMgr
{
    public static RefreshBase Create(SceneCfg.RefGroupCfg cfg)
    {
        RefreshBase rb = null;
        switch(cfg.refreshType)
        {
            case SceneCfg.RefreshType.SameTime:     //怪物波数由刷新点自己控制
                rb = new RefreshSameTime();
                break;
            case SceneCfg.RefreshType.RandomNum:    //怪物波数由刷新组统一控制
                rb = new RefreshRandomNum();
                break;
            default:
                break;
        }

        if (rb != null)
            rb.Init(cfg);
        return rb;
    }
}