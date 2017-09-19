using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UITaskItem : MonoBehaviour {
    public TextEx name;
    public TextEx description;
    public TextEx progress;
    public ImageEx icon;
    public ImageEx bg;
    public StateHandle getRewardBtn;
    public StateHandle goToTaskBtn;
    public GameObject taskDone;
    public StateGroup taskRewardGroup;
    private TaskRewardCfg taskRewardCfg;



    public void Init(TaskRewardCfg taskRewardCfg)
    {
        this.taskRewardCfg = taskRewardCfg;
        name.text = taskRewardCfg.taskName;
        description.text = taskRewardCfg.description;
        icon.Set(taskRewardCfg.icon);
        goToTaskBtn.gameObject.SetActive(true);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[taskRewardCfg.quality];
        bg.Set(qualityCfg.backgroundSquare);

        List<int> itemIds = taskRewardCfg.GetItemIdList();
        List<int> itemNums = taskRewardCfg.GetItemNumList();
        taskRewardGroup.SetCount(itemIds.Count+1);
        for(int i=0;i<taskRewardGroup.Count-1;++i)
        {
            UITaskRewardItem taskRewardItem = taskRewardGroup.Get<UITaskRewardItem>(i);
            taskRewardItem.init(ItemCfg.m_cfgs[itemIds[i]].iconSmall, itemNums[i]);
        }
        UITaskRewardItem vitalityItem = taskRewardGroup.Get<UITaskRewardItem>(taskRewardGroup.Count - 1);
        vitalityItem.initVitality(taskRewardCfg.vitality);
              
        enTaskType taskType=(enTaskType)Enum.Parse(typeof(enTaskType), taskRewardCfg.taskType);
        switch (taskType)
        {
            case (enTaskType.activity):
                {
                    ActivityTask task = new ActivityTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();    
                    break;
                }
            case (enTaskType.lottery):
                {
                    LotteryTask task = new LotteryTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    break;
                }
            case (enTaskType.cost):
                {
                    CostTask task = new CostTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    TimeMgr.instance.AddTimer(0.1f, () =>
                    {
                        goToTaskBtn.gameObject.SetActive(false);
                    });
                    break;
                }
            case (enTaskType.corps):
                {
                    CorpsTask task = new CorpsTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();                 
                    break;
                }
            case (enTaskType.vip):
                {
                    VipTask task = new VipTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    break;
                }
            case (enTaskType.upGrade):
            case (enTaskType.prophetTower):
                {
                    UpgradeTask task = new UpgradeTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    break;
                }
            case (enTaskType.warriorTried):
                {
                    WarriorTriedTask task = new WarriorTriedTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    break;
                }
            case (enTaskType.eliteLv):
                {
                    EliteLvTask task = new EliteLvTask();
                    SetTaskState(task.CanGetReward(taskRewardCfg.id));
                    progress.text = task.getProgress();
                    break;
                }

        }
    }

    void SetTaskState(enRewardState taskState)
    {
        if (taskState == enRewardState.canGetReward)
        {
            TimeMgr.instance.AddTimer(0.1f, () =>
            {
                getRewardBtn.gameObject.SetActive(true);
                goToTaskBtn.gameObject.SetActive(false);
                taskDone.SetActive(false);
                progress.gameObject.SetActive(false);
            });
            getRewardBtn.AddClick(OnGetRewardBtn);
        }
        else if(taskState == enRewardState.cantGetReward)
        {
            TimeMgr.instance.AddTimer(0.1f, () =>
            {
                getRewardBtn.gameObject.SetActive(false);
                goToTaskBtn.gameObject.SetActive(true);
                taskDone.SetActive(false);
                progress.gameObject.SetActive(true);
            });
            goToTaskBtn.AddClick(OnGoToTaskBtn);
        }
        else if (taskState == enRewardState.hasGetReward)
        {
            TimeMgr.instance.AddTimer(0.1f, () =>
            {
                getRewardBtn.gameObject.SetActive(false);
                goToTaskBtn.gameObject.SetActive(false);
                taskDone.SetActive(true);
                progress.gameObject.SetActive(false);
            });         
        }
    }

    void OnGetRewardBtn()
    {
        NetMgr.instance.TaskHandler.SendGetDailyTaskReward(taskRewardCfg.id);
    }

    void OnGoToTaskBtn()
    {
        enTaskProp taskRewardProp = (enTaskProp)Enum.Parse(typeof(enTaskProp), taskRewardCfg.taskRewardTime);
        switch (taskRewardProp)
        {
            case (enTaskProp.goldlvGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.goldLevel, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIGoldLevel>();
                    break;
                }
            case (enTaskProp.levelGet):
                {
                    UIMgr.instance.Open<UILevelSelect>();
                    break;
                }

            case (enTaskProp.arenaGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.arena, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIArena>();
                    break;
                }

            case (enTaskProp.hadesLvGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.hadesLevel, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIHadesLevel>();
                    break;
                }
            case (enTaskProp.guardLvGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.guardLevel, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIGuardLevel>();
                    break;
                }
            case (enTaskProp.venusLvGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.venusLevel, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIVenusLevel>();
                    break;
                }
            case (enTaskProp.advLtyGet):
            case (enTaskProp.topLtyGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.lottery, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UILottery>();
                    break;
                }
            case (enTaskProp.corpsGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.corps, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    Role hero = RoleMgr.instance.Hero;
                    int corpsId = hero.GetInt(enProp.corpsId);                   
                    if(corpsId > 0)
                    {
                        UIMgr.instance.Open<UICorpsBuild>();
                    }
                    else
                    {
                        UIMgr.instance.Open < UICorpsList>();
                    }
                    
                    break;
                }
            case (enTaskProp.vip1Get):
            case (enTaskProp.vip2Get):
            case (enTaskProp.vip3Get):
            case (enTaskProp.vip4Get):
            case (enTaskProp.vip5Get):
            case (enTaskProp.vip6Get):
            case (enTaskProp.vip7Get):
            case (enTaskProp.vip8Get):
            case (enTaskProp.vip9Get):
            case (enTaskProp.vip10Get):
            case (enTaskProp.vip11Get):
            case (enTaskProp.vip12Get):
            case (enTaskProp.vip13Get):
            case (enTaskProp.vip14Get):
            case (enTaskProp.vip15Get):
            case (enTaskProp.vip16Get):
            case (enTaskProp.vip17Get):
            case (enTaskProp.vip18Get):
            case (enTaskProp.vip19Get):
            case (enTaskProp.vip20Get):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.vip, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIVipMain>();
                    break;
                }
            case (enTaskProp.upEquipGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.hero, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIEquip>().SelectEquipUpGrade();
                    break;
                }
            case (enTaskProp.prophetTowerGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.prophetTower, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIProphetTowerLevel>();
                    break;
                }
            case (enTaskProp.eliteLvGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.eliteLevel, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIEliteLevel>();
                    break;
                }
            case (enTaskProp.warriorTriedGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.warriorTried, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIWarriorsTried>();
                    break;
                }
            case (enTaskProp.treasureRobGet):
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.treasureRob, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UITreasureRob>();
                    break;
                }


        }       
    }
}
