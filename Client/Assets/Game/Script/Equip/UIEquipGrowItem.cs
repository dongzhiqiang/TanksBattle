using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIEquipGrowItem : MonoBehaviour
{
    public UIEquipIcon m_oldIcon;
    public UIEquipIcon m_newIcon;

    public void Init(GrowEquipVo grow)
    {
        m_oldIcon.Init(grow.oldEquip.equipId, grow.oldEquip.level, grow.oldEquip.advLv);
        m_newIcon.Init(grow.newEquip.equipId, grow.newEquip.level, grow.newEquip.advLv);
    }


}
