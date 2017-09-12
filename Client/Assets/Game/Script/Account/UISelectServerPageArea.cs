#region Header
/**
 * 名称：UISelectServerPageArea
 
 * 日期：2015.11.29
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class UISelectServerPageArea : MonoBehaviour
{
    #region SerializeFields
    public StateGroup m_group;
    public StateHandle m_btnEnter;
    #endregion

    #region Fields
    UISelectServer m_parent;
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UISelectServer parent)
    {
        m_parent = parent;
        m_btnEnter.AddClick(OnEnter);
    }

    //显示
    public void OnOpenPage(string areaName)
    {
        List<ServerInfo> l = NetMgr.instance.AccountHandler.LoginInfo.serversByArea.Get(areaName);

        //初始化
        m_group.SetCount(l.Count);
        for (int i = 0; i < l.Count; ++i)
        {
            m_group.Get<UIServerItem>(i).Init(l[i]);
        }

        //选中第一个
        if (l.Count > 0)
            m_group.SetSel(0);
    }


    #endregion

    #region Private Methods
    
    
    #endregion
    void OnEnter()
    {
        UIServerItem item = m_group.GetCur<UIServerItem>();
        if (item == null)
        {
            UIMessageBox.Open(LanguageCfg.Get("select_server"), () => { });
            return;
        }
        NetMgr.instance.AccountHandler.ConnectServer(item.s);
    }
    
}
