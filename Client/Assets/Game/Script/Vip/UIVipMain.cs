using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UIVipMain : UIPanel
{
    #region SerializeFields
        
    public UIVip uiVip;
    public UIRecharge uiRecharge;


    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
     
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        uiVip.Init();
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
   
    public void CheckTip()
    {
       

    }


}



