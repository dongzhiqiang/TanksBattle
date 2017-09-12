using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionRandomEvent : SceneAction
{
    public ActionCfg_RandomEvent mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_RandomEvent;
    }

    public override void OnAction()
    {
        string[] eventIds = mActionCfg.eventIds.Split(',');
        string[] rates = mActionCfg.rates.Split(',');
        if (eventIds.Length != rates.Length || eventIds.Length <= 0)
        {
            Debuger.LogError("随机激活事件配置错误");
            return;
        }

        int totalNum = 0;
        List<int> rateList = new List<int>();
        foreach(string rate in rates)
        {
            int r = int.Parse(rate);
            rateList.Add(r);
            totalNum += r;
        }

        int randomNum = Random.Range(0, totalNum);
        int tempNum = 0;
        int idx = 0;
        for(int i = 0; i < rateList.Count; i++)
        {
            tempNum += rateList[i];
            if (tempNum > randomNum)
            {
                idx = i;
                break;
            }
        }

        string eventId = eventIds[idx];
        SceneTrigger tri = SceneEventMgr.instance.GetTrigger(eventId);
        TriggerNone noneTri = tri.mTrigger as TriggerNone;
        if (noneTri != null)
            SceneEventMgr.instance.FireAction(eventId);
        else
            SceneEventMgr.instance.StartEvent(eventId);
    }
}
