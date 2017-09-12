using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetBondDetail : MonoBehaviour
{
    public StateGroup m_pets;
    public Text m_name;
    public Text m_description;
    public Text m_descriptionNext;
    public Text m_star;
    public Text m_starNext;
    //private bool m_eventAdded = false;
    private int m_bondId;


    public void Init(int bondId)
    {
        m_bondId = bondId;

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

        if (active)
        {
            m_star.text = minStar + "星";
        }
        else
        {
            m_star.text = "未激活";
        }
        


        m_description.text = "当前星效果："+LvValue.ParseText(petBondCfg.desc, minStar);
        if(minStar<5)
        {
            m_descriptionNext.gameObject.SetActive(true);
            m_descriptionNext.text = "下一星效果：" + LvValue.ParseText(petBondCfg.desc, minStar+1);
            m_starNext.gameObject.SetActive(true);
            m_starNext.text = (minStar+1) + "星";
        }
        else
        {
            m_descriptionNext.gameObject.SetActive(false);
            m_starNext.gameObject.SetActive(false);
        }
        
    }



}
