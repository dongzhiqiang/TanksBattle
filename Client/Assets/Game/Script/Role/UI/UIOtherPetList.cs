using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIOtherPetList : UIPanel
{

    #region Fields
    public StateGroup m_navBar;
    public StateGroup m_petsHave;
    public StateGroup m_petsNot;
    public UIPetBattleIcon2 m_battleIconPet1Main;
    public UIPetBattleIcon2 m_battleIconPet1Sub1;
    public UIPetBattleIcon2 m_battleIconPet1Sub2;
    public UIPetBattleIcon2 m_battleIconPet2Main;
    public UIPetBattleIcon2 m_battleIconPet2Sub1;
    public UIPetBattleIcon2 m_battleIconPet2Sub2;

    private Role m_hero;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_navBar.AddSel(OnNavSel);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_hero = param as Role;

        m_navBar.SetSel(0);
        RefreshBattlePets();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_hero = null;
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    void OnNavSel(StateHandle s, int idx)
    {
        RefreshPets();
    }

    void RefreshPets()
    {
        var petsPart = m_hero.PetsPart;
        var pets = new List<Role>();
        var existMap = new HashSet<string>();
        var navIdx = m_navBar.CurIdx;

        foreach (Role pet in petsPart.Pets.Values)
        {
            if (navIdx == 0)
                pets.Add(pet);
            else if(navIdx == pet.Cfg.subType)
                pets.Add(pet);
            existMap.Add(pet.GetString(enProp.roleId));
        }

        pets.Sort((Role role1, Role role2) => { return -(role1.GetInt(enProp.power) - role2.GetInt(enProp.power)); });

        m_petsHave.SetCount(pets.Count);
        for (int i = 0; i < pets.Count; ++i)
        {
            var pet = pets[i];
            var guid = pet.GetString(enProp.guid);
            var battleState = 0;
            if (petsPart.IsMainPet(guid))
                battleState = 1;
            else if (petsPart.IsSubPet(guid))
                battleState = 2;
            m_petsHave.Get<UIPetIcon2>(i).Init(pet.GetString(enProp.roleId), pet.GetInt(enProp.level), pet.GetInt(enProp.star), battleState, pet.GetString(enProp.name), true, OnViewPet, pet.GetString(enProp.guid));
        }

        var haveNotPets = new List<RoleCfg>();
        foreach (RoleCfg roleCfg in RoleCfg.GetAll().Values)
        {
            if (roleCfg.roleType != enRoleType.pet)
                continue;
            if (navIdx != 0 && navIdx != roleCfg.subType)
                continue;
            if (existMap.Contains(roleCfg.id))
                continue;
            haveNotPets.Add(roleCfg);
        }

        haveNotPets.Sort((RoleCfg role1, RoleCfg role2) => { return role1.initStar - role2.initStar; });

        m_petsNot.SetCount(haveNotPets.Count);
        for (int i = 0; i < haveNotPets.Count; ++i)
        {
            var petCfg = haveNotPets[i];
            m_petsNot.Get<UIPetIcon2>(i).Init(petCfg.id, 0, petCfg.initStar, 0, petCfg.name, false);
        }
    }

    void OnViewPet(string guid)
    {
        var pet = m_hero.PetsPart.GetPet(guid);
        if (pet != null)
            UIMgr.instance.Open<UIPetInfo>(pet);
    }

    void RefreshBattlePets()
    {
        var heroLv = m_hero.GetInt(enProp.level);
        var petFormation = m_hero.PetFormationsPart.GetPetFormation(enPetFormation.normal);
        var petsPart = m_hero.PetsPart;
        var pet1Main = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet1Main));
        var pet1Sub1 = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet1Sub1));
        var pet1Sub2 = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet1Sub2));
        var pet2Main = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet2Main));
        var pet2Sub1 = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet2Sub1));
        var pet2Sub2 = petsPart.GetPet(petFormation.GetPetGuid(enPetPos.pet2Sub2));

        m_battleIconPet1Main.Init(pet1Main == null ? "" : pet1Main.GetString(enProp.roleId), pet1Main == null ? 0 : pet1Main.GetInt(enProp.star), enPetPos.pet1Main, heroLv, OnViewPet, pet1Main == null ? "" : pet1Main.GetString(enProp.guid));
        m_battleIconPet1Sub1.Init(pet1Sub1 == null ? "" : pet1Sub1.GetString(enProp.roleId), pet1Sub1 == null ? 0 : pet1Sub1.GetInt(enProp.star), enPetPos.pet1Sub1, heroLv, OnViewPet, pet1Sub1 == null ? "" : pet1Sub1.GetString(enProp.guid));
        m_battleIconPet1Sub2.Init(pet1Sub2 == null ? "" : pet1Sub2.GetString(enProp.roleId), pet1Sub2 == null ? 0 : pet1Sub2.GetInt(enProp.star), enPetPos.pet1Sub2, heroLv, OnViewPet, pet1Sub2 == null ? "" : pet1Sub2.GetString(enProp.guid));
        m_battleIconPet2Main.Init(pet2Main == null ? "" : pet2Main.GetString(enProp.roleId), pet2Main == null ? 0 : pet2Main.GetInt(enProp.star), enPetPos.pet2Main, heroLv, OnViewPet, pet2Main == null ? "" : pet2Main.GetString(enProp.guid));
        m_battleIconPet2Sub1.Init(pet2Sub1 == null ? "" : pet2Sub1.GetString(enProp.roleId), pet2Sub1 == null ? 0 : pet2Sub1.GetInt(enProp.star), enPetPos.pet2Sub1, heroLv, OnViewPet, pet2Sub1 == null ? "" : pet2Sub1.GetString(enProp.guid));
        m_battleIconPet2Sub2.Init(pet2Sub2 == null ? "" : pet2Sub2.GetString(enProp.roleId), pet2Sub2 == null ? 0 : pet2Sub2.GetInt(enProp.star), enPetPos.pet2Sub2, heroLv, OnViewPet, pet2Sub2 == null ? "" : pet2Sub2.GetString(enProp.guid));
    }
    #endregion
}