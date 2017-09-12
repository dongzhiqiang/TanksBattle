using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//公会申请界面
public class UICorpsReqPage : MonoBehaviour {
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;

    ////入会设置按钮
    //public StateHandle m_joinSetBtn;

    public void OnInit()
    {
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }
    public void ResetView()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }
    void OnScrollChanged(Vector2 v)
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        m_moreArrow.gameObject.SetActive(part.corpsInfo.reqs.Count >= 4 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }

    public void OnUpdateData()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        int count = part.corpsInfo.reqs.Count;
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(count >= 4 ? true : false);
        for (int i = 0; i < count; i++)
        {
            CorpsReqItem sel = m_GridGroup.Get<CorpsReqItem>(i);
            sel.OnSetData(part.corpsInfo.reqs[i]);

        }
    }
}
