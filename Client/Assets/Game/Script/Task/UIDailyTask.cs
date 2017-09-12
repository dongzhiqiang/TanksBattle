using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UIDailyTask : MonoBehaviour {

    public StateGroup taskGroup;
    public TextEx vitallity;
    public ImageEx progress;
    public ImageEx jiantou;
    public ScrollRect taskScroll;
    public List<GameObject> vitalityBoxItems;
    bool isAddListener = false;
    int canGetRewardNum = 0;
    bool isInit = false;

    void Start()
    {
        taskScroll.onValueChanged.AddListener(OnScrollChanged);
    }
    void OnScrollChanged(Vector2 v)
    {
        jiantou.gameObject.SetActive(taskGroup.Count > 3 && taskScroll.verticalNormalizedPosition > 0.01f);
    }
    
    void AddAllListener()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.Add(MSG_ROLE.NET_ACT_PROP_SYNC, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.Add(MSG_ROLE.NET_OPACT_PROP_SYNC, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.Add(MSG_ROLE.CORPS_BUILD, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.Add(MSG_ROLE.ELITELV_CHANGE, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.AddPropChange(enProp.vipLv, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.AddPropChange(enProp.upEquipTime, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.AddPropChange(enProp.upPetTime, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
        hero.AddPropChange(enProp.towerEnterTime, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                LoadTask();
            }
        });
    }   

    public void Init()
    {
        isInit = true;
        LoadTask();
        if (!isAddListener)
        {            
            AddAllListener();
            UIMainCity.AddOpen(() =>
            {
                AddAllListener();
            });
            isAddListener = true;
        }
    }
     public  void LoadVitalityBox()
    {
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.CheckTip();
        Role hero = RoleMgr.instance.Hero;
        int currentLevel = hero.GetInt(enProp.level);
        List<VitalityCfg> vitalityCfgList = VitalityCfg.GetListByLevel(currentLevel);

        TaskPart taskPart = hero.TaskPart;
        taskPart.CurVitality = taskPart.GetInt(enTaskProp.vitality);
        if (!TimeMgr.instance.IsToday(taskPart.GetLong(enTaskProp.dailyTaskGet)))
        {
            taskPart.CurVitality = 0;
        }


        float total = VitalityCfg.GetTotalVitality();
        vitallity.text = taskPart.CurVitality.ToString();
        progress.fillAmount = taskPart.CurVitality > total ? 1: taskPart.CurVitality / total;
        for (int i = 0; i < vitalityBoxItems.Count; ++i)
        {
            UIVitalityBoxItem boxItem = vitalityBoxItems[i].GetComponent<UIVitalityBoxItem>();
            boxItem.init(vitalityCfgList[i].id, total);
        }
    }

    public void LoadTask()
    {
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.CheckTip();
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        int currentLevel = hero.GetInt(enProp.level);
        int currentVipLv = hero.GetInt(enProp.vipLv);
        List<TaskRewardCfg> taskRewardCfg = TaskRewardCfg.GetCfgsByLevelAndVip(currentLevel, currentVipLv);

        taskGroup.SetCount(taskRewardCfg.Count);

        List<TaskRewardCfg> canGetRewardTask = new List<TaskRewardCfg>();
        List<TaskRewardCfg> cantGetRewardTask = new List<TaskRewardCfg>();
        List<TaskRewardCfg> hasGetRewardTask = new List<TaskRewardCfg>();

        for (int i = 0; i < taskRewardCfg.Count; ++i)
        {
            switch (taskPart.CanGetDailyTaskReward(taskRewardCfg[i]))
            {
                case (enRewardState.canGetReward):
                    canGetRewardTask.Add(taskRewardCfg[i]);
                    break;
                case (enRewardState.cantGetReward):
                    cantGetRewardTask.Add(taskRewardCfg[i]);
                    break;
                case (enRewardState.hasGetReward):
                    hasGetRewardTask.Add(taskRewardCfg[i]);
                    break;
            }
        }

        for (int i = 0; i < taskGroup.Count; ++i)
        {
            UITaskItem taskItem = taskGroup.Get<UITaskItem>(i);
            if (i < canGetRewardTask.Count)
            {
                taskItem.Init(canGetRewardTask[i]);
            }
            else if (i - canGetRewardTask.Count < cantGetRewardTask.Count)
            {
                taskItem.Init(cantGetRewardTask[i - canGetRewardTask.Count]);
            }
            else
            {
                taskItem.Init(hasGetRewardTask[i - canGetRewardTask.Count - cantGetRewardTask.Count]);
            }
        }

        if (isInit)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(taskScroll, 0); });
            canGetRewardNum = canGetRewardTask.Count;
            isInit = false;
        }
       
        int newCanGetRewardNum = canGetRewardTask.Count;

        if (newCanGetRewardNum != canGetRewardNum)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(taskScroll, 0); });
            canGetRewardNum = canGetRewardTask.Count;
        }
        LoadVitalityBox();
    }

    public bool CheckTip()
    {
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        return taskPart.CheckDailyTaskTip();
    }

    
}
