using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UICorpsList : UIPanel
{
    //创建公会按钮
    public StateHandle m_createBtn;
    //搜索按钮
    public StateHandle m_search;
    //输入公会名字
    public InputField m_inputName;
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;
    //左切页
    public StateHandle m_leftHandle;
    //右切页
    public StateHandle m_rightHandle;
    //切换栏
    public StateHandle m_iconBarState;

    public StateHandle m_gridHandle;
    //当前页数
    public TextEx m_page; 

    //记录页签索引
    int m_index;
    int m_totalPage;
    const int MAX_SHOW = 20;
    List<CorpsProps> corps = new List<CorpsProps>();

    public override void OnInitPanel()
    {
        //创建公会
        m_createBtn.AddClick(() =>
        {
            UIMgr.instance.Open<UICreateCorps>();
            
        });
        //搜索公会
        m_search.AddClick(() =>
        {
            List<CorpsProps> pData = new List<CorpsProps>();
            int res;
            if (isNumberic(m_inputName.text, out res) && res > 10000)   //输入的是纯是数字并且大于10000， 需要找出该数字id的公会和改数字作为名字的公会
            {
                CorpsProps p = RoleMgr.instance.Hero.CorpsPart.GetCorpsById(res);
                if (p != null)
                    pData.Add(p);
                CorpsProps p2 = RoleMgr.instance.Hero.CorpsPart.GetCorpsByName(m_inputName.text);
                if (p2 != null)
                    pData.Add(p2);
            }
            else
            {
                CorpsProps p = RoleMgr.instance.Hero.CorpsPart.GetCorpsByName(m_inputName.text);
                if (p != null)
                    pData.Add(p);
            }

            if (pData.Count == 0)
            {
                UIMessage.Show("搜索不到指定id或名字的公会");
                m_GridGroup.SetCount(0);
            }
            else
            {
                m_GridGroup.SetCount(pData.Count);
                
                for (int i = 0; i < pData.Count; i++)
                {
                    UICorpsListItem sel = m_GridGroup.Get<UICorpsListItem>(i);
                    sel.OnSetData(pData[i]);
                }
            }
            m_moreArrow.gameObject.SetActive(false);
            m_page.text = "1";
        });
        //左切换页
        m_leftHandle.AddClick(() =>
        {
            if (m_index > 0) { m_index--; }
            CheckPageIndex();
        });
        //右切换页
        m_rightHandle.AddClick(() =>
        {
            if (m_index < m_totalPage - 1) { m_index++; }
            CheckPageIndex();
        });

        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    public override void OnOpenPanel(object param)
    {
        NetMgr.instance.CorpsHandler.ReqGetAllCorps(RoleMgr.instance.Hero.CorpsPart.getHasReq);
        
    }

    public override void OnClosePanel()
    {
        ResetView();
    }

    public void OnUpdateList(bool resetPage = true)
    {
        if(resetPage)
        {
            CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
            corps = corpsPart.corpsList;
            m_index = 0;
            m_totalPage = corps.Count / MAX_SHOW + 1;
        }
        CheckPageIndex();
    }

    public void ResetView()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    #region PrivateMethod
    //根据页签显示界面
    void CheckPageIndex()
    {
        int allCount = corps.Count;
        int r = m_index * MAX_SHOW + MAX_SHOW > allCount ? allCount - m_index * MAX_SHOW : MAX_SHOW;
        List<CorpsProps> sList = corps.GetRange(m_index * MAX_SHOW, r);
        int count = sList.Count;
        m_GridGroup.SetCount(count);
        if (count == 0)
            m_gridHandle.SetState(1);
        else
            m_gridHandle.SetState(0);
        m_moreArrow.gameObject.SetActive(count >= 4 ? true : false);

        for (int i = 0; i < count; i++)
        {
            UICorpsListItem sel = m_GridGroup.Get<UICorpsListItem>(i);
            sel.OnSetData(sList[i]);
        }
        m_page.text = (m_index+1).ToString();

        //暂时不要
        //if (allCount <= MAX_SHOW)
        //{
        //    m_iconBarState.SetState(3);
        //    return;
        //}

        //if (m_index == 0)
        //    m_iconBarState.SetState(0);
        //else if (m_index > 0 && m_index + 1 < m_totalPage)
        //    m_iconBarState.SetState(1);
        //else
        //    m_iconBarState.SetState(2);
    }
    
    void OnScrollChanged(Vector2 v)
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        m_moreArrow.gameObject.SetActive(part.corpsList.Count >= 4 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }

    //判断是否是纯数字
    bool isNumberic(string message, out int result)
    {
        //是的话则将其转换为数字并将其设为out类型的输出值、返回true, 否则为false
        result = -1;   //result 定义为out 用来输出值
        try
        {
            result = int.Parse(message);
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}
