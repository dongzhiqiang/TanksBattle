#region Header
/**
 * 名称: 游戏对象池中的游戏对象
 
 * 日期：2015.10.9
 * 描述：
 *      1.维护了m_isInPool,多处对一个游戏对象放回池中的处理时会报错
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameObjectPoolObject : MonoBehaviour
{
    #region Fields
    public System.Func<bool> m_onIngoreDestroy;//无视销毁操作的接口，用于某些特殊的特效做延迟销毁
    bool m_isInPool;
#if !ART_DEBUG
    SoundFx3D m_soundStopIfEnd;
    FxSoundCfg m_soundCfg;
#endif

    #endregion


    #region Properties
    public bool IsInPool {
        get { return m_isInPool;}
    }
#if !ART_DEBUG
    public SoundFx3D SoundStopIfEnd
    {
        get { return m_soundStopIfEnd; }
        set { m_soundStopIfEnd = value; }
    }
#endif
    #endregion

    #region Static Methods

    #endregion


    #region Mono Frame
    void OnEnable()
    {

    }

    void OnDiable()
    {

    }
    #endregion
   
    #region frame
    public void OnInit()
    {
        m_isInPool = false;
#if !ART_DEBUG
        m_soundCfg= FxSoundCfg.Get(this.gameObject.name);
#endif
    }

    public void OnPut()
    {
        if (m_isInPool) 
            Debuger.LogError("已经在对象池中");//检错下
        m_isInPool = true;


        //停止对应的音效
#if !ART_DEBUG
        if (m_soundStopIfEnd != null)
        {
            SoundFx3D sfx = m_soundStopIfEnd;
            m_soundStopIfEnd = null;
            SoundMgr.instance.Stop(sfx);
        }
#endif
    }

    public void OnGet()
    {
        if (!m_isInPool) 
            Debuger.LogError("不在在对象池中");//检错下
        m_isInPool = false;
#if !ART_DEBUG
        if (m_soundCfg!=null)
        {
            //检错下
            if (m_soundStopIfEnd != null)
            {
                Debuger.LogError("逻辑错误，特效绑定的音效没有清空:{0}", gameObject.name);
                SoundMgr.instance.Stop(m_soundStopIfEnd);
                m_soundStopIfEnd = null;
            }

            SoundFx3D sfx = SoundMgr.instance.Play3DSound(m_soundCfg.soundId, this.transform);
            if (sfx != null && m_soundCfg.stopIfEnd)
                m_soundStopIfEnd = sfx;
        }
#endif
    }

#endregion

#region Private Methods
    
#endregion
    
}
