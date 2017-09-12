using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;



public class TextEx : Text
{
    public float m_maxPreferredWidth=-1;
    public float m_minPreferredWidth=-1;
    
    public override float preferredWidth { get {
            if (m_maxPreferredWidth == -1 || m_minPreferredWidth == -1)
            {
                if (m_maxPreferredWidth != -1)
                    return Mathf.Min(m_maxPreferredWidth, base.preferredWidth);
                if(m_minPreferredWidth !=-1)
                    return Mathf.Max(m_minPreferredWidth, base.preferredWidth);

                return base.preferredWidth;
            }

            if(m_minPreferredWidth  < m_maxPreferredWidth)
                return Mathf.Clamp(base.preferredWidth, m_minPreferredWidth, m_maxPreferredWidth);

            if (m_minPreferredWidth > m_maxPreferredWidth)
                return Mathf.Clamp(base.preferredWidth, m_maxPreferredWidth, m_minPreferredWidth);

            return m_minPreferredWidth;//等于的情况
        } }
}

