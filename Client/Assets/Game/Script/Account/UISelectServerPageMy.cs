#region Header
/**
 * 名称：UISelectServerPageMy
 
 * 日期：2015.11.29
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class UISelectServerPageMy : MonoBehaviour
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
    public void OnOpenPage()
    {
        LoginInfo loginInfo = NetMgr.instance.AccountHandler.LoginInfo;
        m_group.SetCount(loginInfo.roleList.Count);

        if (loginInfo.roleList.Count <= 0)
        {
            m_group.Get(0).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < loginInfo.roleList.Count; ++i)
            {
                m_group.Get<UIMyServerItem>(i).Init(loginInfo.roleList[i]); 
            }
            m_group.SetSel(0);
        }
    }   

    #endregion

    #region Private Methods
    void OnEnter()
    {
        UIMyServerItem item = m_group.GetCur<UIMyServerItem>();
        if(item == null)
        {
            Debuger.Log("没有选中的服务器1");
            return;
        }
        if (item.role == null)
        {
            Debuger.Log("没有选中的服务器2");
            return;
        }
        NetMgr.instance.AccountHandler.ConnectServer(item.role.serverInfo);
    }
    #endregion
}