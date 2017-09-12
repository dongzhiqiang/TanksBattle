#region Header
/**
 * 名称：ui类模板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UITemplate : UIPanel
{

    #region Fields
   
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
       
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Private Methods
    

    #endregion

    
}
