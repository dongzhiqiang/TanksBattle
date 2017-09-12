using UnityEngine;
using System.Collections;

public class UIMessageBox2 : UIPanel
{

    public class Cxt
    {
        public string title;
        public string content;
        public System.Action onOk;
    }

    public TextEx m_title;
    public TextEx m_content;
    public StateHandle m_btnOk;

    public static void Open(string title, string content, System.Action onOk = null)
    {
        Cxt c = new Cxt();
        c.title = title;
        c.content = content;
        c.onOk = onOk;
        Open(c);
    }

    public Cxt m_cxt;
    public static void Open(Cxt cxt)
    {
        UIMgr.instance.Open<UIMessageBox2>(cxt);
    }

    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnOk.AddClick(OnOk);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_cxt = (Cxt)param;

        m_title.text = m_cxt.title;
        m_content.text = m_cxt.content;
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    void OnOk()
    {
        Close();
        if (m_cxt.onOk != null)
            m_cxt.onOk();
    }

    [ContextMenu("Cl")]
    public void TestClose()
    {
        Close();
    }
}
