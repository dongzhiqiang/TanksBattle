using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIHadesMonsterIcon : MonoBehaviour
{
    public ImageEx m_blood;
    public GameObject m_skillBar;




    public void FreshRole(Role role)
    {
        if (role == null) gameObject.SetActive(false);
        else gameObject.SetActive(true);
        // 计算坐标
        float x = -role.transform.localPosition.x*2.1f-10;
        float y = -role.transform.localPosition.z*2.1f+7;
        this.transform.localPosition = new Vector3(x, y);

        if (m_skillBar!=null)
        {
            Skill curSkill = role.CombatPart.CurSkill;
            if(curSkill !=null && curSkill.Cfg.skillId == "tianxue")
            {
                m_skillBar.SetActive(true);
                float percent = 1f;
                if(curSkill.MaxFrame!=0)
                {
                    percent = (float)curSkill.CurFrame / curSkill.MaxFrame;
                    m_blood.fillAmount = percent;
                }
            }
            else
            {
                m_skillBar.SetActive(false);
            }
        }
        else
        {
            if (m_blood != null)
            {
                float percent = role.GetFloat(enProp.hp) / role.GetFloat(enProp.hpMax);
                m_blood.fillAmount = percent;
            }
        }



    }


}
