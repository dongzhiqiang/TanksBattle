#region Header
/**
 * 名称：可以移动和播放简单动作的角色类
 
 * 日期：2015.9.29
 * 描述：用于美术测试动作和特效
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

//[RequireComponent(typeof(CharacterController))]
public class SimpleRole : MonoBehaviour
{
    public enum enState
    {
        free,
        move,
        attack,
        behit
    }

    public enum enBehitType
    {
        beiji,
        fukong,
        jifei,
        daodi,
        qishen,
    }

    [System.Serializable]
    public class AttackCxt
    {
        public string aniName;
        public WrapMode wrapMode;
        public float duration = 0;
        public bool canRotate=false;//是不是旋转
        public float rotateSpeed = 360;//每秒转多少度
        public bool canMove = false;//是不是可以移动
        public float moveSpeed = 3;//每秒移动多少单位
    }
    #region Fields
    public static Vector3 Default_Gravity_Speed = new Vector3(0, -9.8F, 0);//模仿重力
    
    
    public float m_moveAniSpeed = 6f;//什么速度下动作播放的速度为1
    public float m_moveSpeed = 6f;
    public AttackCxt m_num5Atk = new AttackCxt();
    public AttackCxt m_num4Atk = new AttackCxt();
    public AttackCxt m_num6Atk = new AttackCxt();

    public float m_behitDuration= 0.3f;//被击隔多少时间切换回待机状态
    public float m_fade =0.2f;
    public bool m_needFade = true;
    public bool m_needResetPos = true;

    public bool m_isEmpty = false;
    public bool m_showDebug = true;

    public float m_floatBehitStartSpeed= 15;//初速度
    public float m_floatBehitAccelerated = -50;
    public float m_floatStartSpeed = 20;//初速度
    public float m_floatAcceleratedUp = -50;//上升时加速度
    public float m_floatAcceleratedDown = -10;//下落时加速度
    public float m_floatSpeedUpLimit = 100;//上升时最大速度
    public float m_floatSpeeDownLimit = -100;//下落时最大相反速度

    public float m_flyStartSpeed = 15;//初速度
    public float m_flyAcceleratedUp = -50;//上升时加速度
    public float m_flyAcceleratedDown = 0;//下落时加速度
    public float m_flySpeedUpLimit = 100;//上升时最大速度
    public float m_flySpeeDownLimit = -100;//下落时最大相反速度

    public float m_groundDuration = 0.5f;


    

    Transform m_root;
    AniFxMgr m_ani;
    CharacterController m_cc;
    enState m_state;
    Vector3 m_lastDir = Vector3.zero;
    Dictionary<enState,SimpleRoleState> m_states = new Dictionary<enState,SimpleRoleState>();

    Seeker m_seeker;
    RolePath m_rolePath;//角色寻路辅助类，在寻路插件和角色寻路功能之间提供的方便的接口，提供多点寻路、碰撞回避的功能
    
    
    #endregion

    


    #region Properties
    public Transform Root { get{return m_root;}}
    public Transform Model {get{return  this.transform.Find("model");} }
    public AniFxMgr Ani { get { return m_ani; } }
    public CharacterController CC { get{return m_cc;}}
    public Seeker Seeker { get{return m_seeker;}}
    public RolePath RolePath { get{return m_rolePath;}}
    public enState CurState { get { return m_state; } }
    #endregion


    #region Mono Frame

    void Awake()
    {
 #if ART_DEBUG    
        m_root= this.transform;   
        m_cc = GetComponent<CharacterController>();
        m_ani = this.transform.Find("model").GetComponent<AniFxMgr>();
        m_seeker = GetComponent<Seeker>();
        m_rolePath = new RolePath(m_seeker,m_root);

        m_states[enState.free] = new SimpleRoleFreeState(this);
        m_states[enState.move] = new SimpleRoleMoveState(this);
        m_states[enState.attack] = new SimpleRoleAttackState(this);
        m_states[enState.behit] = new SimpleRoleBehitState(this);
        
        GotoState(enState.free,null);

        
 #else
        this.enabled = false;//这个类是测试用的，游戏运行时屏蔽
#endif
        

    }

    

#if ART_DEBUG
    void OnEnable()
    {
        this.AddComponentIfNoExist<DynamicShadowAgent>();
        m_rolePath.OnEnable();
    }

    public void OnDisable()
    {
        m_rolePath.OnDisable();
    }

    // Update is called once per frame
    void Update()
    {
        m_states[m_state].OnUpdate();
    }

    void OnGUI()
    {
        if (!m_showDebug||CameraMgr.instance == null || CameraMgr.instance.GetFollow() != this.transform)
            return;

        DynamicShadowMgr.instance.UseDynamicShadow =GUILayout.Toggle(DynamicShadowMgr.instance.UseDynamicShadow,"开启动态阴影");
        using (new AutoChangeColor(Color.green))
        {
            GUILayout.Label("{角色浏览器}");
            GUILayout.Label(string.Format("时间缩放:{0}", Time.timeScale));
            if (m_ani == null)
                GUILayout.Label("没有AniFxMgr组件");
            else
            {
                AnimationState st = m_ani.CurSt;
                GUILayout.Label(string.Format("当前的正在播放动作:{0}", st == null ? "没找到" : st.name));
                if (st != null)
                {
                    GUILayout.Label(string.Format("当前帧:{0}", m_ani.CurFrame));
                    GUILayout.Label(string.Format("wrapMode:{0}", st.wrapMode));
                    GUILayout.Label(string.Format("enable:{0}", st.enabled));
                    GUILayout.Label(string.Format("speed:{0}", st.speed));
                    GUILayout.Label(string.Format("time:{0}", st.time));
                    GUILayout.Label(string.Format("length:{0}", st.length));
                    GUILayout.Label(string.Format("normalizedSpeed:{0}", st.normalizedSpeed));
                    GUILayout.Label(string.Format("normalizedTime:{0}", st.normalizedTime));
                }
            }
        }

        //被击相关按钮
        using (new AutoBeginArea(new Rect(Screen.width-200,30,150,100+50*3)))
        {
            if(GUILayout.Button("被击"))
                GotoState(enState.behit,enBehitType.beiji);
                
            GUILayout.Space(30);
            if (GUILayout.Button("浮空"))
                GotoState(enState.behit, enBehitType.fukong);

            GUILayout.Space(30);
            if (GUILayout.Button("击飞"))
                GotoState(enState.behit, enBehitType.jifei);
        }
        
        
    }

    //public Vector3 pathDir =Vector3.zero;
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, 0.1f);
    //    if (pathDir != Vector3.zero)
    //    {
    //        Gizmos.DrawLine(transform.position, transform.position + pathDir.normalized);
    //    }
    //}
#endif
    #endregion



    #region Private Methods
    
    
    #endregion

 

    public Vector3 GetDir()
    {
        if (CameraMgr.instance == null || CameraMgr.instance.GetFollow() != this.transform)
            return Vector3.zero;
        Vector3 caDir = CameraMgr.instance.HorizontalDir;

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            return Quaternion.Euler(0, 45, 0) * caDir;
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
            return Quaternion.Euler(0, 315, 0) * caDir;
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
            return Quaternion.Euler(0, 135, 0) * caDir;
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
            return Quaternion.Euler(0, 225, 0) * caDir;
        else if (Input.GetKey(KeyCode.W))
            return Quaternion.Euler(0, 0, 0) * caDir;
        else if (Input.GetKey(KeyCode.S))
            return Quaternion.Euler(0, 180, 0) * caDir;
        else if (Input.GetKey(KeyCode.A))
            return Quaternion.Euler(0, 270, 0) * caDir;
        else if (Input.GetKey(KeyCode.D))
            return Quaternion.Euler(0, 90, 0) * caDir;
        else
            return Vector3.zero;
    }

    //移动到某点
    public void MovePos(Vector3 pos) { m_rolePath.Move(pos);}

    //停止移动
    public void StopMovePos(){m_rolePath.Stop();}

    public Vector3 GetMovePos() { return m_rolePath.CurTargetPos; }
    public bool IsMove()
    {
        return GetDir() != Vector3.zero || !m_rolePath.Reached;
    }
    public bool CheckAttack()
    {
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            if (m_num5Atk != null)
            {
                GotoState(SimpleRole.enState.attack, m_num5Atk); 
                return true;
            }
            else
                Debuger.LogError("num5攻击动作没有填");
            
        }
        else if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            if (m_num4Atk != null)
            {
                GotoState(SimpleRole.enState.attack, m_num4Atk);     
                return true;
            }
            else
                Debuger.LogError("num4攻击动作没有填");

            
        }
        else if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            if (m_num6Atk != null)
            {
                GotoState(SimpleRole.enState.attack, m_num6Atk);     
                return true;
            }
            else
                Debuger.LogError("num6攻击动作没有填");

            
        }
        
        return false;
    }
    public  bool CheckBeiji()
    {
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            GotoState(enState.behit, enBehitType.beiji);
            return true;
        }
            

        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            GotoState(enState.behit, enBehitType.fukong);
            return true;
        }

        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            GotoState(enState.behit, enBehitType.jifei);
            return true;
        }
        return false;
    }

    public void GotoAuto()
    {
        if (IsMove())
            GotoState(SimpleRole.enState.move, null);
        else
            GotoState(SimpleRole.enState.free, null);
    }

    public void GotoState(enState s, object param)
    {
        enState lastState =m_state;
        m_state = s;
        m_states[s].OnEnter(lastState,param);

    }

}
