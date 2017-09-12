using UnityEngine;

public class UIScrollWrapItem : SequenceHandle
{
    [HideInInspector]
    public Handle m_handle = new Handle();

    private RectTransform m_rectTransform = null;
    public RectTransform RectTransform
    {
        get { return m_rectTransform == null ? m_rectTransform = transform as RectTransform : m_rectTransform; }
    }

    public void InitData(object data)
    {
        OnInitData(data);
    }

    public void SetFactor(float factor)
    {
        if (m_handle != null)
            m_handle.SetFactor(Mathf.Clamp01(factor));

        OnSetFactor(factor);
    }

    #region 用于被继承
    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="data"></param>
    public virtual void OnInitData(object data)
    {
    }

    /// <summary>
    /// 设置偏移比
    /// </summary>
    /// <param name="factor"></param>
    public virtual void OnSetFactor(float factor)
    {
    }
    #endregion
}