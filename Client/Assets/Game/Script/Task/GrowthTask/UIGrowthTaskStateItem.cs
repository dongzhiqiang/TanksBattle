using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIGrowthTaskStateItem : MonoBehaviour {
    public TextEx name;
    public TextEx progress;
    public ImageEx suo;
    public ImageEx kuang;
    public ImageEx tip;
    public bool isLock = false;  
    public GrowthTaskStageCfg growthTaskStageCfg;

    public void Init(GrowthTaskStageCfg growthTaskStageCfg)
    {
        name.text = growthTaskStageCfg.name;
        this.growthTaskStageCfg = growthTaskStageCfg;
        Role hero = RoleMgr.instance.Hero;
        int currentLevel = hero.GetInt(enProp.level);
        if (currentLevel >= growthTaskStageCfg.minLevel)
        {
            isLock = false;
            suo.gameObject.SetActive(false);
        }
        else
        {
            isLock = true;
            suo.gameObject.SetActive(true);
        }

        List<GrowthTaskCfg> growthTaskCfgList = GrowthTaskCfg.GetGrowthTaskCfgByStage(growthTaskStageCfg.id);

        TaskPart taskPart = hero.TaskPart;

        int current = 0, total = growthTaskCfgList.Count;
        bool canGetReward = false;

        for (int i = 0; i < growthTaskCfgList.Count; ++i)
        {
            switch (taskPart.CanGetGrowthTaskReward(growthTaskCfgList[i]).taskState)
            {
                case enRewardState.hasGetReward:
                    {
                        current++;
                        break;
                    }
                case enRewardState.canGetReward:
                    {
                        canGetReward = true;
                        break;
                    }
            }

        }

        progress.text = current + "/" + total;
        if (canGetReward)
        {
            tip.gameObject.SetActive(true);
        }
        else
        {
            tip.gameObject.SetActive(false);
        }
    }
    public bool CheckTip()
    {
        List<GrowthTaskCfg> growthTaskCfgList = GrowthTaskCfg.GetGrowthTaskCfgByStage(growthTaskStageCfg.id);
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        bool canGetReward = false;

        for (int i = 0; i < growthTaskCfgList.Count; ++i)
        {
            switch (taskPart.CanGetGrowthTaskReward(growthTaskCfgList[i]).taskState)
            {
                case enRewardState.canGetReward:
                    {
                        if (!isLock)
                            canGetReward = true;
                        break;
                    }
            }
        }
        tip.gameObject.SetActive(canGetReward);
        return canGetReward;
    } 
}
