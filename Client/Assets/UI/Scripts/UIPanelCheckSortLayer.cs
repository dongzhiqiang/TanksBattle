/*
 * *********************************************************
 * 名称：面板动态层级的辅助类
 
 * 日期：2015.7.14
 * 描述：
 * 1.由于面板是动态层级的，新加到面板里的游戏对象需要重新设置sortingLayer
 * *********************************************************
 */
using UnityEngine;
using System.Collections;


public class UIPanelCheckSortLayer : MonoBehaviour
{
    public int m_refOrder = 0;

    bool m_isCheck = false;
    int m_lastRefOrder = int.MinValue;
    UIPanelBase m_parent =null;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        bool needReset = !m_isCheck;
        Check();
        needReset =needReset&&m_isCheck;//说明在unpdate中check了

        //层级有改变或者check完，更新下
        if (m_isCheck &&m_parent!=null && (needReset || m_lastRefOrder != m_refOrder))
        {
            m_lastRefOrder = m_refOrder;//这里要再一次设置下，因为有可能是改了m_refOrder导致需要刷新的情况
            m_parent.ResetOrder();       
        }
	}

    
    public void Check()
    {
        //重置层级前记录下
        if (m_isCheck)
            return ;
        if (m_parent == null)
        {
            m_parent = Util.GetRoot<UIPanelBase>(this.transform);
            if (m_parent == null)
            {
                Debuger.Log("UIPanelCheckSortLayer找不到UIPanel");
                m_isCheck = true;
                return ;
            }
        }
        m_isCheck = true;
        m_parent.AddSubOrder(this);
        m_lastRefOrder = m_refOrder;
        
    }

}
