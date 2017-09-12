using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class PetFormation
{
    #region Fields
    public int formationId;     //队形ID
    public List<string> formation;       //队形
    #endregion

    #region Properties
    #endregion

    private static PropertyTable m_temp = new PropertyTable();
    private static PropertyTable m_temp2 = new PropertyTable();

    PetFormationsPart m_owner;

    public static PetFormation Create(PetFormationVo vo, PetFormationsPart owner)
    {
        PetFormation petFormation;
        petFormation = new PetFormation();
        petFormation.LoadFromVo(vo);
        petFormation.m_owner = owner;
        return petFormation;
    }

    public void SetOwner(PetFormationsPart owner)
    {
        this.m_owner = owner;
    }

    virtual public void LoadFromVo(PetFormationVo vo)
    {
        formationId = vo.formationId;
        formation = vo.formation;
    }

    public string GetPetGuid(enPetPos petPos)
    {
        int index = (int)petPos;
        if (formation == null || formation.Count <= index)
        {
            Debuger.LogError("阵型列表长度错误");
            return "";
        }
        return formation[index] == null ? "" : formation[index];
    }

    public List<Role> GetMainPets()
    {
        List<Role> roleList = new List<Role>();
        string guid1 = GetPetGuid(enPetPos.pet1Main);
        string guid2 = GetPetGuid(enPetPos.pet2Main);

        Role role = m_owner.Parent;
        if (!string.IsNullOrEmpty(guid1))
        {
            Role pet1 = role.PetsPart.GetPet(guid1);
            if (pet1 != null)
                roleList.Add(pet1);
        }

        if (!string.IsNullOrEmpty(guid2))
        {
            Role pet2 = role.PetsPart.GetPet(guid2);
            if (pet2 != null)
                roleList.Add(pet2);
        }

        return roleList;
    }

    //如果给到宠物是辅助战斗宠物，返回对应的主位宠物，否则返回null
    public Role GetMainPet(string guid)
    {
        if (GetPetGuid(enPetPos.pet1Main) == guid || GetPetGuid(enPetPos.pet1Sub1) == guid || GetPetGuid(enPetPos.pet1Sub2) == guid)
        {
            if (GetPetGuid(enPetPos.pet1Main) != "")
            {
                return m_owner.Parent.PetsPart.GetPet(GetPetGuid(enPetPos.pet1Main));
            }
            else
            {
                return null;
            }
        }
        if (GetPetGuid(enPetPos.pet2Main) == guid || GetPetGuid(enPetPos.pet2Sub1) == guid || GetPetGuid(enPetPos.pet2Sub2) == guid)
        {
            if (GetPetGuid(enPetPos.pet2Main) != "")
            {
                return m_owner.Parent.PetsPart.GetPet(GetPetGuid(enPetPos.pet2Main));
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    public void FreshMainPetProps()
    {
        List<Role> mainPets = GetMainPets();
        foreach (Role mainPet in mainPets)
        {
            mainPet.PropPart.FreshBaseProp();
        }

    }

    //用于过渡的临时函数
    /*
    public static enPetPos OldPosToNewPos(enProp pos)
    {
        switch(pos)
        {
            case enProp.pet1Main:
                return enPetPos.pet1Main;
            case enProp.pet1Sub1:
                return enPetPos.pet1Sub1;
            case enProp.pet1Sub2:
                return enPetPos.pet1Sub2;
            case enProp.pet2Main:
                return enPetPos.pet2Main;
            case enProp.pet2Sub1:
                return enPetPos.pet2Sub1;
            case enProp.pet2Sub2:
                return enPetPos.pet2Sub2;
        }
        return enPetPos.pet1Main;
    }

    public static enProp NewPosToOldPos(enPetPos pos)
    {
        switch (pos)
        {
            case enPetPos.pet1Main:
                return enProp.pet1Main;
            case enPetPos.pet1Sub1:
                return enProp.pet1Sub1;
            case enPetPos.pet1Sub2:
                return enProp.pet1Sub2;
            case enPetPos.pet2Main:
                return enProp.pet2Main;
            case enPetPos.pet2Sub1:
                return enProp.pet2Sub1;
            case enPetPos.pet2Sub2:
                return enProp.pet2Sub2;
        }
        return enProp.pet1Main;
    }
     */
}