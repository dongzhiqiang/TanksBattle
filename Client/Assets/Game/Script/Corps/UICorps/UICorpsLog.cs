using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICorpsLog : UIPanel
{
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;
    //显示区域
    public RectTransform m_gridRect;

    public override void OnInitPanel()
    {
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }
    
    public override void OnOpenPanel(object param)
    {
        RoleMgr.instance.Hero.CorpsPart.SortLogs();   //打开界面时先排序一下
        UpdateLogs();
    }
    //更新日志界面数据
    public void UpdateLogs()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        int count = part.logsList.Count;
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(m_gridRect.sizeDelta.y > 410 ? true : false);
        for (int i = 0; i < count; i++)
        {
            CorpsLogItem sel = m_GridGroup.Get<CorpsLogItem>(i);
            sel.SetLogData((CorpsLogTimeInfo)part.logsList[i]);

        }
    }
    public override void OnClosePanel()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    void OnScrollChanged(Vector2 v)
    {
        m_moreArrow.gameObject.SetActive(m_gridRect.sizeDelta.y > 410 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }
}
