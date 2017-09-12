using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetBondItem : MonoBehaviour
{
    public StateGroup m_pets;
    public Text m_name;
    public StateHandle m_detail;
    public Text m_description;
    public Text m_activeNot;
    private bool m_eventAdded = false;
    private int m_bondId;
    private string m_petId;

    public void Init(int bondId, string petId)
    {
        m_bondId = bondId;
        m_petId = petId;

        PetBondCfg petBondCfg = PetBondCfg.m_cfgs[bondId];
        m_name.text = petBondCfg.name;


        Role role = RoleMgr.instance.Hero;
        List<string> bondPets = PetBondCfg.GetBondPets(bondId);
        int minStar = 5;

        bool active = true;

        m_pets.SetCount(bondPets.Count);

        for (int i = 0; i < bondPets.Count; i++)
        {
            RoleCfg petCfg = RoleCfg.Get(bondPets[i]);

            int star = 0;
            bool hasPet = role.PetsPart.HasPet(bondPets[i], out star);

            if (!hasPet)
            {
                star = petCfg.initStar;
                active = false;
            }

            if (star < minStar)
            {
                minStar = star;
            }

            m_pets.Get<UIPetIcon>(i).Init(bondPets[i], !hasPet, star);
        }

        m_activeNot.gameObject.SetActive(!active);
        
        if (!m_eventAdded)
        {
            m_detail.AddClick(OnDetailClick);
            m_eventAdded = true;
        }

        m_description.text = LvValue.ParseText(petBondCfg.desc, minStar);
        
    }

    void OnDetailClick()
    {
        UIMgr.instance.Open<UIPetBond>(m_petId);
    }


}
