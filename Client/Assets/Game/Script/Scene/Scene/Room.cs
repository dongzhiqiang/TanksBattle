using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RoomState : int
{
    Prepare,
    Loading,
    Init,
    Loaded,
    Exit,
}

public class Room : MonoBehaviour
{
    #region Fields
    RoomCfg m_roomCfg;

    public System.Action OnDrawGizmosCallback;
    public System.Action OnUpdateEventCallback;

    public SimplePool m_fxPool;

    public GameObject mAreaGroup;

    public GameObject mModuleGroup;

    public static Room instance;
    #endregion

    #region Properties
    public RoomCfg roomCfg { get { return m_roomCfg; } }
    public RoomState curState { get; set; }
    public float shieldArea = 5;

    public float startTime;
    #endregion

    #region Public Methods
    public void Exit()
    {
        curState = RoomState.Exit;
        instance = null;
    }

    public void Init(RoomCfg roomCfg)
    {
        instance = this;

        m_roomCfg = roomCfg;
        mAreaGroup = new GameObject("Area");
        mAreaGroup.transform.SetParent(this.transform);

        mModuleGroup = new GameObject("Module");
        mModuleGroup.transform.SetParent(this.transform);


        curState = RoomState.Prepare;
    }

    //public IEnumerator Load(object param = null)
    //{

    //    curState = RoomState.Loading;

    //    mScene.State = LevelState.Loading;
    //    mScene.mParam = param;
        
    //}

    #endregion

    #region Private Methods
    void Update()
    {

        if (!DebugUI.instance.bRunLogic || TimeMgr.instance.IsPause)     //关卡逻辑暂停 或者 游戏逻辑暂停时 都不更新
            return;

        LevelMgr.instance.CurLevel.OnUpdate();

        if (OnUpdateEventCallback != null)
        {
            OnUpdateEventCallback();
        }
    }
    void OnDrawGizmos()
    {

        if (OnDrawGizmosCallback != null)
        {
            OnDrawGizmosCallback();
        }

    }
    #endregion
}
