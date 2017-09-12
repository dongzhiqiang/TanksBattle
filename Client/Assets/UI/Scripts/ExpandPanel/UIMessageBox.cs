using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMessageBox : UIPanel
{
    public enum enState
    {
        none,//没有按钮
        ok,//一个
        okCancel,//两个
    }
    public class Cxt
    {
        public string okText;
        public string cancelText;
        public string content;
        public string title;
        public System.Action onOk;
        public System.Action onCancel;
    }
    public StateHandle m_btnOk;
    public StateHandle m_btnCancel;
    public StateHandle m_btnCancel2;
    public Text m_okText;
    public Text m_cancelText;
    public Text m_content;
    public Text m_title;
    public GameObject m_titleGo;
    public StateHandle m_state;

    
    public static void Open(string content, System.Action onOk = null, System.Action onCancel= null,string okText=null,string cancelText= null, string title = "")
    {
        Cxt c = new Cxt();
        c.okText = okText;
        c.cancelText = cancelText;
        c.title = title;
        c.content = content;
        c.onOk = onOk;
        c.onCancel = onCancel;
        Open(c);
    }

    public Cxt m_cxt;
    public static void Open(Cxt cxt) {
        UIMgr.instance.Open<UIMessageBox>(cxt);
    }
    
    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnOk.AddClick(OnOk);
        m_btnCancel.AddClick(OnCancel);
        m_btnCancel2.AddClick(OnCancel);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_cxt = (Cxt)param;

        m_content.text = m_cxt.content;
        m_okText.text = string.IsNullOrEmpty(m_cxt.okText) ? "确定" : m_cxt.okText;
        m_cancelText.text = string.IsNullOrEmpty(m_cxt.cancelText) ? "取消" : m_cxt.cancelText;

        if (string.IsNullOrEmpty(m_cxt.title))
            m_titleGo.gameObject.SetActive(false);
        else
        {
            m_titleGo.gameObject.SetActive(true);
            m_title.text = m_cxt.title;
        }

        if(m_cxt.onOk == null)
            m_state.SetState((int)enState.none);
        else if(m_cxt.onCancel == null)
            m_state.SetState((int)enState.ok);
        else
            m_state.SetState((int)enState.okCancel);

    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    void OnOk(){
        Close();
        if(m_cxt.onOk !=null)
            m_cxt.onOk();
    }

    void OnCancel()
    {
        Close();
        if (m_cxt.onCancel != null)
            m_cxt.onCancel();
    }

    [ContextMenu("Cl")]
    public void TestClose()
    {
        Close();
    }
}
