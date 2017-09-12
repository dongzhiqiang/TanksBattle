using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICreateCorps : UIPanel
{
    public TextEx m_costTxt;
    public InputField m_input;
    public StateHandle m_cancleBtn;
    public StateHandle m_okBtn;

    string lastMsg = "";
    const int MAX_CHARA = 10;

    //初始化时调用
    public override void OnInitPanel()
    {
        m_okBtn.AddClick(OnOk);
        m_cancleBtn.AddClick(OnCancle);

        m_input.onValueChanged.AddListener(OnInputChange);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        int cost = CorpsBaseCfg.Get().createCost;
        if (RoleMgr.instance.Hero.GetInt(enProp.diamond) >= cost)
            m_costTxt.text = "<color=white>"+ cost +"</color>";
        else
            m_costTxt.text = "<color=red>" + cost + "</color>";

    }

    public override void OnClosePanel()
    {
        m_input.text = "";
    }

    void OnOk()
    {
        //检测一下
        if(RoleMgr.instance.Hero.GetInt(enProp.corpsId) > 0)
        {
            UIMessage.Show("请先退出当前公会");
            return;
        }

        if (string.IsNullOrEmpty(m_input.text))
        {
            UIMessage.Show("公会名字不能为空");
            return;
        }

        string badWords;
        if (BadWordsCfg.HasBadNickNameWords(m_input.text, out badWords))
        {
            UIMessage.Show(string.Format("名字里的“{0}”不允许使用", badWords));
            return;
        }

        NetMgr.instance.CorpsHandler.CreateCorps(m_input.text);
     
    }

    void OnCancle()
    {
        Close();
    }

    //监听输入数值变化
    void OnInputChange(string arg)
    {
        if (StringUtil.CountStrLength(arg) > MAX_CHARA)
            m_input.text = lastMsg;
        else
        {
            m_input.text = arg;
            lastMsg = arg;
        }
    }

}
