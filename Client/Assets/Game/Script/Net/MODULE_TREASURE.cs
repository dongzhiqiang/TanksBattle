using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_TREASURE
{
    public const int CMD_UPGRADE_TREASURE = 1;  //升级神器
    public const int CMD_CHANGE_BATTLE_TREASURE = 2; //设置出战神器
    public const int PUSH_ADD_OR_UPDATE_TREASURE = -1;   //添加或更新神器
    public const int PUSH_UPDATE_BATTLE_TREASURE = -2;   
}

public class RESULT_CODE_TREASURE : RESULT_CODE
{
    public const int TREASURE_MAX_LEVEL = 1; //神器已经满级
    public const int TREASURE_NOT_ENOUGE_ITEM = 2; //所需神器碎片不足
}

public class UpgradeTreasureRequestVo
{
    public int treasureId;
}

public class ChangeBattleTreasureRequestVo
{
    public List<int> battleTreasure;
}

public class AddOrUpdateTreasureVo
{
    public bool isAdd;
    public TreasureVo treasure;
}

public class UpdateBattleTreasureVo
{
    public List<int> battleTreasure;
}

public class TreasureInfoVo
{
    public List<int> battleTreasure;
    public Dictionary<string, TreasureVo> treasures;
}


public class TreasureVo
{
    public TreasureVo()
    {
    }

    public TreasureVo(int treasureId, int level)
    {
        this.treasureId = treasureId;
        this.level = level;
    }

    public int treasureId;    
    public int level;       
}