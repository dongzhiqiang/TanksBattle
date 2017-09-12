#region Header
/**
 * 名称：监听一些全局的输入事件
 
 * 日期：2016.4.23
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class InputMgr : SingletonMonoBehaviour<InputMgr>
{
    public delegate void InputEvent();

    //监听提起
    public event InputEvent PointerUpEvent;
    //监听提起
    public event InputEvent PointerDownEvent;

    void Update()
    {


#if UNITY_EDITOR || UNITY_STANDALONE
        if(PointerDownEvent!=null &&Input.GetMouseButtonDown(0))
        {
            PointerDownEvent();
        }
        if(PointerUpEvent != null && Input.GetMouseButtonUp(0))
        {
            PointerUpEvent();
        }

#else
        if(PointerUpEvent!= null || PointerDownEvent!=null){
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch input = Input.GetTouch(i);

                if (PointerDownEvent!=null &&input.phase == TouchPhase.Began)
                    PointerDownEvent();
                if (PointerUpEvent != null &&((input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended)))
                    PointerUpEvent();
            }
        }
#endif
    }


}