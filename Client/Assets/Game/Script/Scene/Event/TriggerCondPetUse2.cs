using UnityEngine;
using System.Collections;

public class TriggerCondPetUse2 : SceneTrigger
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

        string v1 = m_conditionCfg.stringValue1;
        string v2 = m_conditionCfg.stringValue2;
        if (!string.IsNullOrEmpty(v1) && string.IsNullOrEmpty(v2))     //只携带一只宠物通关
        {
            if (pet1Id == v1 || pet2Id == v1)
            {
                bAchieve = true;
                return;
            }
            else
            {
                bAchieve = false;
                return;
            }
        }
        else //两只都携带
        {
            if ((pet1Id == v1 || pet2Id == v1) && (pet1Id == v2 || pet2Id == v2))
            {
                bAchieve = true;
                return;
            }
            else
            {
                bAchieve = false;
                return;
            }
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
