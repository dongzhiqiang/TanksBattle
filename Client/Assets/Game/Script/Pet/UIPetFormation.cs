using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetFormation : UIPanel
{
    #region SerializeFields
    public StateGroup m_navBar;
    public StateGroup m_pets;
    public UIPetBattleIcon m_battleIconPet1Main;
    public UIPetBattleIcon m_battleIconPet1Sub1;
    public UIPetBattleIcon m_battleIconPet1Sub2;
    public UIPetBattleIcon m_battleIconPet2Main;
    public UIPetBattleIcon m_battleIconPet2Sub1;
    public UIPetBattleIcon m_battleIconPet2Sub2;
    public DropControl m_dropBlank;
    #endregion
    private enPetFormation m_petFormationId;


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
        m_petFormationId = (enPetFormation)param;
        m_battleIconPet1Main.SetFormationId(m_petFormationId);
        m_battleIconPet1Sub1.SetFormationId(m_petFormationId);
        m_battleIconPet1Sub2.SetFormationId(m_petFormationId);
        m_battleIconPet2Main.SetFormationId(m_petFormationId);
        m_battleIconPet2Sub1.SetFormationId(m_petFormationId);
        m_battleIconPet2Sub2.SetFormationId(m_petFormationId);
        UpdatePets();
        UpdateBattlePets();
        GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void Refresh()
    {
        UpdatePets();
        UpdateBattlePets();
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

        m_pets.SetCount(pets.Count);
        for (int i = 0; i < pets.Count; i++)
        {
            m_pets.Get<UIPetBrief>(i).Init(pets[i], null);
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
        NetMgr.instance.PetHandler.SendUnchoosePet(m_petFormationId, iconData.pos);
    }

}