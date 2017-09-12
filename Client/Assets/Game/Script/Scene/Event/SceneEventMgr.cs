using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneEventMgr : Singleton<SceneEventMgr>
{

    
    public class SaveData
    {
        public Dictionary<string, SceneTrigger> m_triggerDict = new Dictionary<string, SceneTrigger>();
        public List<SceneTrigger> m_allTriggerList = new List<SceneTrigger>();
        public string m_cantStartEventId = "";  //保存初始不激活的事件id
    }

    //保存事件名和事件的对应关系
    Dictionary<string, SceneTrigger> triggerDict = null;
    //保存所有关卡事件
    List<SceneTrigger> allTriggerList = null;
    //碰撞区域列表
    List<AreaTrigger> m_areaTriggers = new List<AreaTrigger>();

    //保存切换场景时不需要释放的事件
    Dictionary<string, SaveData> m_saveData = new Dictionary<string, SaveData>();
    //保存通关条件事件
    public List<SceneTrigger> conditionTriggerList = new List<SceneTrigger>();
    //开始不激活的事件
    public string cantStartList = "";
    string logicFile = "";
    public string roomId = "";


    public Dictionary<string, SceneTrigger> mTriggerDict { get { return triggerDict; } }


    public void LoadEvent(int sceneIdx)
    {
        if (sceneIdx < 0)
            return;

        //加载新关卡事件时先暂停之前的事件
        SetAllEventRunOrStop(true);

        SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;

        RoomCfg cfg = LevelMgr.instance.CurLevel.roomCfg;
        if (cfg.sceneFileName.Count > 0)
            logicFile = cfg.sceneFileName[sceneIdx];

        triggerDict = new Dictionary<string, SceneTrigger>();
        allTriggerList = new List<SceneTrigger>(); ;

        //新关卡只加载一次通关条件
        if (roomId != cfg.id)
        {
            roomId = cfg.id;
            conditionTriggerList.Clear();
            List<int> taskIds = cfg.GetTaskIdList();
            if (taskIds != null)    //主城等没有通关条件
            {
                for (int i = 0; i < taskIds.Count; i++)
                {
                    RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);
                    if (conditionCfg != null)
                    {
                        SceneTrigger trigger = TriggerFactory.GetCondition(conditionCfg);
                        if (trigger != null)
                            conditionTriggerList.Add(trigger);
                    }
                }
            }

        }

        SaveData data;

        //有此关卡事件 设为run
        if (m_saveData.TryGetValue(logicFile, out data))
        {
            triggerDict = data.m_triggerDict;
            allTriggerList.AddRange(conditionTriggerList);  //把通关条件放在前面 通关条件有杀死所有怪 关卡事件里如果也有杀死所有怪 要让通关条件在前触发
            allTriggerList.AddRange(data.m_allTriggerList);
            SetAllEventRunOrStop(true);
        }
        else
        {
            string cantStartEventIds = "";
            for (int i = 0; i < sceneData.mCheckList.Count; i++)
            {
                SceneCfg.CheckCfg check = sceneData.mCheckList[i];

                SceneAction action = new SceneAction();
                SceneAction act = action;

                SceneTrigger trigger = new SceneTrigger();
                trigger.mFlag = check.checkFlag;
                trigger.mActionList.Clear();
                triggerDict[check.checkFlag] = trigger;
                allTriggerList.Add(trigger);

                SceneTrigger triggerTemp = trigger;
                for (int idx = 0; idx < check.eventCfgList.Count; idx++)
                {
                    SceneTrigger newTrigger = TriggerFactory.GetTrigger(check.eventCfgList[idx]);
                    if (newTrigger != null)
                    {
                        newTrigger.mFlag = check.checkFlag;
                        triggerTemp.mTrigger = newTrigger;
                        triggerTemp = newTrigger;
                        allTriggerList.Add(newTrigger);
                    }
                }

                for (int idx = 0; idx < check.actionCfgList.Count; idx++)
                {
                    SceneAction tempAction = ActionFactory.GetAction(check.actionCfgList[idx]);
                    if (check.actionCfgList[idx].eveType == (int)SceneCfg.ActionType.RandomEvent)   //保存下初始不激活的事件
                    {
                        ActionRandomEvent randAct = tempAction as ActionRandomEvent;
                        cantStartEventIds += randAct.mActionCfg.eventIds;
                    }
                    trigger.mActionList.Add(tempAction);
                }

                if (trigger.mActionList.Count > 1)
                    trigger.mActionList.Sort((SceneAction x, SceneAction y) =>
                    {
                        return x._idx == y._idx ? 0 : (x._idx > y._idx ? 1 : -1);
                    });

                trigger.mTriggerType = check.triggerType;
                trigger.mTriggerCount = check.triggerNum;
                trigger.mTriggerTimes = check.triggerTimes;

            }

            data = new SaveData();
            data.m_allTriggerList = allTriggerList;
            data.m_triggerDict = triggerDict;
            data.m_cantStartEventId = cantStartEventIds;
            m_saveData.Add(logicFile, data);

            allTriggerList = new List<SceneTrigger>();
            allTriggerList.AddRange(conditionTriggerList);
            allTriggerList.AddRange(data.m_allTriggerList);

            cantStartList = cantStartEventIds;

            SetAllEventRunOrStop(true);
        }

        //创建触发区域
        m_areaTriggers.Clear();
        for (int i = 0; i < sceneData.mAreaList.Count; i++)
        {
            if (sceneData.mAreaList[i].areaType == SceneCfg.AreaType.Normal)
            {
                AreaTrigger area = CreateArea(sceneData.mAreaList[i]);
                if (area != null)
                    m_areaTriggers.Add(area);
            }
        }

        //先隐藏 Unity Bug？ 主角进入时离挡板很远就能碰到
        if (Room.instance != null)
        {
            Room.instance.OnUpdateEventCallback += UpdateEvent;
            Room.instance.mAreaGroup.gameObject.SetActive(false);
        }

        return;
    }

    public void FireAction(string eventFlag)
    {
        SceneTrigger trigger;
        if (triggerDict.TryGetValue(eventFlag, out trigger))
        {
            if (trigger != null)
                trigger.OnFireAction();
            else
                Debuger.LogError("没有配置的条件或已经删除的条件: " + eventFlag);
        }
        
    }

    public SceneTrigger GetTrigger(string eventFlag)
    {
        SceneTrigger trigger;
        triggerDict.TryGetValue(eventFlag, out trigger);
        return trigger;
    }

    public void UpdateEvent()
    {
        for (int i = 0; i < allTriggerList.Count; i++)
            allTriggerList[i].Update();
    }

    public AreaTrigger GetAreaTriggerByFlag(string flag)
    {
        foreach (AreaTrigger area in m_areaTriggers)
        {
            if (area.mTriggerId == flag)
                return area;
        }
        return null;
    }

    public void OnExit()
    {
        if (allTriggerList == null || m_areaTriggers == null)
            return;

        foreach (SceneTrigger tri in allTriggerList)
            tri.OnRelease();

        triggerDict.Clear();

        foreach (AreaTrigger area in m_areaTriggers)
            Object.Destroy(area);

        m_areaTriggers.Clear();

        foreach(SaveData data in m_saveData.Values)
        {
            data.m_allTriggerList.Clear();
            data.m_triggerDict.Clear();
        }

        m_saveData.Clear();

        Room.instance.OnUpdateEventCallback -= UpdateEvent;
    }

    public void OnSaveExit()
    {
        Room.instance.OnUpdateEventCallback -= UpdateEvent;
    }

    public void SetAllEventRunOrStop(bool bPause)
    {
        if (allTriggerList != null)
        {
            foreach (SceneTrigger tri in allTriggerList)
                tri.mRun = bPause;
        }
    }

    public void StartEvent()
    {
        if (allTriggerList != null)
        {
            foreach (SceneTrigger tri in allTriggerList)
            {
                if (SceneMgr.instance.CurSceneIdx > 0 && conditionTriggerList.Contains(tri))     //切换关卡时如果是通关条件 不要再start
                    continue;
                if (!string.IsNullOrEmpty(cantStartList) && !string.IsNullOrEmpty(tri.mFlag) && cantStartList.Contains(tri.mFlag))
                    continue;
                tri.Start();
            }
        }

    }

    public void StartEvent(string eventId)
    {
        foreach (SceneTrigger tri in allTriggerList)
        {
            if (eventId == tri.mFlag) 
                tri.Start();
        }
    }

    //移除的是关卡里的事件 和通关条件无关
    public void RemoveEvent(string flag)
    {
        SceneTrigger tri = null;
        triggerDict.TryGetValue(flag, out tri);
        if (tri != null)
        {
            foreach (var act in tri.mActionList)
                act.OnRelease();
            tri.OnRelease();
            while (tri.mTrigger != null)
            {
                if (allTriggerList.Contains(tri))
                    allTriggerList.Remove(tri);
                tri = tri.mTrigger;
            }
            triggerDict.Remove(flag);
        }
    }

    public AreaTrigger CreateArea(SceneCfg.AreaCfg areaCfg = null)
    {
        AreaTrigger area = null;
        if (areaCfg != null)
        {
            GameObject go = new GameObject(areaCfg.areaFlag);
            go.transform.SetParent(Room.instance.mAreaGroup.transform, false);

            go.transform.position = areaCfg.pos;
            go.transform.localScale = areaCfg.size;
            go.transform.rotation = Quaternion.Euler(areaCfg.dir);
            LayerMgr.instance.SetLayer(go, enGameLayer.levelTrigger);

            Rigidbody rd = go.AddComponent<Rigidbody>();
            rd.isKinematic = true;

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.enabled = true;
            area = go.AddComponent<AreaTrigger>();
            area.mTriggerId = areaCfg.areaFlag;

            go.gameObject.SetActive(areaCfg.bActivate);
        }

        return area;
    }
}
