using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// 输入框
/// </summary>
public class UIInputBox : UIPanel
{
    public class Cxt
    {
        public string title;
        public string content;
        //输入字数限制
        public int limit;
        public bool isMultiline;
        public string ok;
        public string cancel;
        public string tip;
        public System.Func<string, bool> callback;
    }

    public TextEx m_title;
    public InputField m_input;
    public InputField m_input2;
    public StateHandle m_cancleBtn;
    public StateHandle m_okBtn;
    public StateHandle m_multiline;
    public TextEx m_okText;
    public TextEx m_cancelText;
    public TextEx m_tip;

    Cxt m_cxt;

    string lastMsg = "";

    public static void Show(string title, string defaultValue, int chaLimit, System.Func<string, bool> callback, bool isMultiline = false, string okMsg = "确定", string cancelMsg = "取消", string tip= "")
    {
        Cxt c = new Cxt();
        c.title = title;
        c.content = defaultValue;
        c.callback = callback;
        c.limit = chaLimit;
        c.isMultiline = isMultiline;
        c.ok = okMsg;
        c.cancel = cancelMsg;
        c.tip = tip;
        Open(c);
    }

    public static void Open(Cxt cxt)
    {
        UIMgr.instance.Open<UIInputBox>(cxt);
    }

    //初始化时调用
    public override void OnInitPanel()
    {
        m_okBtn.AddClick(OnOk);
        m_cancleBtn.AddClick(OnCancle);

        m_input.onValueChanged.AddListener(OnInputChange);
        m_input2.onValueChanged.AddListener(OnInputChange2);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_cxt = (Cxt)param;
        m_title.text = m_cxt.title;
        if (m_cxt.isMultiline)
        {
            m_input2.text = m_cxt.content;
            m_tip.text = m_cxt.tip;
        }
        else
            m_input.text = m_cxt.content;
        
        m_okText.text = m_cxt.ok;
        m_cancelText.text = m_cxt.cancel;
        m_multiline.SetState(m_cxt.isMultiline ? 1 : 0);
    }

    public override void OnClosePanel()
    {
    }

    void OnOk()
    {
        m_cxt.content = m_cxt.isMultiline ? m_input2.text : m_input.text;
        if(m_cxt.callback(m_cxt.content))  //回调true关闭界面
            Close();
    }

    void OnCancle()
    {
        Close();
    }

    //监听输入数值变化
    void OnInputChange(string arg)
    {
        if (StringUtil.CountStrLength(arg) > m_cxt.limit)
            m_input.text = lastMsg;
        else
        {
            m_input.text = arg;
            lastMsg = arg;
        }
    }
    void OnInputChange2(string arg)
    {
        if (StringUtil.CountStrLength(arg) > m_cxt.limit)
            m_input2.text = lastMsg;
        else
        {
            m_input2.text = arg;
            lastMsg = arg;
        }
    }


}
