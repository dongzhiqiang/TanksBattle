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



public class UILevelAreaWave : UILevelArea
{
    
    #region Fields
    public TextEx m_wave;
    public StateHandle m_num;
    public TextEx m_desc;

    public float startTime;
    public float duration;
    public float endTime;

    int m_obExit;
    int m_maxWave = 0;
    int m_curWave = 0;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.wave; } }
    public override bool IsOpenOnStart { get{return false;} }
    public int CurWave { get { return m_curWave;  } }

    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {

    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        m_obExit = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, OnSceneExit);
        if (!reopen)
            m_curWave = 0;
    }

    protected override void OnUpdateArea()
    {

    }

    //关闭
    protected override void OnCloseArea()
    {
        if (m_obExit != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obExit);
            m_obExit = EventMgr.Invalid_Id;
        }
    }

    protected override void OnRoleBorn()
    {
       
    }



    public void SetWave(int wave, int maxWave)
    {
        m_wave.text = string.Format(m_curWave + "/" + maxWave);
        StartCoroutine(CoUpdateWave(wave, maxWave));
    }

    public void SetDesc(string desc)
    {
        m_desc.text = desc;
    }

    public void AddWave()
    {
        m_curWave++;
        if (m_curWave > m_maxWave)
        {
            Debuger.LogError("波数超过最大波");
            return;
        }
        //m_wave.text = string.Format(m_curWave + "/" + m_maxWave);
        StartCoroutine(CoUpdateWave(m_curWave, m_maxWave));
    }

    public void Clear()
    {

    }

    #endregion

    #region Private Methods


    void OnSceneExit()
    {
        Clear();
    }

    IEnumerator CoUpdateWave(int wave, int maxWave)
    {

        m_maxWave = maxWave;
        m_curWave = wave;
        if (wave  > 0)
        {
            m_num.SetState(0);
            yield return new WaitForSeconds(startTime);
        }
        
        m_wave.text = string.Format(wave + "/" + maxWave);
        yield return new WaitForSeconds(duration);
        m_num.SetState(1);

        yield return 0;
    }

    #endregion


}
