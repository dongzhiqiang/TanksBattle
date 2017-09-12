#region Header
/**
 * 名称：角色碰撞监听者
 
 * 日期：2016.1.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RoleColliderListener : MonoBehaviour
{
    #region Fields
    HashSet<RoleModel> m_colliderRoles = new HashSet<RoleModel>();
    Collider m_collider;//碰撞
    bool m_cache = false;
    Action< Role> m_onTrigger;
    bool m_isPlaying = false;
    #endregion


    #region Properties

    public HashSet<RoleModel> ColliderRoles { get{return m_colliderRoles;}}
    #endregion

    #region Static Methods
    public static RoleColliderListener Set(GameObject go,Action<Role> onTrigger)
    {
        var collider = go.GetComponent<Collider>();
        if(collider==null)
        {
            Debuger.LogError("碰撞监听器，只有Collider上的isTrigger勾上才可以监听:{0}", go.name);
            return null;
        }

        var listener = go.AddComponentIfNoExist<RoleColliderListener>();
        if (!listener.Init(onTrigger))
            return null;
        return listener;
    }

    #endregion


    #region Mono Frame
    void Awake()
    {
        Cache();
    }

    void OnDisable()
    {
        Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        RoleModel roleModel =other.GetComponent<RoleModel>();
        if(roleModel == null )return;
        if(!m_colliderRoles.Add(roleModel))
        {
            Debuger.LogError("逻辑错误 飞出物碰撞重复碰到角色:{0}", roleModel.name);
        }

        if(roleModel.Parent!= null && roleModel.Parent.State == Role.enState.alive)
            m_onTrigger(roleModel.Parent);
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == null )
            return;
        RoleModel roleModel = other.GetComponent<RoleModel>();
        if (roleModel == null) return;
        m_colliderRoles.Remove(roleModel);
    }
    #endregion

    void Cache()
    {
        if (m_cache) return;
        m_cache = true;
        m_collider = this.GetComponent<Collider>();
        if (m_collider == null)
            return;
        m_collider.enabled = false;
        if(!m_collider.isTrigger)
        {
            Debuger.LogError("碰撞监听器，只有Collider上的isTrigger勾上才可以监听:{0}", this.gameObject.name);
        }

        //必须加刚体并设置动力学，见unity物理引擎的文档，静态碰撞和触发型碰撞的区别
        Rigidbody r = this.AddComponentIfNoExist<Rigidbody>();
        r.isKinematic = true;

        //设置成只碰到角色的层
        LayerMgr.instance.SetLayer(this.gameObject, enGameLayer.flyerTrigger);
    }

    public  bool Init(Action< Role> onTrigger)
    {
        Cache();

        if(m_collider == null)
        {
            Debuger.LogError("碰撞监听器初始化失败，找不到碰撞:{0}", this.gameObject.name);
            return false;
        }
        
        if(m_isPlaying)
        {
            Debuger.LogError("碰撞监听器暂时不支持监听多个:{0}", this.gameObject.name);
            return false;
        }


        if (m_colliderRoles.Count > 0)
        {
            Debuger.LogError("逻辑错误，碰撞监听器初始化的时候发现碰撞到的角色列表不为空:{0}", this.gameObject.name);
            m_colliderRoles.Clear();
        }
        m_onTrigger = onTrigger;
        this.enabled = true;
        m_collider.enabled = true;
        m_isPlaying = true;
        return true;
    }

    public void Stop()
    {
        this.enabled = false;
        if (m_collider == null)
            return;
        m_collider.enabled = false;
        m_colliderRoles.Clear();
        m_onTrigger = null;
        m_isPlaying = false;
    }
    

}
