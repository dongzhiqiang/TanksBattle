using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UILoading : UIPanel
{
    public Text m_percent;
    public Text m_tips;
    public ImageEx m_bgImg;
    public ImageEx m_tipsImg;

    //注意都是以100为单位的
    public float m_maxAdd = 1f;//一帧最多增长
    public float m_minAdd = 0.05f;//一帧最少增长
    public float m_maxProgress = 100;//最大进度，达到这个值就不动了
    public float m_progress = 0;//当前值，进度条可能会显示得比这个值大，取决于m_minAdd

    public float tipsChangeTime = 12f;

    int prevId = -1;
    float m_curProgress;
    public List<int> tipsIdList = new List<int>();
    
    public float CurProgress { get { return m_curProgress; } }

    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        tipsIdList.Clear();
        m_curProgress = 0;
        InvokeRepeating("ChangeTips", 0, tipsChangeTime);
        m_percent.text = "0%";
        
    }
   

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
       
        CancelInvoke();

        m_curProgress = 0;
        m_progress = 0;
        prevId = -1;
        m_percent.text = "0%";

        
    }
    
    public void SetBgImg(string bgRes)
    {
        if (!string.IsNullOrEmpty(bgRes))
            m_bgImg.Set(bgRes);
        else
            m_bgImg.Set("ui_big_bg_01");
    }

    public void ChangeTips()
    {
        int num = LoadingTipsCfg.m_cfgs.Count;
        if (num <= 0)
        {
            m_tips.text = "英雄即将出场，加载不耗流量";
            return;
        }
            

        //就填了一个，不换
        if (tipsIdList.Count == 1)
        {
            UpdateTips(tipsIdList[0]);
            return;
        }

        int id = prevId;

        

        if (tipsIdList.Count == 0)
        {
            while (id == prevId)
            {
                int idx = Random.Range(0, num);
                LoadingTipsCfg cfg;
                LoadingTipsCfg.m_cfgs.TryGetValue(idx, out cfg);
                if (cfg != null)
                    id = cfg.id;
                else
                {
                    UpdateTips(1);
                    return;
                }
            }
        }
        else
        {
            while (id == prevId)
                id = tipsIdList[Random.Range(0, tipsIdList.Count)];
        }

        prevId = id;
        UpdateTips(prevId);
    }

    void UpdateTips(int tipsId)
    {
        LoadingTipsCfg cfg = LoadingTipsCfg.GetCfg(tipsId);
        if (cfg == null)
        {
            Debug.LogError("没找到加载提示配置 id" + tipsId);
            return;
        }

        m_tips.text = cfg.desc;
        //m_tipsImg.Set(cfg.image);
    }

    public void Update()
    {
        if (m_curProgress >= m_maxProgress)
            return;
        else if(m_curProgress >= m_progress)
            m_curProgress += m_minAdd ;
        else
        {
            m_curProgress += m_maxAdd;
            if (m_curProgress > m_progress)
                m_curProgress = m_progress;
        }
        //Debuger.Log("进度:{0}", m_curProgress);
        if (m_curProgress > 100)
            m_curProgress = 100;
        m_percent.text = string.Format("{0}%", (int)m_curProgress);
    }

    public void SetProgress(float progress,float maxProgress=-1)
    {
        m_progress = progress;
        m_maxProgress =maxProgress==-1? progress: maxProgress;
    }
    
}
