using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_FLAME
{
    public const int CMD_UPGRADE_FLAME = 1; // 升级圣火
    public const int PUSH_ADD_OR_UPDATE_FLAME = -1;   //添加或更新圣火

}

public class RESULT_CODE_FLAME : RESULT_CODE
{
    public const int FLAME_NO_ENOUGE_GOLD = 6; // 金币不足
}

public class UpgradeFlameRequestVo
{
    public int flameId;
    public List<ItemVo> items;
}

public class UpgradeFlameResultVo
{
    public int levelAdd;
}


public class AddOrUpdateFlameVo
{
    public bool isAdd;
    public FlameVo flame;
}


public class FlameVo
{
    public FlameVo()
    {
    }

    public FlameVo(int flameId, int level, int exp)
    {
        this.flameId = flameId;
        this.level = level;
        this.exp = exp;
    }

    public int flameId;     //圣火ID
    public int level;       //圣火等级
    public int exp;       //经验值
}