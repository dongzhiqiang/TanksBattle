#region Header
/**
 * 名称：ui的子页面的类模板
 
 * 日期：201x.x.x
 * 描述：新建ui的子页面的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UIPage : MonoBehaviour
{
    
    #region Fields
    UIPanel m_parent;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    protected virtual void OnInitPage() { }

    //显示
    protected virtual void OnOpenPage() { }

    //更新
    protected virtual void OnUpdatePage() { }

    //关闭
    protected virtual void OnClosePage() { }

    #endregion

    #region Private Methods
    
    #endregion

    public void InitPage(UIPanel parent)
    {
        m_parent = parent;
        OnInitPage();
    }

    public void OpenPage()
    {
        OnOpenPage();
    }

    //这个接口一般比较少用，界面具体实现不一定要支持
    public void UpdatePage()
    {
        OnUpdatePage();
    }

    //这个接口一般比较少用，界面具体实现不一定要支持
    public void ClosePage()
    {
        OnClosePage();
    }
    
    public T GetParent<T>()where T:UIPanel
    {
        return (T)m_parent;
    }


}
