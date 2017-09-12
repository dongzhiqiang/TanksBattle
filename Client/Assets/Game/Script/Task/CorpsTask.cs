using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CorpsTask : Task
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
        CorpsPart corpsPart = hero.CorpsPart;
        TaskPart taskPart = hero.TaskPart;
        int total = taskRewardCfg.taskProp;
        long taskGetTime = taskPart.GetLong(taskRewardProp);       
        int taskNum = 0;
        bool isBuildCorps = corpsPart.CheckTodayHasBuild();    


        progress = taskNum.ToString() + "/" + total.ToString();

        if (!TimeMgr.instance.IsToday(taskGetTime) && isBuildCorps)
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
