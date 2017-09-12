using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LotteryTask : Task
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
        OpActivityPart opActivityPart = hero.OpActivityPart;
        TaskPart taskPart = hero.TaskPart;
        int total = taskRewardCfg.taskProp;
        long taskGetTime = taskPart.GetLong(taskRewardProp);
        long taskTime1 = 0;
        long taskTime10 = 0;
        int taskNum = 0;

      
        if (taskFieldList.Count == 2)
        {
            enOpActProp time1, time2;
            time1 = (enOpActProp)Enum.Parse(typeof(enOpActProp), taskFieldList[0]);
            time2 = (enOpActProp)Enum.Parse(typeof(enOpActProp), taskFieldList[1]);
            taskTime1 = opActivityPart.GetLong(time1);
            taskTime10 = opActivityPart.GetInt(time2);
        }
        else
            return enRewardState.cantGetReward;


        progress = taskNum.ToString() + "/" + total.ToString();

        if (!TimeMgr.instance.IsToday(taskGetTime) && (TimeMgr.instance.IsToday(taskTime1) || TimeMgr.instance.IsToday(taskTime10)))
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
