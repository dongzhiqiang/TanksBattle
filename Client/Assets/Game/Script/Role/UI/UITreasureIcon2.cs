using UnityEngine;
using System.Collections;

public class UITreasureIcon2 : MonoBehaviour
{
    public ImageEx m_icon;
    public TextEx m_nameOrLevel;
    public int nameLvType = 0;
    public TextEx m_isBattle;
    public StateHandle m_battlePos;    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="treasureId"></param>
    /// <param name="level"></param>
    /// <param name="nameLvType">0 名字、等级，1 只名字，2 只等级</param>
    /// <param name="battlePos">0-2，出战位置，3不出战</param>
    public void Init(int treasureId, int level, int battlePos = -1)
    {
        var treasureCfg = treasureId <= 0 ? null : TreasureCfg.m_cfgs[treasureId];

        m_icon.Set(treasureCfg == null ? null : treasureCfg.icon);

        if (treasureCfg == null)
        {
            m_nameOrLevel.text = "";

            if (m_isBattle != null)
                m_isBattle.gameObject.SetActive(false);
            if (m_battlePos != null)
                m_battlePos.SetState(3);
            foreach (ImageEx image in GetComponentsInChildren<ImageEx>())
                image.SetGrey(true);
        }            
        else
        {
            if (m_nameOrLevel != null)
            {
                switch (nameLvType)
                {
                    case 1:
                        m_nameOrLevel.text = treasureCfg.name;
                        break;
                    case 2:
                        m_nameOrLevel.text = "Lv." + level;
                        break;
                    default:
                        m_nameOrLevel.text = treasureCfg.name + " Lv." + level;
                        break;
                }
            }

            if (m_isBattle != null)
                m_isBattle.gameObject.SetActive(battlePos >= 0);
            if (m_battlePos != null)
                m_battlePos.SetState(battlePos < 0 ? 3 : battlePos);
            if (level > 0)
            {
                foreach (ImageEx image in GetComponentsInChildren<ImageEx>())
                    image.SetGrey(false);
            }
            else
            {
                foreach (ImageEx image in GetComponentsInChildren<ImageEx>())
                    image.SetGrey(true);
            }
        }
    }
}
