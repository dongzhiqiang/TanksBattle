using UnityEngine;
using UnityEngine.UI;
public enum UIChatDockType
{
    left,
    right,
}

public class UIChatMsgItem : MonoBehaviour
{
    public RectTransform m_contentRect;
    public TextEx m_msgContent;
    public TextEx m_heroName;
    public TextEx m_sendTime;
    public RectTransform m_headRect;
    public ImageEx m_heroHead;
    public TextEx m_roleLv;
    public ImageEx m_vipBg;
    public TextEx m_vipLv;
    public ImageEx m_contentBg;
    public string m_bgImgLeft;
    public string m_bgImgRight;

    private HorizontalLayoutGroup m_itemLayout;
    private RectTransform m_rectTransform;

    public void Init(UIChatDockType dock, bool showHead, bool showName, string content, string heroName, long sendTime, string head = null, int roleLv = 0, int vipLv = 0)
    {
        m_headRect.gameObject.SetActive(showHead);
        m_heroName.gameObject.SetActive(showName);
        m_msgContent.text = content;
        m_heroName.text = heroName;
        m_sendTime.text = StringUtil.FormatTimeSpan2(sendTime);
        m_heroHead.Set(head);
        m_roleLv.text = roleLv.ToString();
        m_vipBg.gameObject.SetActive(vipLv > 0);
        m_vipLv.text = "VIP " + vipLv;

        if (m_itemLayout == null)
        {
            m_itemLayout = GetComponent<HorizontalLayoutGroup>();
            m_rectTransform = transform as RectTransform;
        }

        switch (dock)
        {
            case UIChatDockType.left:
                {
                    m_headRect.SetAsFirstSibling();
                    m_heroName.rectTransform.SetAsFirstSibling();
                    m_heroName.alignment = TextAnchor.MiddleLeft;
                    m_sendTime.alignment = TextAnchor.MiddleRight;
                    m_rectTransform.pivot = new Vector2(0, 1);
                    m_contentBg.Set(m_bgImgLeft);
                }
                break;
            case UIChatDockType.right:
                {
                    m_headRect.SetAsLastSibling();
                    m_heroName.rectTransform.SetAsLastSibling();
                    m_heroName.alignment = TextAnchor.MiddleRight;
                    m_sendTime.alignment = TextAnchor.MiddleLeft;
                    m_rectTransform.pivot = new Vector2(1, 1);
                    m_contentBg.Set(m_bgImgRight);
                }
                break;
        }
    }
}