#region Header
/**
 * 名称：UISelectServerPageRecommend
 
 * 日期：2015.11.29
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UISelectServerPageRecommend : MonoBehaviour
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
        
        //初始化
        m_group.SetCount(loginInfo.serversByRecommend.Count);
        for (int i = 0; i < loginInfo.serversByRecommend.Count; ++i)
        {
            m_group.Get<UIServerItem>(i).Init(loginInfo.serversByRecommend[i]);
        }

        //选中第一个
        if (loginInfo.serversByRecommend.Count > 0)
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
            Debuger.Log("没有选中的服务器");
            return;
        }
        NetMgr.instance.AccountHandler.ConnectServer(item.s);
    }
    
}
