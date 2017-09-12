using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelsPart : RolePart
{

    #region Constant
    #endregion

    #region Fields
    Dictionary<string, LevelInfo> m_levels;
    string m_curLevelId;
    string m_curNodeId;
    Dictionary<string, Dictionary<string, int>> m_starsReward;

    #endregion


    #region Properties
    public override enPart Type { get { return enPart.levels; } }
    public string CurLevelId { get { return m_curLevelId; } set { m_curLevelId = value; } }
    public string CurNodeId { get { return m_curNodeId; } set { m_curNodeId = value; } }
    public Dictionary<string, Dictionary<string, int>> StarsReward { get { return m_starsReward; } set { m_starsReward = value; } }
    public Dictionary<string, LevelInfo> Levels { get { return m_levels; } }

    public List<ItemVo> DropBossItems { get; set; }
    public List<ItemVo> DropSpecialItems { get; set; }
    public List<ItemVo> DropMonsterItems { get; set; }
    public List<ItemVo> DropBoxItems { get; set; }
    #endregion


    #region Frame
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        m_levels = new Dictionary<string, LevelInfo>();
        if (vo.levelInfo != null)
        {
            m_curLevelId = vo.levelInfo.curLevel;
            m_curNodeId = vo.levelInfo.curNode;
            m_starsReward = vo.levelInfo.starsReward;
            if (m_starsReward == null)
                m_starsReward = new Dictionary<string, Dictionary<string, int>>();
            foreach (var info in vo.levelInfo.levels)
            {
                m_levels.Add(info.Key, info.Value);
            }
        }
    }

    public void UpdateLevelInfo(LevelInfo info)
    {
        if (info == null)
            return;

        if (m_levels.ContainsKey(info.roomId))
            m_levels[info.roomId] = info;
        else
            m_levels.Add(info.roomId, info);
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_levels != null)
            m_levels.Clear();
    }


    //获取星星数
    public int GetStarsByNodeId(string nodeId)
    {
        int starsNum = 0;

        foreach (LevelInfo info in m_levels.Values)
        {
            if (info.nodeId == nodeId)
            {
                if (info.starsInfo.Count > 3)
                    Debuger.LogError("关卡{0}的通关条件可能修改了，要先清空关卡", info.nodeId);
                foreach (int v in info.starsInfo.Values)
                {
                    starsNum += v;
                }
            }

        }

        return starsNum;
    }

    public int GetAllStars()
    {
        int starsNum = 0;
        foreach (LevelInfo info in m_levels.Values)
        {
            if (info.starsInfo.Count > 3)
                Debuger.LogError("关卡{0}的通关条件可能修改了，要先清空关卡", info.nodeId);
            foreach (int v in info.starsInfo.Values)
            {
                starsNum += v;
            }
        }
        return starsNum;
    }

    //获取已经打过的章节里的关卡个数
    public int GetLevelNumByNodeId(string nodeId)
    {
        int levelNum = 0;

        foreach (LevelInfo info in m_levels.Values)
        {
            if (info.nodeId == nodeId)
                levelNum++;
        }

        return levelNum;
    }

    //获取已经通关的章节里的关卡个数
    public int GetWinNumByNodeId(string nodeId)
    {
        int levelNum = 0;

        foreach (LevelInfo info in m_levels.Values)
        {
            if (info.nodeId == nodeId && info.isWin)
                levelNum++;
        }

        return levelNum;
    }

    public LevelInfo GetLastLevelByNodeId(string nodeId)
    {
        LevelInfo levelInfo = null;
        foreach (LevelInfo info in m_levels.Values)
        {
            if (info.nodeId == nodeId)
            {
                if (levelInfo == null)
                    levelInfo = info;

                if (int.Parse(info.roomId) > int.Parse(levelInfo.roomId))
                    levelInfo = info;
            }
        }

        return levelInfo;
    }

    //获取要打的章节ID  //默认取打通关的 传false是取所有记录的关卡的最后一关
    public string GetLastNodeId(bool bWin = true)
    {
        string nodeId = "0";
        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(m_curLevelId);
        if (roomCfg == null)
            return nodeId;

        int haveNum = GetLevelNumByNodeId(roomCfg.roomNodeId);

        if (bWin)
        {
            LevelInfo levelInfo = GetLastLevelByNodeId(roomCfg.roomNodeId);
            if (levelInfo != null && !levelInfo.isWin)
                haveNum--;
        }

        List<RoomCfg> roomList;
        RoomCfg.mRoomDict.TryGetValue(roomCfg.roomNodeId, out roomList);
        if (roomList == null)
            return nodeId;

        if (haveNum >= roomList.Count)
        {
            nodeId = (int.Parse(roomCfg.roomNodeId) + 1).ToString();
            if (RoomCfg.mRoomDict.ContainsKey(nodeId))
                return nodeId;
            else
                return roomCfg.roomNodeId;
        }
        else
            return roomCfg.roomNodeId;
    }

    public LevelInfo GetLevelInfoById(string levelId)
    {
        LevelInfo level;
        if (m_levels.TryGetValue(levelId, out level))
            return level;
        level = new LevelInfo();
        level.roomId = levelId;
        return level;
    }

    public RoomCfg GetPrevLevel(RoomCfg cfg)
    {
        int n = int.Parse(cfg.id);
        int id = n % 10;
        if (id <= 1 && n < 9)
            return null;
        else
            return RoomCfg.GetRoomCfgByID((n - 1).ToString());

    }
    public RoomCfg GetNextLevel(RoomCfg cfg)
    {
        int n = int.Parse(cfg.id);
        int id = n % 10;
        if (id < 10)
        {
            string roomId = (n + 1).ToString();
            if (CanOpenLevel(roomId))
                return RoomCfg.GetRoomCfgByID(roomId);
            else
                return null;
        }
        else
            return null;
    }

    public int GetEnterNum(LevelInfo info)
    {
        int enterNum = info.enterNum;
        if (!TimeMgr.instance.IsToday(info.lastEnter))
            enterNum = 0;
        return enterNum;
    }

    public RoomCfg GetCurChallengeLevel()
    {
        string levelId = CurLevelId;
        if (levelId != "0")
        {
            if (GetLevelInfoById(levelId).isWin)
                levelId = (int.Parse(levelId) + 1).ToString();
            return RoomCfg.GetRoomCfgByID(levelId);
        }
        return RoomCfg.GetRoomCfgByID("1");
    }

    public RoomCfg GetCurChallengeNode()
    {
        string levelId = CurLevelId;
        if (levelId != "0")
        {
            if (GetLevelInfoById(levelId).isWin)
                levelId = (int.Parse(levelId) + 1).ToString();
            return RoomCfg.GetRoomCfgByID(levelId);
        }
        return RoomCfg.GetRoomCfgByID("1");
    }

    public bool CanOpenNode(string nodeId)
    {
        int preId = int.Parse(nodeId) - 1;
        RoomNodeCfg nodeCfg = RoomNodeCfg.Get(nodeId);
        List<RoomCfg> preNodeList;
        if (preId == 0)
            preNodeList = null;
        else
            RoomCfg.mRoomDict.TryGetValue(preId.ToString(), out preNodeList);

        //章节没有配
        if (preId > 0 && preNodeList == null)
            return false;

        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
            return false;

        if (hero.GetInt(enProp.level) < nodeCfg.openLevel)
            return false;

        //没有前面章节 或者 有前面章节但没打完 则本章节锁住
        if (preNodeList != null && preNodeList.Count > GetWinNumByNodeId(preId.ToString()))
            return false;
        else
            return true;
    }

    public bool CanOpenLevel(string levelId)
    {
        RoomCfg cfg = RoomCfg.GetRoomCfgByID(levelId);
        return CanOpenLevel(cfg);
    }
    public bool CanOpenLevel(RoomCfg cfg)
    {
        if (cfg == null)
            return false;
        int roomid = 0;
        int.TryParse(cfg.id, out roomid);
        if (roomid == 0)
        {
            Debuger.LogError("关卡所属章节配置错误 关卡id：" + cfg.id);
            return false;
        }

        LevelInfo info = GetLevelInfoById((roomid - 1).ToString());
        if (int.Parse(cfg.preLevelId) == 0 || (info != null && info.isWin == true))
            return true;
        else
            return false;
    }

    public void OpenLevel(string levelId)
    {
        RoomCfg cfg = RoomCfg.GetRoomCfgByID(levelId);
        OpenLevel(cfg);
    }
    public void OpenLevel(RoomCfg cfg)
    {
        if (!CanOpenLevel(cfg))
            return;
        UIMgr.instance.Open<UILevelDetail>(cfg);
    }

    public bool CanEnterLevel(string levelId)
    {
        LevelInfo info = GetLevelInfoById(levelId);
        return CanEnterLevel(info);
    }

    public bool CanEnterLevel(LevelInfo info)
    {
        if (info == null)
            return false;

        RoomCfg cfg = RoomCfg.GetRoomCfgByID(info.roomId);

        if (GetEnterNum(info) >= cfg.maxChallengeNum)
        {
            UIMessage.Show("今天挑战次数已用完，明天再来吧");
            return false;
        }

        if (Parent.GetStamina() < cfg.staminaCost)
        {
            UIMessage.Show("体力不足，无法进入");
            return false;
        }

        return true;
    }

    public void EnterLevel(string levelId)
    {
        LevelInfo info = GetLevelInfoById(levelId);
        EnterLevel(info);
    }

    public void EnterLevel(LevelInfo info)
    {
        if (Main.instance.isSingle)
        {
            LevelMgr.instance.ChangeLevel(info.roomId);
            return;
        }
        if (CanEnterLevel(info))
            NetMgr.instance.LevelHandler.SendEnter(info.roomId);
    }


    #endregion


    #region Private Methods

    #endregion
}
