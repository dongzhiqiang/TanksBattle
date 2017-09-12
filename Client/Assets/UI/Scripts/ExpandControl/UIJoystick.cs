#region Header
/**
 * 名称：摇杆
 
 * 日期：2016.1.5
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIJoystick : MonoBehaviour
{
    #region Fields
    public StateHandle m_joystick;
    public RectTransform m_bk;
    public RectTransform m_center;
    public float m_maxDis = 150f;
    public float m_disScale = 0.65f;
    public float m_verticalAngleScale = 0.8f;
    public float m_sliderCheckTime = 0.2f;//这个时间内按下又提起认为是滑动
    public float m_sliderCheckTimePC = 0.1f;//这个时间内按下又提起认为是滑动

    public Action<Vector2> m_onJoystickDown;//回调参数是按下的点
    public Action<Vector2> m_onJoystickDrag;//回调参数是delta
    public Action m_onJoystickUp;
    public Action m_onJoystickSlider;

    //判断是否在使用手柄
    bool m_usingJoystick = false;
    bool m_isStarted = false;
    Vector2 m_bkFirstPos;
    Vector2 m_centerFirstPos;
    Vector2 m_oldDir = Vector2.zero;//用于pc上键盘控制方向
    Vector2 m_oldDirRight = Vector2.zero;//右侧摇杆控制翻滚
    float m_lastDownTime;

    #endregion


    #region Properties
    public bool IsDraging { get { return m_joystick.m_curState == 1; } }
    #endregion

    #region Static Methods
    public static void StaticFun()
    {

    }
    #endregion


    #region Mono Frame
    void Start()
    {
        m_isStarted = true;

        int UI_LAYER = LayerMask.NameToLayer("UI");
        int UI_HIGHT_LAYER = LayerMask.NameToLayer("UIHight");

        m_bkFirstPos = m_bk.anchoredPosition;
        m_centerFirstPos = m_center.anchoredPosition;
        m_joystick.AddPointDown((PointerEventData data) =>
        {
#if !ART_DEBUG
            //如果引导把中心块隐藏了，就不响应了
            if (TeachMgr.instance.PlayNow && !m_center.gameObject.activeInHierarchy)
                return;
#endif
            var cam = data.pressEventCamera;
            if (cam == null)
                cam = data.pointerPress && data.pointerPress.layer == UI_HIGHT_LAYER ? UIMgr.instance.UICameraHight : UIMgr.instance.UICamera;
            Vector2 pos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_center.parent as RectTransform, data.position, cam, out pos))
                return;
            JoystickDown(pos);
        });
        m_joystick.AddDrag((PointerEventData data) =>
        {
#if !ART_DEBUG
            //如果引导把中心块隐藏了，就不响应了
            if (TeachMgr.instance.PlayNow && !m_center.gameObject.activeInHierarchy)
                return;
#endif
            var cam = data.pressEventCamera;
            if (cam == null)
                cam = data.pointerDrag && data.pointerDrag.layer == UI_HIGHT_LAYER ? UIMgr.instance.UICameraHight : UIMgr.instance.UICamera;
            Vector2 pos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_center.parent as RectTransform, data.position, cam, out pos))
                return;
            JoystickDrag(pos);
        });
        m_joystick.AddPointUp((PointerEventData data) =>
        {
            JoystickUp();
        });
    }

    /// <summary>
    /// 用于摇杆翻滚
    /// </summary>
    /// <returns></returns>
    IEnumerator Slider()
    {
        m_onJoystickSlider();
        yield return new WaitForSeconds(0.2f);

        Vector2 dir = GetLeftJoystickDir();
        if (dir == Vector2.zero)
        {
            //JoystickUp();
            //模拟UI事件，为了方便引导
            m_joystick.ExecuteUp(m_bkFirstPos + dir);
            m_joystick.ExecuteEndDrag(m_bkFirstPos + dir);
        }            
        else
        {
            //JoystickDrag(m_bkFirstPos + dir);
            //模拟UI事件，为了方便引导
            m_joystick.ExecuteDrag(m_bkFirstPos + dir);
        }
    }

    /// <summary>
    /// 检测当前是否在应用手柄
    /// </summary>
    /// <returns></returns>
    bool isJoystick()
    {
        if (GetLeftJoystickDir() != Vector2.zero || GetRightJoystickDir() != Vector2.zero)
        {
            m_usingJoystick = true;
            return true;
        }
        else
        {
            return false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 dir;
#if UNITY_EDITOR || UNITY_STANDALONE
        
        //是否在使用手柄
        if(isJoystick())
        {
            //手柄输入
            dir = GetLeftJoystickDir();
        }
        else
        {
            //键盘输入
            dir = GetDir();
        }
#else
        dir = GetLeftJoystickDir();
#endif
        //控制人物移动
        if (dir != m_oldDir)
        {
            if (m_oldDir == Vector2.zero && dir != Vector2.zero)
            {
                //JoystickDown(m_bkFirstPos);
                //JoystickDrag(m_bkFirstPos + dir);
                //模拟UI事件，为了方便引导
                m_joystick.ExecuteDown(m_bkFirstPos);
                m_joystick.ExecuteBeginDrag(m_bkFirstPos);
                m_joystick.ExecuteDrag(m_bkFirstPos + dir);
            }
            else if (m_oldDir != Vector2.zero && dir == Vector2.zero)
            {
                //JoystickUp();
                //模拟UI事件，为了方便引导
                m_joystick.ExecuteUp(m_bkFirstPos + dir);
                m_joystick.ExecuteEndDrag(m_bkFirstPos + dir);
            }                
            else
            {
                //JoystickDrag(m_bkFirstPos + dir);
                //模拟UI事件，为了方便引导
                m_joystick.ExecuteDrag(m_bkFirstPos + dir);
            }                
            m_oldDir = dir;
        }

        //手柄控制人物翻滚
        Vector2 dirRight = GetRightJoystickDir();

        if (dirRight != m_oldDirRight)
        {
            if (m_oldDirRight == Vector2.zero && dirRight != Vector2.zero)
            {
                //JoystickDown(m_bkFirstPos);
                //JoystickDrag(m_bkFirstPos + dirRight);
                //模拟UI事件，为了方便引导
                m_joystick.ExecuteDown(m_bkFirstPos);
                m_joystick.ExecuteBeginDrag(m_bkFirstPos);
                m_joystick.ExecuteDrag(m_bkFirstPos + dirRight);

                StartCoroutine(Slider());
            }
            m_oldDirRight = dirRight;
        }
    }
    #endregion
   


    #region Private Methods

    /// <summary>
    /// 获取键盘输入
    /// </summary>
    /// <returns></returns>
    Vector2 GetDir()
    {
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            return new Vector2(100, 100);
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
            return new Vector2(-100, 100);
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
            return new Vector2(100, -100);
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
            return new Vector2(-100, -100);
        else if (Input.GetKey(KeyCode.W))
            return new Vector2(0, 100);
        else if (Input.GetKey(KeyCode.S))
            return new Vector2(0, -100);
        else if (Input.GetKey(KeyCode.A))
            return new Vector2(-100, 0);
        else if (Input.GetKey(KeyCode.D))
            return new Vector2(100, 0);
        else
            return Vector2.zero;
    }

    /// <summary>
    /// 获取左侧摇杆参数
    /// </summary>
    /// <returns></returns>
    Vector2 GetLeftJoystickDir()
    {
       // Debug.Log("水平：" + Input.GetAxis("LeftJoystickHorizontal"));
        //Debug.Log("竖直：" + Input.GetAxis("LeftJoystickVertical"));


        if (Input.GetAxis("LeftJoystickVertical") ==1&& Input.GetAxis("LeftJoystickHorizontal")==1)
            return new Vector2(100, 100);
        else if (Input.GetAxis("LeftJoystickVertical") == 1 && Input.GetAxis("LeftJoystickHorizontal") == -1)
            return new Vector2(-100, 100);
        else if (Input.GetAxis("LeftJoystickVertical") == -1 && Input.GetAxis("LeftJoystickHorizontal") == 1)
            return new Vector2(100, -100);
        else if (Input.GetAxis("LeftJoystickVertical") == -1 && Input.GetAxis("LeftJoystickHorizontal") == -1)
            return new Vector2(-100, -100);
        else if (Input.GetAxis("LeftJoystickVertical") == 1)
            return new Vector2(0, 100);
        else if (Input.GetAxis("LeftJoystickVertical") == -1)
            return new Vector2(0, -100);
        else if (Input.GetAxis("LeftJoystickHorizontal")==-1)
            return new Vector2(-100, 0);
        else if (Input.GetAxis("LeftJoystickHorizontal") == 1)
            return new Vector2(100, 0);
        else
            return Vector2.zero;
    }

    /// <summary>
    /// 获取右侧摇杆参数
    /// </summary>
    /// <returns></returns>
    Vector2 GetRightJoystickDir()
    {
        // Debug.Log("水平：" + Input.GetAxis("LeftJoystickHorizontal"));
        //Debug.Log("竖直：" + Input.GetAxis("LeftJoystickVertical"));


        if (Input.GetAxis("RightJoystickVertical") == 1 && Input.GetAxis("RightJoystickHorizontal") == 1)
            return new Vector2(100, 100);
        else if (Input.GetAxis("RightJoystickVertical") == 1 && Input.GetAxis("RightJoystickHorizontal") == -1)
            return new Vector2(-100, 100);
        else if (Input.GetAxis("RightJoystickVertical") == -1 && Input.GetAxis("RightJoystickHorizontal") == 1)
            return new Vector2(100, -100);
        else if (Input.GetAxis("RightJoystickVertical") == -1 && Input.GetAxis("RightJoystickHorizontal") == -1)
            return new Vector2(-100, -100);
        else if (Input.GetAxis("RightJoystickVertical") == 1)
            return new Vector2(0, 100);
        else if (Input.GetAxis("RightJoystickVertical") == -1)
            return new Vector2(0, -100);
        else if (Input.GetAxis("RightJoystickHorizontal") == -1)
            return new Vector2(-100, 0);
        else if (Input.GetAxis("RightJoystickHorizontal") == 1)
            return new Vector2(100, 0);
        else
            return Vector2.zero;
    }
    #endregion

    public void JoystickDown(Vector2 pos)
    {
        m_bk.anchoredPosition = pos;
        m_center.anchoredPosition = pos;
        m_joystick.SetState(1);
        m_lastDownTime = Time.time;

        if(m_onJoystickDown!= null)
            m_onJoystickDown(pos);
    }

    public void JoystickDrag(Vector2 pos)
    {
        Vector2 link = pos - m_bk.anchoredPosition;
        if (link.sqrMagnitude <= m_maxDis * m_maxDis)
            m_center.anchoredPosition = pos;
        else
            m_center.anchoredPosition = m_bk.anchoredPosition + link.normalized * m_maxDis;

        if (m_onJoystickDrag != null)
            m_onJoystickDrag(m_center.anchoredPosition - m_bk.anchoredPosition);
    }

    public void JoystickUp()
    {
        if (!m_isStarted)
            return;

        float sq =(m_center.anchoredPosition - m_bk.anchoredPosition).sqrMagnitude;
        m_bk.anchoredPosition = m_bkFirstPos;
        m_center.anchoredPosition = m_centerFirstPos;
        m_joystick.SetState(0);

        if (m_onJoystickUp  != null) 
            m_onJoystickUp();
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Time.time - m_lastDownTime < m_sliderCheckTimePC && sq > 100 && m_onJoystickSlider != null && !m_usingJoystick)
            m_onJoystickSlider();
#else
      if (Time.time - m_lastDownTime < m_sliderCheckTime && sq >100&& m_onJoystickSlider != null&&!m_usingJoystick)
            m_onJoystickSlider();
#endif    
        m_usingJoystick = false;

    }

}
