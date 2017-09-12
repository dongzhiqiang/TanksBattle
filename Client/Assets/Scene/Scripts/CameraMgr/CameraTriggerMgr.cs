using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//镜头触发器组的管理器,注意组不会自动激活
public class CameraTriggerMgr : MonoBehaviour {
    public static CameraTriggerMgr instance;
    public CameraTriggerGroup m_cur = null;
    bool m_cache=false;

    public List<CameraTriggerGroup> m_groups = new List<CameraTriggerGroup>();

    public string[] GroupNames
    {
        get { 
            Cache();
            string[] strs = new string[m_groups.Count];
            for (int i = 0; i < m_groups.Count; ++i)
            {
                strs[i] = m_groups[i].name;
            }
            return strs;
        }

    }

    public CameraTriggerGroup CurGroup
    {
        get { Cache(); return m_cur; }
        set {
            Cache();  
            if (value == null){
                Debuger.LogError("不能设置当前镜头组为空");
                return;
            }
            
            m_cur.SetGroupActive(false);
            m_cur =value;
            m_cur.SetGroupActive(true);
        }
    }

    void Awake()
    {
        Cache();
    }

    void OnDestroy()
    {
        instance = null;
    }

    void Cache()
    {
        if(m_cache)return;
        m_cache =true;
        instance = this;
        Reset();
        //如果没有默认的，那么第一个就是默认的
        if (m_cur == null && m_groups.Count > 0)
        {
            m_cur = m_groups[0];
        }
    }
    
    public void Reset()
    {
        //重新收集所有的触发器组
        Transform t = this.transform;
        m_groups.Clear();
        CameraTriggerGroup group;
        for (int i = 0, imax = t.childCount; i < imax; ++i)
        {
            group = t.GetChild(i).GetComponent<CameraTriggerGroup>();
            if (group)
            {
                group.Reset();
                group.SetGroupActive(false);
                m_groups.Add(group);
            }
        }
    }
	
	
	// Update is called once per frame
	void Update () {
	
	}

    //根据名字找到
    public CameraTriggerGroup GetGroupByName(string name)
    {
        Cache();
        foreach(CameraTriggerGroup g in m_groups){
            if (g.gameObject.name == name)
                return g;
        }
        
        return null;
    }


    //获取一个别人没有用过的名字
    public string GetUnUseGroupName()
    {
        int i = m_groups.Count+1;
        while (GetGroupByName("Group" + i) != null)
        {
            ++i;
        }
        return "Group" + i;
    }
    public CameraTriggerGroup AddGroup()
    {
        GameObject go = new GameObject(GetUnUseGroupName(), typeof(CameraTriggerGroup));
        Transform t = go.transform;
        CameraTriggerGroup group = go.GetComponent<CameraTriggerGroup>();
        t.SetParent( transform,false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        group.m_defaultInfo = new CameraInfo(CameraMgr.instance.m_curCameraInfo);
        //group.m_defaultInfo.priority = CameraInfo.Camera_Default_Priority;
        Reset();
        return group;
    }
}
