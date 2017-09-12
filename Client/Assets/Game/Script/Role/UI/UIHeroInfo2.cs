using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIHeroInfo2 : UIPanel
{
    #region Fields
    public UIEquipIcon[] m_equips;

    public UI3DView m_roleMod;
    public TextEx m_heroName;

    public TextEx m_power;
    public TextEx m_level;
    public TextEx m_heroId;
    public TextEx m_corpsName;
    public TextEx m_hp;
    public TextEx m_atk;
    public TextEx m_def;
    public TextEx m_dmgDef;
    public TextEx m_damage;
    public TextEx m_critical;
    public TextEx m_criticalDef;
    public TextEx m_criticalDmg;
    public TextEx m_fire;
    public TextEx m_fireDef;
    public TextEx m_ice;
    public TextEx m_iceDef;
    public TextEx m_thunder;
    public TextEx m_thunderDef;
    public TextEx m_dark;
    public TextEx m_darkDef;

    public TextEx m_weaponTitle;
    public TextEx m_weaponDmgType;
    public TextEx m_weaponHitType;
    public TextEx m_weaponRouseDesc;

    public RectTransform m_petParent;
    public TextEx m_petNum;
    public StateGroup m_pets;
    public StateHandle m_btnAllPets;

    public RectTransform m_treasureParent;
    public TextEx m_treasureNum;
    public StateGroup m_treasures;
    public StateHandle m_btnAllTreasures;

    public RectTransform m_arenaParent;
    public TextEx m_arenaRank;
    public TextEx m_arenaScore;
    public ImageEx m_arenaGrade;
    public StateGroup m_arenaHeads;    

    public RectTransform m_flameParent;
    public StateGroup m_flameProps;

    private Role m_targetRole;
    private bool m_needDestroy = false;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnAllPets.AddClick(OnBtnAllPets);
        m_btnAllTreasures.AddClick(OnBtnAllTreasures);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        if (param is Role)
        {
            m_targetRole = (Role)param;
            m_needDestroy = false;
        }
        else if (param is FullRoleInfoVo)
        {
            FullRoleInfoVo roleVo = (FullRoleInfoVo)param;
            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            cxt.OnClear();
            cxt.roleId = roleVo.props["roleId"].String;

            try
            {
                m_targetRole = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
                m_needDestroy = true;
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex.Message);
                return;
            }
        }
        else
        {
            return;
        }

        RefreshUI();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        if (m_targetRole != null && m_needDestroy)
        {
            RoleMgr.instance.DestroyRole(m_targetRole, false);
            m_targetRole = null;
        }
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private void RefreshUI()
    {
        RefreshModel();
        RefreshEquips();
        RefreshProps1();
        RefreshProps2();
        RefreshProps3();
        RefreshPets();
        RefreshTreasures();
        RefreshArena();
        RefreshFlame();
    }

    private void RefreshModel()
    {
        m_roleMod.ResetRotation();
        m_roleMod.SetModel(m_targetRole.Cfg.mod, m_targetRole.Cfg.uiModScale, true);        
        m_heroName.text = m_targetRole.GetString(enProp.name);
    }

    private void RefreshEquips()
    {
        var equips = m_targetRole.EquipsPart.Equips;

        var uiIdx = 0;

        //为了自定义显示顺序
        var weaponTypes = new enEquipPos[] { enEquipPos.weapon4, enEquipPos.weapon2, enEquipPos.weapon3, enEquipPos.weapon1 };

        for (var i = 0; i < weaponTypes.Length; ++i)
        {
            if (uiIdx >= m_equips.Length)
                return;

            var type = weaponTypes[i];
            Equip equip;
            if (equips.TryGetValue(type, out equip))
            {
                m_equips[uiIdx++].Init(equip.EquipId, equip.Level, equip.AdvLv);
            }
        }

        for (var i = enEquipPos.minNormal; i <= enEquipPos.maxNormal; ++i)
        {
            if (uiIdx >= m_equips.Length)
                return;

            Equip equip;
            if (equips.TryGetValue(i, out equip))
            {
                m_equips[uiIdx++].Init(equip.EquipId, equip.Level, equip.AdvLv);
            }
        }

        for (; uiIdx < m_equips.Length; ++uiIdx)
        {
            m_equips[uiIdx].Init(0, 0, 0);
        }
    }

    void RefreshProps1()
    {
        m_power.text = m_targetRole.GetInt(enProp.power).ToString();
        m_level.text = m_targetRole.GetInt(enProp.level).ToString();
        m_heroId.text = Mathf.Abs(m_targetRole.GetInt(enProp.heroId)).ToString(); //TODO 机器人的主角ID也用普通主角的ID，要修改服务端
        m_corpsName.text = m_targetRole.GetString(enProp.corpsName);
    }

    void RefreshProps2()
    {
        m_hp.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.hpMax));
        m_atk.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.atk));
        m_def.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.def));
        m_dmgDef.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.damageDef));
        m_damage.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.damage));
        m_critical.text = String.Format("{0:F}", m_targetRole.GetFloat(enProp.critical) * 100) + "%";
        m_criticalDef.text = String.Format("{0:F}", m_targetRole.GetFloat(enProp.criticalDef) * 100) + "%";
        m_criticalDmg.text = String.Format("{0:F}", m_targetRole.GetFloat(enProp.criticalDamage) * 100) + "%";
        m_fire.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.fire));
        m_fireDef.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.fireDef));
        m_ice.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.ice));
        m_iceDef.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.iceDef));
        m_thunder.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.thunder));
        m_thunderDef.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.thunderDef));
        m_dark.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.dark));
        m_darkDef.text = "" + Mathf.RoundToInt(m_targetRole.GetFloat(enProp.darkDef));
    }

    void RefreshProps3()
    {
        var curWeapon = m_targetRole.WeaponPart.CurWeapon;
        var weaponCfg = curWeapon.Cfg;
        var elementCfg = curWeapon.CurElementCfg;
        var hitPropCfg = curWeapon.Cfg.HitPropCfg;
        EquipCfg equipCfg = curWeapon.Equip.Cfg;
        m_weaponTitle.text = string.Format("{0} 武器属性", weaponCfg.name);
        m_weaponDmgType.text = string.Format("<color=#C7994C>伤害类型</color> {0}", elementCfg.name);
        m_weaponHitType.text = string.Format("<color=#C7994C>打击属性</color> {0} {1}", hitPropCfg != null ? hitPropCfg.name : "", hitPropCfg != null ? hitPropCfg.desc : "");
        m_weaponRouseDesc.text = string.Format("<color=#C7994C>觉醒效果</color> {0}", equipCfg.rouseDescription);
    }

    void RefreshPets()
    {
        var pets = m_targetRole.PetsPart.Pets;
        var mainPets = m_targetRole.PetsPart.GetMainPets();

        if (pets.Count <= 0)
        {
            m_petParent.gameObject.SetActive(false);
            m_pets.SetCount(0);
            return;
        }

        m_petParent.gameObject.SetActive(true);
        m_petNum.text = "拥有：" + pets.Count;
        m_pets.SetCount(4);
        var idx = 0;
        foreach (var dataItem in pets.OrderByDescending(e => e.Value.GetInt(enProp.power)))
        {
            if (idx >= m_pets.Count)
                return;

            var uiItem = m_pets.Get<UIPetIcon2>(idx++);
            var role = dataItem.Value;
            uiItem.Init(role.GetString(enProp.roleId), role.GetInt(enProp.level), role.GetInt(enProp.star), mainPets.IndexOf(role) >= 0 ? 1 : 0, null, true, OnViewPet, role.GetString(enProp.guid));
        }
        for (; idx < m_pets.Count; ++idx)
        {
            var uiItem = m_pets.Get<UIPetIcon2>(idx);
            uiItem.Init("", 0, 0, 0);
        }
    }

    void OnViewPet(string guid)
    {
        var pet = m_targetRole.PetsPart.GetPet(guid);
        if (pet != null)
            UIMgr.instance.Open<UIPetInfo>(pet);
    }

    void RefreshTreasures()
    {
        var treasures = m_targetRole.TreasurePart.Treasures;
        var battleTreasures = m_targetRole.TreasurePart.BattleTreasures;

        if (treasures.Count <= 0)
        {
            m_treasureParent.gameObject.SetActive(false);
            m_treasures.SetCount(0);
            return;
        }

        m_treasureParent.gameObject.SetActive(true);
        m_treasureNum.text = "拥有：" + treasures.Count;
        m_treasures.SetCount(4);
        var idx = 0;
        foreach (var dataItem in treasures.OrderByDescending(e => e.Value.level))
        {
            if (idx >= m_treasures.Count)
                return;

            var uiItem = m_treasures.Get<UITreasureIcon2>(idx++);
            var treasure = dataItem.Value;
            uiItem.Init(treasure.treasureId, treasure.level, battleTreasures.IndexOf(treasure.treasureId));
        }
        for (; idx < m_treasures.Count; ++idx)
        {
            var uiItem = m_treasures.Get<UITreasureIcon2>(idx);
            uiItem.Init(0, 0);
        }
    }
        
    void RefreshArena()
    {
        var actPart = m_targetRole.ActivityPart;
        var score = actPart.GetInt(enActProp.arenaScore);
        var grade = ArenaGradeCfg.GetGrade(score);
        var gradeCfg = ArenaGradeCfg.Get(grade);
        var myRank = actPart.GetInt(enActProp.arenaRank);
        var myScore = actPart.GetInt(enActProp.arenaScore);
        m_arenaGrade.Set(gradeCfg.nameImg);
        m_arenaRank.text = "排名：" + (myRank < 0 ? "--" : (myRank + 1).ToString());
        m_arenaScore.text = "积分：" + myScore;

        var formPart = m_targetRole.PetFormationsPart;
        var petForm = formPart.GetPetFormation(enPetFormation.normal);
        string pet1Guid = petForm.GetPetGuid(enPetPos.pet1Main);
        string pet2Guid = petForm.GetPetGuid(enPetPos.pet2Main);
        var pet1 = m_targetRole.PetsPart.GetPet(pet1Guid);
        var pet2 = m_targetRole.PetsPart.GetPet(pet2Guid);

        m_arenaHeads.SetCount(3);
        var arenaPos = actPart.GetString(enActProp.arenaPos);
        arenaPos = string.IsNullOrEmpty(arenaPos) ? "1,0,2" : arenaPos;
        var posArr = ArenaBasicCfg.GetArenaPos(arenaPos);
        for (var i = 0; i < posArr.Count; ++i)
        {
            var pos = posArr[i];
            switch (pos)
            {
                case 0:
                    m_arenaHeads.Get<UIHeroOrPetIcon>(i).Init(m_targetRole);
                    break;
                case 1:
                    m_arenaHeads.Get<UIHeroOrPetIcon>(i).Init(pet1);
                    break;
                case 2:
                    m_arenaHeads.Get<UIHeroOrPetIcon>(i).Init(pet2);
                    break;
            }
        }
    }

    void RefreshFlame()
    {
        var flames = m_targetRole.FlamesPart.Flames;

        if (flames.Count <= 0)
        {
            m_flameParent.gameObject.SetActive(false);
            m_flameProps.SetCount(0);
            return;
        }

        m_flameParent.gameObject.SetActive(true);
        var dict = new Dictionary<enProp, float>();
        foreach (var flame in flames.Values)
        {
            var flameCfg = FlameCfg.m_cfgs[flame.FlameId];
            var levelCfg = FlameLevelCfg.Get(flame.FlameId, flame.Level);
            if (levelCfg == null)
                continue;
            PropValueCfg valueCfg = PropValueCfg.Get(levelCfg.attributeId);
            if (valueCfg == null)
                continue;
            var props = valueCfg.props;
            for (var i = 0; i < props.Props.Length; ++i)
            {
                var prop = props.Props[i];
                if (prop == null)
                    continue;
                var propEn = (enProp)i;
                var propVal = prop.Float;
                if (propVal > 0)
                {
                    if (dict.ContainsKey(propEn))
                        dict[propEn] += propVal;
                    else
                        dict[propEn] = propVal;
                }                
            }
        }

        m_flameProps.SetCount(dict.Count);
        var index = 0;
        for (var i = enProp.minFightProp; i < enProp.maxFightProp; ++i)
        {
            var propVal = 0.0f;
            if (dict.TryGetValue(i, out propVal))
            {
                var propCfg = PropTypeCfg.Get(i);
                if (propCfg == null)
                    continue;

                var uiItem = m_flameProps.Get<UITextPairItem>(index++);
                uiItem.Init(propCfg.name, propCfg.format == enPropFormat.FloatRate ? string.Format("{0:P4}", propVal) : propVal.ToString());
            }
        }
        if (index != dict.Count)
            m_flameProps.SetCount(index);
    }

    void OnBtnAllPets()
    {
        UIMgr.instance.Open<UIOtherPetList>(m_targetRole);
    }

    void OnBtnAllTreasures()
    {
        UIMgr.instance.Open<UITreasure2>(m_targetRole);
    }
    #endregion
}
