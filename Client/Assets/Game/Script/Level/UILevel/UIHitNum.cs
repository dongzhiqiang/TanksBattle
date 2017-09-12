#region Header
/**
 * 名称：UIHitNumItem
 
 * 日期：2015.12.24
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIHitNum : MonoBehaviour
{
    public UIArtFont num;
    //public Animator ani;
    
    public ImageEx element;
    public UIArtFont elementNum;
    
    public void Init(int value, enElement elem,int elemValue)
    {
        if(elem== enElement.none|| elemValue == 0)
        {
            num.SetNum(value.ToString());
            if (element!= null)
                element.gameObject.SetActive(false);

            if(elementNum!= null)
                elementNum.gameObject.SetActive(false);
        }
        else
        {
            num.SetNum(value + "+"); 
            if (elementNum!= null)
            {
                element.gameObject.SetActive(true);
                element.Set("ui_guanqia_element_"+(int)elem);
            }
            
            if(elementNum!=null)
            {
                elementNum.SetNum(elemValue.ToString());
                elementNum.gameObject.SetActive(true);
            }
            
        }
        this.gameObject.SetActive(true);

      //  num.SetNum(value.ToString());

        //游戏对象SetAcitve(true)的时候会重新播放动画的，这里不用设置什么
        //ani.enabled = false;
        //ani.Play("", -1, 0);
        //ani.enabled = true;
        //ani.Update(0);
    }
}
