using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIChoosePet : UIPanel
{
    #region SerializeFields
    public StateGroup m_navBar;
    public StateGroup m_pets;
    public StateGroup m_petsHaveNot;
    public UIPetBattleIcon m_battleIconPet1Main;
    public UIPetBattleIcon m_battleIconPet1Sub1;
    public UIPetBattleIcon m_battleIconPet1Sub2;
    public UIPetBattleIcon m_battleIconPet2Main;
    public UIPetBattleIcon m_battleIconPet2Sub1;
    public UIPetBattleIcon m_battleIconPet2Sub2;
    public DropControl m_dropBlank;
    #endregion


    //初始化时调用
    public override void OnInitPanel()
    {
        m_navBar.AddSel(OnNavSel);
        m_battleIconPet1Main.Init(enPetPos.pet1Main);
        m_battleIconPet1Sub1.Init(enPetPos.pet1Sub1);
        m_battleIconPet1Sub2.Init(enPetPos.pet1Sub2);
        m_battleIconPet2Main.Init(enPetPos.pet2Main);
        m_battleIconPet2Sub1.Init(enPetPos.pet2Sub1);
        m_battleIconPet2Sub2.Init(enPetPos.pet2Sub2);
        m_dropBlank.m_onDrop = OnDropBlank;
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_battleIconPet1Main.SetFormationId(enPetFormation.normal);
        m_battleIconPet1Sub1.SetFormationId(enPetFormation.normal);
        m_battleIconPet1Sub2.SetFormationId(enPetFormation.normal);
        m_battleIconPet2Main.SetFormationId(enPetFormation.normal);
        m_battleIconPet2Sub1.SetFormationId(enPetFormation.normal);
        m_battleIconPet2Sub2.SetFormationId(enPetFormation.normal);
        UpdatePets();
        UpdateBattlePets();
        GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1;
    }


    public void UpdateBattlePets()
    {
        m_battleIconPet1Main.UpdatePet();
        m_battleIconPet1Sub1.UpdatePet();
        m_battleIconPet1Sub2.UpdatePet();
        m_battleIconPet2Main.UpdatePet();
        m_battleIconPet2Sub1.UpdatePet();
        m_battleIconPet2Sub2.UpdatePet();
    }

    public void UpdatePets()
    {
        OnNavSel(null, m_navBar.CurIdx);
    }
    


    void OnNavSel(StateHandle s, int idx)
    {
        List<Role> pets = new List<Role>();
        Dictionary<string, bool> existMap = new Dictionary<string, bool>();
        foreach (Role pet in RoleMgr.instance.Hero.PetsPart.Pets.Values)
        {
            if (idx == 0)
            {
                pets.Add(pet);
            }
            else
            {
                if(idx == pet.Cfg.subType)
                {
                    pets.Add(pet);
                }
            }
            existMap[pet.GetString(enProp.roleId)] = true;
        }
        pets.Sort((Role role1, Role role2) =>
        {
            return -(role1.GetInt(enProp.power) - role2.GetInt(enProp.power));
        });
        List<RoleCfg> havePets = new List<RoleCfg>();
        List<RoleCfg> haveNotPets = new List<RoleCfg>();
        foreach(RoleCfg roleCfg in RoleCfg.GetAll().Values)
        {
            if(roleCfg.roleType != enRoleType.pet)
            {
                continue;
            }
            if (idx != 0 && idx != roleCfg.subType)
            {
                continue;
            }
            if(existMap.ContainsKey(roleCfg.id))
            {
                continue;
            }
            int curPieceNumber = RoleMgr.instance.Hero.ItemsPart.GetItemNum(roleCfg.pieceItemId);
            if(curPieceNumber >= roleCfg.pieceNum)
            {
                havePets.Add(roleCfg);
            }
            else
            {
                haveNotPets.Add(roleCfg);
            }
        }
        havePets.Sort((RoleCfg role1, RoleCfg role2) =>
        {
            return role1.initStar - role2.initStar;
        });
        haveNotPets.Sort((RoleCfg role1, RoleCfg role2) =>
        {
            return role1.initStar - role2.initStar;
        });

        m_pets.SetCount(pets.Count+havePets.Count);
        for (int i = 0; i < havePets.Count; i++)
        {
            m_pets.Get<UIPetBrief>(i).Init(null, havePets[i].id);
        }
        for (int i = 0; i < pets.Count; i++)
        {
            m_pets.Get<UIPetBrief>(i + havePets.Count).Init(pets[i], null);
        }
        m_petsHaveNot.SetCount(haveNotPets.Count);
        for (int i = 0; i < haveNotPets.Count; i++)
        {
            m_petsHaveNot.Get<UIPetBrief>(i).Init(null, haveNotPets[i].id);
        }
    }

    void OnDropBlank(object data)
    {
        PetIconData iconData = data as PetIconData;
        if (iconData == null)
        {
            return;
        }
        if(!iconData.canDropOut)
        {
            return;
        }
        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        NetMgr.instance.PetHandler.SendUnchoosePet(enPetFormation.normal, iconData.pos);
    }

    void OnTeachAction(string arg)
    {
        switch (arg)
        {
            case "selectFirstExistsPetForDrag":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return;

                    for (int i = 0; i < m_pets.Count; ++i)
                    {
                        var uiItem = m_pets.Get<UIPetBrief>(i);
                        if (uiItem.Pet != null)
                        {
                            TeachMgr.instance.SetNextStepUIObjParam(uiItem.m_icon.transform as RectTransform, m_battleIconPet1Main.m_icon.transform as RectTransform);
                            break;
                        }
                    }
                }
                break;
            case "selectFirstExistsPetForClick":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return;

                    for (int i = 0; i < m_pets.Count; ++i)
                    {
                        var uiItem = m_pets.Get<UIPetBrief>(i);
                        if (uiItem.Pet != null)
                        {
                            TeachMgr.instance.SetNextStepUIObjParam(uiItem.m_button.transform as RectTransform, uiItem.m_icon.transform as RectTransform);
                            break;
                        }
                    }
                }
                break;
        }
    }

    bool OnTeachCheck(string arg)
    {
        switch (arg)
        {
            case "hasSelMainPet":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null)
                        return false;
                    var petFormation = hero.PetFormationsPart.GetPetFormation(enPetFormation.normal);
                    return !string.IsNullOrEmpty(petFormation.GetPetGuid(enPetPos.pet1Main)) || !string.IsNullOrEmpty(petFormation.GetPetGuid(enPetPos.pet2Main));
                }
        }

        return true;
    }
}