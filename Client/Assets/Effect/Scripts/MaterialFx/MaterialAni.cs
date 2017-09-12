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

[System.Serializable]
public class MaterialAni
{
    public enum enType
    {
        Color,
        Float,
        ScrollingUV,
        TileTexture,
    }
    public static string[] TypeName = new string[] { "颜色", "浮点值", "uv滚动", "贴图Tile" };

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
    public string m_propertyName = "";//如果不填则动态识别

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

    //uv滚动
    public Vector2 m_uvSpeed = new Vector2(0.5f,0.5f);
        
    //tile变化
    public float m_onceDuration=1;
    public int m_columns = 2;                        
    public int m_rows = 2;

    enState m_state;
    string m_findPropertyName;
    bool m_isEnd;
    float m_endTime;
    float m_lastTime;
    float m_curTime;
    int m_lastLoop;
    int m_curLoop;
    float m_curFactor;
    Vector2 m_uvOffset ;
    #endregion


    #region Properties
    public float EndDuration { get{return  m_type == enType.ScrollingUV || m_type == enType.TileTexture?0:m_endDuration;}}
    #endregion


    #region Constructors
    
    #endregion


    #region frame
    public void OnBegin(Material m)
    {
        m_state = enState.init;
        m_lastTime = -1;
        m_lastLoop = -1;
        m_uvOffset = Vector2.zero;
        m_isEnd = false;
        FindPropertyName(m);
        OnUpdateMaterial(m, 0);
    }

    public void OnUpdateMaterial(Material m, float time)
    {
        m_curTime = time;
        m_curLoop = m_onceDuration <= 0 ? m_curLoop + 1 : Mathf.CeilToInt(time / m_onceDuration);
        enState state = CalcState();
        if (!string.IsNullOrEmpty(m_findPropertyName))//找不到属性不需要变化
        {
            //如果uv滚动
            if (m_type == enType.ScrollingUV)
            {
                UpdateScrollingUV(m);

            }
            //如果是纹理的Tile渐变
            else if (m_type == enType.ScrollingUV)
            {
                UpdateTileTexture(m);
            }
            //带渐变的效果
            else
            {
                if (state != m_state || (m_state != enState.middle && m_state != enState.over))//有些状态不需要变化
                {
                    switch (m_type)
                    {
                        case enType.Color: UpdateColor(m); break;//如果是颜色渐变
                        case enType.Float: UpdateFloat(m); break;//如果是float值渐变
                        default: Debuger.LogError("未知的类型{0}", m_type); break;
                    }
                }
            }

        }
          
        m_state = state;
        m_lastTime = m_curTime;
        m_lastLoop = m_curLoop;
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
    void FindPropertyName(Material m)
    {
        if (!string.IsNullOrEmpty(m_propertyName) && m.HasProperty(m_propertyName))
                m_findPropertyName = m_propertyName;
        else if(m_type== enType.Color){
            if (m.HasProperty("_TintColor"))
                m_findPropertyName = "_TintColor";
            else if (m.HasProperty("_Color"))
                m_findPropertyName = "_Color";
            else if (m.HasProperty("_MainColor"))
                m_findPropertyName = "_MainColor";
        }
        else if (m_type == enType.Float)
        {
            if (m.HasProperty("_Strength"))
                m_findPropertyName = "_Strength";
            else if (m.HasProperty("_Factor"))
                m_findPropertyName = "_Factor";
        }
        else if (m_type == enType.TileTexture || m_type == enType.ScrollingUV)
        {
            if (m.HasProperty("_MainTex"))
                m_findPropertyName = "_MainTex";
            else if (m.HasProperty("_MainTexAlpha"))
                m_findPropertyName = "_MainTexAlpha";
        }

        if (string.IsNullOrEmpty(m_findPropertyName))
        {
            Debuger.LogError("找不到材质对应的属性名 ", m_propertyName);
        }
    }

    void UpdateColor(Material m)
    {
        if (m_state == enState.begin || m_state == enState.middle)
            m.SetColor(m_findPropertyName, Color.Lerp(m_beginColor, m_endColor, m_beginCurve.Evaluate(m_curFactor)));
        else if (m_state == enState.end || m_state == enState.over)
            m.SetColor(m_findPropertyName, Color.Lerp(m_beginColor, m_endColor, m_endCurve.Evaluate(m_curFactor)));
    }

    void UpdateFloat(Material m)
    {
        if (m_state == enState.begin || m_state == enState.middle)
            m.SetFloat(m_findPropertyName, Mathf.Lerp(m_beginFloat, m_endFloat, m_beginCurve.Evaluate(m_curFactor)));
        else if (m_state == enState.end || m_state == enState.over)
            m.SetFloat(m_findPropertyName, Mathf.Lerp(m_beginFloat, m_endFloat, m_endCurve.Evaluate(m_curFactor)));
    }
    void UpdateScrollingUV(Material m)
    {
        m_uvOffset += m_uvSpeed * (m_lastTime < 0 ? m_curTime : m_curTime - m_lastTime);
        if (m_uvOffset.x > 1.0f || m_uvOffset.x < -1.0f)
            m_uvOffset.x = 0;

        if (m_uvOffset.y > 1.0f || m_uvOffset.y < -1.0f)
            m_uvOffset.y = 0;
        m.SetTextureOffset(m_findPropertyName, m_uvOffset );
    }
    void UpdateTileTexture(Material m)
    {
        if(m_lastLoop == m_curLoop)
            return;

        int loop =m_curLoop%(m_columns *m_rows);

        //split into x and y indexes. calculate the new offsets
        Vector2 offset = new Vector2((float)loop / m_columns - (loop / m_columns), //x index
                                      1 - ((loop / m_columns) / (float)m_rows));    //y index

        Debuger.Log(offset.ToString());
        // Reset the y offset, if needed
        if (offset.y == 1)
            offset.y = 0.0f;

        Vector2 _textureSize = new Vector2(1f / m_columns, 1f / m_rows);
        // If we have scaled the texture, we need to reposition the texture to the center of the object
        offset.x += ((1f / m_columns) - _textureSize.x) / 2.0f;
        offset.y += ((1f / m_rows) - _textureSize.y) / 2.0f;


        // Update the material
        m.SetTextureScale(m_findPropertyName, _textureSize);
        m.SetTextureOffset(m_findPropertyName, offset);
        
    }
    #endregion


}
