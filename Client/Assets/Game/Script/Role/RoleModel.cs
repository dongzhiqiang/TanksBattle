#region Header
/**
 * 名称：角色模型类
 
 * 日期：2015.9.21
 * 描述：
 *      1.角色和unity的交互类，是渲染世界中游戏对象上的一个组件
 *      2.另外也持有着控制游戏对象的RolePart(比如用于控制动画的AniPart)
 *      3.各个部件的OnUpdate都在Update中执行
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simple.BehaviorTree;

//[RequireComponent(typeof(CharacterController))]
public class RoleModel : MonoBehaviour
{
    #region Fields
    public bool m_undead= false;
    public bool m_unAttack = false;
    public bool m_debugAI = false;

    public Vector3 m_showOffset=Vector3.zero;
    public Color m_showClr = Color.green;
    public bool m_debugProp=false;
    public bool m_debugBuff= false;
    public bool m_debugNotifier = false;
    public bool m_debugHate = false;
    public bool m_debugFlag = false;
    public string m_debugSkillId = "";//技能id|连击技能id|返回值类型，默认都可以填-1，那么就是全部调试
    public string m_debugState = "";//老状态|新状态，默认都可以填-1，那么就是全部调试，出生,空闲,移动,战斗,被击,死亡,动作序列,下落,换武器
    


    Role m_parent;
    SimpleRole m_simpleRole;
    Transform m_root;//根,这里放Transform比放GameObject方便
    Transform m_model;
    CharacterController m_CC;//角色控制器
    Transform m_title;
    Transform m_center;
    Transform m_foot;//脚底方向

    Seeker m_seeker;
    RolePath m_rolePath;//角色寻路辅助类，在寻路插件和角色寻路功能之间提供的方便的接口，提供多点寻路、碰撞回避的功能

    

    RolePart[] m_parts = new RolePart[(int)enPart.max];//模型上的部件

    bool  m_isUpdating;
    bool m_created =false;
    bool m_isShow = true;//是不是显示模型中,false则为隐藏，实际上只是把模型移到了很远的地方，不是真正隐藏
    bool m_isGround = false; //模型是不是处于倒地状态
    float m_height = 1f;
    #endregion


    #region Properties
    public Role Parent { get { return m_parent; } }

    //1 底层(一般是内部调用，非部件一定要慎用)
    public Transform Root { get { return m_root; } }
    public Transform Model { get { return m_model; } }
    public Transform Tran { get { return m_model != null ? m_model : m_root; } }
    public Transform Title { get{return m_title;}}
    public Transform Center { get { return m_center; } }
    public Transform Foot { get { return m_foot; } }
    public SimpleRole SimpleRole { get { return m_simpleRole; } }
    public CharacterController CC { get { return m_CC; } }
    public float Radius { get { return m_CC!=null?m_CC.radius:0.5f; } }
    public float Height { get { return m_height; } }
    public Seeker Seeker { get { return m_seeker; } }
    public RolePath RolePath { get { return m_rolePath; } }
    
    public TranPart TranPart { get { return m_parent.TranPart; } }
    public AniPart AniPart { get { return m_parent.AniPart; } }
    public RenderPart RenderPart { get { return m_parent.RenderPart; } }
    public RoleStateMachine RSMPart { get { return m_parent.RSM; } }
    
    //2 数据层(属性、状态、仇恨）
    public PropPart PropPart { get { return m_parent.PropPart; } }
    public BuffPart BuffPart { get { return m_parent.BuffPart; } }
    public HatePart HatePart { get { return m_parent.HatePart; } }

    //3 战斗层(移动、战斗、死亡等，和RSM有对应关系，本质上是为了更好地控制RSM)
    public DeadPart DeadPart { get{return m_parent.DeadPart;}}
    public MovePart MovePart { get{return m_parent.MovePart;}}
    public CombatPart CombatPart { get{return m_parent.CombatPart;}}

    //4 控制层(ai)
    public AIPart AIPart { get{return m_parent.AIPart;}}


    public bool IsUpdating { get{return m_isUpdating;}}
    public bool IsCreated { get{return m_created;}}
    public bool IsShow { get{return m_isShow;}}
    public bool IsGround { get { return m_isGround; } set { m_isGround = value; } }
    #endregion


    #region Mono Frame
    // Update is called once per frame
    void Update()
    {
        
        m_isUpdating = true;
        RolePart part;
        //这里要先遍历上层部件，因为上层部件会对底层部件做修改，
        //如果修改是在update才执行的，那么这种方式下同一个update就会修改到
        for(int i = (int)(enPart.max)-1;i>=0;--i){
            part = m_parent.GetPart((enPart)i);
            if(part!= null)
                part.OnUpdate();
        }

        if (Root.transform.position.y < -300)
        {
            Debuger.LogError("角色一直往下掉:{0}", Root.transform.position);
            DeadPart.Handle(true);
        }

        m_isUpdating = false;

        //循环结束才让角色死亡或者销毁
        if(!RoleMgr.instance.CheckDestroy())
            RoleMgr.instance.CheckDead();
        
        
        
    }
    #endregion

    #region Frame
    //首次创建调用
    public void OnCreate()
    {
        if (m_created)
        {
            Debuger.LogError("重复初始化");
            return;
        }

        m_root = this.transform;
        m_model = m_root.Find("model");
        m_CC = this.GetComponent<CharacterController>();//允许没有碰撞
        m_simpleRole = m_root.GetComponent<SimpleRole>();


        //动态阴影支持
        if (!m_simpleRole.m_isEmpty)
            this.AddComponentIfNoExist<DynamicShadowAgent>();

        //寻路支持
        if (!m_simpleRole.m_isEmpty)
        {
            m_seeker = GetComponent<Seeker>();
            m_rolePath = new RolePath(m_seeker, m_root);
            m_rolePath.OnEnable();//第一次创建的时候在这里调用
        }
            
        
        m_created = true;

        m_title = m_model.Find("Title");
        if (m_title == null)
        {
            Debuger.LogError("找不到Title.{0}",this.name);
            GameObject go = new GameObject();
            m_title = go.transform;
            m_title.SetParent(m_root);
            m_title.localEulerAngles = Vector3.zero;
            m_title.localPosition = Vector3.up*2;
        }
        m_height = m_title.localPosition.y;
        m_center = m_model.Find("Center");
        if (m_center == null)
        {
            Debuger.LogError("找不到中心点.{0}", this.name);
            GameObject go = new GameObject();
            m_center = go.transform;
            m_center.SetParent(m_root);
            m_center.localEulerAngles = Vector3.zero;
            m_center.localPosition = Vector3.up ;
        }

        RolePartType partType;
        for(int i = 0;i<(int)enPart.max;++i){
            partType = RolePartType.s_parts[i];
            if (partType != null && partType.createType == enPartCreate.model)
            {
                m_parts[i] = (RolePart)System.Activator.CreateInstance(partType.partType);
                m_parts[i].OnCreate(this);
            }
        }   
    }

    void OnEnable()
    {
        if(m_created && m_rolePath!=null)
            m_rolePath.OnEnable();
    }

    public bool m_isApplicationQuit = false;
    void OnApplicationQuit()
    {
        m_isApplicationQuit = true;
    }


    public void OnDisable()
    {
        if (m_isApplicationQuit)
            return;

        if (m_created && m_rolePath != null)
            m_rolePath.OnDisable();
    }

    //初始化
    public bool Init(Role role)
    {
        this.enabled = true;

        if (!m_isShow)
            Show(true);

        //第一次创建要做一些操作
        if (!IsCreated)
            OnCreate();

        //部件设置到role上
        m_parent = role;
        RolePartType partType;
        for (int i = 0; i < (int)enPart.max; ++i)
        {
            partType = RolePartType.s_parts[i];
            if (partType != null && partType.createType == enPartCreate.model)
            {
                m_parent.SetPart(m_parts[i]);
                if(!m_parts[i].Init(m_parent))
                    return false;
            }       
        }


        

        //如果自己是主角，加脚底方向
        if (m_foot != null)
        {
            Debuger.LogError("脚底方向特效没有清空");
            Object.Destroy(m_foot.gameObject);
        }
        if (m_parent.IsHero)
        {
            GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately("fx_humen_prj_root");
            m_foot = go.transform;
            m_foot.parent = m_root;
            m_foot.localPosition = Vector3.zero;
            m_foot.localEulerAngles = Vector3.zero;
            m_foot.localScale = Radius * 2 * m_root.localScale;
        }

        return true;
    }

    public void OnRoleDestroy()
    {
        this.enabled =false;//界面上可能也要用到，所以这里要关掉
        RolePartType partType;
        for(int i = (int)(enPart.max)-1;i>=0;--i)//这里要先遍历上层部件，因为上层部件会对底层部件做修改，
        {
            partType = RolePartType.s_parts[i];
            if (partType != null && partType.createType == enPartCreate.model)
            {
                m_parts[i].OnDestroy();
            }
        }
        m_parent = null;
        if (m_foot != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(m_foot.gameObject);
            m_foot = null;
        }
            
    }
    #endregion

    #region Private Methods
    
    #endregion

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!m_debugProp && !m_debugBuff && !m_debugNotifier&&!m_debugFlag &&
            !DebugUI.instance.debugProp&& 
            !DebugUI.instance.debugBuff&&
            !DebugUI.instance.debugNotifier&& 
            !DebugUI.instance.debugHate&&
            !DebugUI.instance.debugFlag) return;
        if (CameraMgr.instance == null || CameraMgr.instance.CurCamera==null) return;

        using (new AutoChangeContentColor(m_showClr))
        {
            using (new AutoFontSize(12))
            {
                //角色在屏幕上的位置
                Vector3 pos = CameraMgr.instance.CurCamera.WorldToScreenPoint(m_root.position);//x、y是屏幕坐标，左下角(0,0)，z是相机和位置的距离
                Rect rc = new Rect(pos.x - 40 + m_showOffset.x, Screen.height - pos.y - 40 + m_showOffset.y, 250, 500);

                if (m_debugProp||DebugUI.instance.debugProp)
                {
                    PropertyTable values = m_parent.PropPart.Values;
                    PropertyTable buffValues = m_parent.PropPart.AliveValues;
                    PropertyTable buffRates = m_parent.PropPart.AliveRates;
                    PropertyTable props = m_parent.PropPart.Props;
   
                    using (new AutoBeginArea(rc))
                    {
                        string str = "";
                        str += "属性\n";
                        str += string.Format("唯一id:{0}\n", m_parent.Id);
                        str += string.Format("roleId:{0}\n", m_parent.Cfg.id);
                        str += string.Format("lv:{0}\n", m_parent.GetInt(enProp.level));
                        str += string.Format("hp:{0}\n", m_parent.GetInt(enProp.hp));
                        str += string.Format("mp:{0}\n", m_parent.GetInt(enProp.mp));
                        str += string.Format("shield:{0}\n", m_parent.GetInt(enProp.shield));
                        str += string.Format("power:{0}\n", m_parent.GetInt(enProp.power));
                        foreach (PropTypeCfg c in PropTypeCfg.m_cfgs.Values)
                        {
                            str += string.Format("{0}:({1}+{2})*(1+{3})={4}\n", c.name,
                                values.GetFloat((enProp)c.id),
                                buffValues.GetFloat((enProp)c.id),
                                buffRates.GetFloat((enProp)c.id),
                                props.GetFloat((enProp)c.id));
                        }

                        GUILayout.Label(str);
                    }
                    
                    rc.x += 170;
                }

                if (m_debugBuff || DebugUI.instance.debugBuff)
                {
                    using (new AutoBeginArea(rc))
                    {
                        string str = "";
                        str += "状态\n";
                        foreach (Buff buff in BuffPart.Buffs)
                        {
                            str += string.Format("{0} {1} {2}\n", buff.Cfg.id.ToString().PadRight(6), buff.Cfg.type, buff.TimeLeft.ToString("###0.#"));
                        }
                        GUILayout.Label(str);
                    }
                    rc.x += 100;
                }

                if (m_debugNotifier || DebugUI.instance.debugNotifier)
                {
                    using (new AutoBeginArea(rc))
                    {
                        
                        GUILayout.Label(m_parent.Notifier.LogObservers());
                    }
                    rc.x += 150;
                }

                if(m_debugHate || DebugUI.instance.debugHate)
                {
                    using (new AutoBeginArea(rc))
                    {
                        GUILayout.Label(HatePart.LogHates());
                    }
                    rc.x += 150;
                }
                if (m_debugFlag || DebugUI.instance.debugFlag)
                {
                    using (new AutoBeginArea(rc))
                    {
                        GUILayout.Label(PropPart.LogFlags());
                    }
                    rc.x += 150;
                }
            }
        }
        
        
    }
#endif

    //显示或者隐藏本角色
    public void Show(bool show)
    {
        if (m_isApplicationQuit)
            return;
        if(!this.gameObject.activeInHierarchy)
        {
            Debuger.LogError("角色已经被隐藏了，缺仍然被设置显示状态");
            return;
        }

        if (m_isShow == show)
        {
            Debuger.Log("{0}重复{1}", m_parent.Cfg.id,show?"显示":"隐藏");
            return;
        }
            

        m_isShow = show;
        if (m_foot!= null)
            m_foot.gameObject.SetActive(show);

        if (SimpleRole.m_needResetPos)
            m_model.localPosition = show ? Vector3.zero : Vector3.down * 10000;//移动到很远的地方就看不到角色了，这里不能修改SkinMeshRenderer，因为另外有地方控制到
        else
        {
            if (show)
                m_model.localPosition = new Vector3(m_model.localPosition.x, m_model.localPosition.y / 1000, m_model.localPosition.z);
            else
                m_model.localPosition = new Vector3(m_model.localPosition.x, m_model.localPosition.y * 1000, m_model.localPosition.z);
        }
    }

}
