using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActivityMgr : Singleton<ActivityMgr>
{
    private ReqArenaChallengersResultVo m_arenaChallengers = null;
    private ReqArenaLogResultVo m_arenaLogs = null;
    private ReqTreasureRobResultVo m_treasureChallengers = null;
    private List<string> m_treasureRobMsgs = new List<string>();

    public void Init()
    {
        UIMainCity.AddClick(enSystem.activity, () =>
        {
            UIMgr.instance.Open<UIActivity>();
        });

        UIMainCity.AddClick(enSystem.arena, () =>
        {
            UIMgr.instance.Open<UIArena>();
        });
    }

    public void OnArenaChallengers(ReqArenaChallengersResultVo vo)
    {
        if (vo != null)
        {
            if (vo.clientNew)
            {
                Debuger.Log("OnArenaChallengers, client data is new");
                vo = m_arenaChallengers;
            }
            else
            {
                m_arenaChallengers = vo;
            }
        }
        UIMgr.instance.Get<UIArena>().OnUpdateChallengers(vo);
    }

    public long GetArenaChallengersListTime()
    {
        return m_arenaChallengers == null ? 0 : m_arenaChallengers.listTime;
    }

    public long GetArenaChallengersDataTime()
    {
        return m_arenaChallengers == null ? 0 : m_arenaChallengers.dataTime;
    }

    public int GetMyArenaRankVal()
    {
        return m_arenaChallengers == null ? -1 : m_arenaChallengers.myRankVal;
    }

    public void OnArenaLogs(ReqArenaLogResultVo vo)
    {
        List<UICombatLog.CombatLogItem> items = null;
        if (vo != null)
        {
            if (vo.clientNew)
            {
                Debuger.Log("OnArenaLogs, client data is new");
                vo = m_arenaLogs;
            }
            else
            {
                m_arenaLogs = vo;
            }

            if (vo.logs != null)
            {
                items = new List<UICombatLog.CombatLogItem>();
                foreach (var itemVo in vo.logs)
                {
                    var item = new UICombatLog.CombatLogItem();
                    item.win = itemVo.win;
                    item.oldRank = itemVo.oldRank;
                    item.rank = itemVo.rank;
                    item.opHeroId = itemVo.opHeroId;
                    item.opRoleId = itemVo.opRoleId;
                    item.opName = itemVo.opName;
                    item.iconName = ArenaGradeCfg.GetIconByScore(itemVo.opOldScore);
                    item.time = itemVo.time;
                    items.Add(item);
                }
            }
        }

        UIMgr.instance.Get<UICombatLog>().OnUpdateData(items);
    }

    public long GetLastArenaTime()
    {
        return m_arenaLogs == null || m_arenaLogs.logs == null || m_arenaLogs.logs.Count <= 0 ? 0 : m_arenaLogs.logs[0].time;
    }



    public void OnTreasureChallengers(ReqTreasureRobResultVo vo)
    {
        if (vo != null)
        {
            if (vo.clientNew)
            {
                Debuger.Log("OnTreasureChallengers, client data is new");
                vo = m_treasureChallengers;
            }
            else
            {
                m_treasureChallengers = vo;
            }
        }
        UIMgr.instance.Get<UITreasureRob>().Reflesh();
    }

    public long GetTreasureChallengersListTime()
    {
        return m_treasureChallengers == null ? 0 : m_treasureChallengers.listTime;
    }

    public long GetTreasureChallengersDataTime()
    {
        return m_treasureChallengers == null ? 0 : m_treasureChallengers.dataTime;
    }

    public List<TreasureRobChallengerVo> GetTreasureChallengers()
    {
        return m_treasureChallengers == null ? new List<TreasureRobChallengerVo>() : m_treasureChallengers.challengers;
    }

    public List<TreasureRobBattleLogVo> GetTreasureBattleLogs()
    {
        return m_treasureChallengers == null ? new List<TreasureRobBattleLogVo>() : m_treasureChallengers.battleLogs;
    }

    public void OnTreasureRobBattleLog(TreasureRobBattleLogVo vo)
    {
        if(m_treasureChallengers != null)
        {
            m_treasureChallengers.battleLogs.Add(vo);
            if(m_treasureChallengers.battleLogs.Count > 20)
            {
                m_treasureChallengers.battleLogs.RemoveAt(0);
            }
        }
    }

    public void CheckTreasureRobDlg()
    {
        if (m_treasureRobMsgs.Count >0)
        {
            for (int i = 0; i < m_treasureRobMsgs.Count; i++ )
            {
                UIMessage.Show(m_treasureRobMsgs[i]);
            }

            m_treasureRobMsgs.Clear();
        }
    }

    public void AddTreasureRobMsg(string msg)
    {
        m_treasureRobMsgs.Add(msg);
    }
}