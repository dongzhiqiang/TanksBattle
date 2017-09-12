using UnityEngine;

public class UITextPairItem : MonoBehaviour
{
    public TextEx m_title;
    public TextEx m_value;

    public void Init(string title, string value)
    {
        m_title.text = title;
        m_value.text = value;
    }
}
