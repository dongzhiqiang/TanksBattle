using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class CameraTrigger : MonoBehaviour {
    public CameraInfo m_info = new CameraInfo();

    [System.NonSerialized]
    public bool isExpand = true;
    

    CameraHandle m_handle;
    
	// Use this for initialization
	void Awake () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    
    void OnTriggerEnter(Collider other)
    {
        if (other == null || other.gameObject == null || CameraMgr.instance == null || (CameraMgr.instance.GetFollow() != other.GetComponent<Transform>()))
            return;
#if !ART_DEBUG
        //有战斗镜头 没有结束前不能受区域碰撞切换其他镜头
        if (SceneMgr.instance.FightCamera != null)
        {
            return;
        }
#endif
        if (m_handle != null)
        {
            Debuger.LogError("重复进入？？");
            return;
        }
    
        m_handle =CameraMgr.instance.Add(m_info);
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == null || CameraMgr.instance == null || (CameraMgr.instance.GetFollow() != other.GetComponent<Transform>()) || m_handle==null)
            return;
        Clear();
    }

    public void Clear()
    {
        if (m_handle!= null)
        {
            m_handle.Release();
            m_handle = null; 
        }
    }
}
