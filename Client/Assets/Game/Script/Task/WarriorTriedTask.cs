using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WarriorTriedTask : Task
{


    #region Fields
    private string progress = "";


    #endregion

    #region Properties

    #endregion

    #region Frame    
    public override void OnInit() { }
    public override void OnClear() { }
    #endregion

    #region Private    

    #endregion

    public enRewardState CanGetReward(int taskId)
    {
        TaskRewardCfg taskRewardCfg = TaskRewardCfg.m_cfgs[taskId];
        List<string> taskFieldList = taskRewardCfg.GetTaskFieldList();




        enTaskProp taskRewardProp = (enTaskProp)Enum.Parse(typeof(enTaskProp), taskRewardCfg.taskRewardTime);

        int rewardNum = taskRewardCfg.taskProp;
        Role hero = RoleMgr.instance.Hero;
        ActivityPart activityPart = hero.ActivityPart;
        TaskPart taskPart = hero.TaskPart;
        int total = taskRewardCfg.taskProp;
        long taskGetTime = taskPart.GetLong(taskRewardProp);
        long taskTime = 0;
        int taskNum = 0;

        //获取勇士试炼信息
        WarTriedBaseConfig baseCfg = WarTriedBaseCfg.Get();
        WarriorTriedInfo warriorTriedInfo = activityPart.warriorTried;
        taskTime = warriorTriedInfo.uptime;
        taskNum = baseCfg.dailyNum - warriorTriedInfo.remainTried;       

        if (!TimeMgr.instance.IsToday(taskTime))
        {
            taskNum = 0;
        }
        progress = taskNum.ToString() + "/" + total.ToString();

        if (!TimeMgr.instance.IsToday(taskGetTime) && TimeMgr.instance.IsToday(taskTime) && taskNum >= rewardNum)
        {
            return enRewardState.canGetReward;
        }
        else if (TimeMgr.instance.IsToday(taskGetTime))
        {
            return enRewardState.hasGetReward;
        }
        else
        {
            return enRewardState.cantGetReward;
        }




    }

    public string getProgress()
    {
        return progress;
    }
    public override void GetReward() { }
}
