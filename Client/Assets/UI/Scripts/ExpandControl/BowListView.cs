using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class BowListView : MonoBehaviour,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
{
    /** 显示数量 */
    public int m_maxShowItem = 5;
    /** 圆弧的高度和宽度(用于计算半径) */
    public float m_arcHeight;
    public float m_arcWidth;
    /** 最远处节点相对于中间节点的垂直距离 */
    public float m_maxVertDist;
    /** 离开屏幕节点相对于中心节点的垂直距离 */
    public float m_outVertDist;
    /** 最远节点的缩放比例 */
    public float m_minScale;
    /** 节点池 */
    //public List<BowListItem> m_pool;
    /** 节点模板 */
    public BowListItem m_templeItem;
    /** 关联的锁链 */
    public List<BowChain> m_chains;
    /** 关联的齿轮*/
    public List<BowWheel> m_wheels;
    /** 收拉状态 */
    public StateHandle m_handle;
    /** 飞出屏幕状态的偏移弧度 */
    public float m_leaveRadian = 2;
    /** 出现时的动画曲线 */
    public AnimationCurve m_comeAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    /** 离开时的动画曲线 */
    public AnimationCurve m_leaveAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    /** 最远节点的遮罩alpha*/
    public float m_maxAlpha;
    /** 圆周半径*/
    public float m_radius;
    /** 是否制定圆周半径*/
    public bool m_useRadius = false;
    /** 是否循环节点*/
    public bool m_isLoop = true;
    /** 角度偏移(顺时针) */
    public float m_shiftDegree = 0;
    /**自定义节点弧度*/
    public float m_customRadianPerNode = 0;
    /** 最远处节点相对于中间节点的弧度 */
    private float m_maxRadian; 
    /** 离开屏幕节点相对于中间节点的弧度 */
    private float m_outRadian;
    private int m_oldState = 0;
    /** 当前周期弧度 */
    private float m_circleRaidan;
    private float m_difRadian = 0;

    private AnimationCurve m_currentAnimationCurve;
    
    private List<object> m_data = new List<object>();
    private class VirtualListNode
    {
        public bool m_show;
        public int m_usedCount;
        public BowListItem m_item;
        public List<BowListItem> m_addList = new List<BowListItem>();
    }
    private List<VirtualListNode> m_virtualNodes = new List<VirtualListNode>();
    private List<BowListItem> inactiveNodes = new List<BowListItem>();
    private float m_radian = 0;
    public int m_selectedIndex = 0;
    private float m_radianPerNode;
    private float m_startDragRadian;
    private float m_startDragMouseRadian;
    private float m_radianShiftDuration = 10;
    private float m_radianShiftStart;
    private float m_radianShiftEnd;
    private float m_radianShiftStartTime = 0;
    private bool m_radianShiftStop = true;
    private float m_difRadianShiftDuration = 10;
    private float m_difRadianShiftStart;
    private float m_difRadianShiftEnd;
    private float m_difRadianShiftStartTime = 0;
    private bool m_difRadianShiftStop = true;
    private List<Action> m_changeCallbacks = new List<Action>();
    private int m_border = 0;

    float Now
    {
        get
        {
            if (Application.isPlaying)
            {
                return Time.unscaledTime;
            }
            else
            {
                var span = System.DateTime.Now.TimeOfDay;
                return (float)span.TotalSeconds;
            }
        }
    }

    public T GetCurSel<T>()
    {
        if(m_data.Count<=m_selectedIndex)
        {
            return default(T);
        }
        if( m_data[m_selectedIndex] is T)
        {
            return (T)m_data[m_selectedIndex];
        }
        return default(T);
    }

    public void AddChangeCallback(Action callback)
    {
        m_changeCallbacks.Add(callback);
    }

    void RadianShiftStart()
    {
        m_radianShiftStop = false;
        m_radianShiftStartTime = Now;
    }
    
    bool IsRadianShiftPlaying
    {
        get
        {
            return m_radianShiftStartTime > 0;
        }
    }

    void StopRadianShift()
    {
        m_radianShiftStartTime = 0;
    }

    void DifRadianShiftStart()
    {
        m_difRadianShiftStop = false;
        m_difRadianShiftStartTime = Now;
    }

    bool IsDifRadianShiftPlaying
    {
        get
        {
            return m_difRadianShiftStartTime > 0;
        }
    }

    void StopDifRadianShift()
    {
        m_difRadianShiftStartTime = 0;
    }

    void Awake()
    {

        /*
        if (m_templeItem==null)
        {
            m_templeItem = m_pool[0];
        }
        else if(m_pool.IndexOf(m_templeItem)<0)
        {
         */
        m_templeItem.gameObject.SetActive(false);
        inactiveNodes.Add(m_templeItem);
        
        m_oldState = GetHandleState();
        PreInitPool(10);
        for (int i = 0; i < 10; i++)
        { m_data.Add(0); } // for preview
        if (m_selectedIndex >= m_data.Count) m_selectedIndex = m_data.Count - 1;
        if (m_selectedIndex < 0) m_selectedIndex = 0;
        if (m_maxShowItem < 3)
        {
            m_maxShowItem = 3;
        }
        // 计算半径和弧度数据
        float heightHalf = m_arcHeight/2;
        float radianHalf = Mathf.Atan(m_arcWidth / heightHalf);
        if(!m_useRadius)
        { 
            m_radius = heightHalf / Mathf.Sin(radianHalf);
        }
        m_maxRadian = Mathf.Asin(m_maxVertDist / m_radius);
        m_outRadian = Mathf.Asin(m_outVertDist / m_radius);

        int spaceCount = m_maxShowItem / 2;
        m_radianPerNode = m_maxRadian / spaceCount;
        if (m_customRadianPerNode>0.0001)
        {
            m_radianPerNode = m_customRadianPerNode;
        }
        //Debug.Log(m_radianPerNode);
        m_radian = m_selectedIndex * m_radianPerNode;
        /*
        foreach(BowListItem listItem in m_pool)
        {
            listItem.gameObject.SetActive(false);
            inactiveNodes.Add(listItem);
        }*/

        foreach(BowChain chain in m_chains)
        {
            chain.InitChain(m_radius);
        }

        if (GetHandleState() == 0)
        {
            m_difRadian = GetChainEndRadian() - m_radian + m_leaveRadian;//(m_data.Count - 1) * m_radianPerNode + m_leaveRadian;
        }
        
        UpdateNodes(true);
    }

    void RecollectNode( BowListItem node )
    {
        node.gameObject.SetActive(false);
        inactiveNodes.Add(node);
    }

    BowListItem CreateNode()
    {
        BowListItem result;
        if (inactiveNodes.Count == 0)
        {
            result = GameObject.Instantiate(m_templeItem);
            result.transform.parent = this.transform;
        }
        else
        {
            result = inactiveNodes[inactiveNodes.Count - 1];
            inactiveNodes.RemoveAt(inactiveNodes.Count - 1);
        }
        result.gameObject.SetActive(true);
        return result;
    }

    public void PreInitPool(int count)
    {
        List<BowListItem> pool = new List<BowListItem>();
        for(int i=0; i<count; i++)
        {
            pool.Add(CreateNode());
        }
        for (int i = 0; i < count; i++)
        {
            RecollectNode(pool[i]);
        }
    }

    void UpdatePosition( float nodeRadian, BowListItem node)
    {
        float centerX = -m_radius * Mathf.Cos(m_shiftDegree * Mathf.Deg2Rad);
        float centerY = m_radius * Mathf.Sin(m_shiftDegree * Mathf.Deg2Rad);
        float posRadian = nodeRadian + m_shiftDegree * Mathf.Deg2Rad;
        node.gameObject.transform.localPosition = new Vector3(centerX + m_radius * Mathf.Cos(posRadian), centerY - m_radius * Mathf.Sin(posRadian));
        float scale = 1 - Mathf.Abs(nodeRadian) / m_maxRadian * (1 - m_minScale);
        if(m_minScale!=1)node.gameObject.transform.localScale = new Vector3(scale, scale);
        
        if (node.m_mask != null)
        {
            float alpha = Mathf.Abs(nodeRadian) / m_maxRadian * m_maxAlpha;
            Color color = new Color(1, 1, 1, alpha);
            node.m_mask.color = color;
        }
    }

    float GetFormalRadian(float curRadian)
    {
        if(m_circleRaidan == 0)
        {
            return 0;
        }
        return curRadian - Mathf.Floor(curRadian / m_circleRaidan) * m_circleRaidan;
    }

    void MarkShowNodes(float radian,float absRadian,float maxAbsRadian,bool leaveScreen)
    {
        for (int i = 0; i < m_data.Count; i++)
        {
            float nodeRadian = i * m_radianPerNode - radian;
            float nodeRealRadian = nodeRadian + absRadian;
            if (nodeRealRadian > maxAbsRadian && leaveScreen)
            {
                continue;
            }
            VirtualListNode virtualNode = m_virtualNodes[i];
            if (m_isLoop)
            {
                if (Mathf.Abs(nodeRadian) <= m_outRadian)
                {
                    virtualNode.m_show = true;
                }
            }
            else
            {
                float r = i*m_radianPerNode-absRadian;
                if (Mathf.Abs(r) < m_outRadian)
                {
                    virtualNode.m_show = true;
                }
            }
        }
    }

    void ShowNodes(float radian,float absRadian,float maxAbsRadian,bool updateData,bool leaveScreen)
    {
        for (int i = 0; i < m_data.Count; i++)
        {
            float nodeRadian = i * m_radianPerNode - radian;
            float nodeRealRadian = nodeRadian + absRadian;
            if (!m_isLoop)
            {
                nodeRadian = i * m_radianPerNode - absRadian;
                nodeRealRadian = nodeRadian;
                float r = i * m_radianPerNode - absRadian;
                if (r > m_leaveRadian || r < -m_leaveRadian)
                {
                    continue;
                }
            }

            if (nodeRealRadian > maxAbsRadian && leaveScreen)
            {
                continue;
            }
            object data = m_data[i];
            VirtualListNode virtualNode = m_virtualNodes[i];
            if (Mathf.Abs(nodeRadian) > m_outRadian)
            {
                continue;
            }
            BowListItem listItem;
            if (virtualNode.m_usedCount == 0)
            {
                if (virtualNode.m_item == null)
                {
                    virtualNode.m_item = CreateNode();
                    if (virtualNode.m_item == null)
                    {
                        Debug.LogError("list pool not big enough");
                        continue;
                    }
                    virtualNode.m_item.gameObject.name = "item" + i;
                    virtualNode.m_item.SetData(data);
                }
                else
                {
                    if (updateData)
                    {
                        virtualNode.m_item.gameObject.name = "item" + i;
                        virtualNode.m_item.SetData(data);
                    }
                }
                listItem = virtualNode.m_item;
            }
            else
            {
                listItem = CreateNode();
                if (listItem == null)
                {
                    Debug.LogError("list pool not big enough");
                    continue;
                }
                listItem.gameObject.name = "item" + i;
                listItem.SetData(data);
                virtualNode.m_addList.Add(listItem);
            }
            virtualNode.m_usedCount++;
            if (listItem != null)
            {
                UpdatePosition(nodeRadian, listItem);
                if (i == m_selectedIndex)
                {
                    if (IsDifRadianShiftPlaying || IsRadianShiftPlaying)
                    {
                        listItem.SetSelected(false);
                    }
                    else
                    {
                        listItem.SetSelected(true);
                    }
                }
                else
                {
                    listItem.SetSelected(false);
                }
            }
        }
    }

    void UpdateRadian(float curRadian, bool updateData, bool leaveScreen)
    {
        if (!m_isLoop)
        {

            if (curRadian > (m_data.Count-1)*m_radianPerNode)
            {
                curRadian = (m_data.Count - 1) * m_radianPerNode;
            }
            if (curRadian < 0)
            {
                curRadian = 0;
            }
        }
        float radian = GetFormalRadian(curRadian);
        if (!m_isLoop)
        {
            radian = curRadian;
        }
        
        float endRadian = GetChainEndRadian();
        float maxNodeRadian = endRadian;

        // 先判断超出屏幕的节点
        for (int i = 0; i < m_data.Count; i++)
        {
            VirtualListNode virtualNode = m_virtualNodes[i];
            virtualNode.m_show = false;
            virtualNode.m_usedCount = 0;
            foreach( BowListItem item in virtualNode.m_addList)
            {
                RecollectNode(item);
            }
            virtualNode.m_addList.Clear();
        }
        if (m_isLoop && m_data.Count == 1) MarkShowNodes(radian + 2 * m_circleRaidan, curRadian, maxNodeRadian, leaveScreen);
        // 上面的
        MarkShowNodes(radian + m_circleRaidan, curRadian , maxNodeRadian, leaveScreen);
        if(m_isLoop)
        {
            // 中间的
            MarkShowNodes(radian, curRadian, maxNodeRadian, leaveScreen);
            // 下面的
            MarkShowNodes(radian - m_circleRaidan, curRadian, maxNodeRadian, leaveScreen);
            if (m_data.Count == 1) MarkShowNodes(radian - 2 * m_circleRaidan, curRadian, maxNodeRadian, leaveScreen);
        }


        // 先回收超出屏幕的节点，再设置出现的节点
        for (int i = 0; i < m_data.Count; i++)
        {
            VirtualListNode virtualNode = m_virtualNodes[i];
            if (!virtualNode.m_show)
            {
                if (virtualNode.m_item != null)
                {
                    RecollectNode(virtualNode.m_item);
                    virtualNode.m_item = null;
                }
            }
        }

        if (m_isLoop && m_data.Count == 1) ShowNodes(radian + 2 * m_circleRaidan, curRadian, maxNodeRadian, updateData, leaveScreen);
        ShowNodes(radian + m_circleRaidan, curRadian , maxNodeRadian, updateData, leaveScreen);
        if (m_isLoop)
        {
            ShowNodes(radian, curRadian, maxNodeRadian, updateData, leaveScreen);
            ShowNodes(radian - m_circleRaidan, curRadian, maxNodeRadian, updateData, leaveScreen);
            if (m_data.Count == 1) ShowNodes(radian - 2 * m_circleRaidan, curRadian, maxNodeRadian, updateData, leaveScreen);
        }

        foreach (BowChain chain in m_chains)
        {
            chain.UpdateRadian(curRadian, maxNodeRadian, leaveScreen);
        }
        foreach(BowWheel wheel in m_wheels)
        {
            wheel.SetRouteDistance(curRadian * m_radius);
        }
    }

    void UpdateNodes( Boolean updateData )
    {
        //Debug.LogError("rad---:" + m_radian);
        while (m_virtualNodes.Count>m_data.Count)
        {
            if (m_virtualNodes[m_virtualNodes.Count - 1].m_item!=null)
            {
                RecollectNode(m_virtualNodes[m_virtualNodes.Count - 1].m_item);
            }
            m_virtualNodes.RemoveAt(m_virtualNodes.Count - 1);
        }
        while( m_virtualNodes.Count<m_data.Count)
        {
            m_virtualNodes.Add(new VirtualListNode());
        }
        m_circleRaidan = m_data.Count * m_radianPerNode;
        UpdateRadian(m_radian + m_difRadian, updateData, IsDifRadianShiftPlaying || GetHandleState() == 0);
    }

    int GetHandleState()
    {
        if(m_handle==null)
        {
            return 1;
        }

        return m_handle.CurStateIdx;
    }

    public void SetData(List<object> data)
    {
        m_data = data;
        if (m_selectedIndex >= m_data.Count) m_selectedIndex = m_data.Count - 1;
        if (m_selectedIndex < 0) m_selectedIndex = 0;

        if(GetHandleState()==0)
        {
            m_difRadian = GetChainEndRadian() - m_radian + m_leaveRadian;//(m_data.Count - 1) * m_radianPerNode + m_leaveRadian;
        }
         
        UpdateNodes(true);
    }

    public void SelectItem(object item)
    {
        int newIdx = m_data.IndexOf(item);
        if(newIdx >=0)
        {
            m_selectedIndex = newIdx;
            m_radian = m_selectedIndex * m_radianPerNode;
            if (GetHandleState() == 0)
            {
                m_difRadian = GetChainEndRadian() - m_radian + m_leaveRadian;//(m_data.Count - 1) * m_radianPerNode + m_leaveRadian;
            }

            UpdateNodes(true);
        }
    }

    float GetChainEndRadian()
    {
        foreach(BowChain chain in m_chains)
        {
            if(chain.m_outRadiusUsed)
            {
                return chain.GetEndRadian(m_radian);
            }
        }
        return 0;
    }


    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {

    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (IsDifRadianShiftPlaying || GetHandleState() == 0)
        {
            return;
        }
        if (IsRadianShiftPlaying)
        {
            StopRadianShift();
        }
        //Vector3 mousePosWorld = eventData.pressEventCamera.ScreenToWorldPoint(new Vector3(eventData.position.x,eventData.position.y)); 另一种方式
        //Vector3 mouseLocalPosition = transform.InverseTransformVector(mousePosWorld);
        Vector3 centerPosWorld = transform.TransformPoint(- m_radius, 0, 0);
        Vector3 centerPosScreen = eventData.pressEventCamera.WorldToScreenPoint(centerPosWorld);
        m_startDragMouseRadian = Mathf.Atan((eventData.position.y - centerPosScreen.y) / (eventData.position.x - centerPosScreen.x));
        m_startDragRadian = m_radian;
#if !ART_DEBUG
        SoundMgr.instance.Play2DSound(Sound2DType.ui, 109);
#endif
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (IsDifRadianShiftPlaying || GetHandleState() == 0)
        {
            return;
        }
        Vector3 centerPosWorld = transform.TransformPoint(- m_radius, 0, 0);
        Vector3 centerPosScreen = eventData.pressEventCamera.WorldToScreenPoint(centerPosWorld);
        m_radian = m_startDragRadian + Mathf.Atan((eventData.position.y - centerPosScreen.y) / (eventData.position.x - centerPosScreen.x)) - m_startDragMouseRadian;
        if(!m_isLoop)
        {
            if (m_radian > (m_data.Count - 1 - m_border) * m_radianPerNode)
            {
                m_radian = (m_data.Count - 1 - m_border) * m_radianPerNode;
            }
            if (m_radian < m_border * m_radianPerNode)
            {
                m_radian = m_border * m_radianPerNode;
            }
        }

        UpdateNodes(false);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (IsDifRadianShiftPlaying || GetHandleState() == 0)
        {
            return;
        }
        float formalRadian = GetFormalRadian(m_radian);
        
        m_selectedIndex = Mathf.RoundToInt(formalRadian / m_radianPerNode);
        if (m_selectedIndex>=m_data.Count)
        {
            m_selectedIndex -= m_data.Count;
            formalRadian -= m_circleRaidan;
        }
        //Debug.Log(formalRadian + " " + m_selectedIndex);
        m_radianShiftStart = m_radian;
        m_radianShiftEnd = m_selectedIndex * m_radianPerNode - formalRadian + m_radian;
        m_radianShiftDuration = 0.1f;
        RadianShiftStart();
        foreach(Action callback in m_changeCallbacks)
        {
            callback.Invoke();
        }
    }

    public void IncSelection()
    {
        if (IsDifRadianShiftPlaying || GetHandleState() == 0)
        {
            return;
        }
        if (IsRadianShiftPlaying)
        {
            return;
        }
        m_selectedIndex = m_selectedIndex + 1;
        if(m_selectedIndex >= m_data.Count)
        {
            if (m_isLoop)
            {
                m_selectedIndex = 0;
            }
            else
            {
                m_selectedIndex = m_data.Count-1;
            }
        }
        m_radianShiftStart = m_radian;
        m_radianShiftEnd = Mathf.Round(m_radian / m_radianPerNode) * m_radianPerNode + m_radianPerNode;
        m_radianShiftDuration = 0.2f;
        RadianShiftStart();
        foreach (Action callback in m_changeCallbacks)
        {
            callback.Invoke();
        }
    }

    public void DecSelection()
    {
        if (IsDifRadianShiftPlaying || GetHandleState() == 0)
        {
            return;
        }
        if (IsRadianShiftPlaying)
        {
            return;
        }
        m_selectedIndex = m_selectedIndex - 1;
        if (m_selectedIndex < 0)
        {
            if (m_isLoop)
            {
                m_selectedIndex = m_data.Count - 1;
            }
            else
            {
                m_selectedIndex = 0;
            }     
        }
        m_radianShiftStart = m_radian;
        m_radianShiftEnd = Mathf.Round(m_radian / m_radianPerNode) * m_radianPerNode - m_radianPerNode;
        if (m_radianShiftEnd > m_radianShiftStart)
        {
            m_radianShiftEnd = m_radianShiftEnd - m_circleRaidan;
        }
        if (m_radianShiftEnd + m_circleRaidan < m_radianShiftStart)
        {
            m_radianShiftEnd = m_radianShiftEnd + m_circleRaidan;
        }
        m_radianShiftDuration = 0.2f;
        RadianShiftStart();
        foreach (Action callback in m_changeCallbacks)
        {
            callback.Invoke();
        }
    }

    public void SetSelectedIndex(int selectedIndex)
    {
        float formalRadian = GetFormalRadian(m_radian);
        m_selectedIndex = selectedIndex;
        float radian = m_selectedIndex * m_radianPerNode;
        m_radian = m_selectedIndex * m_radianPerNode - formalRadian + m_radian;
        UpdateNodes(false);
    }

    void Update()
    {
        if (GetHandleState() == 0 && m_oldState == 1)
        {
            m_difRadianShiftStart = m_difRadian;
            m_difRadianShiftEnd = GetChainEndRadian()-m_radian + m_leaveRadian;
            m_difRadianShiftDuration = m_handle.Duration;
            m_oldState = 0;
            m_currentAnimationCurve = m_leaveAnimationCurve;
            DifRadianShiftStart();
        }
        if (GetHandleState() == 1 && m_oldState == 0)
        {
            m_difRadianShiftStart = m_difRadian;
            m_difRadianShiftEnd = 0;
            m_difRadianShiftDuration = m_handle.Duration;
            m_oldState = 1;
            m_currentAnimationCurve = m_comeAnimationCurve;
            DifRadianShiftStart();
        }
        if(IsRadianShiftPlaying)
        {
            float factor = (Now-m_radianShiftStartTime)/m_radianShiftDuration;
            //Debug.LogError("f:"+factor);
            if(factor<1)
            {
                m_radian = Mathf.Lerp(m_radianShiftStart, m_radianShiftEnd, factor);
            }
            else
            {
                m_radian = m_radianShiftEnd;
                m_radianShiftStartTime = 0;
            }
            
            UpdateNodes(false);
        }
        else if (!m_radianShiftStop)
        {
            m_radianShiftStop = true;
            UpdateNodes(false);
        }
        if (IsDifRadianShiftPlaying)
        {
            float factor = (Now - m_difRadianShiftStartTime) / m_difRadianShiftDuration;
            if (m_currentAnimationCurve != null)
            {
                factor = m_currentAnimationCurve.Evaluate(factor);
            }
            //Debug.LogError("f:"+factor);
            if (factor < 1)
            {
                m_difRadian = Mathf.Lerp(m_difRadianShiftStart, m_difRadianShiftEnd, factor);
            }
            else
            {
                m_difRadian = m_difRadianShiftEnd;
                m_difRadianShiftStartTime = 0;
            }

            UpdateNodes(false);
        }
        else if (!m_difRadianShiftStop)
        {
            m_difRadianShiftStop = true;
            UpdateNodes(false);
        }
    }

    public void Reflesh()
    {
        foreach(BowListItem item in GetComponentsInChildren<BowListItem>())
        {
            if(item.gameObject.activeSelf)
            {
                item.Reflesh();
            }

        }
    }

    public void SetBorder(int border)
    {
        m_border = border;
    }
}