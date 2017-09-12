using UnityEngine;
using System.Collections;

public class UIMailSelectItem : MonoBehaviour
{
    //提示
    public ImageEx m_tip;
    //邮件标题
    public TextEx m_title;
    ////发送者
    //public TextEx m_sender;
    //发送日期
    public TextEx m_SendTime;
    //附件的标记
    public ImageEx m_attachIcon;
    //选择框
    public ImageEx m_selectImg;

    [HideInInspector]
    public string mailId;

    public void SetData(Mail data)
    {
        mailId = data.mailId;
        m_title.text = data.title;
        //m_sender.text = data.sender;
        m_SendTime.text = StringUtil.FormatDateTime(data.sendTime, "MM月dd日");

        if (data.status == (int)MailStatus.Readed)
            m_tip.gameObject.SetActive(false);
        else
            m_tip.gameObject.SetActive(true);

        if (data.attach.Count > 0) //有附件
            m_attachIcon.gameObject.SetActive(true);
        else
            m_attachIcon.gameObject.SetActive(false);
    }
}
