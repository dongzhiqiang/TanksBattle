using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneTrigger
{
    bool bRun = false;  //是否暂停
    int haveReachTimes = 0; //已触发次数
    bool isTriggering = false;  //事件有延迟后就不是一瞬间的 防止正在触发中的事件没触发完又触发

    public SceneCfg.TriggerType mTriggerType;
    public int mTriggerCount;   //几个条件达成
    public int mTriggerTimes;   //达成几次销毁
    public SceneTrigger mTrigger;
    public List<SceneAction> mActionList = new List<SceneAction>();
    public string mFlag;    //事件组标记

    public bool IsTriggering { get { return isTriggering; } }
    public bool mRun { get { return bRun; } set { bRun = value; } }

    public virtual void Init(EventCfg eventCfg) { }
    public virtual void Start() { bRun = true; }
    public virtual bool bReach() { return true; }
    public virtual void OnRelease() { }
    public virtual void Update() { }
    //事件多次触发的 要重置下状态
    public virtual void Reset() { }
    public virtual void Init(RoomConditionCfg cfg) { }
    public virtual string GetDesc() { return ""; }
    public virtual EventCfg GetEventCfg() { return null; }
    public virtual RoomConditionCfg GetConditionCfg() { return null; }

    public int GetTriggerCount()
    {
        int num = bReach() == true ? 1 : 0;
        if (mTrigger != null)
            num += mTrigger.GetTriggerCount();
        return num;
    }

    public void OnFireAction()
    {
        if (mRun)
        {
            if (isTriggering)
                return;

            if (mTrigger.GetTriggerCount() >= mTriggerCount)
                Room.instance.StartCoroutine(OnAction());
        }
    }

    public IEnumerator OnAction()
    {
        isTriggering = true;

        foreach (SceneAction action in mActionList)
        {
            while (TimeMgr.instance.IsPause)    //暂停是先不触发事件 
                yield return 0;

            if (action._delay > 0)  //有延时要延时触发
            {
                float startTime = TimeMgr.instance.logicTime;
                while (TimeMgr.instance.logicTime - startTime < action._delay)
                    yield return 0;

            }

            action.OnAction();
        }
        haveReachTimes++;
        if (haveReachTimes >= mTriggerTimes && mTriggerTimes != -1)
            SceneEventMgr.instance.RemoveEvent(mFlag);
        else
            mTrigger.Reset();

        isTriggering = false;

        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.EVENT_FINISH, mFlag);
    }
}
