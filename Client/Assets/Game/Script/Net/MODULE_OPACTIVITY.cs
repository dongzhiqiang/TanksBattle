using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_OPACTIVITY
{
    public const int CMD_CHECK_IN = 1;
    public const int CMD_LEVEL_REWARD = 2;
    public const int CMD_VIP_GIFT = 3;
    public const int CMD_DRAW_LOTTERY = 4;
    public const int PUSH_SYNC_PROP = -1;   //同步属性    
    
}
public class RESULT_CODE_OPACTIVITY : RESULT_CODE
{
    public const int CHECK_IN_FAILED = 1;       //签到失败
    public const int LEVEL_REWARD_FAILED = 2;   //获取等级礼包失败
    public const int VIP_GIFT_FAILED = 2;       //vip等级不足或已领取
    public const int LACK_NEED_ITEM = 4;        //缺少所需物品
}

public class CheckInReq
{
    public int day;
    public long curCheckInTime;
    public int itemId;
    public int itemNums;
}

public class CheckInRes
{
    public int checkInId;       
}

public class LvRewardReq
{
    public int levelId;
}

public class LvRewardRes
{
    public int levelId;
}
public class VipGiftReq
{
    public int vipLv;
}

public class VipGiftRes
{
    public int vipLv;
}
public class SyncOpActivityPropVo
{
    public Dictionary<string, Property> props;
}

public class DrawLotteryReq
{
    public int type = LotteryBasicCfg.ADVANCED_TYPE_ID;
    public int subType = LotteryBasicCfg.SUBTYPE_BUY_ONE;
}

public class DrawLotteryRes
{
    public int type = LotteryBasicCfg.ADVANCED_TYPE_ID;
    public int subType = LotteryBasicCfg.SUBTYPE_BUY_ONE;
    public List<int> randIds = new List<int>();
    public List<int> pieceRandIds = new List<int>();   //被分解成碎片的随机项ID（是给神侍的项）
}