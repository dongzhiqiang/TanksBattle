using UnityEngine;
using System.Collections;
using DynamicShadowProjector;
public class DynamicShadowAgent : MonoBehaviour
{
    bool m_cache=false;
    Transform m_root; 
    Transform m_staticShadow;
    Transform m_dynamicShadow;
    Transform m_model;
    DrawTargetObject m_drawTargetObject;
    

    void OnEnable()
    {
        Cache();
        DynamicShadowMgr.instance.Add(this);
    }

    void OnDisable()
    {
        if (DynamicShadowMgr.instance!= null)
            DynamicShadowMgr.instance.Remove(this);
        
    }

    

    void Cache()
    {
        //初始化
        if (m_cache) return;
        m_cache = true;

        m_root = this.transform;
        m_staticShadow = m_root.Find("fx_shadow");
        m_dynamicShadow = m_root.Find("fx_shadow_dynamic");
        if (m_staticShadow == null || m_dynamicShadow == null)
        {
            //Debuger.LogError("模型找不到阴影或者动态阴影:{0}",this.gameObject.name);
        }
        m_model = m_root.Find("model");
        if (m_model == null)
            m_model = m_root;

        if(m_dynamicShadow!= null){
            m_drawTargetObject =m_dynamicShadow.Find("Shadow Projector").GetComponent<DrawTargetObject>();
        }
        

    }

    public void FlashShadow(bool dynamic)
    {
        if (dynamic)
        {
            DisableStaticShadow();
            EnableDynamicShadow();
        }
        else
        {
            DisableDynamicShadow();
            EnableStaticShadow();
        }
    }

    public void DisableAllShadow()
    {
        DisableStaticShadow();
        DisableDynamicShadow();
    }

    public void EnableStaticShadow()
    {
        if(m_staticShadow!= null&&!m_staticShadow.gameObject.activeSelf)
            m_staticShadow.gameObject.SetActive(true);
    }

    public void DisableStaticShadow()
    {
        if (m_staticShadow != null && m_staticShadow.gameObject.activeSelf)
            m_staticShadow.gameObject.SetActive(false);
    }

    public void SetDynamicShadowAngle(float angle)
    {
        if (m_dynamicShadow != null && m_dynamicShadow.parent != m_root)
        {
            m_dynamicShadow.eulerAngles = new Vector3(0, angle, 0);    
        }
        
    }
    public void DynamicShadowFollow()
    {
        if (m_dynamicShadow != null && m_dynamicShadow.parent != m_root)
        {
            m_dynamicShadow.position = m_model.position;
        }
    }
    public void EnableDynamicShadow()
    {
        if (m_dynamicShadow != null && (m_dynamicShadow.parent == m_root || !m_dynamicShadow.gameObject.activeSelf))
        {
            m_dynamicShadow.SetParent(DynamicShadowMgr.instance.gameObject.transform, false);
            if(CameraMgr.instance != null )
                SetDynamicShadowAngle(CameraMgr.instance.m_dynamicShadowAngle);
            DynamicShadowFollow();
            m_dynamicShadow.gameObject.SetActive(true);
        }
    }

    public void DisableDynamicShadow()
    {
        if (m_dynamicShadow != null && (m_dynamicShadow.parent != m_root || m_dynamicShadow.gameObject.activeSelf))
        {
            m_dynamicShadow.SetParent(m_root,false);
            m_dynamicShadow.gameObject.SetActive(false);
        }
        
    }

    public void SetDirtyDynamicShadow()
    {
        if (m_drawTargetObject!=null &&m_dynamicShadow != null && (m_dynamicShadow.parent != m_root || m_dynamicShadow.gameObject.activeSelf))
        {
            m_drawTargetObject.SetCommandBufferDirty();
        }
    }
}
