using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIEliteLevelDescription : MonoBehaviour
{
    public StateHandle m_close;
    public Text m_description;

    public void Init()
    {
        m_close.AddClick(OnClose);
    }

    public void Open(int levelId)
    {
        gameObject.SetActive(true);

        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[levelId];
        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(eliteLevelCfg.roomId);
        m_description.text = roomCfg.roomStory;
    }


    void OnClose()
    {
        gameObject.SetActive(false);
    }
}
