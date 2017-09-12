using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEliteLevelStar : MonoBehaviour
{
    public StateHandle m_close;
    public List<ImageEx> m_stars = new List<ImageEx>();
    public List<TextEx> m_conditions = new List<TextEx>();

    public void Init()
    {
        m_close.AddClick(OnClose);
    }

    public void Open(int levelId)
    {
        gameObject.SetActive(true);

        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[levelId];
        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(eliteLevelCfg.roomId);
        Role hero = RoleMgr.instance.Hero;
        EliteLevel eliteLevel = hero.EliteLevelsPart.GetEliteLevel(levelId);

        List<int> taskIds = roomCfg.GetTaskIdList();
        if (taskIds.Count != 3)
            Debuger.LogError("配置的通关条件不是3个");
        else
        {
            for (int i = 0; i < m_conditions.Count; i++)
            {
                m_conditions[i].text = "";
                RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);

                if (eliteLevel != null && eliteLevel.GetStarState(conditionCfg.id.ToString()) == 1)
                {
                    m_stars[i].gameObject.SetActive(true);
                    m_conditions[i].text = string.Format("<color=green>{0}</color>", conditionCfg.endDesc);
                }
                else
                {
                    m_stars[i].gameObject.SetActive(false);
                    m_conditions[i].text = conditionCfg.endDesc;  
                }
            }
        }
    }


    void OnClose()
    {
        gameObject.SetActive(false);
    }
}
