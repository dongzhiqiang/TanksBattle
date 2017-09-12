using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIListItemPet : MonoBehaviour
{
    public ImageEx m_petIcon;
    public ImageEx m_selImage;
    public ImageEx m_operation;

    void Awake()
    {
        BowListItem listItem = GetComponent<BowListItem>();
        listItem.SetSetDataAction(SetData);
        listItem.SetSetSelectedAction(SetSelected);
    }

    public void SetData(object data)
    {
        if(!(data is Role))
        {
            return;
        }
        Role pet = (Role)data;
        RoleCfg roleCfg = pet.Cfg;
        //Debug.Log(equipCfg.icon);
        m_petIcon.Set(roleCfg.icon);
        m_operation.gameObject.SetActive(pet.PetsPart.CanOperateAndIsBattle());
    }

    public void SetSelected(bool selected)
    {
        m_selImage.gameObject.SetActive(selected);
    }


}