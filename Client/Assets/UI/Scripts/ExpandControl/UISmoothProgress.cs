#region Header
/**
 * 名称：进度条
 
 * 日期：2016.2.29
 * 描述：支持多条,支持渐变
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UISmoothProgress : MonoBehaviour
{
   
    #region Fields
    public ImageEx m_next;//下一条
    public ImageEx m_maskBelow;//位于cur的下面
    public ImageEx m_cur;
    public ImageEx m_maskAbove;//多条的时候需要用到，位于cur的上面
    public TextEx m_num;//剩下多少条
    public string m_format = "x{0}";
    public float m_smooth = 1;
    public bool m_overrideCur=true;
    public AnimationCurve m_smoothCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    

    public Sprite[] m_sprites;

    [SerializeField][HideInInspector]
    float m_targetValue = 1;

    string[] m_spriteNames;
    float m_curValue=1;
    
    #endregion


    #region Properties
    public float Unit { get{return 1f/Num;}}
    public int Num { get{
        if(m_spriteNames != null && m_spriteNames.Length!= 0)
            return m_spriteNames.Length;

        if (m_sprites != null && m_sprites.Length != 0)
            return m_sprites.Length;
        return 1;
    } }
    public float Progress
    {
        get { return m_targetValue; }
        set {
            if (m_targetValue ==value)
                return;

   #if UNITY_EDITOR
			if (!Application.isPlaying) //编辑器下没有运行时，立即切
                SetProgress(value,true); 
            else        
    #endif
                SetProgress(value, false); 
        }
    }
    
    #endregion

    #region Mono Frame
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mathf.Approximately(m_targetValue, m_curValue))
        {
            float delta = m_targetValue - m_curValue;
            float sign = Mathf.Sign(delta);
            float abs = Mathf.Abs(delta);
            float absSmooth= Time.deltaTime * m_smooth * Mathf.Max(0.001f, m_smoothCurve.Evaluate(abs * Num)); 
            if (abs > absSmooth)
                abs = absSmooth;

            delta = abs * sign;


            m_curValue = m_curValue + delta;

            if (sign < 0)//减的话,cur直接变，mask渐变
            {
                int numTarget =  GetNumOfValue(m_targetValue);
                int numCur = GetNumOfValue(m_curValue);
                
                //分三种情况
                if (numTarget == numCur)//当前已经渐变到了同一条血，看不到cur上面的mask
                {
                    SetImageProgress(m_maskBelow, m_curValue);
                    SetImageProgress(m_cur, m_targetValue);
                    SetImageProgress(m_maskAbove, 0);
                }
                else if (numTarget + 1 == numCur)//相隔一条，cur和上下面的mask都看到
                {
                    SetImageProgress(m_maskBelow, (numCur - 1) * Unit - 0.0001f);
                    SetImageProgress(m_cur, m_targetValue);
                    SetImageProgress(m_maskAbove, m_curValue);
                    
                }
                else//相隔多条，看不到cur
                {
                    SetImageProgress(m_maskBelow, (numCur - 1) * Unit - 0.0001f);
                    SetImageProgress(m_cur, 0);
                    SetImageProgress(m_maskAbove, m_curValue);
                }

    
            }
            else//加的话
            {
                //不用mask
                SetImageProgress(m_maskBelow, 0);
                SetImageProgress(m_maskAbove, 0);

                //cur渐变
                SetImageProgress(m_cur,m_curValue);
            }
            FlashNumAndNext();
        }
    }
    #endregion
   


    #region Private Methods
    //从1开始算
    int GetNumOfValue(float value)
    {
        if(Unit==1)return 1;
        if (value <=0)return 1;
        if (value % Unit==0)
            return (int)(value / Unit);

        return (int)(value / Unit)+1;
    }
    int SetImageProgress(ImageEx image, float value)
    {
        if (value<=0)
        {
            if (image!=null) image.fillAmount = 0;
            return 0;
        }

        int num = GetNumOfValue(value);
        float unitValue =  Mathf.Clamp01((value % Unit) / Unit);
        if (unitValue==0)unitValue =1;

        //替换对应条数的图片
        if (m_spriteNames != null && num <=m_spriteNames.Length)
        {
            if (image != null) image.Set(m_spriteNames[num - 1]);
        }
        else if (m_sprites != null && num <= m_sprites.Length)
        {
            if (image != null) image.overrideSprite = m_sprites[num - 1];
        }

        //设置进度
        if (image != null)
            image.fillAmount = unitValue;
        EditorUtil.SetDirty(image);
        return num;
    }

    void FlashNumAndNext()
    {
        if (m_num != null) {
            int num = GetNumOfValue(m_curValue);
            m_num.text = (num <=0|| Num <= 1)? "":string.Format(m_format, num);
            EditorUtil.SetDirty(m_num);
        }

        SetImageProgress(m_next, (GetNumOfValue(m_curValue)-1)*Unit-0.0001f);
    }
    #endregion

    //设置条数，这里要传进来的是每一条的图片名
    public void SetNum(string[] spriteNames)
    {
        m_spriteNames = spriteNames;
    }
    
    //设置进度，0~1
    public void SetProgress(float value, bool immediately)
    {
        if (immediately)
        {
            m_targetValue = Mathf.Clamp01(value);
            m_curValue = m_targetValue;
            //不用mask
            SetImageProgress(m_maskBelow, 0);
            SetImageProgress(m_maskAbove, 0);

            //cur
            SetImageProgress(m_cur, m_curValue);
            FlashNumAndNext();        
        }
        else
        {
            if(m_overrideCur)
                m_curValue = m_targetValue;//策划要求从上一个目标开始算，而不是当前
            m_targetValue = Mathf.Clamp01(value);
        }
    }

}
