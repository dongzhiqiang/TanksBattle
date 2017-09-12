using UnityEngine;
using System.Collections;

public class BornLogic
{
    public enum PlayState
    {
        None,
        Play,
        Stop,
    }
    #region Fields
    float m_beginTime;
    float m_runTime;
    float m_timeScale;
    BornCfg m_bornCfg;
    //int aniIdx;
    float m_endTimeBossUI = 0;  //bossUI结束时间
    //float m_endTimeAni = 0;     //动作播放结束时间
    float m_endTimePause = 0;   //暂停恢复逻辑时间
    float m_endTimeCamera = 0;  //相机转回时间
    float m_bossUIShowTime = 2.0f;
    float m_preFOV;
    bool m_bCloseSetting = false;
    Role m_role;
    GameObject m_fxOb;
    TimeMgr.TimeScaleHandle m_timeScaleHandle;
    SimpleAnimationsCxt m_aniCxt;
    CameraHandle m_cameraHandle;

    PlayState m_statePlayModel;
    PlayState m_statePlayAni;
    PlayState m_statePlayFx;
    PlayState m_statePause;
    PlayState m_stateSlow;
    PlayState m_stateCamera;
    PlayState m_stateBossUI;
    #endregion

    public void Init(Role role, BornCfg cfg)
    {
        m_bornCfg = cfg;
        m_role = role;

        m_beginTime = TimeMgr.instance.realTime;
        m_statePlayAni = PlayState.None;
        m_statePlayFx = PlayState.None;
        m_statePause = PlayState.None;
        m_stateSlow = PlayState.None;
        m_stateCamera = PlayState.None;
        m_stateBossUI = PlayState.None;
        //aniIdx = 0;
        m_timeScale = Time.timeScale;

        m_bCloseSetting = false;
        if (m_bornCfg == null)
            return;


        if (m_bornCfg.modelDelay > 0 && m_bornCfg.type == SceneCfg.BornDeadType.Born)
            m_role.Show(false);

        m_fxOb = null;

        //计算boosUI结束时间
        m_endTimeBossUI = m_bornCfg.bossUITime;
        if (m_endTimeBossUI >= 0)
            m_endTimeBossUI += m_bossUIShowTime;

        m_endTimeCamera = m_bornCfg.cameraStartTime;

        if (m_endTimeCamera >= 0)
        {
            m_endTimeCamera += m_bornCfg.cameraMoveTime;
            m_endTimeCamera += m_bornCfg.cameraStayTime;
        }


        m_endTimePause = m_bornCfg.pauseTime;

        if (m_endTimePause >= 0)
        {
            m_endTimePause = m_bornCfg.startTime;
        }

        if (!string.IsNullOrEmpty(m_bornCfg.aniNameStr))
            m_aniCxt = SimpleAnimationsCxt.Parse(m_bornCfg.aniNameStr);

        //如果有暂停逻辑的处理 先隐藏设置按钮
        if (m_endTimePause >= 0)
        {
            UILevelAreaSetting areaSet = UIMgr.instance.Get<UILevel>().Get<UILevelAreaSetting>();
            if (areaSet != null && areaSet.IsOpen)
            {
                areaSet.CloseArea();
                m_bCloseSetting = true;
            }
        }

        //进入出生死亡直接播出场动作 有些怪从地下出来可能会第一帧直接显示在了上面
        updatePlayAni();

    }

    public void Update()
    {
        if (m_bornCfg != null)
        {
            if (isEnd())
                return;

            m_runTime = TimeMgr.instance.realTime - m_beginTime;
            
            //非自己暂停的 直接退出出生状态
            if (m_statePause != PlayState.Play && TimeMgr.instance.IsPause)
            {
                if (m_role.RSM.CurStateType == enRoleState.born)
                    m_role.RSM.CheckFree(true);
                else if (m_role.RSM.CurStateType == enRoleState.dead)
                {
                    //RoleMgr.instance.DestroyRole(m_role);
                }
                //AniPart aniPart = m_role.RSM.AniPart;
                //aniPart.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop, 0.2f);   //播完动作后播待机

                if (m_bCloseSetting)
                {
                    UILevel uiLevel = UIMgr.instance.Get<UILevel>();
                    if (uiLevel.IsOpen && !uiLevel.Get<UILevelAreaSetting>().IsOpen)
                        uiLevel.Open<UILevelAreaSetting>(true);
                    m_bCloseSetting = false;
                }
                return;
            }

            //更新动作显示
            updatePlayAni();

            //更新模型显示
            if (m_bornCfg.modelDelay > 0 && m_bornCfg.type == SceneCfg.BornDeadType.Born)
                updateModel();

            //更新特效显示
            if (m_bornCfg.fxDelay >= 0)
                updatePlayFx();

            //更新慢动作播放
            if (m_bornCfg.slowStartTime >= 0 && m_bornCfg.slowDurationTime > 0)
                updateSlowAction();

            //更新bossUI显示
            if (m_bornCfg.bossUITime >= 0)
                updateBossUI();

            //更新逻辑
            if (m_bornCfg.pauseTime >= 0 && m_bornCfg.pauseTime != m_bornCfg.startTime)
                updateLogicPase();

            //更新像机
            if (m_bornCfg.cameraStartTime >= 0)
                updateCamera();
        }

    }

    public bool isEnd()
    {
        if (m_runTime < m_bornCfg.aniDelay || (m_aniCxt != null && !m_role.RSM.AniPart.IsAnisOver(m_aniCxt)))   //动作不用时间来判断了
            return false;

        if (m_bornCfg == null || m_runTime > GetTime())
            return true;

        return false;
    }

    //突然强制退出时的处理
    public void OnExit()
    {
        //特效 动作 都不用管
        if (m_statePause == PlayState.Play)
        {
            m_statePause = PlayState.Stop;
            TimeMgr.instance.SubPause();
        }

        if (m_stateSlow == PlayState.Play)
        {
            m_stateSlow = PlayState.Stop;
            if (m_timeScaleHandle != null)
            {
                m_timeScaleHandle.Release();
                m_timeScaleHandle = null;
            }
        }

        if (m_stateCamera == PlayState.Play)
        {
            m_stateCamera = PlayState.Stop;
            if (m_cameraHandle != null)
            {
                m_cameraHandle.Release();
                m_cameraHandle = null;
            }
        }

        UILevel uiLevel = UIMgr.instance.Get<UILevel>();
        if (m_stateBossUI == PlayState.Play)
        {
            m_stateBossUI = PlayState.Stop;
            if (!uiLevel.gameObject.activeSelf)
                UIMgr.instance.Open<UILevel>(true);
            UIMgr.instance.Close<UILevelBossBorn>();
        }

        //如果有暂停逻辑的处理 先隐藏设置按钮
        if (m_bCloseSetting)
        {
            if (uiLevel.IsOpen && !uiLevel.Get<UILevelAreaSetting>().IsOpen)
                uiLevel.Open<UILevelAreaSetting>(true);
            m_bCloseSetting = false;
        }

        if (m_bornCfg != null && m_bornCfg.modelDelay > 0 && m_bornCfg.type == SceneCfg.BornDeadType.Born)
        {
            if (m_role != null && !m_role.IsShow)
                m_role.Show(true);
        }

    }

    #region Private Methods
    void updatePlayAni()
    {
        if (m_aniCxt == null)
            return;

        AniPart aniPart = m_role.RSM.AniPart;
        if (m_statePlayAni == PlayState.None)
        {
            m_statePlayAni = PlayState.Play;

            //第一个动作的渐变去掉
            if (m_aniCxt.anis.Count > 0)
                m_aniCxt.anis[0].fade = 0;
            aniPart.Play(m_aniCxt);
            if (m_bornCfg.aniDelay > 0)
            {
                aniPart.AddPause(m_bornCfg.aniDelay);
            }
        }

        if (m_statePlayAni == PlayState.Play && aniPart.IsAnisOver(m_aniCxt))
        {
            m_statePlayAni = PlayState.Stop;
            if (m_bornCfg.type == SceneCfg.BornDeadType.Born && !isEnd())
                aniPart.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop, 0.2f);   //播完动作后播待机
        }
    }

    void updateModel()
    {
        if (m_bornCfg.type == SceneCfg.BornDeadType.Born)
        {
            if (m_statePlayModel == PlayState.None && m_runTime >= m_bornCfg.modelDelay)
            {
                m_statePlayModel = PlayState.Play;
                m_role.Show(true);
            }
        }
    }

    void updatePlayFx()
    {
        if (m_statePlayFx == PlayState.None && m_runTime >= m_bornCfg.fxDelay)
        {
            m_statePlayFx = PlayState.Play;
            if (!string.IsNullOrEmpty(m_bornCfg.fx.name))
                m_fxOb = m_bornCfg.fx.Create(m_role, null, Vector3.zero,enElement.none);
            else
                m_fxOb = null;
        }
    }

    void updateSlowAction()
    {
        if (m_stateSlow == PlayState.None && m_runTime >= m_bornCfg.slowStartTime)
        {
            m_stateSlow = PlayState.Play;
            m_timeScaleHandle = TimeMgr.instance.AddTimeScale(m_bornCfg.playRate, m_bornCfg.slowDurationTime);
        }

        if (m_stateSlow == PlayState.Play && m_runTime >= (m_bornCfg.slowStartTime + m_bornCfg.slowDurationTime))
        {
            m_stateSlow = PlayState.Stop;
        }
    }

    void updateLogicPase()
    {
        if (m_statePause == PlayState.None && m_runTime >= m_bornCfg.pauseTime)
        {
            m_statePause = PlayState.Play;
            TimeMgr.instance.AddPause();
        }

        if (m_statePause == PlayState.Play && m_runTime >= m_bornCfg.startTime)
        {
            m_statePause = PlayState.Stop;
            TimeMgr.instance.SubPause();
        }
    }

    void updateCamera()
    {
        if (m_stateCamera == PlayState.None && m_runTime >= m_bornCfg.cameraStartTime)
        {
            m_stateCamera = PlayState.Play;
            //切换相机
            if (m_bornCfg.cameraFOV > 0)
            {
                float horizonAngle = m_bornCfg.cameraHorizontalAngle == -1 ? 180 : 180 + m_bornCfg.cameraHorizontalAngle;
                m_cameraHandle = CameraMgr.instance.Still(
                    m_role.transform.position, 
                    m_role.transform.forward, 
                    m_bornCfg.cameraOffset, 
                    m_bornCfg.cameraMoveTime, 
                    m_bornCfg.cameraStayTime, 
                    m_bornCfg.cameraFOV,
                    horizonAngle, 
                    m_bornCfg.cameraVerticalAngle, 
                    -1, 
                    m_bornCfg.cameraOverDuration);
                //m_cameraHandle = CameraMgr.instance.Still(m_role.transform, m_bornCfg.cameraMoveTime, m_bornCfg.cameraStayTime, m_bornCfg.cameraFOV, m_bornCfg.cameraOverDuration);
            }
        }

        if (m_stateCamera == PlayState.Play && m_runTime >= m_endTimeCamera)
        {
            m_stateCamera = PlayState.Stop;
            if (m_cameraHandle != null)
            {
                m_cameraHandle.Release();
                m_cameraHandle = null;
            }

        }
    }

    void updateBossUI()
    {
        if (m_stateBossUI == PlayState.None && m_runTime >= m_bornCfg.bossUITime)
        {
            if (m_role != null)
            {
                UIMgr.instance.Close<UILevel>();
                UIMgr.instance.Open<UILevelBossBorn>(m_role.GetString(enProp.name));
            }

            m_stateBossUI = PlayState.Play;
        }

        if (m_stateBossUI == PlayState.Play && m_runTime >= m_endTimeBossUI)
        {

            if (!UIMgr.instance.Get<UILevel>().gameObject.activeSelf)
                UIMgr.instance.Open<UILevel>(true);
            UIMgr.instance.Close<UILevelBossBorn>();
            m_stateBossUI = PlayState.Stop;
        }

    }

    float GetTime()
    {
        float runTime = m_endTimeBossUI;

        if (runTime < m_endTimeCamera)
            runTime = m_endTimeCamera;

        if (runTime < m_endTimePause)
            runTime = m_endTimePause;

        if (runTime < m_bornCfg.delayExtend)
            runTime = m_bornCfg.delayExtend;

        if (runTime < m_bornCfg.modelDelay && m_bornCfg.type == SceneCfg.BornDeadType.Born)
            runTime = m_bornCfg.modelDelay;

        return runTime;
    }
    #endregion
}
