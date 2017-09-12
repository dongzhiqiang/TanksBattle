using UnityEngine;
using System.Collections;

public class UIGetPetExpIcon : MonoBehaviour
{
    public ImageEx m_headIcon;
    public TextEx m_expVal;

    public void Init(string roleId, int exp)
    {
        m_headIcon.Set(RoleCfg.Get(roleId).icon);
        m_expVal.text = exp.ToString();
    }
}
