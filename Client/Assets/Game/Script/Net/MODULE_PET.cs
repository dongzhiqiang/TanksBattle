using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_PET
{
    public const int CMD_UPGRADE_PET = 1; //升级宠物
    public const int CMD_ADVANCE_PET = 2; //进阶宠物
    public const int CMD_UPSTAR_PET = 3; //升星宠物
    public const int CMD_CHOOSE_PET = 4; //选择战宠
    public const int CMD_UPGRADE_PET_SKILL = 5; //升级宠物技能
    public const int CMD_UPGRADE_PET_TALENT = 6; //升级宠物天赋
    public const int CMD_UNCHOOSE_PET = 7; //选择战宠不出战
    public const int CMD_RECRUIT_PET = 8; //招募宠物
    public const int PUSH_REMOVE_PET = -1;   //删除宠物
    public const int PUSH_ADD_PET = -2;   //添加宠物
    public const int PUSH_ADD_OR_UPDATE_PET_SKILL = -3;   //添加(或更新)宠物技能
    public const int PUSH_ADD_OR_UPDATE_TALENT = -4;   //添加(或更新)天赋
    public const int PUSH_ADD_OR_UPDATE_PET_FORMATION = -5;   //添加(或更新)宠物阵型
}

public class RESULT_CODE_PET : RESULT_CODE
{
    public const int PET_NO_PET = 1;  //没有指定宠物
    public const int PET_MAX_LEVEL = 2; //宠物已达到最大等级
    public const int PET_NO_ENOUGH_ITEM = 3; //道具数量不对
    public const int PET_WRONG_ITEM = 4; //道具类型不对
    public const int PET_MAX_ADV_LEVEL = 5; //宠物达到最大进阶等级
    public const int PET_MAX_STAR = 6; //宠物达到最大星数
    public const int PET_NO_LEVEL = 7; //宠物等级未达到进阶条件
    public const int PET_WRONG_POS = 8; //宠物位参数错误
    public const int PET_NO_SKILL = 9; //宠物无此技能
    public const int PET_NO_TALENT = 10; //宠物无此天赋
    public const int PET_MAX_SKILL = 11; //宠物技能满级
    public const int PET_MAX_TALENT = 12; //宠物天赋满级
    public const int PET_LEVEL_OVER_OWNER = 13; //宠物等级不能超过主人等级
    public const int PET_SKILL_NEED_STAR = 14; //宠物未达到开启技能的星级
    public const int PET_TALENT_NEED_ADV_LV = 15; //宠物未达到开启天赋的等阶
    public const int PET_SKILL_OVER_PET_LV = 16; //技能等级不能超过宠物等级
    public const int PET_TALENT_OVER_PET_ADV_LV = 17; //天赋等阶不能超过宠物等阶允许等级
    public const int PET_EXIST = 18; //宠物已存在
    public const int PET_POS_NEED_LEVEL = 19; //宠物位需要角色等级
}

public class PetSkillVo
{
    public PetSkillVo()
    {
    }

    public PetSkillVo(string skillId, int level)
    {
        this.skillId = skillId;
        this.level = level;
    }

    public string skillId;     //技能ID
    public int level;       //技能等级
}

public class RemovePetRoleVo
{
    public string guid;
}

public class AddOrUpdatePetSkillVo
{
    public string guid;
    public bool isAdd;
    public PetSkillVo petSkill;
}

public class AddOrUpdateTalentVo
{
    public string guid;
    public bool isAdd;
    public TalentVo talent;
}

public class UpgradePetResultVo
{
    public int addLv;         //增加等级数
    public int addExp;        //增加经验数
}

public class ChoosePetResultVo
{
    public List<string> needUpdatePets; // 涉及到的需要刷新属性的宠物列表(设置前后的主位宠物
}

public class UpgradePetRequestVo
{
    public string guid;         //guid
    public int itemId;        //升级使用道具id
    public int num;          //使用道具数量
}

public class AdvancePetRequestVo
{
    public string guid;         //guid
}

public class UpstarPetRequestVo
{
    public string guid;         //guid
}

public class UpstarPetResultVo
{
    public int newStar;         //guid
}


public class ChoosePetRequestVo
{
    public string guid;         //guid
    public int petFormation;    //阵型id
    public int petPos;         //对应的属性位
}

public class UpgradePetSkillRequestVo
{
    public string guid;         //guid
    public string skillId;         //技能id
}

public class UpgradePetTalentRequestVo
{
    public string guid;         //guid
    public string talentId;         //天赋id
}

public class RecruitPetRequestVo
{
    public string roleId;         //宠物roleId
}

public class RecruitPetResultVo
{
    public string guid;         //宠物guid
}

public class UpgradePetSkillResultVo
{
    public string skillId;         //技能id
}

public class UpgradePetTalentResultVo
{
    public string talentId;         //天赋id
}

public class UnchoosePetRequestVo
{
    public int petFormation;    //阵型id
    public int petPos;         //对应的属性位
}

public class TalentVo
{
    public TalentVo()
    {
    }

    public TalentVo(string talentId, int level)
    {
        this.talentId = talentId;
        this.level = level;
    }

    public string talentId;     //天赋ID
    public int level;       //天赋等级
}

public class AddOrUpdatePetFormationVo
{
    public bool isAdd;
    public PetFormationVo petFormation;
}


public class PetFormationVo
{
    public PetFormationVo()
    {
    }

    public PetFormationVo(int formationId, List<string> formation)
    {
        this.formationId = formationId;
        this.formation = formation;
    }

    public int formationId;     //队形ID
    public List<string> formation;       //队形
}