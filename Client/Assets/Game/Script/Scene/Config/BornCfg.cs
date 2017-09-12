using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class BornCfg
{
    public class BornTypeCfg
    {
        public List<BornCfg> mBornCfgList = new List<BornCfg>();
    }

    public BornCfg() { }

    public BornCfg(SceneCfg.BornDeadType type)
    {
        this.type = type;
        typeName = "";
        fxDelay = -1;
        modelDelay = -1;
        aniNameStr = "";
        aniName = new string[3] { "", "", "" };
        aniDelay = 0;
        pauseTime = -1;
        startTime = 0;
        slowStartTime = -1;
        slowDurationTime = 0;
        playRate = 1;
        bossUITime = -1;
        cameraStartTime = -1;
        cameraMoveTime = 0;
        cameraStayTime = 0;
        cameraOverDuration = -1;
        cameraFOV = -1;
        cameraOffset = Vector3.zero;
        cameraVerticalAngle = -1;
        cameraHorizontalAngle = -1;
        delayExtend = 0;

        fx = new FxCreateCfg();
    }
    public string typeName; //方式名
    public SceneCfg.BornDeadType type; //类型
    public float fxDelay;   //特效延迟时间
    public float modelDelay; //模型延迟显示
    public string aniNameStr; //动作
    public string[] aniName; //动作
    public float aniDelay;  //动画延迟时间
    public float pauseTime; //战斗逻辑暂停时间
    public float startTime; //战斗逻辑开始时间
    public float slowStartTime; //慢动作开始播放时间
    public float slowDurationTime;   //慢动作持续播放时间
    public float playRate;  //播放慢动作速率
    public float bossUITime;    //显示BOOS UI提示时间
    public float cameraStartTime;   //镜头开始转时间
    public float cameraMoveTime;    //镜头推进时间
    public float cameraStayTime;    //镜头停留时间
    public float cameraFOV;    //镜头视野
    public Vector3 cameraOffset = Vector3.zero;
    public float cameraVerticalAngle = -1;
    public float cameraHorizontalAngle = -1;
    public float cameraOverDuration; //镜头返回时间 -1默认使用下一个镜头的时间
    public float delayExtend; //扩展延时  有怪要全部动作特效等播放完仍然不进入下个状态 卡在那里
    public FxCreateCfg fx;

    public static BornTypeCfg mBornCfg = new BornTypeCfg();
    static HashSet<string> preLoads = new HashSet<string>();


    public static void Init()
    {
        mBornCfg = Util.LoadJsonFile<BornTypeCfg>("scene/BornCfg");
        if (mBornCfg == null)
            mBornCfg = new BornTypeCfg();
    }

    public static BornCfg GetCfg(string name)
    {
        foreach (BornCfg cfg in mBornCfg.mBornCfgList)
            if (cfg.typeName == name)
                return cfg;
        return null;
    }

    public static void PreLoad(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;
        if (preLoads.Contains(name))
            return;
        preLoads.Add(name);

        BornCfg cfg = GetCfg(name);
        if (cfg == null)
            return;
        cfg.fx.PreLoad();
    }

    public static void PreLoad(string n1, string n2, string n3)
    {
        BornCfg.PreLoad(n1);
        BornCfg.PreLoad(n2);
        BornCfg.PreLoad(n3);
    }
}
