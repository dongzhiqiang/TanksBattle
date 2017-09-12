using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_ELITE_LEVEL
{
    public const int CMD_ENTER_ELITE_LEVEL = 1;     //进入精英关卡
    public const int CMD_END_ELITE_LEVEL = 2;      //结算精英关卡
    public const int CMD_SWEEP_ELITE_LEVEL = 3;      //扫荡精英关卡
    public const int CMD_GET_FIRST_REWARD = 4;      //获取首杀奖励
    public const int CMD_RESET_ELITE_LEVEL = 5;      //重置精英关卡

    public const int PUSH_ADD_OR_UPDATE_ELITE_LEVEL = -1;   //推送精英关卡修改

}

public class RESULT_CODE_ELITE_LEVEL : RESULT_CODE
{
    public const int ELITE_LEVEL_NOT_EXISTS = 1;      //精英关卡不存在
    public const int ELITE_LEVEL_NO_STAMINA = 2;      //体力不足，不能进关卡
    public const int ELITE_LEVEL_NO_NUM = 3;          //次数用完
    public const int ELITE_LEVEL_NO_RECORD = 4;       //结算时没有进入关卡的记录
    public const int ELITE_LEVEL_MUST_PASS = 5;       //未通关不能扫荡
    public const int ELITE_LEVEL_SWEEP_COND = 6;      //未满足扫荡条件
    public const int ELITE_LEVEL_REWARDED = 7;       //已领取过首杀奖励
    public const int ELITE_LEVEL_NO_RESET_NUM = 8;      //重置次数已用完
}

//进入关卡
public class EnterEliteLevelRequestVo
{
    public int levelId;
}

//关卡结算
public class EndEliteLevelRequestVo
{
    public int levelId;
    public int time;
    public bool isWin;
    public Dictionary<string, int> starsInfo;
    public List<ItemVo> monsterItems;
    public List<ItemVo> specialItems;
    public List<ItemVo> bossItems;
    public List<ItemVo> boxItems;
}

//进入关卡返回
public class EnterEliteLevelResultVo
{
    public int levelId;
    public Dictionary<string, List<ItemVo>> dropItems;

}

//结算返回
public class EndEliteLevelResultVo
{
    public int levelId;
    public bool isWin;
    public int heroExp;
    public int pet1Exp;
    public int pet2Exp;
}

public class SweepEliteLevelRequestVo
{
    public int levelId;
    public bool multiple;
}

public class SweepEliteLevelResultVo
{
    public int levelId;

    public List<SweepLevelRewardInfo> rewards;
}

public class GetFirstRewardRequestVo
{
    public int levelId;

}

public class GetFirstRewardResultVo
{
    public int levelId;
}

public class ResetEliteLevelRequestVo
{
    public int levelId;

}

public class ResetEliteLevelResultVo
{
    public int levelId;
}

public class AddOrUpdateEliteLevelVo
{
    public bool isAdd;
    public EliteLevelVo eliteLevel;
}

public class EliteLevelVo
{
    public EliteLevelVo()
    {
    }

    public int levelId;
    public bool passed;
    public Dictionary<string, int> starsInfo = new Dictionary<string, int>();
    public int enterTime;
    public int count;
    public int resetCount;
    public bool firstRewarded;
}