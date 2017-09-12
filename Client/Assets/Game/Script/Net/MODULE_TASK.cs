using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_TASK
{    
    public const int CMD_GET_DAILY_TASK_REWARD = 1; //获取每日任务奖励
    public const int CMD_GET_VITALITY_REWARD = 2; //获取活跃度奖励
    public const int CMD_GET_GROWTH_TASK_REWARD = 3; //获取成长任务奖励
    public const int PUSH_SYNC_PROP = -1;   //同步属性    
}
public class RESULT_CODE_TASK: RESULT_CODE
{
    public const int GET_DAILY_TASK_REWARD_FAILED = 1; //获取每日任务奖励失败
    public const int GET_VITALITY_REWARD_FAILED = 2;//获取活跃度奖励失败
    public const int GET_GROWTH_TASK_REWARD_FAILED = 3; //获取成长任务奖励失败


}

public class GetTaskRewardReq
{
    public  int taskId;
}

public class GetTaskRewardRes
{
    public int taskId;
}

public class GetVitalityRewardReq
{
    public int vitalityId;
}

public class GetVitalityRewardRes
{
    public int vitalityId;
}
public class SyncTaskPropVo
{
    public Dictionary<string, Property> props;
}


