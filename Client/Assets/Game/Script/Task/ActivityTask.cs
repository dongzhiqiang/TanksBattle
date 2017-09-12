using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActivityTask : Task {


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
        ActivityPart ActivityPart = hero.ActivityPart;
        TaskPart taskPart = hero.TaskPart;
        int total = taskRewardCfg.taskProp;
        long taskGetTime = taskPart.GetLong(taskRewardProp);
        long taskTime=0;
        int taskNum=0;

    
        if(taskFieldList.Count==1)
        {
            enActProp time;
            time = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[0]);         
            taskTime = ActivityPart.GetLong(time);
            taskNum = 1;

        }
        else if (taskFieldList.Count == 2)
        {
            enActProp time, cnt;
            time = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[0]);
            cnt = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[1]);
            taskTime = ActivityPart.GetLong(time);
            taskNum = ActivityPart.GetInt(cnt);
        }
        else if(taskFieldList.Count==3)
        {
            enActProp time, cnt1,cnt2;
            time = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[0]);
            cnt1 = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[1]);
            cnt2 = (enActProp)Enum.Parse(typeof(enActProp), taskFieldList[2]);
            taskTime = ActivityPart.GetLong(time);
            taskNum = ActivityPart.GetInt(cnt1)+ ActivityPart.GetInt(cnt2);
        }
        else
            return enRewardState.cantGetReward;

        if (!TimeMgr.instance.IsToday(taskTime))
        {
            taskNum = 0;
        }
        progress = taskNum.ToString() + "/" + total.ToString();

        if (!TimeMgr.instance.IsToday(taskGetTime) &&TimeMgr.instance.IsToday(taskTime) && taskNum >= rewardNum)
        {
            return enRewardState.canGetReward;
        }
        else if(TimeMgr.instance.IsToday(taskGetTime))
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
