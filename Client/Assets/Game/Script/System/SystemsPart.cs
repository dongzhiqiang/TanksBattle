#region Header
/**
 * 名称：物品部件
 
 * 日期：2015.9.21
 * 描述：背包和装备
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SystemsPart : RolePart
{
    #region Fields
    /// <summary>
    /// 系统ID与系统的映射
    /// </summary>
    private Dictionary<int, SystemData> m_systems = new Dictionary<int, SystemData>();
    /// <summary>
    /// 引导的数据
    /// </summary>
    private Dictionary<string, int> m_teaches = new Dictionary<string, int>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.systems; } }
    public Dictionary<int, SystemData> Systems { get { return m_systems; } }
    public Dictionary<string, int> Teaches { get { return m_teaches; } }
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {  
        if( vo.systems != null )
        {
            if (m_systems == null)
                m_systems = new Dictionary<int, SystemData>();
            else
                m_systems.Clear();
            foreach (SystemVo systemVo in vo.systems)
            {
                SystemData system = SystemData.Create(systemVo);
                AddOrUpdateSystem(system);
            }
        }

        if (vo.teaches != null)
        {
            m_teaches = vo.teaches;            
        }
        else if (m_teaches != null)
        {
            m_teaches.Clear();
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_systems != null)
            m_systems.Clear();
        if (m_teaches != null)
            m_teaches.Clear();
    }

    void AddOrUpdateSystem(SystemData system)
    {

        if (m_systems == null)
            return;

        m_systems[system.SystemId] = system;

    }

    public void AddOrUpdateSystem(SystemVo systemVo)
    {
        if (m_systems == null)
            return;

        SystemData system;
        bool activeChanged;
        //bool tipChanged;
        if (m_systems.TryGetValue(systemVo.systemId, out system))
        {
            bool oldActive = system.Active;
            system.LoadFromVo(systemVo);
            activeChanged = system.Active && !oldActive;
        }
        else
        {
            system = SystemData.Create(systemVo);
            AddOrUpdateSystem(system);
            activeChanged = system.Active;
        }
        // fire
        if(activeChanged)
        {
            SystemMgr.instance.FireActive((enSystem)system.SystemId);
        }
    }

    public SystemData GetSystem(int systemId)
    {
        SystemData result;
        if( m_systems.TryGetValue(systemId, out result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    public int GetTeachVal(string key)
    {
        if (m_teaches == null || key == null)
            return 0;
        int val = 0;
        m_teaches.TryGetValue(key, out val);
        return val;
    }

    public bool SetTeachData(string key, int val)
    {
        if (m_teaches == null)
            return false;
        var oldVal = GetTeachVal(key);
        if (oldVal == val)
            return true;
        if (!TeachConfig.IsTeachKeyOK(key))
            return false;
        m_teaches[key] = val;
        NetMgr.instance.SystemHandler.SendSetTeachData(key, val);
        return true;
    }
    #endregion


    #region Private Methods

    #endregion
}