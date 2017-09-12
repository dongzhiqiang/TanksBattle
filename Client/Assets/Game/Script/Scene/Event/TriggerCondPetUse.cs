using UnityEngine;
using System.Collections;

public class TriggerCondPetUse : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
    }

    public override void Start()
    {
        base.Start();

        bAchieve = false;


        string pet1Id = "";
        string pet2Id = "";

        PetFormation petFormation = RoleMgr.instance.Hero.PetFormationsPart.GetCurPetFormation();
        string guid1 = petFormation.GetPetGuid(enPetPos.pet1Main);
        string guid2 = petFormation.GetPetGuid(enPetPos.pet2Main);

        if (!string.IsNullOrEmpty(guid1))
        {
            Role pet1 = RoleMgr.instance.Hero.PetsPart.GetPet(guid1);
            if (pet1 != null)
                pet1Id = pet1.Cfg.id;
        }

        if (!string.IsNullOrEmpty(guid2))
        {
            Role pet2 = RoleMgr.instance.Hero.PetsPart.GetPet(guid2);
            if (pet2 != null)
                pet2Id = pet2.Cfg.id;
        }
        

        if (string.IsNullOrEmpty(m_conditionCfg.stringValue1))  //不携带任何宠物通关
        {
            if (!string.IsNullOrEmpty(pet1Id) || !string.IsNullOrEmpty(pet2Id)) //携带了就失败
                bAchieve = false;
            else
                bAchieve = true;
        }

        if (!string.IsNullOrEmpty(m_conditionCfg.stringValue1))     //指定只携带一只宠物通关
        {
            if (!string.IsNullOrEmpty(pet1Id) && !string.IsNullOrEmpty(pet2Id)) //两只都携带 失败
            {
                bAchieve = false;
                return;
            }

            if (!string.IsNullOrEmpty(pet1Id) && pet1Id == m_conditionCfg.stringValue1)   //第一只是要求宠物
                bAchieve = true;

            if (!string.IsNullOrEmpty(pet2Id) && pet2Id == m_conditionCfg.stringValue1)   //第二只是要求宠物
                bAchieve = true;
        }

    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
    }

    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

}
