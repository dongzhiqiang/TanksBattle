#region Header
/**
 * 名称：UILevelAreaHead
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaNum : UILevelArea
{
    public enum enType
    {
        redSub,
        blueSub,
        redSubCritical,
        greenAdd,
        blueAdd,
        redSubStrong,
        redSubWeak,
        redSubHero,
    }
    #region Fields
    public List<SimplePool> m_pools = new List<SimplePool>();

    RectTransform m_tran;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.num; } }
    public override bool IsOpenOnStart { get{return true;} }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        m_tran = this.GetComponent<RectTransform>();
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        ClearNum();
    }

    protected override void OnUpdateArea()
    {
       
    }

    //关闭
    protected override void OnCloseArea()
    {

    }

    protected override void OnRoleBorn()
    {
        
    }

    #endregion

    #region Private Methods
    GameObject Get(enType type, Vector3 worldPos)
    {
        if (m_pools.Count <= (int)type)
        {
            Debuger.LogError("UIWorldText没有对应类型的对象池:{0}", type);
            return null;
        }

        if (CameraMgr.instance == null)
        {
            Debuger.LogError("UIWorldText找不到CameraMgr");
            return null;
        }

        //场景相机
        Camera ca = CameraMgr.instance.CurCamera;
        if (ca == null)
        {
#if !ART_DEBUG && UNITY_EDITOR
            if (Main.instance == null)
                ca = GameObject.FindObjectOfType<Camera>();
#endif     
#if ART_DEBUG
            ca = GameObject.FindObjectOfType<Camera>();
#endif
            if (ca == null)
            {
                Debuger.LogError("UIWorldText找不到场景相机");
                return null;
            }
        }

        //ui相机
        Camera caUI = UIMgr.instance == null ? GameObject.Find("UICameraHight").GetComponent<Camera>() : UIMgr.instance.UICameraHight;
        if (caUI == null)
        {
#if !ART_DEBUG && UNITY_EDITOR
            if (Main.instance == null)
                caUI = GameObject.FindObjectOfType<Camera>();
#endif     
#if ART_DEBUG
            caUI = GameObject.FindObjectOfType<Camera>();
#endif
            if (caUI == null)
            {
                Debuger.LogError("UIWorldText找不到UIHight相机");
                return null;
            }
        }

        //计算在屏幕上的位置
        Vector3 screenPos = ca.WorldToScreenPoint(worldPos);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height || screenPos.z <= 0)
            return null;

        //设置位置
        RectTransform rt = m_pools[(int)type].Get(true).GetComponent<RectTransform>();
        Vector2 pos2D;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_tran,
            screenPos,
            caUI, out pos2D))
        {
            rt.anchoredPosition = pos2D;
        }
        else
            Debuger.LogError("UIWorldText计算不出2d位置");

        return rt.gameObject;
    }

    #endregion

    public void ShowNum(enType t,Vector3 worldPos, int value, enElement elem, int elemValue)
    {
        if (!m_parent.gameObject.activeSelf || !this.gameObject.activeSelf)
            return;

        GameObject go = Get(t, worldPos);
        if (go==null)
            return;
        go.GetComponent<UIHitNum>().Init(value, elem, elemValue);
    }

    public void ClearNum()
    {
        foreach (SimplePool p in m_pools)
        {

            p.Clear();
        }

    }
}
