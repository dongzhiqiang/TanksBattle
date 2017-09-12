using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//用来记录序列的类，由于序列化类引用自己会导致死锁，所以需要这么个外部类
//注意同个时间段的子处理合并的时候，要继承即时属性(isDurationInvalid)
public class SequenceHandle : MonoBehaviour
{
    [HideInInspector]
    public List<Handle> m_handles = new List<Handle>();

    [ContextMenu("重排列")]
    public void ResetId()
    {
        for (int i=0;i<m_handles.Count;++i)
        {
            m_handles[i].m_id = i;
        }
    }

    public int MaxCount
    {
        get
        {
            int count = 0;
            int max = 0;
            float time = -1;
            foreach (var h in m_handles)
            {
                if (Mathf.Approximately(h.m_delay, time))
                {
                    ++count;
                    if (count > max)
                        max = count;
                }
                else
                {
                    count = 1;
                    time = h.m_delay;
                }
            }

            return max;
        }
    }

    //同步下和父处理共用的属性
    public void SyncHandle(Component comp, Handle parent)
    {
        foreach (var h in m_handles)
        {
            h.m_isRealtime = parent.m_isRealtime;
            h.m_rate = parent.m_rate;
        }
        EditorUtil.SetDirty(this);
    }

    public void AddSubHandle(Component comp, Handle parent, Handle.Type type, float time)
    {
        int maxId = 0;
        foreach (var h in m_handles)
        {
            if (maxId <= h.m_id)
                maxId = h.m_id + 1;
        }

        Handle handleTo = GetByTime(time);
        bool isExpand = handleTo == null ? true : handleTo.m_seqIsExpand;
        

        Handle sub = new Handle();
        sub.m_id = maxId;
        sub.m_delay = time;
        sub.m_isRealtime = parent.m_isRealtime;
        sub.m_rate = parent.m_rate;
        sub.SetType(type);
        sub.m_seqIsExpand = isExpand;
        sub.m_seqIsShow = true;
        sub.m_isDurationInvalid = handleTo == null ? sub.m_isDurationInvalid : handleTo.m_isDurationInvalid;
        m_handles.Add(sub);
        Sort(this, parent);
    }

    public void Sort(Component comp, Handle parent)
    {
        //m_handles.Sort((Handle a, Handle b) => { return a.m_delay.CompareTo(b.m_delay); });
        //这里用默认的sort同优先级会乱序，所以使用自己写的插入排序实现
        int count = m_handles.Count;
        for (int j = 1; j < count; j++)
        {
            Handle key = m_handles[j];

            int i = j - 1;
            for (; i >= 0 && m_handles[i].m_delay.CompareTo(key.m_delay) > 0; i--)
            {
                m_handles[i + 1] = m_handles[i];
            }
            m_handles[i + 1] = key;
        }
        EditorUtil.SetDirty(this);
    }
    public void RemoveSubHandle(Component comp, Handle parent, Handle h)
    {
        m_handles.Remove(h);
        EditorUtil.SetDirty(this);
    }

    public void RemoveSubHandle(Component comp, Handle parent, float time)
    {
        List<Handle> removes = new List<Handle>();
        foreach (var h in m_handles)
        {
            if (Mathf.Approximately(h.m_delay , time))
                removes.Add(h);
        } 

        foreach(var h in removes)
            m_handles.Remove(h);
        EditorUtil.SetDirty(this);
    }

    public void OffsetTime(Component comp, Handle parent,  List<Handle> hs, float offset)
    {
        if (hs == null)
        {
            Debuger.LogError("空？offset:" + offset);
            return;
        }
        Handle handleTo = null;
        foreach (var h in hs)
        {
            if (handleTo == null || Mathf.Approximately(h.m_delay + offset, handleTo.m_delay))
                handleTo = GetByTime(offset);

            h.m_delay = h.m_delay + offset;
            h.m_seqIsExpand = handleTo == null ? h.m_seqIsExpand : handleTo.m_seqIsExpand;
            h.m_isDurationInvalid = handleTo == null ? h.m_isDurationInvalid : handleTo.m_isDurationInvalid;
        }

        Sort(comp, parent);
    }

    public void ChangeTime(Component comp, Handle parent, Handle h, float time)
    {

        if (h == null)
        {
            Debuger.LogError("空？" + time);
            return;
        }
        Handle handleTo = GetByTime(time);
        bool isExpand = handleTo == null ? h.m_seqIsExpand : handleTo.m_seqIsExpand;

        h.m_delay = time;
        h.m_seqIsExpand = isExpand;
        h.m_isDurationInvalid = handleTo == null ? h.m_isDurationInvalid : handleTo.m_isDurationInvalid;
        Sort(comp, parent);
    }

    public void ChangeTime(Component comp, Handle parent, float from, float to)
    {
        Handle handleTo = GetByTime(to);
        List<Handle> l = GetAllByTime(from);
        if (l.Count == 0)
        {
            Debuger.LogError("空？ 时间:" + from);
            return;
        }

        bool isExpand = handleTo == null ? l[0].m_seqIsExpand : handleTo.m_seqIsExpand;


        foreach (var h in l)
        {
            h.m_delay = to;
            h.m_seqIsExpand = isExpand;
            h.m_isDurationInvalid = handleTo == null ? h.m_isDurationInvalid : handleTo.m_isDurationInvalid;
        }

        Sort(comp,parent);
    }

    public Handle GetByTime( float time)
    {
        foreach (var h in m_handles)
        {
            if (Mathf.Approximately(h.m_delay, time))
            {
                return h;
            }
        }
        return null;
    }

    public List<Handle> GetAllByTime( float time)
    {
        List<Handle> l = new List<Handle>();
        foreach (var h in m_handles)
        {
            if (Mathf.Approximately(h.m_delay, time))
                l.Add(h);
        }
        return l;
    }

    public Handle GetClosest(float time, int idx = 0, float max = 0.05f)
    {
        float dis = max;
        float tem;
        int count = 0;
        Handle find = null;
        foreach (var h in m_handles)
        {
            tem = Mathf.Abs(h.m_delay - time);
            if (Mathf.Approximately(tem, dis))
                ++count;
            else if (tem < dis)
            {
                dis = Mathf.Abs(tem);
                count = 1;
            }

            if (count == idx + 1)
            {
                find = h;
                break;
            }

        }
        return find;
    }

    public List<Handle> GetRange(float left, float right)
    {
        if (left > right)
        {
            float tem = left;
            left = right;
            right = tem;
        }
        List<Handle> l = new List<Handle>();
        foreach (var h in m_handles)
        {
            if (h.m_delay >= left && h.m_delay <= right)
                l.Add(h);
        }
        return l;
    }

    public void ToggleExpand( float time)
    {
        foreach (var h in GetAllByTime(time))
        {
            h.m_seqIsExpand = !h.m_seqIsExpand;
        }
    }

    public void SetDurationInvalid(float time,bool invalid){
        foreach (var h in GetAllByTime(time))
        {
            h.m_isDurationInvalid = invalid;
        }
        EditorUtil.SetDirty(this);
    }

    public Handle GetSubHandle(int id)
    {
        foreach (var h in m_handles)
        {
            if (h.m_id == id)
                return h;
        }
        return null;
    }
}
