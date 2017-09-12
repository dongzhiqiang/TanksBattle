using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

enum enPlayerState
{
    start,
    login,
    selectServer,
    selectRole,
    playGame,
}

class PlayerStateMachine
{
    private static PlayerStateMachine s_instance = null;

    private Dictionary<enPlayerState, PlayerState> m_states = new Dictionary<enPlayerState, PlayerState>();
    private PlayerState m_curState = null;

    public static PlayerStateMachine Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new PlayerStateMachine();
            return s_instance;
        }
    }
    public PlayerStateMachine()
    {
        m_states[enPlayerState.start] = new PlayerStateStart();
        m_states[enPlayerState.login] = new PlayerStateLogin();
        m_states[enPlayerState.selectServer] = new PlayerStateSelectServer();
        m_states[enPlayerState.selectRole] = new PlayerStateSelectRole();
        m_states[enPlayerState.playGame] = new PlayerStatePlayGame();

        //先进入启动状态
        GotoState(enPlayerState.start);
    }
    public bool GotoState(enPlayerState newState, object param = null)
    {
        if (m_curState != null && m_curState.GetStateType() == newState)
        {
            m_curState.OnStateMsg(param);
            return true;
        }

        if (m_curState != null)
            m_curState.Leave();
        m_curState = m_states[newState];
        m_curState.Enter(param);
        return true;
    }
    public enPlayerState GetCurStateType()
    {
        return m_curState.GetStateType();
    }
    public PlayerState GetCurState()
    {
        return m_curState;
    }
    public PlayerState GetState(enPlayerState state)
    {
        return m_states[state];
    }
    public bool NetMsgFilter(int module, int command)
    {
        return m_curState.OnNetMsgFilter(module, command);
    }
}