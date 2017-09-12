using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[NetModule(MODULE.MODULE_PET)]
public class PetHandler
{
    [NetHandler(MODULE_PET.PUSH_REMOVE_PET)]
    public void RemovePetRoleVo(RemovePetRoleVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.PetsPart.RemovePet(info.guid);        
    }

    [NetHandler(MODULE_PET.PUSH_ADD_PET)]
    public void OnAddPet(FullRoleInfoVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.PetsPart.AddPet(info);
    }

    [NetHandler(MODULE_PET.PUSH_ADD_OR_UPDATE_PET_SKILL)]
    public void OnAddOrUpdatePetSkill(AddOrUpdatePetSkillVo info)
    {
        Role role = RoleMgr.instance.Hero;
        Role pet = role.PetsPart.GetPet(info.guid);
        PetSkillsPart petSkillsPart = pet.PetSkillsPart;
        petSkillsPart.AddOrUpdatePetSkill(info.petSkill);
    }

    [NetHandler(MODULE_PET.PUSH_ADD_OR_UPDATE_TALENT)]
    public void OnAddOrUpdateTalent(AddOrUpdateTalentVo info)
    {
        Role role = RoleMgr.instance.Hero;
        Role pet = role.PetsPart.GetPet(info.guid);
        TalentsPart talentsPart = pet.TalentsPart;
        talentsPart.AddOrUpdateTalent(info.talent);
        //刷新计算属性
        pet.PropPart.FreshBaseProp();
        //如果是装备辅助位的宠物，还要刷新主位宠物的计算属性
        Role mainPet = role.PetsPart.GetMainPet(info.guid);
        if (mainPet != null)
        {
            mainPet.PropPart.FreshBaseProp();
        }
    }

    //发送,升级
    public void SendUpgradePet(string guid, int itemId, int num)
    {
        UpgradePetRequestVo request = new UpgradePetRequestVo();
        request.guid = guid;
        request.itemId = itemId;
        request.num = num;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_UPGRADE_PET, request);
    }

    //接收，升级
    [NetHandler(MODULE_PET.CMD_UPGRADE_PET)]
    public void OnUpgrade(UpgradePetResultVo info)
    {
        
        if (info.addLv > 0)
        {
            //UIFxPanel.ShowFx("fx_ui_shengjichenggong");
            var uiPet = UIMgr.instance.Get<UIPet>();
            if (uiPet != null)
            {// 特效播放移到宠物界面了
                if (UIMgr.instance.Get<UIChoosePet>() != null)
                {
                    UIMgr.instance.Fresh<UIChoosePet>();
                }
                if (UIMgr.instance.Get<UIPet>() != null)
                {
                    UIMgr.instance.Get<UIPet>().Refresh();
                }

                var pet = uiPet.CurPet;
                if (pet != null && pet.GetInt(enProp.level) >= 5)
                    TeachMgr.instance.OnDirectTeachEvent("pet", "petLvReach5");
            }            
        }
        else
        {
            if (UIMgr.instance.Get<UIChoosePet>() != null)
            {
                UIMgr.instance.Fresh<UIChoosePet>();
            }
            if (UIMgr.instance.Get<UIPet>() != null)
            {
                UIMgr.instance.Get<UIPet>().Refresh();
            }
        }
    }

    //发送,进阶
    public void SendAdvancePet(string guid)
    {
        AdvancePetRequestVo request = new AdvancePetRequestVo();
        request.guid = guid;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_ADVANCE_PET, request);
    }

    //接收，进阶
    [NetHandler(MODULE_PET.CMD_ADVANCE_PET)]
    public void OnAdvance()
    {
        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().m_pageAdvance.StartAdvanceFx();
        }
    }

    //发送,升星
    public void SendUpstarPet(string guid)
    {
        UpstarPetRequestVo request = new UpstarPetRequestVo();
        request.guid = guid;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_UPSTAR_PET, request);
    }

    //接收，升星
    [NetHandler(MODULE_PET.CMD_UPSTAR_PET)]
    public void OnUpstar(UpstarPetResultVo info)
    {
        //星级改变了，因为羁绊变更要对所有宠物算一次属性
        Role role = RoleMgr.instance.Hero;
        role.PetsPart.FreshPetProps();

        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().m_pageUpstar.StartUpstarFx(info.newStar);
        }
    }

    //发送,选择战斗宠物
    public void SendChoosePet(string guid, enPetFormation petFormation, enPetPos petPos)
    {
        ChoosePetRequestVo request = new ChoosePetRequestVo();
        request.guid = guid;
        request.petFormation = (int)(petFormation);
        request.petPos = (int)(petPos);
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_CHOOSE_PET, request);
    }

    //接收，选择战斗宠物
    [NetHandler(MODULE_PET.CMD_CHOOSE_PET)]
    public void OnChoosePet(ChoosePetResultVo info)
    {
        if (info==null)
        {
            return;
        }

        var hero = RoleMgr.instance.Hero;

        var setMainPet = false;
        PetFormation petFormation = hero.PetFormationsPart.GetPetFormation(enPetFormation.normal);
        var pet1Main = petFormation.GetPetGuid(enPetPos.pet1Main);
        var pet2Main = petFormation.GetPetGuid(enPetPos.pet2Main);
        foreach (string guid in info.needUpdatePets)
        {
            Role pet = hero.PetsPart.GetPet(guid);
            if(pet != null)
            {
                pet.PropPart.FreshBaseProp();

                if (guid == pet1Main || guid == pet2Main)
                    setMainPet = true;
            }
        }
        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPetFormation>().IsOpen)
        {
            UIMgr.instance.Get<UIPetFormation>().Refresh();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().Refresh();
        }
        if (setMainPet)
        {
            TeachMgr.instance.OnDirectTeachEvent("pet", "setMainPet");
        }
        RoleMgr.instance.Hero.Fire(MSG_ROLE.PROP_CHANGE + (int)enProp.power);    
        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(false);
    }


    //发送,升级技能
    public void SendUpgradePetSkill(string guid, string skillId)
    {
        UpgradePetSkillRequestVo request = new UpgradePetSkillRequestVo();
        request.guid = guid;
        request.skillId = skillId;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_UPGRADE_PET_SKILL, request);
    }

    //接收，升级技能
    [NetHandler(MODULE_PET.CMD_UPGRADE_PET_SKILL)]
    public void OnUpgradePetSkill(UpgradePetSkillResultVo info)
    {
        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().m_pageSkill.StartFx(info.skillId);
        }
    }

    //发送,升级天赋
    public void SendUpgradePetTalent(string guid, string talentId)
    {
        UpgradePetTalentRequestVo request = new UpgradePetTalentRequestVo();
        request.guid = guid;
        request.talentId = talentId;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_UPGRADE_PET_TALENT, request);
    }

    //接收，升级天赋
    [NetHandler(MODULE_PET.CMD_UPGRADE_PET_TALENT)]
    public void OnUpgradePetTalent(UpgradePetTalentResultVo info)
    {
        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().m_pageTalent.StartFx(info.talentId);
        }
    }

    //发送,选择不战斗宠物
    public void SendUnchoosePet(enPetFormation petFormation, enPetPos petPos)
    {
        UnchoosePetRequestVo request = new UnchoosePetRequestVo();
        request.petFormation = (int)(petFormation);
        request.petPos = (int)(petPos);
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_UNCHOOSE_PET, request);
    }

    //接收，选择不战斗宠物
    [NetHandler(MODULE_PET.CMD_UNCHOOSE_PET)]
    public void OnUnchoosePet(ChoosePetResultVo info)
    {
        if (info == null)
        {
            return;
        }
        foreach (string guid in info.needUpdatePets)
        {
            Role pet = RoleMgr.instance.Hero.PetsPart.GetPet(guid);
            if (pet != null)
            {
                pet.PropPart.FreshBaseProp();
            }
        }
        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPetFormation>().IsOpen)
        {
            UIMgr.instance.Get<UIPetFormation>().Refresh();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().Refresh();
        }
        RoleMgr.instance.Hero.Fire(MSG_ROLE.PROP_CHANGE + (int)enProp.power);       
        UIPowerUp.SaveNewProp(RoleMgr.instance.Hero);
        UIPowerUp.ShowPowerUp(false);
    }

    //发送,招募宠物
    public void SendRecruitPet(string roleId)
    {
        RecruitPetRequestVo request = new RecruitPetRequestVo();
        request.roleId = roleId;
        NetMgr.instance.Send(MODULE.MODULE_PET, MODULE_PET.CMD_RECRUIT_PET, request);
    }

    //接收，招募宠物
    [NetHandler(MODULE_PET.CMD_RECRUIT_PET)]
    public void OnRecruitPet(RecruitPetResultVo info)
    {
        //因为羁绊变更要对所有宠物算一次属性
        Role role = RoleMgr.instance.Hero;
        role.PetsPart.FreshPetProps();

        if (UIMgr.instance.Get<UIChoosePet>() != null)
        {
            UIMgr.instance.Fresh<UIChoosePet>();
        }
        if (UIMgr.instance.Get<UIPet>() != null)
        {
            UIMgr.instance.Get<UIPet>().Refresh();
        }

        Role pet = role.PetsPart.GetPet(info.guid);
        if(pet == null)
        {
            Debuger.LogError("招募宠物返回的guid错误");
            return;
        }
        UIPetRecruitContext cxt = new UIPetRecruitContext(pet.GetString(enProp.roleId), pet.GetInt(enProp.star));
        UIMgr.instance.Open<UIPetRecruit>(cxt);
    }

    [NetHandler(MODULE_PET.PUSH_ADD_OR_UPDATE_PET_FORMATION)]
    public void OnAddOrUpdatePetFormation(AddOrUpdatePetFormationVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.PetFormationsPart.AddOrUpdatePetFormation(info.petFormation);
        //role.PropPart.FreshBaseProp();
    }
}