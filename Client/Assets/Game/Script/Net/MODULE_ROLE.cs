using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_ROLE
{
    public const int CMD_REQ_HERO_INFO = 1; //请求主角信息（包括宠物）
    public const int CMD_REQ_PET_INFO = 2;  //请求宠物信息
    public const int PUSH_SYNC_PROP = -1;   //同步属性
}

public class RESULT_CODE_ROLE : RESULT_CODE
{
    public const int ROLE_NOT_EXISTS = 1;   //角色不存在
    public const int PET_NOT_EXISTS = 2;    //宠物不存在
}

public class FullRoleInfoVo
{
    public Dictionary<string, Property> props;
    public List<FullRoleInfoVo> pets;
    public List<ItemVo> items;
    public List<EquipVo> equips;
    public LevelInfoVo levelInfo;
    public List<PetSkillVo> petSkills;
    public List<TalentVo> talents;
    public Dictionary<string, Property> actProps;
    public WeaponInfoVo weapons;
    public List<Mail> mails;
    public Dictionary<string, Property> opActProps;
    public List<SystemVo> systems;
    public Dictionary<string, int> teaches;
    public List<FlameVo> flames;
    public Dictionary<string, Property> taskProps;
    public FriendInfoVo social;
    public List<Shop> shops;
    public CorpsInfo corps;
    public List<EliteLevelVo> eliteLevels;
    public WarriorTriedInfo warriorTried;
    public ProphetTowerInfo prophetTower;
    public List<PetFormationVo> petFormations;
    public TreasureInfoVo treasures;
}

public class RequestHeroInfoVo
{
    public int heroId;
}

public class RoleSyncPropVo
{
    public string guid;
    public Dictionary<string, Property> props;
}

public class RequestPetInfoVo
{
    public int heroId;
    public string guid;
}