#region Header
/**
 * 名称：类模板
 
 * 日期：201x.xx.xx
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Csv;

public class AsyncOpInitCfg
{
    bool _isDone = false;
    float _progress = 0;
    List<Action> _ops;
    public bool isDone { get { return _isDone; } }

    public float progress { get { return _progress; } }

    public AsyncOpInitCfg(List<Action> ops)
    {
        _ops = ops;
        
        Main.instance.StartCoroutine(CoInit());
    }

    IEnumerator CoInit()
    {
        yield return 2;
        int totalCount = _ops.Count;
        long lastTick = System.DateTime.Now.Ticks;
        for (int i = 0;i< _ops.Count;++i)
        {
            _ops[i]();
            _progress = i / (float)_ops.Count;

            long curTick = System.DateTime.Now.Ticks;
            //耗时超过50ms下一帧
            if (curTick- lastTick > System.TimeSpan.TicksPerMillisecond*50)
            {
                //Debuger.LogError("配置表加载到:{0}",i);
                lastTick = curTick;
                yield return 0;
            }
            
        }
        //结束清空操作
        CsvUtil.Clear();
        CsvReader.Clear();
        PoolMgr.instance.GCCollect();//垃圾回收下，解析表可能会有大量的垃圾
        _progress = 1f;
        _isDone = true;
    }
}

public class CfgMgr : Singleton<CfgMgr>
{

    public AsyncOpInitCfg Init()
    {
        List<Action> ops = new List<Action>();
        //配置表初始化
        ops.Add(ConfigValue.Init);
        ops.Add(LanguageCfg.Init);
        ops.Add(SoundCfg.Init);
        ops.Add(SkillLvRateCfg.Init);
        ops.Add(SkillLvValueCfg.Init);
        ops.Add(HitPropCfg.Init);
        ops.Add(RoleCfg.Init);
        ops.Add(RoleFxCfg.Init);
        ops.Add(RoleLvExpCfg.Init);
        ops.Add(WeaponCfg.Init);
        ops.Add(PropTypeCfg.Init);//注意要在涉及属性的表初始化之前解析
        ops.Add(PropBasicCfg.Init);
        ops.Add(PropValueCfg.Init);
        ops.Add(PropRateCfg.Init);
        ops.Add(PropDistributeCfg.Init);
        ops.Add(RoleLvPropCfg.Init);
        ops.Add(MonsterLvPropCfg.Init);
        ops.Add(RoleTypePropCfg.Init);
        ops.Add(BuffType.Init);
        ops.Add(BuffCfg.Init);
        ops.Add(QTECfg.Init);
        ops.Add(QTECfg2.Init);
        ops.Add(ItemCfg.Init);
        ops.Add(ItemTypeCfg.Init);
        ops.Add(ItemAchieveCfg.Init);
        ops.Add(ItemAchieveTypeCfg.Init);
        ops.Add(EquipCfg.Init);
        ops.Add(EquipUpgradeCostCfg.Init);
        ops.Add(EquipAdvanceCostCfg.Init);
        ops.Add(EquipRouseCostCfg.Init);
        ops.Add(EquipAdvanceRateCfg.Init);
        ops.Add(EquipRouseRateCfg.Init);
        ops.Add(EquipPosCfg.Init);
        ops.Add(SystemCfg.Init);
        ops.Add(QualityCfg.Init);
        ops.Add(RewardCfg.Init);
        ops.Add(RoomNodeCfg.Init);
        ops.Add(RoomCfg.Init);
        ops.Add(BornCfg.Init);
        ops.Add(RoomConditionCfg.Init);
        ops.Add(GoldLevelBasicCfg.Init);
        ops.Add(GoldLevelModeCfg.Init);
        ops.Add(HadesLevelBasicCfg.Init);
        ops.Add(HadesLevelModeCfg.Init);
        ops.Add(VenusLevelBasicCfg.Init);
        ops.Add(VenusLevelButtonCfg.Init);
        ops.Add(VenusLevelRewardCfg.Init);
        ops.Add(GuardLevelBasicCfg.Init);
        ops.Add(GuardLevelModeCfg.Init);
        ops.Add(GuardLevelWaveCfg.Init);
        ops.Add(FlameCfg.Init);
        ops.Add(FlameLevelCfg.Init);
        ops.Add(FlameMaterialCfg.Init);
        ops.Add(ErrorCodeCfg.Init);
        ops.Add(LevelEvaluateCfg.Init);
        ops.Add(StorySaveCfg.Init);
        ops.Add(StoryRoleCfg.Init);
        ops.Add(ArenaBasicCfg.Init);
        ops.Add(ArenaGradeCfg.Init);
        ops.Add(ArenaRankCfg.Init);
        ops.Add(RoleSystemSkillCfg.Init);
        ops.Add(SystemSkillCfg.Init);
        ops.Add(HeroTalentCfg.Init);
        ops.Add(SkillLvCostCfg.Init);
        ops.Add(ElementCfg.Init);
        ops.Add(LoadingTipsCfg.Init);
        ops.Add(CheckInRewardCfg.Init);
        ops.Add(OpActivitiySortCfg.Init);
        ops.Add(TeachConfig.Init);
        ops.Add(AniSoundCfg.Init);
        ops.Add(FxSoundCfg.Init);
        ops.Add(FriendMaxCfg.Init);
        ops.Add(RoleMenuCfg.Init);
        ops.Add(TaskRewardCfg.Init);
        ops.Add(VitalityCfg.Init);
        ops.Add(PowerFactorCfg1.Init);
        ops.Add(PowerFactorCfg2.Init);
        ops.Add(LevelRewardCfg.Init);
        ops.Add(VipCfg.Init);
        ops.Add(GrowthTaskCfg.Init);
        ops.Add(GrowthTaskStageCfg.Init);
        ops.Add(CorpsBaseCfg.Init);
        ops.Add(CorpsCfg.Init);
        ops.Add(CorpsFuncCfg.Init);
        ops.Add(CorpsPosFuncCfg.Init);
        ops.Add(CorpsLogCfg.Init);
        ops.Add(CorpsBuildCfg.Init);
        ops.Add(BadWordsCfg.Init);
        ops.Add(ExchangeShopCfg.Init);
        ops.Add(WaresCfg.Init);
        ops.Add(VipGiftCfg.Init);
        ops.Add(LotteryBasicCfg.Init);
        ops.Add(LotteryRandPool.Init);
        ops.Add(LotteryPreview.Init);
        ops.Add(ArenaBuyCfg.Init);
        ops.Add(StaminaBuyCfg.Init);
        ops.Add(BigQteTableCfg.Init);
        ops.Add(ActivityCfg.Init);
        ops.Add(SweepLevelCfg.Init);
        ops.Add(EliteLevelCfg.Init);
        ops.Add(EliteLevelBasicCfg.Init);
        ops.Add(EliteLevelResetCfg.Init);
        ops.Add(StarRewardCfg.Init);
        ops.Add(WarTriedBaseCfg.Init);
        ops.Add(TriedRefreshCostCfg.Init);
        ops.Add(TriedLevelCfg.Init);
        ops.Add(TriedStarCfg.Init);
        ops.Add(TreasureRobBasicCfg.Init);
        ops.Add(StrongerBasicCfg.Init);
        ops.Add(StrongerDetailCfg.Init);
        ops.Add(StrongerProgressCfg.Init);
        ops.Add(StrongerHeroCfg.Init);
        ops.Add(StrongerProgressCfg.Init);
        ops.Add(TreasureCfg.Init);
        ops.Add(TreasureLevelCfg.Init);
        ops.Add(RankBasicConfig.Init);
        ops.Add(ProphetTowerCfg.Init);
        ops.Add(ProphetTowerStageCfg.Init);

        return new AsyncOpInitCfg(ops);
    }

    

}
