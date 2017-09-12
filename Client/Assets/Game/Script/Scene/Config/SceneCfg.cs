using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System;

public class EDITORWIDTH
{
    public const float BTNWIDTH = 80;
    public const float BTNBIGWIDTH = 120;
    public const float POPWIDTH = 220;
    public const float LBWIDHT = 100;
    public const float LBBIGWIDHT = 250;
    public const int SPACESMALL = 5;
    public const int SPACEBIG = 10;
}

public class SceneCfg
{

    public static string[] RefreshTypeName = new string[] { "同时刷出", "随机几个"};


    public static string[] CampName = new string[] { "主角", "敌方", "阵营1", "阵营2", "阵营3", "阵营4", "阵营5", "阵营6", "中立" };

    public static string[] BoxStateTypeName = new string[] { "回血", "回蓝" };

    public enum BoxStateType
    {
        AddHp,
        AddMp,
    }

    public enum BornDeadType
    {
        Born = 1,
        Dead = 2,
        GroundDead = 3,
    }

    public enum RefreshType     //刷怪方式
    {
        SameTime,   //全部死后同时刷出
        RandomNum,  //随机个数刷出
    }

    public static string[] DangbanName = new string[] { "fx_dangban_huo", "DangBan_Effect", "DangBanEffect_muzhalan", "DangBanEffect_pumingku", "DangBanEffect_diyu" , "DangBanEffect_Huang" };
    public enum DangbanType : int
    {
        None = -1,
        Dangban_huo,
        Dangban_guang,
        Dangban_muzhalan,
        Dangban_pinminku,
        Dangban_diyu,
        Dangban_huang,
    }

    public static string[] TriggerTypeName = new string[] { "都达成", "任意几个" };
    public enum TriggerType : int
    {
        AllReach,   //都达成
        Random,     //任意几个
    }

    public enum AreaType
    {
        Normal,
        DangBan,
    }

    public enum EventType : int
    {
       
        //关卡事件
        StartLevel,     //开始关卡
        EnterTime,      //进入场景时长
        NpcIDDead,      //某个类型的NPC死亡
        Win,            //通关
        Area,           //区域
        RoleEnter,      //角色进入
        RoleDead,       //某个标记的角色死亡
        Lose,           //失败
        RefreshDead,    //刷新点怪全死
        RoleBlood,      //血量控制
        RoleNum,        //角色数量
        GroupDeadNum,   //刷新组角色死几个
        None,           //无触发
        FinishEvent,    //完成事件

        //通关条件
        FINISH_LEVEL,   //通关副本
        BLOOD_LIMIT,    //限血通关
        TIME_LIMIT,     //限时通关
        OPEN_BOX,       //开启宝箱
        ROLE_DEAD_ALIVE,  //角色死亡或不死
        REACH_AREA,     //到达区域
        KILL_TIMES,     //完成连杀
        KILL_MONSTER,   //杀怪个数
        WEAPON_LIMIT,   //限用武器
        SKILL_USE,      //使用技能
        PET_USE,       //限用宠物
        STATE_TIMES,   //状态次数
        BREAK_ITEM,    //打碎物品
        HURT_NUM,      //被击次数
        CONTINUE_HIT,   //连击次数
        TRAP_NUM,       //陷阱次数
        PET_ALIVE,      //宠物不死
        KILL_ALL,      //杀死所有怪
        PET_USE2,       //限用宠物

    }

    public enum ActionType : int
    {
        Skill, // 释放某个技能
        RemoveNpcId, // 移除NPC
        ActivaRefresh, // 激活刷新点
        Win, // 胜利
        Lose, // 失败
        CreateHero, //创建主角
        ActivateDangban,    //激活挡板
        HideDangban,        //隐藏挡板
        ShowDir,        //激活指向
        HideDir,        //激活指向
        Story,          //触发剧情
        Buff,           //添加状态
        ShowWave,       //显示波数
        AddWave,        //增加波数
        HideWave,       //隐藏波数
        ChangeScene,    //切换场景
        ShowTarget,     //显示目标
        Camera,         //镜头转向
        HideAllRole,    //隐藏所有角色
        ShowAllRole,    //显示所有角色
        StartTeach,     //开始引导
        NextTeach,      //推进引导
        KillAllMonster, //杀死所有怪
        PauseRefresh,   //暂停刷新组
        RestartRefresh, //重启刷新组
        EnterFightCamera, //进入战斗镜头
        LeaveFightCamera, //离开战斗镜头
        ShowIdea,       //显示内心独白
        HideIdea,       //隐藏内心独白
        ShowTips,       //显示提示
        HideTips,       //隐藏提示
        GlobalFly,      //全局飞出物
        BuffRemove,     //移除buff
        ChangeAI,       //改变AI
        TimeScale,      //时间缩放
        EventGroup,     //事件组
        Msg,            //发出消息
        ActivateArea,   //激活区域
        KillMonster,    //杀死怪物
        FireEvent,      //触发事件
        BGMusic,        //背景音乐
        RemoveEvent,    //移除事件
        None,           //空响应
        RandomEvent,    //随机激活事件(无条件的事件激活时直接触发)
    }

    //刷新点
    public class RefPointCfg
    {
        public Vector3 pos = Vector3.zero;
        public Vector3 dir = Vector3.up;
        public enCamp camp = enCamp.camp2;
        public int buffId = 0;
        public string roleId = "10000";
        public string pointFlag = "";
        public float bornDelay = 0;   //出场延时
        public string ai = "-1"; //默认ai
        public string bornTypeId = "";  //出生效果id
        public string deadTypeId = "";  //死亡效果id
        public string groundDeadTypeId = ""; //倒地死亡效果
        public int isShowBloodBar = 0;
        public int isShowFriendBloodBar = 0;
        public int isShowTargetBar = 0;
        public List<Vector3> pathfindingList = new List<Vector3>();     //寻路点列表
        public int boxAddNum = 0;
        public BoxStateType boxAddType = 0;
        public int boxBuffId = 0;
        public HateCfg hate = new HateCfg();

        public RefPointCfg()
        {

        }

        public RefPointCfg(RefPointCfg cfg)
        {
            pos = new Vector3(cfg.pos.x, cfg.pos.y, cfg.pos.z);
            dir = new Vector3(cfg.dir.x, cfg.dir.y, cfg.dir.z);
            camp = cfg.camp;
            roleId = cfg.roleId;
            pointFlag = cfg.pointFlag;
            bornDelay = cfg.bornDelay;
            ai = cfg.ai;
            bornTypeId = cfg.bornTypeId;
            deadTypeId = cfg.deadTypeId;
            isShowBloodBar = cfg.isShowBloodBar;
            isShowFriendBloodBar = cfg.isShowFriendBloodBar;
            isShowTargetBar = cfg.isShowTargetBar;
            buffId = cfg.buffId;
            boxAddNum = cfg.boxAddNum;
            boxAddType = cfg.boxAddType;
            boxBuffId = cfg.boxBuffId;
            hate.CopyFrom(cfg.hate);

        pathfindingList.Clear();
            for (int i = 0; i < cfg.pathfindingList.Count; i++ )
            {
                pathfindingList.Add(new Vector3(cfg.pathfindingList[i].x, cfg.pathfindingList[i].y, cfg.pathfindingList[i].z));
            }
        }
    }

    //刷新组
    public class RefGroupCfg
    {
        public RefreshType refreshType = RefreshType.SameTime;
        public List<RefPointCfg> Points = new List<RefPointCfg>();
        public string groupFlag = "";
        public string nextGroupFlag = "";
        public int nextWaveDelay = 0;
        public float delayTime = 0;
        public int refreshNum = 1;
        public int pointNum = 1;    //随机刷怪个数

        public void Init(RefGroupCfg cfg, string newFlag)
        {
            refreshType = cfg.refreshType;
            Points.Clear();
            for (int i = 0; i < cfg.Points.Count; i++)
            {
                RefPointCfg pointCfg = new RefPointCfg(cfg.Points[i]);

                if (string.IsNullOrEmpty(newFlag))
                    pointCfg.pointFlag = cfg.Points[i].pointFlag;
                else
                    pointCfg.pointFlag = string.Format("{0}-{1}", newFlag, i + 1); //复制不能完全把标记也复制 标记会记载角色身上 不能重复

                Points.Add(pointCfg);
            }

            if (string.IsNullOrEmpty(newFlag))
                groupFlag = cfg.groupFlag;
            else
                groupFlag = newFlag;    //复制不能完全把标记也复制 标记会记载角色身上 不能重复

            if (string.IsNullOrEmpty(newFlag))
                nextGroupFlag = cfg.nextGroupFlag;
            else
                nextGroupFlag = "";

            nextWaveDelay = cfg.nextWaveDelay;
            delayTime = cfg.delayTime;
            refreshNum = cfg.refreshNum;
            pointNum = cfg.pointNum;
        }

    }
    public class BornInfo
    {
        public Vector3 mPosition;
        public Vector3 mEulerAngles;
        public string mBornTypeId;
        public string mDeadTypeId;
        public string mGroundDeadTypeId;
        public HateCfg hate = new HateCfg();

        public Vector3 mPet1Position;
        public Vector3 mPet1EulerAngles;
        public string mPet1BornTypeId;
        public string mPet1DeadTypeId;
        public string mPet1GroundDeadTypeId;
        public HateCfg pet1hate = new HateCfg();

        public Vector3 mPet2Position;
        public Vector3 mPet2EulerAngles;
        public string mPet2BornTypeId;
        public string mPet2DeadTypeId;
        public string mPet2GroundDeadTypeId;
        public HateCfg pet2hate = new HateCfg();

        public BornInfo()
        {
            mPosition = Vector3.zero;
            mEulerAngles = Vector3.one;
            mBornTypeId = "";
            mDeadTypeId = "";
            mGroundDeadTypeId = "";

            mPet1Position = Vector3.zero;
            mPet1EulerAngles = Vector3.one;
            mPet1BornTypeId = "";
            mPet1DeadTypeId = "";
            mPet1GroundDeadTypeId = "";

            mPet2Position = Vector3.zero;
            mPet2EulerAngles = Vector3.one;
            mPet2BornTypeId = "";
            mPet2DeadTypeId = "";
            mPet2GroundDeadTypeId = "";
        }
    }
    public class CheckSaveCfg
    {
        public int eveType;
        public string content;
    }

    public class AreaCfg
    {
        public bool bActivate = true;
        public string areaFlag = "";
        public Vector3 pos = Vector3.zero;
        public Vector3 dir = Vector3.one;
        public Vector3 size = Vector3.one;
        public AreaType areaType;
        public SceneCfg.DangbanType dangbanType = SceneCfg.DangbanType.None;
        public void Init(string areaFlag, Vector3 pos, Vector3 dir, Vector3 size, SceneCfg.DangbanType dangbanType, AreaType areaType = AreaType.Normal, bool bActivate=true)
        {
            this.areaFlag = areaFlag;
            this.pos = pos;
            this.size = size;
            this.dir = dir;
            this.areaType = areaType;
            this.dangbanType = dangbanType;
            this.bActivate = bActivate;
        }
    }
    
    public class CheckCfg
    {
        public TriggerType triggerType = TriggerType.AllReach;
        public int triggerNum = 0;
        public int triggerTimes = 1;
        public string checkFlag = "";

        public List<CheckSaveCfg> eventCfgList = new List<CheckSaveCfg>();
        public List<CheckSaveCfg> actionCfgList = new List<CheckSaveCfg>();

    }

    public class CameraItem
    {
        public string groupId = "";         //刷新组
        public string pointId = "";         //刷新点id
        public string roleId = "";          //角色id
        public Vector3 offset = Vector3.zero;      //偏移
        public float verticalAngle = -1;    //高度角
        public float horizontalAngle = -1; //水平角
        public float fov = -1; //视野
        public float distance = -1;  //距离
        public float moveTime = 0;  //移动时间
        public float stayTime = 0;  //停留时间
        public float overDuration = 0;  //返回时间
        public float accSpeed = 0;  //加速度
        public float decSpeed = 0;  //减速度
        public float maxSpeed = 0;  //最大速度
    }

    public class CameraCfg
    {
        public string cameraFlag = "";
        public List<CameraItem> cameraList = new List<CameraItem>();
    }

    public class PossCfg {
        public string name = "";
        public List<Vector3> ps = new List<Vector3>();
        
        public int GetClosestPosIdx(Vector3 pos)
        {
            int find = -1;
            float min = float.MaxValue;
            for (int i=0;i< ps.Count;++i)
            {
                float sq = Util.XZSqrMagnitude(pos, ps[i]);
                if(find == -1|| sq< min)
                {
                    find = i;
                    min = sq;
                }
            }
            return find;
        }

        public float GetClosestDis(Vector3 pos)
        {
            int find = -1;
            float min = float.MaxValue;
            for (int i = 0; i < ps.Count; ++i)
            {
                float sq = Util.XZSqrMagnitude(pos, ps[i]);
                if (find == -1 || sq < min)
                {
                    find = i;
                    min = sq;
                }
            }
            if (find == -1)
                Debuger.LogError("逻辑错误，路点计算最近距离出错:{0}", name);
            return Mathf.Sqrt(min);
        }
    }


    //需要保存的数据
    public class SceneData
    {
        string filePath = "scene/";
        public string sceneName = "";
        public List<RefGroupCfg> mRefGroupList = new List<RefGroupCfg>();
        public List<BornInfo> mBornList = new List<BornInfo>();
        public List<CheckCfg> mCheckList = new List<CheckCfg>();
        public List<AreaCfg> mAreaList = new List<AreaCfg>();
        public List<CameraCfg> mCameraList = new List<CameraCfg>();
        public List<string> mStoryIdList = new List<string>();  //所有本关卡配置的剧情id
        public List<string> mGroupIdList = new List<string>();  //显示怪物波数缩涉及到的所有刷新组组ID
        public List<PossCfg> mPoss = new List<PossCfg>();

        public string mShowWaveGroupId = "";     //显示波数的刷新组id

        Dictionary<string, PossCfg> possIdx = null;


        //public List<Vector3> mFindPointList = new List<Vector3>();

        public void Save(string fileName = null)
        {
            string path = "";
            if (string.IsNullOrEmpty(fileName))
            {
                path = string.Format("{0}{1}", filePath, sceneName);
            }
            else
            {
                path = fileName;
            }
            
            Util.SaveJsonFile(path, this);
        }

        void Init()
        {

        }

        public void GetBornPosAndEuler(SceneCfg.BornInfo bornInfo,out Vector3 pos,out Vector3 euler)
        {
            pos = Vector3.zero;
            euler = Vector3.zero;
            if (bornInfo != null)
            {
                pos = bornInfo.mPosition;
                euler = bornInfo.mEulerAngles;
                return;
            }

            CameraTriggerMgr caTriggerMgr = CameraTriggerMgr.instance;
            CameraTriggerGroup caTriggerGroup = null;
            string cameraStr = Room.instance.roomCfg.cameraGroupName[SceneMgr.instance.CurSceneIdx];
            if (!string.IsNullOrEmpty(cameraStr))
                caTriggerGroup = caTriggerMgr.GetGroupByName(cameraStr);
            else
                caTriggerGroup = caTriggerMgr.CurGroup;

            if (caTriggerGroup == null)
            {
                Debug.Log("主角出生错误");
                return;
            }

            pos = caTriggerGroup.m_bornPos;
            
        }

        public PossCfg GetPossCfgByNames(string n)
        {
            if(possIdx == null)
            {
                possIdx = new Dictionary<string, PossCfg>();
                foreach (var p in mPoss)
                {
                    possIdx[p.name] = p;
                }
            }
            return possIdx.Get(n);
        }
    }
}
