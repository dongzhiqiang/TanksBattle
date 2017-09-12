#region Header
/**
 * 名称：材质变化的基类
 
 * 日期：2015.10.20
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityStandardAssets.ImageEffects;
using System;

[System.Serializable]
public class CameraAni
{
    public enum enType
    {
        Color,
        Float,
    }
    public static string[] TypeName = new string[] { "颜色", "浮点值" };
    public enum enState
    {
        init,
        begin,//开始渐变
        middle,//中间阶段，不渐变
        end,//结束渐变
        over,//完        
    }
    #region Fields
    public enType m_type = enType.Color;
    public string m_fieldName = "strength";

    //只对颜色和float有用
    public float m_beginDuration=0;
    public AnimationCurve m_beginCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));//变化曲线
    public float m_endDuration = 0;
    public AnimationCurve m_endCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -1f),new Keyframe(1f, 0f, -1f, 0f));//变化曲线

    //颜色变化
    public Color m_beginColor = new Color(1f,1f,1f,1f);
    public Color m_endColor = new Color(1f, 1f, 1f, 1f);

    //值变化
    public float m_beginFloat=0;
    public float m_endFloat = 1;


    enState m_state;
    FieldInfo m_fieldInfo;
    bool m_isEnd;
    float m_endTime;
    float m_lastTime;
    float m_curTime;
    float m_curFactor;
    #endregion


    #region Properties
    
    #endregion

    #region frame
    public void OnBegin(PostEffectsBase fx)
    {
        m_state = enState.init;
        m_lastTime = -1;
        m_isEnd = false;
        
        //找到对应的字段
        Type t = fx.GetType();
        m_fieldInfo = t.GetField(m_fieldName, BindingFlags.IgnoreCase|BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (m_fieldInfo==null)
            m_fieldInfo = t.GetField("m_"+m_fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);    
        if (m_fieldInfo == null) 
            Debuger.LogError("找不到字段：{0}", m_fieldName);
        
        OnUpdateCamera(fx, 0);
    }

    public void OnUpdateCamera(PostEffectsBase fx, float time)
    {
        m_curTime = time;
        enState state = CalcState();
        if (m_fieldInfo != null)//找不到属性不需要变化
        {
            switch (m_type)
            {
                case enType.Color: UpdateColor(fx); break;//如果是颜色渐变
                case enType.Float: UpdateFloat(fx); break;//如果是float值渐变
                default: Debuger.LogError("未知的类型{0}", m_type); break;
            }
        }
          
        m_state = state;
        m_lastTime = m_curTime;
    }

    //注意这里不是销毁，而是用于处理结束渐变
    public void OnEnd()
    {
        //进入end状态
        m_isEnd = true;

    }

    //销毁
    public void OnStop()
    {

    }
    #endregion


    #region Private Methods
    //注意别的地方不要调用，只能给OnUpdateMaterial调用
    enState CalcState()
    {
        if (m_state == enState.init)
        {
            m_curFactor = 0;
            return enState.begin;
        }
        else if (m_state == enState.begin)
        {
            float factor = m_beginDuration <= 0 ? 1 : m_curTime / m_beginDuration;    
            m_curFactor = Mathf.Clamp01(factor);
            return factor >= 1 ? enState.middle : enState.begin;
        }
        else if (m_state == enState.middle)
        {
            m_curFactor =1;
            m_endTime = m_curTime;
            return m_isEnd ? enState.end : enState.middle;
        }
        else if(m_state == enState.end)
        {
            float factor = m_endDuration <= 0 ? 1 : (m_curTime - m_endTime )/ m_endDuration;
            m_curFactor = Mathf.Clamp01(factor);
            return factor >= 1 ? enState.over : enState.end;
        }
        else if(m_state == enState.over)
        {
            m_curFactor = 1;
            return enState.over;
        }
        else
        {
            Debuger.LogError("未知的状态:{0}",m_state);
            return enState.over;
        }
    }

    void UpdateColor(PostEffectsBase fx)
    {
        if (m_state == enState.begin || m_state == enState.middle)
            m_fieldInfo.SetValue(fx,Color.Lerp(m_beginColor, m_endColor, m_beginCurve.Evaluate(m_curFactor)));
        else if (m_state == enState.end || m_state == enState.over)
            m_fieldInfo.SetValue(fx,Color.Lerp(m_beginColor, m_endColor, m_endCurve.Evaluate(m_curFactor)));
    }

    void UpdateFloat(PostEffectsBase fx)
    {
        if (m_state == enState.begin || m_state == enState.middle)
            m_fieldInfo.SetValue(fx, Mathf.Lerp(m_beginFloat, m_endFloat, m_beginCurve.Evaluate(m_curFactor)));
        else if (m_state == enState.end || m_state == enState.over)
            m_fieldInfo.SetValue(fx, Mathf.Lerp(m_beginFloat, m_endFloat, m_endCurve.Evaluate(m_curFactor)));
    }
    
    #endregion


}
