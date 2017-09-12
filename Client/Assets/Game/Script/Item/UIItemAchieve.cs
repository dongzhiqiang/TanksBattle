using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIItemAchieve : MonoBehaviour
{
    public StateHandle m_state;
    public TextEx m_name;
    public StateHandle m_button;
    private int m_achieveId;
    private bool m_eventAdded = false;

    bool IsOpen()
    {
        ItemAchieveCfg achieveCfg = ItemAchieveCfg.m_cfgs[m_achieveId];
        ItemAchieveTypeCfg achieveTypeCfg = ItemAchieveTypeCfg.m_cfgs[achieveCfg.type];
        if (achieveTypeCfg.systemId == enSystem.none)
        {
            return false;
        }
        string msg;
        if (!SystemMgr.instance.IsEnabled(achieveTypeCfg.systemId, out msg))
        {
            return false;
        }
        if (achieveTypeCfg.systemId == enSystem.scene)
        {
            if (!string.IsNullOrEmpty(achieveCfg.param))
            {
                LevelsPart part = RoleMgr.instance.Hero.LevelsPart;
                RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(achieveCfg.param);
                if (RoomCfg.GetRoomCfgByID((roomCfg.preLevelId))!=null)
                if(!part.GetLevelInfoById(roomCfg.preLevelId).isWin)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void Init(int achieveId)
    {
        m_achieveId = achieveId;
        ItemAchieveCfg achieveCfg = ItemAchieveCfg.m_cfgs[achieveId];
        ItemAchieveTypeCfg achieveTypeCfg = ItemAchieveTypeCfg.m_cfgs[achieveCfg.type];
        m_name.text = achieveCfg.text;
        if(IsOpen())
        {
            m_state.SetState(0);
        }
        else
        {
            m_state.SetState(1);
        }
        if(!m_eventAdded)
        {
            m_eventAdded = true;
            m_button.AddClick(OnClick);
        }
    }

    void OnClick()
    {
        if(!IsOpen())
        {
            return;
        }
        ItemAchieveCfg achieveCfg = ItemAchieveCfg.m_cfgs[m_achieveId];
        ItemAchieveTypeCfg achieveTypeCfg = ItemAchieveTypeCfg.m_cfgs[achieveCfg.type];
        switch (achieveTypeCfg.systemId)
        {
            case enSystem.scene:
                if (!string.IsNullOrEmpty(achieveCfg.param))
                {
                    RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(achieveCfg.param);
                    UIMgr.instance.Open<UILevelDetail>(roomCfg);
                }
                else
                {
                    UIMgr.instance.Open<UILevelSelect>();
                }
                
                break;
            case enSystem.arena:
                UIMgr.instance.Open<UIArena>();
                break;
            case enSystem.dailyTask:
                UIMgr.instance.Open<UITask>();
                break;
            case enSystem.goldLevel:
            case enSystem.hadesLevel:
            case enSystem.guardLevel:
                UIMgr.instance.Open<UIActivity>();
                break;
            case enSystem.sign:
                UIMgr.instance.Open<UIOpActivity>();
                break;
            case enSystem.vip:
                UIMgr.instance.Open<UIOpActivity>();
                break;

        }
        this.GetComponentInParent<UIPanel>().Close();
    }

}
