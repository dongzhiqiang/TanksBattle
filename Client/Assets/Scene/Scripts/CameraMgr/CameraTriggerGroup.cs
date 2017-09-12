using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraTriggerGroup : MonoBehaviour {
    public Vector3 m_bornPos= Vector3.zero;
    public CameraInfo m_defaultInfo = new CameraInfo();
    public List<CameraTrigger> m_triggers = new List<CameraTrigger>();

    CameraHandle m_handle;
    public List<CameraTrigger> Triggers { get{return m_triggers;}}

    void Awake()
    {
       
    }

    public void Reset()
    {
        m_triggers = new List<CameraTrigger>(this.GetComponentsInChildren<CameraTrigger>(true));
        
    }
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    

    public void SetGroupActive(bool active)
    {
        if (active)
        {
            Clear();
            
            gameObject.SetActive(false);

            //这里暂时用默认镜头的参考点作为出生点
            m_handle = CameraMgr.instance.Set(m_defaultInfo);

            gameObject.SetActive(true);            
        }
        else
        {
            Clear();
            gameObject.SetActive(false);
        }
    }

    void Clear()
    {
        if (m_handle != null)
        {
            m_handle.Release();
            m_handle = null;
        }
        foreach (CameraTrigger t in m_triggers)
        {
            t.Clear();
        }
    }

    public CameraTrigger GetTriggerByName(string name)
    {
        foreach (CameraTrigger t in m_triggers)
        {
            if (t.name == name)
            {
                return t;
            }
        }

        return null;
    }
    public void SetTriggerActive(string name, bool active)
    {
        CameraTrigger t=  GetTriggerByName(name);
        if (t == null)
        {
            Debuger.LogError(string.Format("没有找到触发器，名字:{0}", name));
            return;
        }
        SetTriggerActive(t, active);
    }

    public void SetTriggerActive(CameraTrigger t, bool active)
    {
        t.gameObject.SetActive(active);
    }
    public string GetUnUseTriggerName()
    {
        int i = m_triggers.Count + 1;
        while (GetTriggerByName("Trigger" + i.ToString().PadLeft(2, '0')) != null)
        {
            ++i;
        }
        return "Trigger" + i.ToString().PadLeft(2, '0');
    }
    public  void AddTrigger()
    {
        GameObject go = new GameObject(GetUnUseTriggerName(), typeof(BoxCollider),typeof(CameraTrigger));
        go.layer = LayerMask.NameToLayer("CameraTrigger");
        Transform t = go.transform;
        CameraTrigger trigger = go.GetComponent<CameraTrigger>();
        BoxCollider box = go.GetComponent<BoxCollider>();
        t.SetParent(this.transform,false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale= new Vector3(5,10,5);
        box.isTrigger = true;
        box.enabled = true;
        trigger.m_info = new CameraInfo(CameraMgr.instance.m_curCameraInfo);
        
        Reset();
    }

    public void RemoveTrigger(CameraTrigger t)
    {
        t.Clear();
        m_triggers.Remove(t);
        GameObject.Destroy(t.gameObject);
    }
}
