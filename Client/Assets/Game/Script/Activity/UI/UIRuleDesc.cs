using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIRuleDesc : UIPanel
{
    public Text m_txt;

    public override void OnInitPanel()
    {
    }

    public override void OnOpenPanel(object param)
    {
        m_txt.text = param==null?"":param.ToString();
    }
    public override void OnClosePanel()
    {

    }
}
