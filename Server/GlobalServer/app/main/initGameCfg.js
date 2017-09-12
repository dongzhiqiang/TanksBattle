"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../libs/logUtil");
var gameCfg = require("../logic/gameConfig/gameConfig");

////////////逻辑模块////////////
var TestConfig = require("../logic/gameConfig/testConfig").TestConfig;
var EquipConfig = require("../logic/gameConfig/equipConfig").EquipConfig;
var ItemConfig = require("../logic/gameConfig/itemConfig").ItemConfig;
var EquipUpgradeCostConfig = require("../logic/gameConfig/equipUpgradeCostConfig").EquipUpgradeCostConfig;
var EquipAdvanceCostConfig = require("../logic/gameConfig/equipAdvanceCostConfig").EquipAdvanceCostConfig;
var EquipAdvanceRateConfig = require("../logic/gameConfig/equipAdvanceRateConfig").EquipAdvanceRateConfig;
var EquipRouseCostConfig = require("../logic/gameConfig/equipRouseCostConfig").EquipRouseCostConfig;
var EquipRouseRateConfig = require("../logic/gameConfig/equipRouseRateConfig").EquipRouseRateConfig;
var RoleConfig = require("../logic/gameConfig/roleConfig").RoleConfig;
var EquipInitListConfig = require("../logic/gameConfig/equipInitListConfig").EquipInitListConfig;
var RewardConfig = require("../logic/gameConfig/rewardConfig").RewardConfig;
var LvExpConfig = require("../logic/gameConfig/lvExpConfig").LvExpConfig;
var PetUpgradeCostConfig = require("../logic/gameConfig/petUpgradeCostConfig").PetUpgradeCostConfig;
var PetAdvanceCostConfig = require("../logic/gameConfig/petAdvanceCostConfig").PetAdvanceCostConfig;
var PetUpstarCostConfig = require("../logic/gameConfig/petUpstarCostConfig").PetUpstarCostConfig;
var PetAdvLvPropRateConfig = require("../logic/gameConfig/petAdvLvPropRateConfig").PetAdvLvPropRateConfig;
var PetStarPropRateConfig = require("../logic/gameConfig/petStarPropRateConfig").PetStarPropRateConfig;
var PetTalentLvConfig = require("../logic/gameConfig/petTalentLvConfig").PetTalentLvConfig;
var PetBondConfig = require("../logic/gameConfig/petBondConfig").PetBondConfig;
var PetBattleAssistRateConfig = require("../logic/gameConfig/petBattleAssistRateConfig").PetBattleAssistRateConfig;
var PetPosConfig = require("../logic/gameConfig/petPosConfig").PetPosConfig;
var TalentConfig = require("../logic/gameConfig/talentConfig").TalentConfig;
var TalentPosConfig = require("../logic/gameConfig/talentPosConfig").TalentPosConfig;
var LevelConfig = require("../logic/gameConfig/levelConfig").LevelConfig;
var ValueConfig = require("../logic/gameConfig/valueConfig").ConfigValueConfig;
var LanguageConfig = require("../logic/gameConfig/languageConfig").LanguageConfig;
var GoldLevelBasicCfg = require("../logic/gameConfig/goldLevelConfig").GoldLevelBasicCfg;
var GoldLevelModeCfg = require("../logic/gameConfig/goldLevelConfig").GoldLevelModeCfg;
var HadesLevelBasicCfg = require("../logic/gameConfig/hadesLevelConfig").HadesLevelBasicCfg;
var HadesLevelModeCfg = require("../logic/gameConfig/hadesLevelConfig").HadesLevelModeCfg;
var HadesBaseRewardCfg = require("../logic/gameConfig/hadesLevelConfig").HadesBaseRewardCfg;
var HadesEvaluateRewardCfg = require("../logic/gameConfig/hadesLevelConfig").HadesEvaluateRewardCfg;
var VenusLevelBasicCfg = require("../logic/gameConfig/venusLevelConfig").VenusLevelBasicCfg;
var VenusLevelRewardCfg = require("../logic/gameConfig/venusLevelConfig").VenusLevelRewardCfg;
var GuardLevelBasicCfg = require("../logic/gameConfig/guardLevelConfig").GuardLevelBasicCfg;
var GuardLevelModeCfg = require("../logic/gameConfig/guardLevelConfig").GuardLevelModeCfg;
var GuardBaseRewardCfg = require("../logic/gameConfig/guardLevelConfig").GuardBaseRewardCfg;
var GuardEvaluateRewardCfg = require("../logic/gameConfig/guardLevelConfig").GuardEvaluateRewardCfg;
var GuardEvaluateCfg = require("../logic/gameConfig/guardLevelConfig").GuardEvaluateCfg;
var FlameConfig = require("../logic/gameConfig/flameConfig").FlameConfig;
var FlameLevelConfig = require("../logic/gameConfig/flameLevelConfig").FlameLevelConfig;
var FlameMaterialConfig = require("../logic/gameConfig/flameMaterialConfig").FlameMaterialConfig;
var PropBasicConfig = require("../logic/gameConfig/propBasicConfig").PropBasicConfig;
var PropDistributeConfig = require("../logic/gameConfig/propDistributeConfig").PropDistributeConfig;
var PropTypeConfig = require("../logic/gameConfig/propTypeConfig").PropTypeConfig;
var PropValueConfig = require("../logic/gameConfig/propValueConfig").PropValueConfig;
var RoleLvPropConfig = require("../logic/gameConfig/roleLvPropConfig").RoleLvPropConfig;
var RoleTypePropConfig = require("../logic/gameConfig/roleTypePropConfig").RoleTypePropConfig;
var ValueConfigModule = require("../logic/gameConfig/valueConfig");
var ArenaBasicCfg = require("../logic/gameConfig/arenaConfig").ArenaBasicCfg;
var ArenaGradeCfg = require("../logic/gameConfig/arenaConfig").ArenaGradeCfg;
var ArenaRobotCfg = require("../logic/gameConfig/arenaConfig").ArenaRobotCfg;
var RobotConfig = require("../logic/gameConfig/robotConfig").RobotConfig;
var SkillLvCostConfig = require("../logic/gameConfig/skillLvCostConfig");
var SkillLvValueConfig = require("../logic/gameConfig/skillLvValueConfig").SkillLvValueConfig;
var SkillLvRateConfig = require("../logic/gameConfig/skillLvRateConfig").SkillLvRateConfig;
var RoleSkillConfig = require("../logic/gameConfig/roleSkillConfig");
var WeaponConfig = require("../logic/gameConfig/weaponConfig");
var HeroTalentConfig = require("../logic/gameConfig/heroTalentConfig");
var SystemConfig = require("../logic/gameConfig/systemConfig").SystemConfig;
var ElementConfig = require("../logic/gameConfig/elementConfig").ElementConfig;
var CheckInRewardConfig=require("../logic/gameConfig/checkInRewardConfig").CheckInRewardConfig;
var TaskRewardConfig=require("../logic/gameConfig/taskRewardConfig").TaskRewardConfig;
var VitalityRewardConfig=require("../logic/gameConfig/vitalityRewardConfig").VitalityRewardConfig;
var FriendMaxConfig = require("../logic/gameConfig/friendMaxConfig").FriendMaxConfig;
var LevelRewardConfig=require("../logic/gameConfig/levelRewardConfig").LevelRewardConfig;
var VipConfig = require("../logic/gameConfig/vipConfig").VipConfig;
var VipGiftConfig = require("../logic/gameConfig/vipGiftConfig").VipGiftConfig;
var GrowthTaskConfig = require("../logic/gameConfig/growthTaskConfig").GrowthTaskConfig;
var CorpsBaseConfig = require("../logic/gameConfig/corpsConfig").CorpsBaseConfig;
var CorpsLevelConfig = require("../logic/gameConfig/corpsConfig").CorpsLevelConfig;
var CorpsPosFuncConfig = require("../logic/gameConfig/corpsConfig").CorpsPosFuncConfig;
var CorpsDeclareConfig = require("../logic/gameConfig/corpsConfig").CorpsDeclareConfig;
var CorpsBuildConfig = require("../logic/gameConfig/corpsConfig").CorpsBuildConfig;
var ExchangeShopConfig = require("../logic/gameConfig/exchangeShopConfig").ExchangeShopConfig;
var WaresConfig = require("../logic/gameConfig/waresConfig").WaresConfig;
var LotteryBasicCfg = require("../logic/gameConfig/lotteryConfig").LotteryBasicCfg;
var LotteryRandPool = require("../logic/gameConfig/lotteryConfig").LotteryRandPool;
var ArenaBuyConfig = require("../logic/gameConfig/arenaBuyConfig").ArenaBuyConfig;
var StaminaBuyConfig = require("../logic/gameConfig/staminaBuyConfig").StaminaBuyConfig;
var TestItemConfig = require("../logic/gameConfig/valueConfig").TestItemConfig;
var SweepLevelCfg = require("../logic/gameConfig/levelConfig").SweepLevelCfg;

////////////模块内数据////////////

/**
 * 格式说明：
 * 配置对象：{配置名:文件路径名|文件路径名数组|子配置对象}，如果是文件路径名或文件路径名数组，相当于是{file:文件路径名|文件路径名数组}
 * 配置对象：{file:文件路径名|文件路径名数组,rowType:行数据类（可选，默认就是Object类型）,rowKey:主键列名（可选，没主键就读取为数组）}
 * 文件路径名：*.csv|*.json，就这两种格式，如果是json，rowType和rowKey无效
 * 文件路径名数组：只能是*.csv的数组
 */
var cfgObjs1 = {
    "test"  : {file:"test.csv", rowType:TestConfig, rowKey:"id"},
    "test2"  : {file:["test.csv","test2.csv"], rowType:TestConfig, rowKey:"id"},
    "equip" : {file:"equip/equip.csv", rowType:EquipConfig, rowKey:"id"},
    "equipUpgradeCost" : {file:"equip/equipUpgradeCost.csv", rowType:EquipUpgradeCostConfig, rowKey:"id"},
    "equipAdvanceCost": {file:"equip/equipAdvanceCost.csv", rowType:EquipAdvanceCostConfig, rowKey:"id"},
    "equipAdvanceRate": {file:"equip/equipAdvanceRate.csv", rowType:EquipAdvanceRateConfig, rowKey:"id"},
    "equipRouseCost": {file:"equip/equipRouseCost.csv", rowType:EquipRouseCostConfig, rowKey:"id"},
    "equipRouseRate": {file:"equip/equipRouseRate.csv", rowType:EquipRouseRateConfig, rowKey:"id"},
    "equipInitList" : {file:"equip/equipInitList.csv", rowType:EquipInitListConfig, rowKey:"id"},
    "item" : {file:"item/item.csv", rowType:ItemConfig, rowKey:"id"},
    "reward" : {file:"reward/reward.csv", rowType:RewardConfig, rowKey:"id"},
    "petUpgradeCost" : {file:"pet/petUpgradeCost.csv", rowType:PetUpgradeCostConfig, rowKey:"id"},
    "petAdvanceCost" : {file:"pet/petAdvanceCost.csv", rowType:PetAdvanceCostConfig, rowKey:"id"},
    "petUpstarCost" : {file:"pet/petUpstarCost.csv", rowType:PetUpstarCostConfig, rowKey:"id"},
    "petAdvLvPropRate" : {file:"pet/petAdvLvPropRate.csv", rowType:PetAdvLvPropRateConfig, rowKey:"advLv"},
    "petStarPropRate" : {file:"pet/petStarPropRate.csv", rowType:PetStarPropRateConfig, rowKey:"star"},
    "petTalentLv" : {file:"pet/petTalentLv.csv", rowType:PetTalentLvConfig, rowKey:"id"},
    "petBond" : {file:"pet/petBond.csv", rowType:PetBondConfig, rowKey:"id"},
    "petBattleAssistRate" : {file:"pet/petBattleAssistRate.csv", rowType:PetBattleAssistRateConfig, rowKey:"id"},
    "petPos" : {file:"pet/petPos.csv", rowType:PetPosConfig, rowKey:"id"},
    "talent" : {file:"pet/talent.csv", rowType:TalentConfig, rowKey:"id"},
    "talentPos" : {file:"pet/talentPos.csv", rowType:TalentPosConfig, rowKey:"id"},
    "lvExp" : {file:"role/lvExp.csv", rowType:LvExpConfig, rowKey:"level"},
    "role" : {file:"role/role.csv", rowType:RoleConfig, rowKey:"id"},
    "room" : {file:"room/room.csv", rowType:LevelConfig, rowKey:"id"},
    "configValue" : {file:"other/configValue.csv", rowType:ValueConfig, rowKey:"name"},
    "languageCfg" : {file:"other/languageCfg.csv", rowType:LanguageConfig, rowKey:"key"},
    "goldLevelBasic" : {file:"activity/goldLevelBasic.csv", rowType:GoldLevelBasicCfg},
    "goldLevelMode" : {file:"activity/goldLevelMode.csv", rowType:GoldLevelModeCfg, rowKey:"mode"},
    "hadesLevelBasic" : {file:"activity/hadesLevelBasic.csv", rowType:HadesLevelBasicCfg},
    "hadesLevelMode" : {file:"activity/hadesLevelMode.csv", rowType:HadesLevelModeCfg, rowKey:"mode"},
    "hadesBaseReward" : {file:"activity/hadesBaseReward.csv", rowType:HadesBaseRewardCfg, rowKey:"id"},
    "hadesEvaluateReward" : {file:"activity/hadesEvaluateReward.csv", rowType:HadesEvaluateRewardCfg, rowKey:"id"},
    "venusLevelBasic" : {file:"activity/venusLevelBasic.csv", rowType:VenusLevelBasicCfg},
    "guardLevelBasic" : {file:"activity/guardLevelBasic.csv", rowType:GuardLevelBasicCfg},
    "guardLevelMode" : {file:"activity/guardLevelMode.csv", rowType:GuardLevelModeCfg, rowKey:"mode"},
    "guardBaseReward" : {file:"activity/guardBaseReward.csv", rowType:GuardBaseRewardCfg, rowKey:"id"},
    "guardEvaluateReward" : {file:"activity/guardEvaluateReward.csv", rowType:GuardEvaluateRewardCfg, rowKey:"id"},
    "guardEvaluate" : {file:"activity/guardEvaluate.csv", rowType:GuardEvaluateCfg, rowKey:"evaluate"},
    "flame" : {file:"flame/flame.csv", rowType:FlameConfig, rowKey:"id"},
    "flameLevel" : {file:"flame/flameLevel.csv", rowType:FlameLevelConfig},
    "flameMaterial" : {file:"flame/flameMaterial.csv", rowType:FlameMaterialConfig, rowKey:"id"},
    "propBasic" : {file:"property/propBasic.csv", rowType:PropBasicConfig},
    "propDistribute" : {file:"property/propDistribute.csv", rowType:PropDistributeConfig, rowKey:"id"},
    "propType" : {file:"property/propType.csv", rowType:PropTypeConfig, rowKey:"id"},
    "propValue" : {file:"property/propValue.csv", rowType:PropValueConfig, rowKey:"id"},
    "roleLvProp" : {file:"property/roleLvProp.csv", rowType:RoleLvPropConfig, rowKey:"lv"},
    "roleTypeProp" : {file:"property/roleTypeProp.csv", rowType:RoleTypePropConfig, rowKey:"id"},
    "venusLevelReward" : {file:"activity/venusLevelReward.csv", rowType:VenusLevelRewardCfg, rowKey:"evaluate"},
    "arenaBasic" : {file:"activity/arenaBasicCfg.csv", rowType:ArenaBasicCfg},
    "arenaGrade" : {file:"activity/arenaGradeCfg.csv", rowType:ArenaGradeCfg, rowKey:"grade"},
    "arenaRobot" : {file:"activity/arenaRobotCfg.csv", rowType:ArenaRobotCfg},
    "robot" : {file:"property/robotCfg.csv", rowType:RobotConfig, rowKey:"robotId"},
    "skillLvCost" : {file:"systemSkill/skillLvCost.csv", rowType:SkillLvCostConfig, rowKey:"id"},
    "skillLvValue" : {file:"systemSkill/skillLvValue.csv", rowType:SkillLvValueConfig, rowKey:"id"},
    "skillLvRate" : {file:"systemSkill/skillLvRate.csv", rowType:SkillLvRateConfig},
    "roleSkill" : {file:"systemSkill/roleSkill.csv", rowType:RoleSkillConfig},
    "weapon" : {file:"role/weapon.csv", rowType:WeaponConfig, rowKey:"id"},
    "heroTalent" : {file:"systemSkill/heroTalent.csv", rowType:HeroTalentConfig, rowKey:"id"},
    "system" : {file:"system/system.csv", rowType:SystemConfig, rowKey:"id"},
    "element" : {file:"role/element.csv", rowType:ElementConfig},
    "checkInReward"  : {file:"opActivity/checkInReward.csv", rowType:CheckInRewardConfig},
    "taskReward"  : {file:"task/taskReward.csv", rowType:TaskRewardConfig, rowKey:"id"},
    "vitalityReward"  : {file:"task/vitalityReward.csv", rowType:VitalityRewardConfig, rowKey:"id"},
    "friendMaxConfig" : {file:"friend/friendMax.csv", rowType:FriendMaxConfig, rowKey:"level"},
    "levelReward"  : {file:"opActivity/levelReward.csv", rowType:LevelRewardConfig, rowKey:"id"},
    "vip"  : {file:"vip/vip.csv", rowType:VipConfig, rowKey:"level"},
    "vipGift"  : {file:"vip/vipGift.csv", rowType:VipGiftConfig, rowKey:"level"},
	"growthTask"  : {file:"task/growthTask.csv", rowType:GrowthTaskConfig, rowKey:"id"},
    "corpsBase" : {file:"corps/corpsBase.csv", rowType:CorpsBaseConfig},
    "corps" : {file:"corps/corps.csv", rowType:CorpsLevelConfig, rowKey:"level"},
    "posFunc" : {file:"corps/posFunc.csv", rowType:CorpsPosFuncConfig, rowKey:"posLevel"},
    "initDeclare" : {file:"corps/initDeclare.csv", rowType:CorpsDeclareConfig, rowKey:"id"},
    "corpsBuild" : {file:"corps/corpsBuild.csv", rowType:CorpsBuildConfig, rowKey:"id"},
    "exchangeShop"  : {file:"exchangeShop/exchangeShop.csv", rowType:ExchangeShopConfig, rowKey:"id"},
    "wares"  : {file:"exchangeShop/wares.csv", rowType:WaresConfig, rowKey:"id"},
    "lotteryBasic"  : {file:"opActivity/lotteryBasicCfg.csv", rowType:LotteryBasicCfg, rowKey:"typeId"},
    "lotteryRandPool"  : {file:"opActivity/lotteryRandPool.csv", rowType:LotteryRandPool, rowKey:"randId"},
    "arenaBuy"  : {file:"vip/arenaBuy.csv", rowType:ArenaBuyConfig, rowKey:"arenaBuyNum"},
    "staminaBuy"  : {file:"vip/staminaBuy.csv", rowType:StaminaBuyConfig, rowKey:"staminaBuyNum"},
    "sweepLevelCfg"  : {file:"room/sweepLevelCfg.csv", rowType:SweepLevelCfg, rowKey:"type"},
    "testItem"  : {file:"other/testItem.csv", rowType:TestItemConfig, rowKey:"itemId"},
};

var cfgObjs2 = {
    "test" : "test.json"
};


////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * (){
    logUtil.info("游戏配置模块开始初始化...");
    yield gameCfg.loadConfig(cfgObjs1);
    yield gameCfg.loadConfig(cfgObjs2);
    logUtil.info("游戏配置模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * (){
    logUtil.info("游戏配置模块开始销毁...");
    //其实没啥事做
    logUtil.info("游戏配置模块结束销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;