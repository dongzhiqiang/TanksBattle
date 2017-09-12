#region Header
/**
 * 名称：用于播放ui特效的面板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UIFxPanel : UIPanel
{
    
    //播放ui特效
    public static void ShowFx(string fx)
    {
        ShowFx(fx, Vector3.zero) ;
    }

    public static void ShowFx(string fx, Vector3 pos) 
    {
        UIMgr.instance.Get<UIFxPanel>().Show(fx,pos);
    }
    #region Fields
    public static int Hight_Layer;
    public GameObject m_fxTest;
    #endregion

    #region Properties
    
    #endregion

    #region Frame
    void Awake()
    {
        Hight_Layer = LayerMask.NameToLayer("UIHight");
    }
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

    void SetLayer(Transform t)
    {
        t.gameObject.layer = Hight_Layer;
    }
    #endregion
    public void Show(string fx,Vector3 pos)
    {
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Get(fx, pos, OnLoadUIFx,false);
    }

    void OnLoadUIFx(GameObject go, object param)
    {
        go.SetActive(false);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(this.transform, false);
        Util.DoAllChild(rt,SetLayer);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = (Vector3)param;
        rt.sizeDelta = Vector2.zero;

        if (rt.GetComponent<FxDestroy>() == null)
            Debuger.LogError("ui特效没有加销毁脚本:{0}", go.name);

        go.SetActive(true);
    }

    
#if ART_DEBUG
    void OnGUI()
    {
        if (GUILayout.Button("测试特效"))
        {
            if (m_fxTest.transform.parent == null && GameObject.Find(m_fxTest.name) == null)//如果是预制体，那么创建
                m_fxTest = (GameObject)GameObject.Instantiate(m_fxTest);
            OnLoadUIFx(m_fxTest, Vector3.zero)  ;
        }
    }
#else
    [ContextMenu("TestFx")]
    void TestFx(){
        if (Main.instance==null)return ;
        Show(m_fxTest.name,Vector3.zero);
        
    }
#endif

}
