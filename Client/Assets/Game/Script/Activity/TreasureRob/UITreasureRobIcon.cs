using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITreasureRobIcon : MonoBehaviour
{
    public Text m_name;
    public UIItemIcon m_item;
    public StateHandle m_button;
    public StateHandle m_background;
    public Text m_power;
    public Text m_level;
    private TreasureRobChallengerVo m_challenger;
    private bool m_eventAdded = false;

    public void Init(TreasureRobChallengerVo challenger)
    {
        m_challenger = challenger;

        m_name.text = challenger.info.name;
        m_item.Init(challenger.itemId, challenger.itemNum);
        m_background.SetState(challenger.itemNum == 10 ? 0 : 1);
        m_power.text = challenger.info.power.ToString();
        m_level.text = "Lv." + challenger.info.level;

        if(!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_eventAdded = true;
        }
    }

    void OnClick()
    {
        // TODO 判断次数
        NetMgr.instance.ActivityHandler.SendStartTreasureRob(m_challenger.info.key, -1);
    }

}
