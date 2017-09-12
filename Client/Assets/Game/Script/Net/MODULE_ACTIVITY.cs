using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_ACTIVITY
{
    public const int CMD_ENTER_GOLD_LEVEL   = 1;//挑战金币副本
    public const int CMD_SWEEP_GOLD_LEVEL   = 2;
    public const int CMD_END_GOLD_LEVEL     = 3;
    public const int CMD_ENTER_HADES_LEVEL  = 4;//挑战哈迪斯之血
    public const int CMD_SWEEP_HADES_LEVEL  = 5;
    public const int CMD_END_HADES_LEVEL    = 6;
    public const int CMD_ENTER_VENUS_LEVEL  = 7;//挑战爱神的恩赐
    public const int CMD_END_VENUS_LEVEL    = 8;
    public const int CMD_REQ_CHALLENGERS    = 9;
    public const int CMD_START_CHALLENGE    = 10;
    public const int CMD_END_ARENA_COMBAT   = 11;
    public const int CMD_ARENA_BUY_CHANCE   = 12;
    public const int CMD_ARENA_COMBAT_LOG   = 13;
    public const int CMD_ENTER_GUARD_LEVEL  = 14;
    public const int CMD_END_GUARD_LEVEL    = 15;
    public const int CMD_SWEEP_GUARD_LEVEL  = 16;
    public const int CMD_REQ_WARRIOR_TRIED = 17;   //请求勇士试炼信息
    public const int CMD_REFRESH_WARRIOR = 18;     //刷新勇士试炼
    public const int CMD_ENTER_WARRIOR_LEVEL = 19;        //进入勇士试炼
    public const int CMD_END_WARRIOR_LEVEL = 20;        //结束勇士试炼
    public const int CMD_WARRIOR_REWARD = 21;           //勇士试炼领取奖励
    public const int CMD_REQ_TREASURE_ROB = 22;
    public const int CMD_START_TREASURE_ROB = 23;
    public const int CMD_END_TREASURE_ROB = 24;
    public const int CMD_REFRESH_TREASURE_ROB = 25;
    public const int CMD_ARENA_GET_POS = 26; //获取竞技场阵型
    public const int CMD_ARENA_SET_POS = 27; //设置竞技场阵型
    public const int CMD_ENTER_TOWER        = 28; //预言者之塔 挑战进入
    public const int CMD_END_TOWER          = 29; //预言者之塔 挑战结束
    public const int CMD_GET_TOWER_REWARD   = 30; //预言者之塔 领取随机奖励

    public const int PUSH_SYNC_PROP         = -1;   //同步属性    
    public const int PUSH_TREASURE_ROB_BATTLE_LOG = -2;
}


public class RESULT_CODE_ACTIVITY : RESULT_CODE
{
    public const int DAY_MAX_CNT = 1; //今日次数已用完
    public const int IN_COOL_DOWN = 2; //还在冷却中
    public const int PRE_MODE_WRONG = 3; //请通过前一个难度
    public const int HERO_LVL_WRONG = 4; //您的等级还不够
    public const int LVL_NO_START = 5; //还没有开始
    public const int CAN_NOT_SWEEP = 6; //本难度暂不可以扫荡
    public const int ACTIVITY_NOT_START = 7; //活动尚未开启
    public const int ROLE_NOT_EXISTS = 8; //角色不存在
    public const int ARENA_NOT_CHALLENGER = 9; //要挑战的角色不在挑战对方列表
    //public const int DIAMOND_INSUFFICIENT = 10; //钻石不足
    public const int BUY_CHANCE_MAX_COUNT = 11; //今日购买次数达到上限
    public const int ACTIVITY_ENTERED = 12; //已挑战过该活动
    public const int TOWER_NO_TYPE = 17;      //挑战类型没有
    public const int TOWER_NO_CONFIG = 18;    //未找到对应配置
    public const int TOWER_CANT_OPEN_RANDOM = 19; //没有达到打开随即挑战的层级
}

public class EnterGoldLevelResultVo
{
    public int mode;
}

public class EndGoldLevelResultVo
{
    public string rate;
    public int gold;
    public int score;
    public int damage;
    public Dictionary<string, int> rewards;
}

public class EnterHadesLevelResultVo
{
    public int mode;
}

public class EndHadesLevelResultVo
{
    public int evaluation;
    public int exp;
    public int wave;
    public int bossCount;
    public List<ItemVo> itemList;
}

public class EndVenusLevelVo
{
    public int evaluation; //评价
    public float percentage; //百分比
}

public class EndVenusLevelResultVo
{
    public int evaluation;
    public float percentage; //百分比
    public int soul;
    public int stamina;
}

public class ArenaChallengerVo
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int score;
    public string roleId;
    public string pet1Guid;
    public string pet1RoleId;
    public string pet2Guid;
    public string pet2RoleId;
}

public class ArenaChallengerWithRankVo
{
    public int rank;
    public ArenaChallengerVo info;
}

public class ReqArenaChallengersResultVo
{
    public bool clientNew;  //客户端数据是否本来就是最新的
    public long listTime;   //对方ID列表更新时间
    public long dataTime;   //对手信息更新时间
    public int myRankVal;
    public List<ArenaChallengerWithRankVo> challengers;
}

public class ReqEndArenaCombatResultVo
{
    public bool weWin;
    public int myRankVal;
    public int myOldRankVal;
    public int myScoreVal;
    public int myOldScoreVal;
    public Dictionary<string, int> rewards;
    public int rewardId;
    public Dictionary<string, int> upgradeRewards;
    public int upgradeRewardId;
}

public class ArenaLogItemVo
{
    public bool win;
    public int oldRank;
    public int rank;
    public int opHeroId;
    public string opRoleId;
    public string opName;
    public int opOldScore;
    public long time;
}

public class ReqArenaLogResultVo
{
    public bool clientNew;
    public List<ArenaLogItemVo> logs;
}

public class EnterGoldLevelVo
{
    public int mode; //难度模式
}

public class SweepGoldLevelVo
{
    public int mode; //难度模式
    public int hpMax;
}

public class EndGoldLevelVo
{
    public int hp;
    public int hpMax;
}

public class EnterHadesLevelVo
{
    public int mode; //难度模式
}

public class EnterVenusLevelVo
{

}

public class EnterGuardLevelVo
{
    public int mode; //难度模式
}

public class EnterGuardLevelResultVo
{
    public int mode;
}

public class SweepGuardLevelVo
{
    public int mode; //难度模式
}

public class EndGuardLevelVo
{
    public int wave; //波次
    public int point; 
}

public class EndGuardLevelResultVo
{
    public int evaluation;
    public int exp;
    public int wave;
    public int point;
    public List<ItemVo> itemList;
}

public class SweepHadesLevelVo
{
    public int mode; //难度模式
}

public class EndHadesLevelVo
{
    public int wave; //波次
    public int bossCount; //剩余boss数
}

public class ReqArenaChallengersVo
{
    public long listTime;
    public long dataTime;
}

public class ReqStartChallengeVo
{
    public int heroId;
}

public class ReqEndArenaCombatVo
{
    public bool weWin;
}

public class ReqArenaLogVo
{
    public long lastTime;   //最近一条记录的时间
}

public class ArenaPosVo
{
    public string arenaPos;
}

public class SyncActivityPropVo
{
    public Dictionary<string, Property> props;
}


//勇士试炼的副本信息
public class WarriorTriedLevel
{
    public int star;   //星级
    public string room;  //room Id
    public int status;  //状态 0未完成 1已完成未领奖 2已领奖
    public List<ItemVo> rewards;
}

//勇士试炼信息
public class WarriorTriedInfo
{
    public int remainTried;   //剩余试炼次数
    public int refresh;   //刷新次数
    public long uptime;  //更新时间
    public List<WarriorTriedLevel> trieds;   //试炼的副本信息
}

public class WarriorTriedDataReq
{
}

public class WarriorTriedDataRes
{
    public WarriorTriedInfo triedData;
    public bool reset;
}

public class RefreshWarriorReq
{
}

public class RefreshWarriorRes
{
    public int refresh;
    public List<WarriorTriedLevel> trieds;
}

public class EnterWarriorReq
{
    public int index;
}

public class EnterWarriorRes
{
    public string roomId;
    public int star;
    public int index;
}

public class EndWarriorReq
{
    public int index;
    public bool isWin;
}

public class EndWarriorRes
{
    public int index;
    public bool isWin;
    public int remainTried;
}

public class GetWarriorRewardReq
{
    public int index;
}

public class GetWarriorRewardRes
{
    public int index;
    public List<WarriorTriedLevel> trieds;
}

public class TreasureChallengerVo
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int score;
    public string roleId;
    public string pet1Guid;
    public string pet1RoleId;
    public string pet2Guid;
    public string pet2RoleId;
}

public class TreasureRobChallengerVo
{
    public int rank;
    public int itemId;
    public int itemNum;
    public TreasureChallengerVo info;
}

public class ReqTreasureRobResultVo
{
    public bool clientNew;
    public long listTime;   //对方ID列表更新时间
    public long dataTime;   //对手信息更新时间
    public List<TreasureRobChallengerVo> challengers;
    public List<TreasureRobBattleLogVo> battleLogs;
}

public class ReqTreasureRobRequestVo
{
    public long listTime;   //对方ID列表更新时间
    public long dataTime;   //对手信息更新时间
    public bool useGold;
}

public class StartTreasureRobRequestVo
{
    public int heroId;   
    public int battleLogIndex;   
}

public class EndTreasureRobRequestVo
{
    public bool weWin;
}

public class EndTreasureRobResultVo
{
    public bool weWin;
    public int itemId;
    public int itemNum;
}

public class TreasureRobBattleLogVo
{
    public int heroId;
    public string name;
    public int itemId;
    public int itemNum;
    public bool iStart;
    public bool iWin;
    public long time;
    public bool revenged;
}

//预言者之塔信息
public class ProphetTowerInfo
{
    public List<int> getRewardState = new List<int>();
    public List<int> randomId = new List<int>();
}

//预言者之塔挑战界面请求进入
public class EnterTowerReq
{
    public int towerType;
}

//预言者之塔挑战界面请求进入返回
public class EnterTowerRes
{
    public int towerType;
    public string roomId;
    public ProphetTowerInfo towerInfo;
}

//预言者之塔挑战结束
public class EndTowerReq
{
    public int towerType;
    public bool isWin;
    public string roomId;
    public float useTime;
}
//预言者之塔挑战结束返回
public class EndTowerRes
{
    public int towerType;
    public string roomId;
    public bool isWin;
    public Dictionary<string, int> rewards = new Dictionary<string, int>();
}
//预言者之塔领取随机奖励
public class GetTowerRewardReq
{
    public int idx;
}
//预言者之塔挑战结束返回
public class GetTowerRewardRes
{
    public int idx;
}
