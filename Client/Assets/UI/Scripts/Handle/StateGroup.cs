/*
 * *********************************************************
 * 名称：状态组控件
 
 * 日期：2015.7.18
 * 描述：
 * 1.组内所有子状态控件的状态转换的功能。比如tab类型的话，一个控件为按下状态，那么其他控件为提起状态
 * 2.点击监听
 * 3.自动查找子状态控件
 * 4.动态创建状态控件
 * *********************************************************
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;


public class StateGroup : MonoBehaviour {
    public enum CtrlType
    {
        none,//不改变状态
        tab
    }

    public List<StateHandle> m_stateHandles = new List<StateHandle>();
    public StateHandle m_itemTemplate = null;   //元素模板，增加元素时使用，为null时，就用现有元素的最后一个
    public bool m_autoFindOnStart = true;
    public CtrlType m_ctrlType = CtrlType.none;

    bool m_cached = false;
    StateHandle m_curStateHandle = null;
    Action<StateHandle, int> m_onSel;   //参数0是item，参数1是item在group中对应的索引

    static Action<StateGroup, int> ms_globalStateHook;

    //注意如果SetCount(0)的话，实际是隐藏自己，而不是删掉所有的子控件，这个时候Count不为0
    public int Count { get { Cache(); return m_stateHandles.Count; } }
    public StateHandle CurStateHandle { get { Cache(); return m_curStateHandle;} }
    public int CurIdx { get { Cache(); return m_curStateHandle != null ? m_stateHandles.IndexOf(m_curStateHandle) : -1; } }

    public StateHandle this[int index] { get { Cache(); return Get(index); } }

    public static void AddGlobalHook(Action<StateGroup, int> onSel = null)
    {
        if (onSel != null)
            ms_globalStateHook += onSel;
    }
    public static void RemoveGlobalHook(Action<StateGroup, int> onSel = null)
    {
        if (onSel != null)
            ms_globalStateHook -= onSel;
    }

    public IEnumerator GetEnumerator()
    {
        Cache();
        return this.m_stateHandles.GetEnumerator();
    }

    // Use this for initialization
    void Start()
    {
        Cache();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region 框架
    void Cache()//有时候别的函数的执行可能先于Start()函数，这时候Cache()下就相当于先执行了Start()
    {
        if (m_cached)
            return;
        
        if (m_autoFindOnStart)
        {
            Find();
        }

        //类型状态机
        switch (m_ctrlType)
        {
            case CtrlType.none: break;
            case CtrlType.tab: OnCacheTab(); break;//如果是tab控件，确保有且只有一个处于选中状态
            default: Debuger.LogError(string.Format("未作cache的控件类型", m_ctrlType)); break;
        }
        
        //添加监听
        foreach (var s in m_stateHandles)
        {
            if (s == null)
            {
                Debuger.LogError("StateGroup 的子状态控件里有null");
                continue;
            }
            s.AddClickEx(OnSel);
        }

        for(int i = 0;i<m_stateHandles.Count;++i)
            m_stateHandles[i].gameObject.name = "item" + i;
        m_cached = true;
    }

    [ContextMenu("FindChild")]
    void MenuFind()
    {
        Find();
        m_autoFindOnStart = false;
        EditorUtil.SetDirty(this);
    }

    void Find()
    {
        Transform t = this.transform;
        for (int i = 0; i < t.childCount; ++i)
        {
            StateHandle s = t.GetChild(i).GetComponent<StateHandle>(); ;
            if (s== null)
                continue;

            if (m_stateHandles.IndexOf(s)!=-1)
                continue;
            m_stateHandles.Add(s);
        }
        EditorUtil.SetDirty(this);
    }

    public void AddSel(Action<StateHandle, int> cb, bool reset = false)
    {
        Cache();
        if (m_onSel == null || reset)
        {
            m_onSel = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onSel.GetInvocationList();
        foreach (Delegate d in inlist)
        {
            if (d == cb)
                return;
        }

        m_onSel += cb;
    }

    public void SetSel(int idx, bool isDuration =true)
    {
        Cache();
        if (idx < 0 || idx >= m_stateHandles.Count)
        {
            Debuger.LogError(string.Format("索引不存在!{0}/{1}", idx, m_stateHandles.Count));
            OnSel(m_stateHandles[0], isDuration, false);
            return;
        }
        
        OnSel(m_stateHandles[idx],isDuration,false);
    }

    public void MoveChildTo(int fromIdx, int toIdx)
    {
        fromIdx = Mathf.Clamp(fromIdx, 0, m_stateHandles.Count - 1);
        toIdx = Mathf.Clamp(toIdx, 0, m_stateHandles.Count);

        if (fromIdx == toIdx)
            return;
        
        var item = m_stateHandles[fromIdx];
        //从后往前移
        if (fromIdx > toIdx)
        {
            m_stateHandles.RemoveAt(fromIdx);
            m_stateHandles.Insert(toIdx, item);
        }
        //从前往后移
        else
        {
            m_stateHandles.Insert(toIdx, item);
            m_stateHandles.RemoveAt(fromIdx);
        }
        transform.GetChild(fromIdx).SetSiblingIndex(toIdx);
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void MoveChildToFirst(int fromIdx)
    {
        MoveChildTo(fromIdx, 0);
    }

    public void MoveChildToLast(int fromIdx)
    {
        MoveChildTo(fromIdx, m_stateHandles.Count - 1);
    }

    public void OnSel(StateHandle selStateHandle)
    {
        
        OnSel(selStateHandle, true,true);
    }

    public void OnSel(StateHandle selStateHandle, bool isDuration,bool byStateHandle )
    {
        if (m_cached == false)//初始化中的话不用处理
            return;

        //类型状态机
        bool selSuccess = true;
        switch (m_ctrlType)
        {
            case CtrlType.none: m_curStateHandle = selStateHandle;break;
            case CtrlType.tab: selSuccess = OnSelTab(selStateHandle, isDuration, byStateHandle); break;
            default:Debuger.LogError(string.Format("未作处理的控件类型",m_ctrlType));break;
        }

        if (selSuccess && m_onSel != null)
        {
            m_onSel(selStateHandle, CurIdx);

            if (ms_globalStateHook != null)
                ms_globalStateHook(this, CurIdx);
        }
    }

    public StateHandle Get(int idx)
    {
        Cache();
        if (idx >= m_stateHandles.Count || idx < 0)
            return null;
        return m_stateHandles[idx];
    }
    public T Get<T>(int idx) where T : Component
    {
        Cache();
        if (idx >= m_stateHandles.Count || idx < 0)
            return null;
        return m_stateHandles[idx].GetComponent<T>();
    }

    public T GetCur<T>() where T : Component
    {
        return CurStateHandle == null ? null:CurStateHandle.GetComponent<T>();
    }

    //提供设置大小的功能
    public void SetCount(int count)
    {
        Cache();
        if (count == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
            this.gameObject.SetActive(true);


        //多退少补
        int curCount = m_stateHandles.Count;
        StateHandle s;
        if (count < curCount)
        {
            for (int i = count; i < curCount; ++i)
            {
                s = m_stateHandles[i];
                //这里不要把m_itemTemplate对应的对象销毁了
                if (m_itemTemplate == s)
                    s.gameObject.SetActive(false);
                else
                    UnityEngine.Object.Destroy(s.gameObject);                
            }
            m_stateHandles.RemoveRange(count, curCount - count);
        }
        else if (count > curCount)
        {
            GameObject go;
            Transform t;
            for (int i = curCount; i < count; ++i)
            {
                StateHandle template = m_itemTemplate != null ? m_itemTemplate : m_stateHandles[m_stateHandles.Count - 1];
                go = GameObject.Instantiate(template.gameObject) as GameObject;
                go.gameObject.name = "item"+i;
                t = go.transform;
                t.SetParent(this.transform,false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = m_stateHandles[m_stateHandles.Count - 1].gameObject.transform.localScale;
                if(go.layer != this.gameObject.layer) go.layer = this.gameObject.layer;
                if(go.activeSelf == false)go.SetActive(true);
                s =go.GetComponent<StateHandle>();
                s.AddClickEx(OnSel);
                m_stateHandles.Add(s);
            }
        }
    }

    #endregion

    #region 不同的控件类型的特殊处理，如果以后类型很多可以拆到别的类里或者用partial
    public void OnCacheTab()
    {
        //如果是tab控件，确保有且只有一个处于选中状态
        StateHandle sNeedSet = null;//需要设置到提起状态的控件
        bool needSet = true;
        foreach (var s in m_stateHandles)
        {
            if (s == null)
                continue;
            s.EnableCtrlState = false;//让状态控件，自己不能控制状态
            if (s.m_curState == 0 && needSet == true)//找到需要设置为提起状态的控件
                sNeedSet = s;
            if (s.m_curState == 1 && needSet == false)//只有一个控件能为提起状态，其他控件要设置为0
                s.SetState(0, false);
            if (s.m_curState == 1 && needSet == true)//找到一个已经设置为提起状态的控件的话，记录下
            {
                needSet = false;
                sNeedSet = s;
            }
        }
        if (needSet && sNeedSet != null)//设置为选中状态
            sNeedSet.SetState(1, false);
        m_curStateHandle = sNeedSet;//当前选中的状态控件
    }

    public bool OnSelTab(StateHandle selStateHandle, bool isDuration, bool byStateHandle)
    {
        if (byStateHandle && m_curStateHandle == selStateHandle && selStateHandle.CurStateIdx ==0)
            return false;
        m_curStateHandle = selStateHandle;
        m_curStateHandle.SetState(1, isDuration);
        foreach (StateHandle s in m_stateHandles)
        {
            if (s == m_curStateHandle || s.m_curState != 1)
                continue;

            s.SetState(0, isDuration);
        }
        return true;
    }
    #endregion
}
