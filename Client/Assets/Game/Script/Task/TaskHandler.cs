using UnityEngine;
using System.Collections;

[NetModule(MODULE.MODULE_TASK)]
public class TaskHandler
{

    public TaskHandler()
    {
        UIMainCity.AddClick(enSystem.dailyTask, () =>
        {
            UIMgr.instance.Open<UITask>();
        });
    }

    [NetHandler(MODULE_TASK.PUSH_SYNC_PROP)]
    public void RoleSyncPropVo(SyncTaskPropVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;


        TaskPart part = role.TaskPart;
        part.OnSyncProps(info);
    }

    //发送,领取每日任务奖励
    public void SendGetDailyTaskReward(int taskId)
    {
        GetTaskRewardReq request = new GetTaskRewardReq();
        request.taskId = taskId;
        NetMgr.instance.Send(MODULE.MODULE_TASK, MODULE_TASK.CMD_GET_DAILY_TASK_REWARD, request);
    }

    //接收,获取每日任务奖励
    [NetHandler(MODULE_TASK.CMD_GET_DAILY_TASK_REWARD)]
    public void OnGetDailyTaskReward(GetTaskRewardRes res)
    {
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.GetDailyTaskReward(res.taskId);
        uiTask.uiDailyTask.LoadTask();         
    }

    //发送，获取活跃度奖励
    public void SendGetVitalityReward(int vitalityId)
    {
        GetVitalityRewardReq request = new GetVitalityRewardReq();
        request.vitalityId = vitalityId;
        NetMgr.instance.Send(MODULE.MODULE_TASK, MODULE_TASK.CMD_GET_VITALITY_REWARD, request);
    }

    //接收,获取活跃度奖励
    [NetHandler(MODULE_TASK.CMD_GET_VITALITY_REWARD)]
    public void OnGetVitalityReward(GetVitalityRewardRes res)
    {
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.GetVitalityReward(res.vitalityId);
        uiTask.uiDailyTask.LoadVitalityBox();        
    }


    //发送,领取成长任务奖励
    public void SendGetGrowthTaskReward(int taskId)
    {
        GetTaskRewardReq request = new GetTaskRewardReq();
        request.taskId = taskId;
        NetMgr.instance.Send(MODULE.MODULE_TASK, MODULE_TASK.CMD_GET_GROWTH_TASK_REWARD, request);
    }

    //接收,获取成长任务奖励
    [NetHandler(MODULE_TASK.CMD_GET_GROWTH_TASK_REWARD)]
    public void OnGetGrowthTaskReward(GetTaskRewardRes res)
    {
        UITask uiTask = UIMgr.instance.Get<UITask>();
        uiTask.GetGrowthTaskReward(res.taskId);
        uiTask.uiGrowthTask.ReFreshGrowthTask(true);     
    }

}
