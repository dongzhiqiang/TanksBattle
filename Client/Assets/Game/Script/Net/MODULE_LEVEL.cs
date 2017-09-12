using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MODULE_LEVEL
{
    /** 进入关卡 */
    public const int CMD_ENTER = 1;
    /** 结束关卡 */
    public const int CMD_END = 2;
    /** 扫荡 */
    public const int CMD_SWEEP = 3;
    /** 评星奖励 */
    public const int CMD_STAR = 4;
}

public class RESULT_CODE_LEVEL : RESULT_CODE
{
    public const int LEVEL_NO_NUM = 1;          //今日次数已用完
    public const int LEVEL_NO_STAMINA = 2;      //今日体力已用完
    public const int LEVEL_NO_RECORD = 3;       //挑战结束的关卡服务端并没有记录
    public const int LEVEL_CANNOT_USE_AI = 4;   //不可挂机
    public const int LEVEL_NOT_EXSTIS = 5;      //关卡不存在
    public const int MUST_PASS_FIRST = 6;       //必须先通过此关
    public const int SWEEP_COND_NOT_MATCH = 7;  //不符合扫荡条件
    public const int LEVEL_STAR_CANT_GET = 8;   //不符合领取星级奖励条件
    public const int LEVEL_CANNT_AUTOBATTLE = 9;//不允许使用自动战斗
    public const int LEVEL_MUST_AUTOBATTLE = 10;  //只能使用自动战斗
}

//进入关卡
public class LevelEnterReqVo
{
    public string roomId;
    public string nodeId;
}

//关卡结算
public class LevelEndReqVo
{
    public string roomId;
    public int time;
    public bool isWin;
    public Dictionary<string, int> starsInfo;
    public List<ItemVo> monsterItems;
    public List<ItemVo> specialItems;
    public List<ItemVo> bossItems;
    public List<ItemVo> boxItems;
}

//领取星级奖励
public class LevelStarReqVo
{
    public string nodeId;
    public int starNum;
}

//关卡更新信息
public class LevelUpdateResVo
{
    public LevelInfo level;
    public string curNode;
    public string curLevel;
    public Dictionary<string, List<ItemVo>> dropItems;

}

//结算返回
public class LevelEndResVo
{
    public string roomId;
    public bool isWin;
    public int heroExp;
    public int pet1Exp;
    public int pet2Exp;
    //奖励
    public LevelUpdateResVo updateInfo;
}

//关卡信息
public class LevelInfoVo
{
    public string curNode;      //当前挑战的章节
    public string curLevel;    //当前要挑战的关卡
    public Dictionary<string, Dictionary<string, int>> starsReward;    //星级奖励记录
    public Dictionary<string, LevelInfo> levels;  //挑战过的关卡
}

public class LevelStarRewardRes
{
    public string nodeId;
    public int starNum;
}

public class SweepLevelReq
{
    public string roomId;
    public bool multiple;
}

public class SweepLevelRewardInfo
{
    public int heroExp;
    public Dictionary<string, int> petExps; //神侍GUID和给的经验的映射
    public Dictionary<string, int> items;   //物品ID和给的数量的映射
}

public class SweepLevelRes
{
    public string roomId;
    public string curNode;
    public string curLevel;
    public LevelInfo levelInfo;

    public List<SweepLevelRewardInfo> rewards;
}