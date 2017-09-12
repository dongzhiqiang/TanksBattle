using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITreasureRobBattleLog : MonoBehaviour
{
    public Text m_time;
    public Text m_txt;
    public StateHandle m_button;
    public StateHandle m_state;
    private TreasureRobBattleLogVo m_battleLog;
    private int m_index;
    private bool m_eventAdded = false;

    public void Init(TreasureRobBattleLogVo battleLog, int index)
    {
        m_battleLog = battleLog;
        m_index = index;
        int state = 0;

        long difTime = TimeMgr.instance.GetTimestamp() - battleLog.time;
        if(difTime < 60)
        {
            m_time.text = string.Format("{0}秒前", difTime);
        }
        else if (difTime < 3600)
        {
            m_time.text = string.Format("{0}分钟前", difTime/60);
        }
        else if (difTime < 3600*24)
        {
            m_time.text = string.Format("{0}小时前", difTime/3600);
        }
        else
        {
            m_time.text = string.Format("{0}天前", difTime / (3600*24));
        }

        if(battleLog.iStart)
        {
            if(battleLog.iWin)
            {
                m_txt.text = string.Format(LanguageCfg.Get("treasure_rob_i_win"), battleLog.name, ItemCfg.m_cfgs[battleLog.itemId].name, battleLog.itemNum);
            }
            else
            {
                m_txt.text = string.Format(LanguageCfg.Get("treasure_rob_i_lose"), battleLog.name);
            }
        }
        else
        {
            if (battleLog.iWin)
            {
                m_txt.text = string.Format(LanguageCfg.Get("treasure_rob_op_lose"), battleLog.name, ItemCfg.m_cfgs[battleLog.itemId].name);
            }
            else
            {
                m_txt.text = string.Format(LanguageCfg.Get("treasure_rob_op_win"), battleLog.name, ItemCfg.m_cfgs[battleLog.itemId].name, battleLog.itemNum);
                if(!battleLog.revenged)
                {
                    state = 1;
                }
            }
        }

        m_state.SetState(state);

        if(!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_eventAdded = true;
        }
    }

    void OnClick()
    {
        // TODO 判断次数
        NetMgr.instance.ActivityHandler.SendStartTreasureRob(m_battleLog.heroId, m_index);
    }

}
