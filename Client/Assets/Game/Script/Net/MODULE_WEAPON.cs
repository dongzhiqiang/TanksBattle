using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MODULE_WEAPON
{
    public const int CMD_CHANGE_WEAPON = 1;    // 更换武器
    public const int CMD_SKILL_LEVEL_UP = 2;   //升级武器的技能 
    public const int CMD_TALENT_LEVEL_UP = 3;   //铭文升级
    public const int CMD_CHANGE_ELEMENT =4;   //切换武器元素属性

}

public class ResultCodeWeapon {
    public const int LEVEL_ERROR = 1;//要升的等级不是下一级，可能重复操作
    public const int ROLE_LEVEL_LIMIT = 2;//要升的级数不能超过角色等级
    public const int NO_ENOUGH_GOLD = 3;//金币不足
    public const int NO_ENOUGH_ITEM = 4;//材料不足
    public const int LEVEL_MAX = 5;//已经满级
    public const int WEAPON_POS_ERROR = 6; // 装备位错误
    public const int ELEMENT_POS_ERROR = 7; // 属性位置错误

};


public class WeaponInfoVo
{
    public int curWeapon = 0;
    public List<Weapon> weapons;
}

public class WeaponChangeReq
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon   
}

public class WeaponChangeRes
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon   
}

public  class WeaponSkillLevelUpReq
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public int skill = 0;           //武器的第几个技能
    public int lv = 0;               //要升到的等级
}

public class WeaponSkillLevelUpRes
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public int skill = 0;           //武器的第几个技能
    public int lv = 0;               //要升到的等级
}

public class WeaponSkillTalentUpReq
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public int skill = 0;           //武器的第几个技能
    public int talent = 0;           //技能的第几个天赋
    public int lv = 0;               //要升到的等级
}

public class WeaponSkillTalentUpRes
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public int skill = 0;           //武器的第几个技能
    public int talent = 0;           //技能的第几个天赋
    public int lv = 0;               //要升到的等级
}

public class WeaponElementChangeReq
{
    public int weapon = 0;        //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public int idx = 0;           //第几个位置的元素属性要和第0个换
}

public class WeaponElementChangeRes
{
    public int weapon = 0;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    public List<int> elements ;       
}
