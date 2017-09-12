using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum enQteStateType
{
    QteClickContinue,   //持续按
    QteClickRandom,     //随机按
    //QteClickHold,       //按住
}


public enum enQteEventType
{
    Qte_Operate,
    Qte_Ani,
    Qte_TimeScale,
    Qte_Blur,
    Qte_EventGroup,
    Qte_Fx,
}

public enum enQteEventObjType
{
    Qte_Obj_Hero,
    Qte_Obj_Target,
    Qte_Obj_Camera,
}

#region 表配置类
public class BigQteTableCfg
{
    #region Fields
    public string id = "";  //大qte技能id
    public string name = ""; //配置名
    #endregion

    public static List<BigQteTableCfg> mTableCfg = new List<BigQteTableCfg>();
    public static Dictionary<string, BigQteCfg> mCfgDict = new Dictionary<string, BigQteCfg>();
    public static List<BigQteCfg> mCfgList = new List<BigQteCfg>();

    public static void Init()
    {
        mTableCfg = Csv.CsvUtil.Load<BigQteTableCfg>("systemSkill/bigQteCfg");
        mCfgDict.Clear();
        mCfgList.Clear();

        for (int i = 0; i < mTableCfg.Count; i++)
        {
            BigQteCfg cfg = BigQteCfg.LoadCfg(mTableCfg[i].name);
            if (cfg == null)
            {
                //Debug.LogError("加载配置失败" + mTableCfg[i].id);
                continue;
            }
            else
            {
                mCfgDict.Add(mTableCfg[i].id, cfg);
                mCfgList.Add(cfg);
            }
        }
    }

    public static BigQteCfg GetCfg(string skillId)
    {
        BigQteCfg cfg;
        mCfgDict.TryGetValue(skillId, out cfg);
        return cfg;
    }
}

public class BigQteUseCfg
{
    #region Fields

    #endregion
}

#endregion

#region 事件相关
//事件基类
[JsonCanInherit]
public class QteEvent
{
    public static string[] TypeName = { "输入操作", "播放动作", "时间缩放", "屏幕模糊" , "事件组", "屏幕特效"};
    public static string[] OperateName = { "持续按", "随机按" }; // "按住"

    public enQteEventType m_type;
    public float startTime;     //相对于阶段开始时间计算的 
    public float duration;

    bool m_isRunning = false;

    public string name { get; set; }
    public float EndTime { get { return duration + startTime; } }
    public BigQte CurQte { get; set; }
    public bool IsRunning { get { return m_isRunning; } set { m_isRunning = value; } }

    public virtual void Init() { }
    public virtual void Start() { }
    public virtual void Update(float time) { }
    public virtual void Stop() { }

    public static QteEvent CreateEvent(enQteEventType type, BigQte qte)
    {
        QteEvent e = null;
        switch (type)
        {
            case enQteEventType.Qte_Ani:
                e = new QteEventAni();
                break;
            case enQteEventType.Qte_Operate:
                e = new QteEventOperate();
                break;
            case enQteEventType.Qte_TimeScale:
                e = new QteEventTimeScale();
                break;
            case enQteEventType.Qte_Blur:
                e = new QteEventCamera();
                break;
            case enQteEventType.Qte_EventGroup:
                e = new QteEventGroup();
                break;
            case enQteEventType.Qte_Fx:
                e = new QteEventFx();
                break;
        }
        if (e == null)
        {
            Debuger.LogError("没有的qte事件类型 " + type);
            return null;
        }
        e.m_type = type;
        e.CurQte = qte;
        return e;
    }
}

public class QteStageEventsInfo
{
    public float duration;
    public List<QteEvent> events = new List<QteEvent>();
}
//阶段配置
public class QteStageInfo
{
    public int idx;
    public string name { get; set; }
    public QteStageEventsInfo winInfo = new QteStageEventsInfo();
    public QteStageEventsInfo loseInfo;
    public QteStageInfo()
    {
    }
}

#endregion

public class BigQteCfg
{
    public string Name;
    public float Duration;
    public List<QteStageInfo> stages = new List<QteStageInfo>();

    public float GetStageStartTime(int stageIdx)
    {
        if (stageIdx <= 0) return 0;

        float duration = 0;
        for (int i = 0; i < stages.Count; i++)
        {
            if (i < stageIdx)
                duration += stages[i].winInfo.duration;
        }
        return duration;
    }
    public static BigQteCfg LoadCfg(string name)
    {
        return Util.LoadJsonFile<BigQteCfg>("qte/" + name);
    }

    public void SaveCfg(string name)
    {
        string fileName = string.Format("qte/{0}", name);
        Util.SaveJsonFile(fileName, this);
    }
}

