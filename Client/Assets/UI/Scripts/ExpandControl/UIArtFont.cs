#region Header
/**
 * 名称：UINum
 
 * 日期：2015.12.3
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIArtFont : MonoBehaviour
{
    //对齐类型
    public enum enAlign
    {
        left,
        middle,
        right,
        none
    }
    #region Fields
    public static Vector2 s_alignLeft = new Vector2(0, 0.5f);
    public static Vector2 s_alignMiddle = new Vector2(0, 0.5f);
    public static Vector2 s_alignRight = new Vector2(0, 0.5f);
    public static Vector2 s_archer = new Vector2(0.5f, 0.5f);

    public string m_prefix;//前缀
    public string m_num;
    public float m_space=0f;
    public enAlign m_align = enAlign.left;
    #endregion


    #region Properties
    
    #endregion

    #region Mono Frame
    void Start()
    {
        
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}
    #endregion
   


    #region Private Methods
    
    #endregion

    public void SetNum(string num ,bool force =false)
    {
        if(string.IsNullOrEmpty(m_prefix)){
            Debuger.LogError("前缀不能为空");
        }

        if (num == m_num && !force)
            return;
        m_num = num;

        //设置好图片，并计算总的图片宽度
        Transform t =this.transform;
        Sprite s;
        int i=0;
        RectTransform subTran;
        Image subImage;
        float totalWidth=0;
        foreach(char c in num){
            s = UIMgr.GetSprite(m_prefix + c);
            if (s == null)continue;

            //获取子节点，没有则创建新的
            if(i<t.childCount)
                subTran = t.GetChild(i) as RectTransform;
            else
            {
                subTran = new GameObject("tmp", typeof(Image)).GetComponent<RectTransform>();
                subTran.SetParent(t,false);
                subTran.localScale = Vector3.one;
                subTran.anchorMin = s_archer;
                subTran.anchorMax = s_archer;
            }
                
            //设置图片
            subTran.gameObject.SetActive(true);
            subImage =subTran.AddComponentIfNoExist<Image>();
            subImage.sprite = s;
            subTran.sizeDelta = s.rect.size;
            totalWidth += s.rect.size.x;
            if (m_align == enAlign.left)
                subTran.pivot = s_alignLeft;
            else if (m_align == enAlign.right)
                subTran.pivot = s_alignRight;
            else if (m_align == enAlign.middle)
                subTran.pivot = s_alignMiddle;
                
            ++i;
        }
        
        //隐藏多余的节点
        GameObject subGo;
        for (int i2 = i; i2 < t.childCount; ++i2)
        {
            subGo= t.GetChild(i2).gameObject;
            EditorUtil.SetDirty(subGo);
            subGo.SetActive(false);
        }
            
        if(m_align != enAlign.none)
        {
            //根据对齐方式计算从左边开始的起点
            Vector3 pos;
            if (m_align == enAlign.left)
                pos = new Vector3(0, 0, 0);
            else if (m_align == enAlign.right)
                pos = new Vector3(-totalWidth - (i <= 1 ? 0 : ((i - 1) * m_space)), 0, 0);
            else 
                pos = new Vector3((-totalWidth - (i <= 1 ? 0 : ((i - 1) * m_space))) / 2f, 0, 0);


            //排列下
            for (int i2 = 0; i2 < i; ++i2)
            {
                subTran = t.GetChild(i2) as RectTransform;
                subTran.anchoredPosition3D = pos;
                EditorUtil.SetDirty(subTran.gameObject);

                pos.x += m_space + subTran.sizeDelta.x;
            }
        }
        


        EditorUtil.SetDirty(this);
    }

    

}
