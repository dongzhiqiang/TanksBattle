using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomCfg
{
    #region Fields
    public string id = "0";  //房间ID
    public string roomName = "";    //房间名
    public string preLevelId = "0";  //前置关卡ID
    public string roomNodeId = "0";  //房间所属地图
    public List<string> cameraGroupName = new List<string>(); //当前关卡镜头组名
    public int roomType = 0;    //关卡类型
    public string roomStory = "";   //房间描述
    public string targetDesc = "";  //目标描述
    public int[] battleState;
    public List<string> sceneFileName = new List<string>();   //房间资源文件名
    public string roomSprite = "";  //关卡图标
    public List<string> sceneId = new List<string>();     //场景名
    public string loadingBg = "";   //加载背景
    public string loadingTips = ""; //加载提示id字符串
    public int bgmId = 0;           //背景音乐
    public int limitTime = 0;       //限制时间
    public string levelProp = "";//副本固定属性
    public string levelPropRate = "";//副本属性分配比例
    public int levelLv = 0;//副本怪物等级
    public float hitProp = 1;//副本打击属性倍率
    public int maxChallengeNum = 0; //每日最大挑战次数
    public int powerNum = 99999; //推荐战斗力
    public int staminaCost = 0;  //消耗体力
    public int petNum = 2;      //限制携带侍宠
    public int expReward = 0;   //奖励经验
    public int petExp = 0;      //宠物经验奖励
    public string taskId;    //三星条件列表
    public int dropId = 0;      //掉落id
    public int time = -1;   //通关时间
    public string monsterDrop;//小怪掉落
    public string specialDrop;//精英掉落
    public string bossDrop;//boss掉落
    public string boxDrop;//宝箱掉落
    public string monsterRandom;    //客户端小怪掉落随机概率
    public string specialRandom;  //客户端精英掉落随机概率
    public string bossRandom;     //客户端boss掉落随机概率
    public string boxRandom;     //客户端宝箱掉落随机概率
    public int[][] rewardShow;   //客户端展示奖励
    public bool needPetTip;
    public bool needPowerTip;
    public string[] levelFlag = { }; //关卡标记 ai用到
    public LvValue hpRate;
    public int petFormation;

    #endregion

    public static List<RoomCfg> mAllRoomList = new List<RoomCfg>();
    public static Dictionary<string, List<RoomCfg>> mRoomDict = new Dictionary<string, List<RoomCfg>>();
    public static Dictionary<string, SceneCfg.SceneData> m_sceneCfgDict = new Dictionary<string, SceneCfg.SceneData>();
    public static void Init()
    {
        //m_cfg = Csv.CsvUtil.Load<string, RoomCfg>("room/room", "id");
        mAllRoomList = Csv.CsvUtil.Load<RoomCfg>("room/room");

        m_sceneCfgDict.Clear();
        mRoomDict.Clear();

        for (int i = 0; i < mAllRoomList.Count; i++)
        {
            RoomCfg cfg = mAllRoomList[i];
            //根据章节ID整理章节关卡添加到字典
            if (cfg.id != LevelMgr.MainRoomID)
            {
                if (mRoomDict.ContainsKey(cfg.roomNodeId))
                {
                    mRoomDict[cfg.roomNodeId].Add(cfg);
                }
                else
                {
                    List<RoomCfg> roomCfgList = new List<RoomCfg>();
                    roomCfgList.Add(cfg);
                    mRoomDict.Add(cfg.roomNodeId, roomCfgList);
                }
            }
        }

    }

    public static RoomCfg GetRoomCfgByID(string roomID)
    {
        foreach (RoomCfg roomCfg in mAllRoomList)
        {
            if (roomCfg.id == roomID)
                return roomCfg;
        }
        return null;
    }


    public static SceneCfg.SceneData GetSceneCfg(string key)
    {
        SceneCfg.SceneData sceneData = new SceneCfg.SceneData();
        if (m_sceneCfgDict.TryGetValue(key, out sceneData))
        {
            return sceneData;
        }

        string path = string.Format("scene/{0}", key);
        sceneData = Util.LoadJsonFile<SceneCfg.SceneData>(path);
        if (sceneData != null)
        {
            sceneData.sceneName = key;
            m_sceneCfgDict.Add(key, sceneData);
        }
        else
        {
            sceneData = new SceneCfg.SceneData();
            sceneData.sceneName = key;
            m_sceneCfgDict.Add(key, sceneData);
        }
        return sceneData;
    }

    public static string GetRoomNameById(string roomId)
    {
        foreach (RoomCfg room in mAllRoomList)
        {
            if (room.id == roomId)
                return room.roomName;
        }
        return "";
    }

    public List<int> GetTaskIdList()
    {
        List<int> taskIds = new List<int>();

        if (!string.IsNullOrEmpty(taskId))
        {
            string[] taskIdStr = taskId.Split('|');
            for (int i = 0; i < taskIdStr.Length; i++)
            {
                taskIds.Add(int.Parse(taskIdStr[i]));
            }
        }

        return taskIds;
    }

    public List<int> GetLoadingTipsId()
    {
        List<int> tipsIds = new List<int>();

        if (!string.IsNullOrEmpty(loadingTips))
        {
            string[] tipsIdStr = loadingTips.Split('|');
            for (int i = 0; i < tipsIdStr.Length; i++)
            {
                tipsIds.Add(int.Parse(tipsIdStr[i]));
            }
        }

        return tipsIds;
    }
}
