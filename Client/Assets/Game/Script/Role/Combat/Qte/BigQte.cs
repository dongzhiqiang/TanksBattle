using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BigQte
{
    #region Fields
    BigQteCfg m_cfg;
    float m_curRunning;
    GameObject m_qteObj;
    bool m_isPlaying;
    float m_prevTime;     //记录的是运行时的时间
    int m_prevStageIdx = -1;     //上一个阶段的序列
    bool m_isWin = true;

    public static GameObject mQteCamera;
    public float m_recordTime;     //暂停时记录的时间

    #endregion

    #region Properties

    public static QTECfg2 EventCfg;

    public static BigQte CurQte;
    public Transform ModelTarget { get; set; }
    public Transform ModelHero { get; set; }
    public Transform ModelCamera { get; set; }

    public static System.Action OnQteChangedCallback;

    public int CurStageIdx
    {
        get
        {
            return m_prevStageIdx;
        }
    }

    public bool CurStageIsWin { get; set; }
    public bool IsWin
    {
        get
        {
            return m_isWin;
        }
        set
        {
            m_isWin = value;
        }
    }

    public bool IsPlaying
    {
        get { return m_isPlaying; }
        set { m_isPlaying = value; }
    }
    public BigQteCfg Cfg
    {
        get { return m_cfg; }
        set { m_cfg = value; }
    }
    public float CurRunning
    {
        get { return m_curRunning; }
        set
        {
            m_curRunning = value + m_recordTime;
            m_curRunning = m_curRunning < 0 ? 0 : m_curRunning;
            m_curRunning = m_curRunning > m_cfg.Duration ? m_cfg.Duration : m_curRunning;
        }
    }

    public Role Target { get; set; }
    public Role Hero { get { return RoleMgr.instance.Hero; } }
    public Camera CurCamera { get; set; }

    #endregion
    public void PlayOrPause(Role targert = null)
    {
        IsPlaying = !IsPlaying;
        m_prevTime = TimeMgr.instance.logicTime;

        m_recordTime = CurRunning;
        if (IsPlaying && CurRunning == 0)
        {
            Init();
        }

        if (Application.isPlaying)
        {
            mQteCamera = GameObjectPool.GetPool(GameObjectPool.enPool.Other).GetImmediately("mod_camera");

            if (mQteCamera == null)
                return;
            if (IsPlaying)
                UIMgr.instance.Close<UILevel>();

            Transform CurCamera = CameraMgr.instance.Tran;
            if (CurCamera == null)
                return;

            mQteCamera.transform.position = RoleMgr.instance.Hero.transform.position;
            mQteCamera.transform.rotation = RoleMgr.instance.Hero.transform.rotation;

            //隐藏场景相机 用qte相机
            CurCamera.gameObject.SetActive(false);
            
            //Transform qteCamera = mQteCamera.transform.Find("Camera");
            //Transform qteCamera = mQteCamera.GetComponentInChildren<Camera>().transform;
            MonoListener mono = mQteCamera.GetComponent<MonoListener>();
            if (mono == null)
                mono = MonoListener.Get(mQteCamera);
            mono.onLateUpdate = UpdateCo;

            //相机放到相应模型下
            ModelCamera = mQteCamera.transform.Find(EventCfg.cameraMod);
            if (ModelCamera == null)
                return;
            Transform Camera01 = ModelCamera.Find("Camera01");
            Transform bone = Camera01.Find("Bone02");
            bone.AddComponentIfNoExist<Camera>();
            bone.AddComponentIfNoExist<AudioListener>();
            Target = targert;
        }
    }

    public void PrevKeyframe()
    {
        IsPlaying = false;
        float time = (float)System.Math.Round(CurRunning - 0.01f, 2);
        m_recordTime = 0.0f;
        CurRunning = time;
    }

    public void NextKeyframe()
    {
        IsPlaying = false;
        float time = (float)System.Math.Round(CurRunning + 0.01f, 2);
        m_recordTime = 0.0f;
        CurRunning = time;
    }
    public QteStageInfo AddStage()
    {
        if (Cfg == null) return null;
        QteStageInfo info = new QteStageInfo();
        info.name = "第" + Cfg.stages.Count + "阶段";
        info.winInfo.duration = Random.Range(0.5f, 1.0f);
        info.idx = Cfg.stages.Count;
        Cfg.stages.Add(info);
        return info;
    }

    public static BigQte Get(string id)
    {
        BigQte qte = new BigQte();
        BigQteCfg cfg = new BigQteCfg();
        cfg.Name = id;
        cfg.Duration = 8;
        qte.Cfg = cfg;
        qte.Init();
        CurQte = qte;
        return qte;
    }
    public static BigQte Get(BigQteCfg bigQteCfg)
    {
        BigQte qte = new BigQte();
        qte.Cfg = bigQteCfg;
        qte.Init();
        CurQte = qte;
        //if (OnQteChangedCallback != null)
        //    OnQteChangedCallback();
        return qte;
    }

    void UpdateCo()
    {
        if (!IsPlaying)
            return;
        UpdateEvent(TimeMgr.instance.logicTime - m_prevTime);
        return;
    }


    //根据阶段序列值得到起始时间
    public float GetStageStartTime(int idx)
    {
        if (idx > Cfg.stages.Count)
            return 0;
        return Cfg.GetStageStartTime(idx);
    }

    //根据时间获取当前阶段
    public int GetStageByCurTime()
    {
        float time = 0;
        for (int i = 0; i < Cfg.stages.Count; i++)
        {
            QteStageInfo stage = Cfg.stages[i];
            float prevTime = time;
            if (!CurStageIsWin && i == CurStageIdx && stage.loseInfo != null)
                time += stage.loseInfo.duration;
            else
            {
                time += stage.winInfo.duration;
            }
            if (prevTime <= CurRunning && time > CurRunning)
                return i;
        }
        return Cfg.stages.Count - 1;
    }

    public void UpdateEvent(float time, bool bStart = false)
    {
        CurRunning = time;

        if (CurRunning >= Cfg.Duration)
        {
            Stop();
            return;
        }
        if (Cfg == null || Cfg.stages.Count <= 0)
            return;
        int idx = GetStageByCurTime();

        float runTime = CurRunning - GetStageStartTime(idx);

        bool bInit = false;
        if (m_prevStageIdx != idx)
        {
            if (m_prevStageIdx != -1)
            {
                //前一个阶段结束
                List<QteEvent> prevEvents = Cfg.stages[m_prevStageIdx].winInfo.events;
                for (int i = 0; i < prevEvents.Count; i++)
                    prevEvents[i].Stop();
            }

            m_prevStageIdx = idx;
            bInit = true;

            if (!CurStageIsWin && m_prevStageIdx > 0)     //本阶段是失败的
            {
                IsPlaying = false;
                //结束
                Stop();
                return;
            }
        }

        List<QteEvent> events = Cfg.stages[idx].winInfo.events; ;
        CurStageIsWin = true;
        if (!IsWin && idx > 0 && Cfg.stages[idx].loseInfo != null)
        {
            events = Cfg.stages[idx].loseInfo.events;
            CurStageIsWin = false;
        }


        for (int i = 0; i < events.Count; i++)
        {
            QteEvent e = events[i];
            if (bInit)  //不是同一个阶段 需要初始化
                e.Init();
            if (runTime >= e.startTime && e.EndTime > runTime)
            {
                //更新此事件前先start
                if (!e.IsRunning)   //事件开始时调用start
                    e.Start();
                e.Update(runTime - e.startTime);
                e.IsRunning = true;
            }
            else
            {
                if (e.IsRunning)    //事件结束时调stop
                    e.Stop();
                e.IsRunning = false;
            }

        }
    }

    public void Init()
    {
        for (int i = 0; i < Cfg.stages.Count; i++)
        {
            List<QteEvent> winEvents = Cfg.stages[i].winInfo.events;
            for (int j = 0; j < winEvents.Count; j++)
            {
                winEvents[j].CurQte = this;
                winEvents[j].name = j + ":" + QteEvent.TypeName[(int)winEvents[j].m_type];
                winEvents[j].Init();
                winEvents[j].IsRunning = false;
            }

            if (Cfg.stages[i].loseInfo != null)
            {
                List<QteEvent> loseEvents = Cfg.stages[i].loseInfo.events;
                for (int j = 0; j < loseEvents.Count; j++)
                {
                    loseEvents[j].CurQte = this;
                    loseEvents[j].name = j + ":" + QteEvent.TypeName[(int)loseEvents[j].m_type];
                    loseEvents[j].Init();
                    loseEvents[j].IsRunning = false;
                }
            }
        }
    }

    public void Stop()
    {
        for (int i = 0; i < Cfg.stages.Count; i++)
        {
            List<QteEvent> winEvents = Cfg.stages[i].winInfo.events;
            for (int j = 0; j < winEvents.Count; j++)
            {
                winEvents[j].Stop();
                winEvents[j].IsRunning = false;
            }

            if (Cfg.stages[i].loseInfo != null)
            {
                List<QteEvent> loseEvents = Cfg.stages[i].loseInfo.events;
                for (int j = 0; j < loseEvents.Count; j++)
                {
                    loseEvents[j].Stop();
                    loseEvents[j].IsRunning = false;
                }
            }
        }

        m_recordTime = 0.0f;
        CurRunning = 0.0f;
        IsPlaying = false;
        m_prevStageIdx = -1;
        //IsWin = true;

        if (!Application.isPlaying)
            return;

        UILevel uiLevel = UIMgr.instance.Get<UILevel>();
        if (!uiLevel.IsOpen)
            UIMgr.instance.Open<UILevel>(true);
        UIMgr.instance.Close<UIQte>();

        //隐藏场景相机 用qte相机
        Transform cameraTran = CameraMgr.instance.Tran;
        cameraTran.position = mQteCamera.transform.position;
        cameraTran.rotation = mQteCamera.transform.rotation;
        cameraTran.gameObject.SetActive(true);
        GameObjectPool.GetPool(GameObjectPool.enPool.Other).Put(mQteCamera);
        mQteCamera = null;

        Hero.RSM.CheckFree();
        Target.RSM.CheckFree();
    }

    public void SetRunning(float time)
    {
        CurRunning = time;
        UpdateEvent(CurRunning, true);
    }
}