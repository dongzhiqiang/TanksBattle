using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIMessage : UIPanel
{
    public SimplePool m_pool;
    public float m_split=30;
    
    public static void Show(string msg)
    {
        UIMgr.instance.Get<UIMessage>().ShowMsg(msg);
    }

    //配置配的客户端飘字
    public static void ShowFlowTip(string key, params object[] param)
    {
#if !ART_DEBUG
        UIMgr.instance.Get<UIMessage>().ShowMsg(string.Format(LanguageCfg.Get(key), param));
     
#endif
    }

    //UIMessage.ShowError(MODULE.MODULE_ACCOUNT,ResultCodeWeapon.LEVEL_ERROR);
    public static void ShowError(int module, int errCode)
    {
#if !ART_DEBUG
        Show(ErrorCodeCfg.GetErrorDesc(module, errCode));
#endif
    }


    //初始化时调用
    public override void OnInitPanel()
    {

    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        Clear();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    public void ShowMsg(string msg)
    {
        GameObject go = m_pool.Get();
        RectTransform tCur = go.transform as RectTransform;
        RectTransform t;
        Vector2 pos;
        tCur.Find("offset/image/txt").GetComponent<Text>().text = msg;
        t = tCur.Find("offset") as RectTransform;
        t.GetComponent<CanvasGroup>().alpha = 1;
        pos = tCur.anchoredPosition;
        pos.y = 0;
        tCur.anchoredPosition = pos;

        List<GameObject> l = new List<GameObject>(m_pool.GetUnsing());
        if (l.Count <= 1)
            return;
        //把老的往上顶
        l.Sort((GameObject go1, GameObject go2) =>
        {
            RectTransform t1 = go1.transform as RectTransform;
            RectTransform t2 = go2.transform as RectTransform;
            if (t1.anchoredPosition.y == t1.anchoredPosition.y)
            {
                float time1 = t1.GetComponent<SimpleHandle>().CurTime;
                float time2 = t2.GetComponent<SimpleHandle>().CurTime;
                if (time1 == time2)
                    return 0;
                else
                    return time1 < time2 ? -1 : 1;
            }
            else
                return t1.anchoredPosition.y < t2.anchoredPosition.y ? -1 : 1;
        });


        for (int i = 1; i < l.Count; ++i)
        {
            tCur = l[i - 1].transform as RectTransform;
            t = l[i].transform as RectTransform;
            pos = t.anchoredPosition;
            if (pos.y - tCur.anchoredPosition.y < m_split)
            {
                pos.y = tCur.anchoredPosition.y + m_split;
                t.anchoredPosition = pos;
            }
        }
    }

    void Clear()
    {
        m_pool.Clear();
    }

    //void OnGUI()
    //{
    //    if (GUILayout.Button("展示一条"))
    //    {
    //        ShowMsg("测试用");
    //    }
    //}
}
