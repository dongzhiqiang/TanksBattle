/*
 * *********************************************************
 * 名称：图片跟随类
 
 * 日期：2015.7.9
 * 描述：
 * 当image是fill模式下，跟随fill的大小移动
 * *********************************************************
 */
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageFillFollow : MonoBehaviour {
    public Image m_image;
    public RectTransform m_follow;
    public float m_minHide = 0;
    public float m_maxHide = 1;
    public float m_offset = 0;

	// Use this for initialization
	void Start () {
        Follow();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void LateUpdate()
    {
        Follow();
    }

    void Follow()
    {
        //检查隐藏的情况
        if (m_image == null || 
            m_follow == null || 
            m_image.type!= Image.Type.Filled || 
            !(m_image.fillMethod == Image.FillMethod.Horizontal || m_image.fillMethod == Image.FillMethod.Vertical)||
            m_image.fillAmount < m_minHide || m_image.fillAmount > m_maxHide
            )
        {
            if(m_follow != null && m_follow.gameObject.activeSelf == true)
                m_follow.gameObject.SetActive(false);
            return;
        }

        if (m_follow.gameObject.activeSelf == false)
            m_follow.gameObject.SetActive(true);

        RectTransform rt = m_image.rectTransform ;
        if (m_image.fillMethod == Image.FillMethod.Horizontal)
        {
            m_follow.anchoredPosition = new Vector2(m_offset + (m_image.fillOrigin == (int)Image.OriginHorizontal.Left ? rt.rect.width * m_image.fillAmount : -rt.rect.width * m_image.fillAmount), m_follow.anchoredPosition.y);
        }
        else
        {
            m_follow.anchoredPosition = new Vector2(m_follow.anchoredPosition.x, m_offset + (m_image.fillOrigin == (int)Image.OriginVertical.Bottom ? rt.rect.height * m_image.fillAmount : -rt.rect.height * m_image.fillAmount));
        }
        
        
    }
}
