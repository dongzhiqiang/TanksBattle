using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UIGrowthTask : MonoBehaviour {

        
    public ImageEx jiantou;
    public ImageEx leftJiantou;
    public ImageEx rightJiantou;
    public StateGroup taskGroup;
    public StateGroup stateGroup;
    public ScrollRect taskScroll;
    public ScrollRect stateScroll;
    int currentStage = 1;
    int lastState = 1;
    bool isAddListener = false;   
    bool isInit = false;
    bool isUp = false;
    bool isReset = false;
    int canGetRewardNum = 0;

    void Start()
    {
        taskScroll.onValueChanged.AddListener(OnTaskScrollChanged);
        stateScroll.onValueChanged.AddListener(OnStateScrollChanged);
    }

    void Update()
    {       
    }

    private void OnTaskScrollChanged(Vector2 v)
    {
        jiantou.gameObject.SetActive(taskGroup.Count > 3 && taskScroll.verticalNormalizedPosition > 0.01f );
    }
    private void OnStateScrollChanged(Vector2 v)
    {        
        leftJiantou.gameObject.SetActive(stateGroup.Count > 5 && stateScroll.horizontalNormalizedPosition > 0.01f);
        rightJiantou.gameObject.SetActive(stateGroup.Count > 5 && stateScroll.horizontalNormalizedPosition < 0.99f);
        //Debug.Log(stateScroll.horizontalNormalizedPosition);
    }
    void AddAllListener()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.Add(MSG_ROLE.WEAPON_SKILL_CHANGE, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
        hero.Add(MSG_ROLE.ADD_FRIEND, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
        hero.Add(MSG_ROLE.JOIN_CORPS, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
        hero.Add(MSG_ROLE.EQUIP_CHANGE, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
        hero.AddPropChange(enProp.powerTotal, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
        hero.Add(MSG_ROLE.PET_NUM_CHANGE, () =>
        {
            if (UIMgr.instance.Get<UITask>().IsOpen)
            {
                ReFreshGrowthTask(false);
            }
        });
      
    }

    public void Init()
    {
        canGetRewardNum = 0;
        isInit = true;     
        stateGroup.AddSel(OnSelectStateItem);
        ReFreshGrowthTask(true);        
        if (!isAddListener)
        {
            isAddListener = true;
            Role hero = RoleMgr.instance.Hero;
            AddAllListener();
            UIMainCity.AddOpen(() =>
            {
                AddAllListener();
            });     
        }        
    }

    void OnSelectStateItem(StateHandle s, int idx)
    {
        UIGrowthTaskStateItem stageItem = s.GetComponent<UIGrowthTaskStateItem>();
        if(!stageItem.isLock)
        {
            isUp = isReset;
            isReset = true;
            stateGroup.Get<UIGrowthTaskStateItem>(currentStage - 1).kuang.gameObject.SetActive(false);            
            stageItem.kuang.gameObject.SetActive(true);
            if(idx+1==stageItem.growthTaskStageCfg.id)
            {
                currentStage = idx + 1;
            }
            else
            {
                Debug.LogError("成长任务阶段表配置错误");
            }
            LoadTask();
        }
        else
        {
            UIMessage.Show("等级达到" + stageItem.growthTaskStageCfg.minLevel + "级才能开启");
        }

             
    }


    public void ReFreshGrowthTask(bool isReset)
    {
        this.isReset = isReset;
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.CheckTip();
        LoadState();
        Role hero = RoleMgr.instance.Hero;
        int currentLevel = hero.GetInt(enProp.level);
        int stage = -1;
        for(int i=0;i<stateGroup.Count;++i)
        {
            UIGrowthTaskStateItem item = stateGroup.Get<UIGrowthTaskStateItem>(i);
            if (item.CheckTip())
            {
                stage = item.growthTaskStageCfg.id;
                break;
            }
            if (currentLevel>=item.growthTaskStageCfg.minLevel&&currentLevel<=item.growthTaskStageCfg.maxLevel)
            {
                stage = item.growthTaskStageCfg.id;
            }            
        }
        if (stage != -1 && isReset)
        {
            stateGroup.SetSel(stage - 1);
        }
        else
        {
            stateGroup.SetSel(currentStage-1);
        }
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(stateScroll, currentStage-1); });        
    }

    public void LoadTask()
    {       
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        List<GrowthTaskCfg> growthTaskCfgList = GrowthTaskCfg.GetGrowthTaskCfgByStage(currentStage);        
        //Debug.Log(growthTaskCfgList.Count);
        taskGroup.SetCount(growthTaskCfgList.Count);    
       
        List<GrowthTaskCfg> canGetRewardTask = new List<GrowthTaskCfg>();
        List<GrowthTaskCfg> cantGetRewardTask = new List<GrowthTaskCfg>();
        List<GrowthTaskCfg> hasGetRewardTask = new List<GrowthTaskCfg>();

        for (int i = 0; i < growthTaskCfgList.Count; ++i)
        {
            switch (taskPart.CanGetGrowthTaskReward(growthTaskCfgList[i]).taskState)
            {
                case (enRewardState.canGetReward):
                    {
                        canGetRewardTask.Add(growthTaskCfgList[i]);
                        break;
                    }
                case (enRewardState.cantGetReward):
                    cantGetRewardTask.Add(growthTaskCfgList[i]);
                    break;
                case (enRewardState.hasGetReward):
                    hasGetRewardTask.Add(growthTaskCfgList[i]);
                    break;
            }
        }
        if(isInit)
        {
            lastState = currentStage;
            canGetRewardNum = canGetRewardTask.Count;
        }
        else
        {
            if(lastState!=currentStage)
            {
                canGetRewardNum = canGetRewardTask.Count;
            }
            lastState = currentStage;
        }
        int newCanGetRewardNum = canGetRewardTask.Count;

        if (growthTaskCfgList.Count > 0)
        {
            for (int i = 0; i < taskGroup.Count; ++i)
            {
                UIGrowthTaskItem taskItem = taskGroup.Get<UIGrowthTaskItem>(i);
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
        }       
        if (isInit||newCanGetRewardNum!=canGetRewardNum||isUp)
        {
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(taskScroll, 0); });
            isInit = false;
            isUp = false;          
        }

        
    }

    public void LoadState()
    {
        Dictionary<int, GrowthTaskStageCfg> growthTaskStageCfg = GrowthTaskStageCfg.m_cfgs;
        stateGroup.SetCount(growthTaskStageCfg.Count);
        for(int i=0;i<stateGroup.Count;++i)
        {
            UIGrowthTaskStateItem item = stateGroup.Get<UIGrowthTaskStateItem>(i);
            item.Init(growthTaskStageCfg[i + 1]);
        }
    }

    public bool CheckTip()
    {            
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        return taskPart.CheckGrowthTaskTip();


    }
    void CheckStateTip()
    {
        for (int i = 0; i < stateGroup.Count; ++i)
        {
            UIGrowthTaskStateItem item = stateGroup.Get<UIGrowthTaskStateItem>(i);
            item.CheckTip();
        }
    }

}
