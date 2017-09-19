using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIGrowthTaskItem : MonoBehaviour
{
    public TextEx name;
    public TextEx description;
    public TextEx progress;
    public ImageEx icon;
    public ImageEx bg;
    public StateHandle getRewardBtn;
    public StateHandle goToTaskBtn;
    public GameObject taskDone;
    public StateGroup taskRewardGroup;
    private GrowthTaskCfg growthTaskCfg;  
    public enRewardState state = enRewardState.cantGetReward;

    public void Init(GrowthTaskCfg growthTaskCfg)
    {

        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;

        this.growthTaskCfg = growthTaskCfg;
        name.text = growthTaskCfg.name;
        description.text = growthTaskCfg.description;
        icon.Set(growthTaskCfg.icon);

        QualityCfg qualityCfg = QualityCfg.m_cfgs[growthTaskCfg.quality];
        bg.Set(qualityCfg.backgroundSquare);

        List<int> itemIds = growthTaskCfg.GetItemIdList();
        List<int> itemNums = growthTaskCfg.GetItemNumList();
        taskRewardGroup.SetCount(itemIds.Count);
        for (int i = 0; i < taskRewardGroup.Count; ++i)
        {
            UITaskRewardItem taskRewardItem = taskRewardGroup.Get<UITaskRewardItem>(i);
            taskRewardItem.init(ItemCfg.m_cfgs[itemIds[i]].icon, itemNums[i]);
        }
           
        GrowthTask growthTask = taskPart.CanGetGrowthTaskReward(growthTaskCfg);
        progress.text = growthTask.progress;
        SetTaskState(growthTask.taskState);
    }

    void SetTaskState(enRewardState taskState)
    {
        state = taskState;
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
        else if (taskState == enRewardState.cantGetReward)
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
        NetMgr.instance.TaskHandler.SendGetGrowthTaskReward(growthTaskCfg.id);
    }

    void OnGoToTaskBtn()
    {    
        switch ((enTaskType)Enum.Parse(typeof(enTaskType), growthTaskCfg.type))
        {
            case enTaskType.arena:
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
            case enTaskType.normalLv:
            case enTaskType.specialLv:
                {
                    UIMgr.instance.Open<UILevelSelect>();
                    List<int> paramsList = growthTaskCfg.GetParamList();
                    int node = paramsList[1];
                    UIMgr.instance.Get<UILevelSelect>().mNodeGroup.SetSel(node-1);
                    break;
                }
            case enTaskType.corps:
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.corps, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UICorpsList>();
                    break;
                }
            case enTaskType.daily:
                {
                    UIMgr.instance.Get<UITask>().btnsGroup.SetSel(0);
                    break;
                }
            case enTaskType.equipAdvLv:
            case enTaskType.equipStar:
            case enTaskType.power:
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.hero, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIEquip>();
                    break;
                }
            case enTaskType.friend:
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.social, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIFriend>();
                    break;
                }
            case enTaskType.weaponSkill:
                {
                    string errMsg = "";
                    if (!SystemMgr.instance.IsEnabled(enSystem.weapon, out errMsg))
                    {
                        UIMessage.Show(errMsg);
                        return;
                    }
                    UIMgr.instance.Open<UIWeapon>();
                    break;
                }
        }
   
    }
}
