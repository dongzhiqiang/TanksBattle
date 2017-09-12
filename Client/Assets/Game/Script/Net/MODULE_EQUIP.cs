using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MODULE_EQUIP
{
    /** 升级装备 */
    public const int CMD_UPGRADE = 1;
    /** 进阶装备 */
    public const int CMD_ADVANCE = 2;
    /** 觉醒装备 */
    public const int CMD_ROUSE = 3;
    /** 更换武器 */
    //public const int CMD_CHANGE_WEAPON = 4;
    /** 一键升级 */
    public const int CMD_UPGRADE_ONCE = 5;
    /** 全部升级 */
    public const int CMD_UPGRADE_ALL = 6;
    // push
    /** 添加或更新装备 */
    public const int PUSH_ADD_OR_UPDATE_EQUIP = -1;
    /** 删除装备 */
    public const int PUSH_REMOVE_EQUIP = -2;
}

public class RESULT_CODE_EQUIP : RESULT_CODE
{
    public const int EQUIP_PLAYER_LEVEL_LIMIT = 1; //装备等级超过角色等级
    public const int EQUIP_LEVEL_LIMIT = 2; // 装备等级超过配置等级
    public const int EQUIP_NO_ENOUGE_ITEM = 3; // 所需材料不足
    public const int EQUIP_NO_LEVEL = 4; // 装备等级未达到升品条件
    public const int EQUIP_IS_NOT_POS_TYPE = 5; // 装备位错误
}

public class AddOrUpdateEquipVo
{
    public string guidOwner;         //所有者
    public bool isAdd;
    public EquipVo equip;
}

public class RemoveEquipVo
{
    public string guidOwner;
    public enEquipPos index;
}
public class EquipVo
{
    public EquipVo()
    {
    }

    public EquipVo(int equipId, int level, int advLv)
    {
        this.equipId = equipId;
        this.level = level;
        this.advLv = advLv;
    }

    public int equipId;     //装备配置ID
    public int level;       //装备等级
    public int advLv;       //装备等阶
}

public class UpgradeEquipRequestVo
{
    public string ownerGUID;         //所有者guid
    public int equipPosIndex;        //装备位
}

public class AdvanceEquipRequestVo
{
    public string ownerGUID;         //所有者guid
    public int equipPosIndex;        //装备位
}

public class RouseEquipRequestVo
{
    public string ownerGUID;         //所有者guid
    public int equipPosIndex;        //装备位
}

public class UpgradeOnceEquipRequestVo
{
    public string ownerGUID;         //所有者guid
    public int equipPosIndex;        //装备位
}

public class UpgradeAllEquipRequestVo
{
    public string ownerGUID;         //所有者guid
}




public class GrowEquipResultVo
{
    /** 角色唯一ID */
    public string guidOwner;
    /** 旧装备数据 */
    public EquipVo oldEquip;
    /** 新装备数据 */
    public EquipVo newEquip;
}

public class GrowEquipVo
{
    /** 旧装备数据 */
    public EquipVo oldEquip;
    /** 新装备数据 */
    public EquipVo newEquip;
}

public class UpgradeAllEquipResultVo
{
    /** 角色唯一ID */
    public string guidOwner;
    /** 装备数据 */
    public List<GrowEquipVo> equipList;
}