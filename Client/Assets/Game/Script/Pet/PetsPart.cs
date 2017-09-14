#region Header
/**
 * 名称：仇恨部件
 
 * 日期：2015.9.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PetsPart:RolePart
{
    #region Fields
    Dictionary<string, Role> m_pets;
    
    #endregion

    private static PropertyTable tempProps = new PropertyTable();
    private static PropertyTable tempProps1 = new PropertyTable();

    #region Properties
    public override enPart Type { get { return enPart.max; } }
    public Dictionary<string, Role> Pets { get { return m_pets; } }
    public Role Owner { get { return this.Parent.Parent; } }
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if(vo.pets == null)
        {
            return;
        }
        m_pets = new Dictionary<string, Role>();
        foreach( FullRoleInfoVo roleVo in vo.pets )
        {
            //Debug.Log("pet");
            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            cxt.OnClear();
            cxt.Init(
                roleVo.props["guid"].String, 
                roleVo.props["name"].String, 
                roleVo.props["roleId"].String, 
                roleVo.props["level"].Int, 
                Parent.GetCamp(), 
                Vector3.zero, 
                Vector3.zero,
                "",
                "",
                "",
                AIPart.PetAI);
            Role pet = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
            pet.Parent = this.Parent;//设置主人
            m_pets[pet.GetString(enProp.guid)] = pet;
        }
        //重算基础属性(羁绊，出战的影响)
        FreshPetProps();
    }

    //网络属性有改变
    public override void OnSyncProps(List<int> props) {
        //如果是装备辅助位的宠物，还要刷新主位宠物的计算属性
        if(!m_parent.IsHero)
        {
            Role owner = m_parent.PetsPart.Owner;
            Role mainPet = owner.PetsPart.GetMainPet(PropPart.GetString(enProp.guid));
            if (mainPet != null)
            {
                mainPet.PropPart.FreshBaseProp();
            }
        }
    
    }

    public void FreshPetProps()
    {
        if (m_pets == null)
            return;

        // 对所有宠物重算一次基础属性
        foreach (Role pet in m_pets.Values)
        {
            pet.PropPart.FreshBaseProp();
        }
        PetFormation petFormation = m_parent.PetFormationsPart.GetCurPetFormation();
        // 对主位的宠物重算一次基础属性
        string mainPetGUID;
        mainPetGUID = petFormation.GetPetGuid(enPetPos.pet1Main);
        if (mainPetGUID != "")
        {
            Role mainPet = GetPet(mainPetGUID);
            if (mainPet != null)
            {
                mainPet.PropPart.FreshBaseProp();
            }
        }
        mainPetGUID = petFormation.GetPetGuid(enPetPos.pet2Main);
        if (mainPetGUID != "")
        {
            Role mainPet = GetPet(mainPetGUID);
            if (mainPet != null)
            {
                mainPet.PropPart.FreshBaseProp();
            }
        }
    }

    public Role GetPetByRoleId(string roleId)
    {
        if (m_pets == null)
            return null;

        foreach (Role pet in m_pets.Values)
        {
            if (pet.GetString(enProp.roleId) == roleId)
            {
                return pet;
            }
        }
        return null;
    }

    public bool HasPet(string roleId, out int star)
    {
        star = 0;
        if (m_pets == null)
            return false;

        foreach (Role pet in m_pets.Values)
        {
            if(pet.GetString(enProp.roleId) == roleId)
            {
                star = pet.GetInt(enProp.star);
                return true;
            }
        }
        return false;
    }

    public bool IsMainPet(string guid)
    {
        PetFormation petFormation = m_parent.PetFormationsPart.GetCurPetFormation();
        return petFormation.GetPetGuid(enPetPos.pet1Main) == guid || petFormation.GetPetGuid(enPetPos.pet2Main) == guid;
    }

    public bool IsSubPet(string guid)
    {
        PetFormation petFormation = m_parent.PetFormationsPart.GetCurPetFormation();
        return petFormation.GetPetGuid(enPetPos.pet1Sub1) == guid || petFormation.GetPetGuid(enPetPos.pet1Sub2) == guid ||
            petFormation.GetPetGuid(enPetPos.pet2Sub1) == guid || petFormation.GetPetGuid(enPetPos.pet2Sub2) == guid;
    }

    public bool IsBattlePet(string guid)
    {
        return IsMainPet(guid) || IsSubPet(guid);
    }

    //如果给到宠物是辅助战斗宠物，返回对应的主位宠物，否则返回null
    public Role GetMainPet(string guid)
    {
        PetFormation petFormation = m_parent.PetFormationsPart.GetCurPetFormation();
        return petFormation.GetMainPet(guid);
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {

    }

    

    public override void OnClear()
    {
        if (m_pets == null)
            return;

        foreach (var pet in m_pets.Values)
        {
            RoleMgr.instance.DestroyRole(pet, false);
        }
        m_pets.Clear();
        m_pets = null;
    }

    public override void OnPreLoad()
    {
        if(m_parent.PetFormationsPart ==null)
        {
            //非主角，可能是宠物
            return;
        }

        //加载宠物模型
        PetFormation petFormation = m_parent.PetFormationsPart.GetCurPetFormation();
        string guid1 = petFormation.GetPetGuid(enPetPos.pet1Main);
        string guid2 = petFormation.GetPetGuid(enPetPos.pet2Main);

        if (!string.IsNullOrEmpty(guid1))
        {
            Role pet1 = GetPet(guid1);
            if (pet1 != null)
                pet1.PreLoad();

        }

        if (!string.IsNullOrEmpty(guid2))
        {
            Role pet2 = GetPet(guid2);
            if (pet2 != null)
                pet2.PreLoad();
        }
    }

    public Role GetPet(string guid)
    {
        if (m_pets == null || string.IsNullOrEmpty(guid))
            return null;

        Role role;
        m_pets.TryGetValue(guid, out role);
        return role;
    }

    public List<Role> GetMainPets()
    {
        PetFormation petFormation = Parent.PetFormationsPart.GetCurPetFormation();
        return petFormation.GetMainPets();
    }

    //获取所有出战神侍，包括主战和助战
    public List<Role> GetFightPets()
    {
        List<Role> roleList = new List<Role>();
        List<string> guids = new List<string>();
        PetFormation petFormation = Parent.PetFormationsPart.GetCurPetFormation();
        string guid1 = petFormation.GetPetGuid(enPetPos.pet1Main);
        string guid2 = petFormation.GetPetGuid(enPetPos.pet1Sub1);
        string guid3 = petFormation.GetPetGuid(enPetPos.pet1Sub2);
        string guid4 = petFormation.GetPetGuid(enPetPos.pet2Main);
        string guid5 = petFormation.GetPetGuid(enPetPos.pet2Sub1);
        string guid6 = petFormation.GetPetGuid(enPetPos.pet2Sub2);
        guids.Add(guid1);
        guids.Add(guid2);
        guids.Add(guid3);
        guids.Add(guid4);
        guids.Add(guid5);
        guids.Add(guid6);


        for(int i=0;i<guids.Count;++i)
        {
            if (!string.IsNullOrEmpty(guids[i]))
            {
                Role pet1 = GetPet(guids[i]);
                if (pet1 != null)
                    roleList.Add(pet1);
            }
        }

        return roleList;
    }

    public bool RemovePet(string guid)
    {
        if (m_pets == null)
            return false;
        Role role = GetPet(guid);
        if (role == null)
            return false;
        m_pets.Remove(guid);
        RoleMgr.instance.DestroyRole(role, false);
        role.Fire(MSG_ROLE.PET_NUM_CHANGE, null);
        return true;
    }

    public void AddPet(FullRoleInfoVo roleVo)
    {
        if (m_pets == null)
            return;

        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.OnClear();
        cxt.Init(
            roleVo.props["guid"].String, 
            roleVo.props["name"].String, 
            roleVo.props["roleId"].String, 
            roleVo.props["level"].Int, 
            enCamp.camp1, 
            Vector3.zero, 
            Vector3.zero, 
            "", 
            "",
            "",
            AIPart.PetAI);
        Role pet = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
        pet.Parent = this.Parent;//设置主人
        m_pets[pet.GetString(enProp.guid)] = pet;
        pet.Fire(MSG_ROLE.PET_NUM_CHANGE, null);
        pet.PetsPart.AddCheckPetTip(OnCheckPetTip);
        CheckPetTip();
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        if (Owner != null) // null表明宠物列表尚未初始化完毕(或者不是宠物)，初始化完成之后再重新进行宠物计算(因为涉及战宠，羁绊)
        {
            Role owner = Owner; //
            PetFormation petFormation = owner.PetFormationsPart.GetCurPetFormation();
            if (petFormation.GetPetGuid(enPetPos.pet1Main) == m_parent.GetString(enProp.guid) || petFormation.GetPetGuid(enPetPos.pet2Main) == m_parent.GetString(enProp.guid))
            {
                //float oldHpMax = values.GetFloat(enProp.hpMax);
                //float oldPower = values.GetFloat(enProp.power);
                //float oldPowerRate = rates.GetFloat(enProp.power);
                enPetPos petPos1;
                enPetPos petPos2;
                int assistId1;
                int assistId2;
                if (petFormation.GetPetGuid(enPetPos.pet1Main) == m_parent.GetString(enProp.guid))
                {
                    petPos1 = enPetPos.pet1Sub1;
                    petPos2 = enPetPos.pet1Sub2;
                    assistId1 = 1;
                    assistId2 = 2;
                }
                else
                {
                    petPos1 = enPetPos.pet2Sub1;
                    petPos2 = enPetPos.pet2Sub2;
                    assistId1 = 3;
                    assistId2 = 4;
                }
                string petGUID;
                petGUID = petFormation.GetPetGuid(petPos1);
                if (petGUID != "")
                {
                    Role pet = owner.PetsPart.GetPet(petGUID);
                    if (pet != null)
                    {
                        PropertyTable.Copy(pet.PropPart.Props, tempProps);
                        PropertyTable.Mul(tempProps, PetBattleAssistRateCfg.m_cfgs[assistId1].props, tempProps);
                        PropertyTable.Add(values, tempProps, values);
                        /*
                        PropertyTable.Mul(tempProps, RoleTypePropCfg.powerProp, tempProps1);
                        PropertyTable.Mul(tempProps, RoleTypePropCfg.powerRateProp, tempProps);
                        values.AddFloat(enProp.power, PropertyTable.Sum(tempProps1));
                        rates.AddFloat(enProp.power, PropertyTable.Sum(tempProps));
                         */
                    }
                }
                petGUID = petFormation.GetPetGuid(petPos2);
                if (petGUID != "")
                {
                    Role pet = owner.PetsPart.GetPet(petGUID);
                    if (pet != null)
                    {
                        PropertyTable.Copy(pet.PropPart.Props, tempProps);
                        PropertyTable.Mul(tempProps, PetBattleAssistRateCfg.m_cfgs[assistId2].props, tempProps);
                        PropertyTable.Add(values, tempProps, values);
                        /*
                        PropertyTable.Mul(tempProps, RoleTypePropCfg.powerProp, tempProps1);
                        PropertyTable.Mul(tempProps, RoleTypePropCfg.powerRateProp, tempProps);
                        values.AddFloat(enProp.power, PropertyTable.Sum(tempProps1));
                        rates.AddFloat(enProp.power, PropertyTable.Sum(tempProps));
                         */
                    }
                }
                //Debuger.Log("宠物出战 角色增加生命值:{0}", values.GetFloat(enProp.hpMax) - oldHpMax);
               // Debuger.Log("宠物出战 角色增加战斗力:{0}", values.GetFloat(enProp.power) - oldPower);
                //Debuger.Log("宠物出战 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate);
            }

            // 羁绊
            //float oldHpMax2 = values.GetFloat(enProp.hpMax);
            //float oldHpMaxRate2 = rates.GetFloat(enProp.hpMax);
            //float oldPowerRate2 = rates.GetFloat(enProp.power);
            RoleCfg roleCfg = RoleCfg.Get(m_parent.GetString(enProp.roleId));
            for (int i = 0; i < roleCfg.petBonds.Count; i++)
            {
                PetBondCfg petBondCfg = PetBondCfg.m_cfgs[roleCfg.petBonds[i]];
                List<string> bondPets = PetBondCfg.GetBondPets(roleCfg.petBonds[i]);
                if (bondPets.Count == 0)
                {
                    continue;
                }
                int minStar = 5;
                bool hasBond = true;
                for (int j = 0; j < bondPets.Count; j++)
                {
                    int star;
                    if (owner.PetsPart.HasPet(bondPets[j], out star))
                    {
                        if(minStar > star)
                        {
                            minStar = star;
                        }
                    }
                    else
                    {
                        hasBond = false;
                        break;
                    }

                }
                if (!hasBond)
                {
                    continue;
                }
                foreach(var cxt in petBondCfg.m_rateCxts)
                {
                    float v = cxt.value.GetByLv(minStar);

                    if (!cxt.value.isPercent)
                    {
                        values.AddFloat(cxt.prop, v);
                    }
                    else
                    {
                        rates.AddFloat(cxt.prop, v);
                    }
                }
                /*
                rates.AddFloat(enProp.power, petBondCfg.powerRate.GetByLv(minStar));
                 */
            }
            //Debuger.Log("宠物羁绊 角色增加生命值:{0}", values.GetFloat(enProp.hpMax) - oldHpMax2);
            //Debuger.Log("宠物羁绊 角色增加生命值系数:{0}", rates.GetFloat(enProp.hpMax) - oldHpMaxRate2);
            //Debuger.Log("宠物羁绊 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate2);
        }
    }

    public bool CanOperate()
    {
        return CanUpgrade() || CanAdvance() || CanUpstar() || CanUpgradeSkill() || CanUpgradeTalent();
    }

    public bool CanOperateAndIsBattle()
    {
        return IsBattle() && CanOperate();  
    }

    public bool IsBattle()
    {
        return Owner.PetsPart.IsBattlePet(m_parent.GetString(enProp.guid));
    }

    public bool CanUpgrade()
    {
        if (Owner == null)
        {
            return false;
        }
        RoleCfg roleCfg = m_parent.Cfg;
        if (m_parent.GetInt(enProp.level) >= ConfigValue.GetInt("maxPetLevel"))
        {
            return false;
        }
        if (m_parent.GetInt(enProp.level) >= Owner.GetInt(enProp.level))
        {
            return false;
        }
        int allExp = PropPart.GetInt(enProp.exp);
        int needExp = PetUpgradeCostCfg.GetCostExp(roleCfg.upgradeCostId, m_parent.GetInt(enProp.level));
        foreach (ItemCfg itemCfg in ItemCfg.m_cfgs.Values)
        {
            if (itemCfg.type == ITEM_TYPE.EXP_ITEM)
            {
                allExp += Owner.ItemsPart.GetItemNum(itemCfg.id) * int.Parse(itemCfg.useValue1);
            }
        }
        if(allExp>=needExp)
        {
            return true;
        }
        return false;
    }

    public bool CanAdvance()
    {
        if (Owner == null)
        {
            return false;
        }
        RoleCfg roleCfg = m_parent.Cfg;
        if (m_parent.GetInt(enProp.advLv) >= roleCfg.maxAdvanceLevel)
        {
            return false;
        }
        if (PetAdvLvPropRateCfg.m_cfgs[m_parent.GetInt(enProp.advLv)].needLv > m_parent.GetInt(enProp.level))
        {
            return false;
        }
        List<CostItem> costItems = PetAdvanceCostCfg.GetCost(roleCfg.advanceCostId, m_parent.GetInt(enProp.advLv));
        int needItemId;
        if (!Owner.ItemsPart.CanCost(costItems, out needItemId))
        {
            return false;
        }
        return true;
    }

    public bool CanUpstar()
    {
        if (Owner == null)
        {
            return false;
        }
        RoleCfg roleCfg = m_parent.Cfg;
        if (m_parent.GetInt(enProp.star) >= roleCfg.maxStar)
        {
            return false;
        }
        List<CostItem> costItems = PetUpstarCostCfg.GetCost(roleCfg.upstarCostId, m_parent.GetInt(enProp.star));
        int needItemId;
        if (!Owner.ItemsPart.CanCost(costItems, out needItemId))
        {
            return false;
        }
        return true;
    }

    public bool CanUpgradeSkill()
    {
        if (Owner == null)
        {
            return false;
        }

        List<PetSkill> petSkills = new List<PetSkill>();

        PetSkill petSkill = new PetSkill();
        petSkills.Add(m_parent.PetSkillsPart.GetPetSkill(m_parent.Cfg.atkUpSkill));
        foreach (string skillId in m_parent.Cfg.skills)
        {
            petSkills.Add(m_parent.PetSkillsPart.GetPetSkill(skillId));
        }
        for (int i = 0; i < petSkills.Count; i++)
        {
            petSkill = petSkills[i];

            RoleSystemSkillCfg skillCfg = RoleSystemSkillCfg.Get(m_parent.Cfg.id, petSkill.skillId);
            if (m_parent.GetInt(enProp.star) < skillCfg.needPetStar)
            {
                continue;
            }
            if (petSkill.level >= ConfigValue.GetInt("maxSkillLevel"))
            {
                continue;
            }
            if (petSkill.level >= m_parent.GetInt(enProp.level))
            {
                continue;
            }
            List<CostItem> costItems = SkillLvCostCfg.GetCost(skillCfg.levelCostId, petSkill.level);
            int needItemId;
            if (!Owner.ItemsPart.CanCost(costItems, out needItemId))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    public bool CanUpgradeTalent()
    {
        if (Owner == null)
        {
            return false;
        }

        for (int i = 0; i < m_parent.Cfg.talents.Count;i++ )
        {
            string talentId = m_parent.Cfg.talents[i];
            Talent talent = m_parent.TalentsPart.GetTalent(talentId);
            TalentCfg talentCfg = TalentCfg.m_cfgs[talent.talentId];
            TalentPosCfg talentPosCfg = TalentPosCfg.m_cfgs[i];
            if (m_parent.GetInt(enProp.advLv) < talentPosCfg.needAdvLv)
            {
                continue;
            }
            if (talent.level >= talentCfg.maxLevel)
            {
                continue;
            }
            PetAdvLvPropRateCfg advLvCfg = PetAdvLvPropRateCfg.m_cfgs[m_parent.GetInt(enProp.advLv)];
            if (talent.level >= advLvCfg.maxTalentLv)
            {
                continue;
            }
            List<CostItem> costItems = PetTalentLvCfg.GetCost(talentCfg.upgradeId, talent.level);
            int needItemId;
            if (!Owner.ItemsPart.CanCost(costItems, out needItemId))
            {
                continue;
            }

            return true;
        }
        return false;
    }

    bool HasPetCanOperate()
    {
        foreach (Role pet in m_pets.Values)
        {
            if (pet.PetsPart.CanOperate() || pet.EquipsPart.HasEquipCanOperate())
            {
                return true;
            }
        }
        return false;
    }

    public bool HasBattlePetCanOperate()
    {
        PetFormation petFormation = m_parent.PetFormationsPart.GetPetFormation(enPetFormation.normal);
        for (enPetPos i = enPetPos.pet1Main; i <= enPetPos.pet2Sub2; i++)
        {
            string guid = petFormation.GetPetGuid(i);
            if (!string.IsNullOrEmpty(guid))
            {
                Role pet = GetPet(guid);
                if (pet != null)
                {
                    if (pet.PetsPart.CanOperate() || pet.EquipsPart.HasEquipCanOperate())
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }


    public void InitCheckPetTip()
    {
        //if (m_parent.IsHero != null)
        //{
        CheckPetTip();
        m_parent.Add(MSG_ROLE.ITEM_CHANGE, OnCheckPetTip);
        m_parent.AddPropChange(enProp.level, OnCheckPetTip);
        m_parent.AddPropChange(enProp.gold, OnCheckPetTip);
        m_parent.Add(MSG_ROLE.PET_FORMATION_CHANGE, OnCheckPetTip);
        foreach (Role pet in m_pets.Values)
        {
            pet.PetsPart.AddCheckPetTip(OnCheckPetTip);
        }
        //}
    }

    public void AddCheckPetTip(EventObserver.OnFire onFire)
    {
        m_parent.Add(MSG_ROLE.EQUIP_CHANGE, onFire);
        m_parent.Add(MSG_ROLE.PET_SKILL_CHANGE, onFire);
        m_parent.Add(MSG_ROLE.PET_TALENT_CHANGE, onFire);
        m_parent.AddPropChange(enProp.level, onFire);
        m_parent.AddPropChange(enProp.advLv, onFire);
        m_parent.AddPropChange(enProp.star, onFire);
    }

    public bool HasRecruitPet()
    {
        foreach (RoleCfg roleCfg in RoleCfg.GetAll().Values)
        {
            if (roleCfg.roleType != enRoleType.pet)
            {
                continue;
            }
            if (GetPetByRoleId(roleCfg.id) != null)
            {
                continue;
            }
            int curPieceNumber = RoleMgr.instance.Hero.ItemsPart.GetItemNum(roleCfg.pieceItemId);
            if (curPieceNumber >= roleCfg.pieceNum)
            {
                return true;
            }
        }
        return false;
    }

    
    #endregion

    #region Private Methods
    void CheckPetTip()
    {
        SystemMgr.instance.SetTip(enSystem.pet, HasBattlePetCanOperate() || HasRecruitPet());
    }

    void OnCheckPetTip()
    {
        CheckPetTip();
    }
    #endregion
}
