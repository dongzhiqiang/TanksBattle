using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelExpItem : MonoBehaviour
{
    public ImageEx m_headIcon;
    public TextEx m_exp;

    public void Init(string icon, int exp)
    {
        m_headIcon.Set(icon);
        m_exp.text = "+" + exp;
    }
}
