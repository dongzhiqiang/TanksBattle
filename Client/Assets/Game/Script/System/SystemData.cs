using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class SystemData
{
    #region Fields
    int m_systemId;
    bool m_active;
    #endregion

    #region Properties
    public int SystemId { get { return m_systemId; } set { m_systemId = value; } }
    public bool Active { get { return m_active; } set { m_active = value; } }
    #endregion

    public static SystemData Create(SystemVo vo)
    {
        SystemData system;
        system = new SystemData();
        system.LoadFromVo(vo);
        return system;
    }

    virtual public void LoadFromVo(SystemVo vo)
    {
        m_systemId = vo.systemId;
        m_active = vo.active;
    }
}