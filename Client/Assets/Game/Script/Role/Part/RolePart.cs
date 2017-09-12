#region Header
/**
 * 名称：角色部件
 
 * 日期：2015.9.21
 * 描述：将角色需要的功能拆成单独的模块，这个类是这些单独模块的基类
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class RolePart
{
    #region Fields
    protected Role m_parent;
    #endregion


    #region Properties
    public Role Parent{get{return m_parent;}}
    public abstract enPart Type { get;}

    //1 模型层(一般是内部调用，非部件一定要慎用)
    public RoleModel RoleModel { get { return m_parent.RoleModel; } }
    public Transform transform { get { return m_parent.transform; } }
    public TranPart TranPart { get { return m_parent.TranPart; } }
    public AniPart AniPart { get { return m_parent.AniPart; } }
    public RenderPart RenderPart { get { return m_parent.RenderPart; } }
    public RoleStateMachine RSM { get { return m_parent.RSM; } }

    //2 数据层(属性、状态、仇恨）
    public PropPart PropPart { get { return m_parent.PropPart; } }
    public BuffPart BuffPart { get { return m_parent.BuffPart; } }
    public HatePart HatePart { get { return m_parent.HatePart; } }

    //3 战斗层(移动、战斗、死亡等，和RSM有对应关系，本质上是为了更好地控制RSM)
    public DeadPart DeadPart { get { return m_parent.DeadPart; } }
    public MovePart MovePart { get { return m_parent.MovePart; } }
    public CombatPart CombatPart { get { return m_parent.CombatPart; } }

    //4 控制层(ai)
    public AIPart AIPart { get { return m_parent.AIPart; } }

    public EquipsPart EquipsPart { get { return m_parent.EquipsPart; } }
    public WeaponPart WeaponPart { get { return m_parent.WeaponPart; } }
    public TreasurePart TreasurePart { get { return m_parent.TreasurePart; } }
    #endregion


    #region Frame    

    public bool Init(Role parent)
    {
        m_parent = parent;
        return OnInit();
    }

    

    //属于角色的部件在角色第一次创建的时候调用，属于模型的部件在模型第一次创建的时候调用
    public virtual void OnCreate(RoleModel model) { }

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public abstract bool OnInit();

    //网络数据初始化,注意只有网络角色会被调用
    public virtual void OnNetInit(FullRoleInfoVo vo) { }


    //模型创建的时候被调用,可以安全调用其他部件，在所有部件OnModelInit才进行OnPostInit
    public abstract void OnPostInit();

    //模型销毁时被调用
    public virtual void OnDestroy() { }

    //角色销毁的时候被调用
    public virtual void OnClear() { }

    //每帧更新
    public virtual void OnUpdate() { }

    //计算战斗力
    public virtual void OnFreshBaseProp(PropertyTable values,PropertyTable rates) { }

    //网络属性有改变
    public virtual void OnSyncProps(List<int> props){}

    //预加载，注意只有网络角色会调用到这个接口，关卡中的怪物是不会的
    public virtual void OnPreLoad() { }
    #endregion


    #region Private Methods
    
    #endregion
}
