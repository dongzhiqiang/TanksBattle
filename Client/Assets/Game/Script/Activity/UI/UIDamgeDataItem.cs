using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIDamgeDataItem : MonoBehaviour
{
    public ImageEx m_headImg;
    public ImageEx m_progress;
    public TextEx m_roleName;
    public TextEx m_value;

    public void Init(string roleId, string roleName, int value, int maxValue)
    {
        m_headImg.Set(RoleCfg.GetHeadIcon(roleId));
        m_progress.fillAmount = Mathf.Clamp01((float)value / (float)maxValue);
        m_roleName.text = roleName;
        m_value.text = value.ToString();
    }
}
