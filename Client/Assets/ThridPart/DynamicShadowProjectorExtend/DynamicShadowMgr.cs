using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicShadowMgr : SingletonMonoBehaviour<DynamicShadowMgr>
{
    int m_observer;
    bool m_useDynamicShadow = true;

    List<DynamicShadowAgent> m_activeAgents = new List<DynamicShadowAgent>();//活动中的阴影
    List<DynamicShadowAgent> m_removes= new List<DynamicShadowAgent>();

    public bool UseDynamicShadow { get{return m_useDynamicShadow;}set{
        if(m_useDynamicShadow == value)return;
        m_useDynamicShadow = value;
        for (int i = 0; i < m_activeAgents.Count; ++i)
        {
            m_activeAgents[i].FlashShadow(UseDynamicShadow);
        }
    }}

	// Use this for initialization
	void Start () {
        //这里要重新设置下已经加的阴影的角度，可能之前相机管理器还没初始化好，角度没设置成功
	    if(CameraMgr.instance != null )
        {
            float angle= CameraMgr.instance.m_dynamicShadowAngle;
            for (int i = 0; i < m_activeAgents.Count; ++i)
            {
                m_activeAgents[i].SetDynamicShadowAngle(angle);
            }
        }
	}

    void OnEnable()
    {
#if !ART_DEBUG
        m_observer=EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.WEAPON_RENDER_CHANGE, OnRenderChange);
#endif
    }

    public bool m_isApplicationQuit = false;
    void OnApplicationQuit()
    {
        m_isApplicationQuit = true;
    }

    void OnDisable()
    {
        if (m_isApplicationQuit)
            return;
#if !ART_DEBUG
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
#endif
    }
	
    // Update is called once per frame
    void LateUpdate()
    {
        //延迟删除要删除的阴影
        if (m_removes.Count != 0)
        {
            for (int i = 0; i < m_removes.Count; ++i)
            {
                m_removes[i].DisableAllShadow();
                m_activeAgents.Remove(m_removes[i]);
            } 
            m_removes.Clear();
        }

        //跟随
        if(UseDynamicShadow)
        {
            for (int i = 0; i < m_activeAgents.Count; ++i)
            {
                m_activeAgents[i].DynamicShadowFollow();
            }    
        }
        
    }

#if !ART_DEBUG
    void OnRenderChange(object param1, object param2, object param3, EventObserver observer){
        Role r = observer.GetParent<Role>();
        DynamicShadowAgent agent =r.transform.GetComponent<DynamicShadowAgent>();
        if(agent == null)
            return;
        agent.SetDirtyDynamicShadow();
    }
#endif

    public void Add(DynamicShadowAgent agent)
    {
        if (m_removes.Contains(agent))
        {
            m_removes.Remove(agent);
            return;
        }
        if (m_activeAgents.Count > 40)
        {
            Debuger.LogError("动态阴影管理器中的动态阴影太多，可能泄露");
        }
        m_activeAgents.Add(agent);
        agent.FlashShadow(UseDynamicShadow);
    }

    //这里不能马上就删除，留到下一个update再删除，因为unity会报这个错误Cannot change GameObject hierarchy while activating or deactivating the parent.
    public void Remove(DynamicShadowAgent agent)
    {
        m_removes.Add(agent);
        
    }

    
}
