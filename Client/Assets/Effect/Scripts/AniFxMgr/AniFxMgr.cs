#region Header
/**
 * 名称：动作绑定的特效
 
 * 日期：2015.9.29
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class AniClipCfg{
    public int frame =10;
    public float speed = 1f;

    public void CopyFrom(AniClipCfg cfg)
    {
        if (cfg == null) return;

        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance);
    }
}

//将单个动作分成多段，每一段播放速度不一样
public class AniRateCfg
{
    public List<AniClipCfg> clips = new List<AniClipCfg>();
    public float speed = 1f;

    public void CopyFrom(AniRateCfg cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance);

        //复制其他
        clips.Clear();
        AniClipCfg clip;
        foreach(AniClipCfg c2 in cfg.clips)
        {
            clip = new AniClipCfg();
            clips.Add(clip);
            clip.CopyFrom(c2);
        }
            
    }
}

public enum enAniFxElement
{
    none = 0,//无属性
    fire = 1,//火
    ice = 2,//冰
    thunder = 3,//雷
    dark = 4,//冥
    max = 4,
    runtimeClean = 5,//用于运行时区分的值
}

public class AniFxMgr : MonoBehaviour
{
    //角色都有的动作
    public const string Ani_Enter = "chuchang";
    public const string Ani_DaiJi = "zhandoudaiji";
    public const string Ani_PaoBu = "paobu";
    public const string Ani_SiWang = "siwang";

    public const string Ani_BeiJi01 = "beiji01";
    public const string Ani_BeiJi02 = "beiji02";
    public const string Ani_FuKong01 = "fukong01";
    public const string Ani_FuKong02 = "fukong02";
    public const string Ani_DaoDi= "daodi";
    public const string Ani_JiFei = "jifei";
    public const string Ani_QiShen = "qishen";

    public static string[] Element_Names = new string[] { "无", "火", "冰", "雷", "冥"};

    #region Fields
    public List<AniFxGroup> m_groups=new List<AniFxGroup>();
    public enAniFxElement m_testElement = enAniFxElement.none;
    public string m_search;

    Transform m_root;
    Animation m_ani;
    Dictionary<string, AnimationState> m_sts=new Dictionary<string,AnimationState>();
    AnimationState m_curSt;
    AniFxGroup m_curGroup;
    SimpleRole m_simpleRole;
    bool m_cache = false;
    int m_curFrame=-1;//当前循环的第几帧,根据动作的normalTime，比如从一个动作的一半开始播放，那么当前是
    int m_curLoop = 0;//第几个循环
    int m_curTotalFrame = -1;//总的在动作的第几帧,相对于开始时间，必定从0开始
    bool m_isEnd=false;//是不是播放结束了
    AniRateCfg m_defaultRateCfg = new AniRateCfg();
    AniRateCfg m_curRateCfg;
    int m_curRateIdx=-1;
    int m_curRateEndFrame=-1;
    float m_lastPauseTime;
    float m_duration;
    float m_pauseSpeed;
    enAniFxElement m_runtimeElement = enAniFxElement.runtimeClean;
#if !ART_DEBUG
    List<SoundFx3D> m_sfxStopIfEnd = new List<SoundFx3D>();
    LinkedList<AniSoundCfg> m_sounds = new LinkedList<AniSoundCfg>();
#endif
    string m_modName = "";
    #endregion


    #region Properties
    public AnimationState CurSt{get{return m_curSt;}}
    public AniFxGroup CurGroup { get{return m_curGroup;}}
    public int CurFrame { get{return m_curFrame;}}
    public int CurTotalFrame { get { return m_curTotalFrame; } }
    public enAniFxElement RuntimeElement {
        get { return m_runtimeElement == enAniFxElement.runtimeClean ? m_testElement : m_runtimeElement; }
        set { m_runtimeElement = value; }
    }
    public Dictionary<string, AnimationState> Sts { get { return m_sts; } }
    #endregion


    #region Mono Frame
    void Awake()
    {
        Cache();
    }

    void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //卡帧中那么就不用更新其他了
        if(m_lastPauseTime!=-1){
            if(m_lastPauseTime+m_duration >Time.time)
                return;

            ResetPause();
        }

        CheckRate();
        CheckFrame();
        CheckSound();
    }
    #endregion



    #region Private Methods
    void Cache()
    {
        if (m_cache) return;
        m_cache = true;
        m_root = this.transform.parent;
        m_ani = this.GetComponent<Animation>();
        m_simpleRole = m_root.GetComponent<SimpleRole>();
        if (m_ani != null)
        {
            foreach (AnimationState st in m_ani)
            {
                if (st == null)
                    continue;
                if (m_sts.ContainsKey(st.name))
                {
                    Debuger.LogError("{0}动作重复了:{1}", m_root.name, st.name);
                    continue;
                }
                m_sts[st.name] = st;
            }
        }
        else
            Debuger.LogError("角色找不到Animation");

        //动作特效预加载
        foreach (AniFxGroup g in m_groups)
        {
            g.Init(this);
        }

        //找到模型名
        m_modName = m_root.name;
        if (m_modName.EndsWith("(Clone)"))
            m_modName =m_modName.Substring(0, m_modName.Length - 7);


    }

    //计算一个动作当前在第几帧
    //timeFrame 指的是当前是第几帧，frame指的是这个循环内的第几帧
    void CalcSt(AnimationState st,out int frame,out int loop, out bool isEnd )
    {
        frame=0;
        loop=0;
        isEnd = false;
        if (st.wrapMode == WrapMode.Loop || st.wrapMode == WrapMode.PingPong || st.wrapMode == WrapMode.ClampForever)//不自动结束的循环方式的计算
        {
            loop = (int)(m_curSt.time / m_curSt.length);
            frame = (int)((m_curSt.time % m_curSt.length) / Util.One_Frame);
        }
        else
        {
            loop = 0;
            frame = (int)(Mathf.Min(m_curSt.time, m_curSt.length) / Util.One_Frame);
            isEnd = !st.enabled || st.normalizedTime > 1;//判断下结束      
        }
    }
    //检测下时间到了就更新对应的特效帧
    void CheckFrame()
    {
        if (m_curGroup == null)
        {
            //如果没有特效组，但是有动作，那么要判断是不是结束
            if (m_curSt != null && !m_curSt.enabled && !m_isEnd)
                m_isEnd =true;

            if (m_curSt!= null)
                m_curTotalFrame = (int)(m_curSt.time / Util.One_Frame);
            return;
        }

        if(m_isEnd)return;
            
        //计算当前的真实的帧
        int frame;
        int loop;
        bool isEnd;
        CalcSt(m_curSt,out frame, out loop, out  isEnd);

        //执行，确保执行且执行一次
        int maxFrame = (int)(m_curSt.length / Util.One_Frame);
        if (loop != m_curLoop || frame!= m_curFrame)
        {
            int left ;
            int right ;
            for(int i = m_curLoop;i<=loop;++i){
                left = i == m_curLoop ? m_curFrame+1 : 0;
                right = i == loop ? frame : maxFrame;
                for (int j = left; j <= right; ++j)
                {
                    m_curGroup.UpdateFrame(j, i, m_isEnd, m_root);//每一帧都会执行到
                    ++m_curTotalFrame;
                }
                    
            }
        }

        m_curLoop = loop;
        m_curFrame = frame;

        //如果结束了
        if (isEnd)
        {
            m_curGroup.End();
            m_isEnd = true;
        }
    }

    float GetRateSpeed()
    {
        if (m_curRateIdx==-1)
            return m_curRateCfg.speed;
        else
            return m_curRateCfg.clips[m_curRateIdx].speed;
    }
    //检查动作速率调整
    void CheckRate()
    {
        if (m_curSt == null ||m_curRateCfg == null || m_curRateIdx ==-1)
            return;
        int curFrame = (int)(m_curSt.time  / Util.One_Frame);
        if(m_curRateEndFrame ==-1)
            Debuger.LogError("逻辑出错 m_curRateEndFrame ==-1");
        if (curFrame >= m_curRateEndFrame)
        {
            ++m_curRateIdx;
            if(m_curRateIdx < m_curRateCfg.clips.Count){
                m_curRateEndFrame=m_curRateCfg.clips[m_curRateIdx].frame;
            }
            else
            {
                m_curRateIdx = -1;  
                m_curRateEndFrame = -1;
            }

            m_curSt.speed = GetRateSpeed();
        }
            
    }

    //检查播放声音
    void CheckSound()
    {
#if !ART_DEBUG
        if (m_sounds.Count == 0)
            return;
        AniSoundCfg cfg;
        SoundFx3D sfx;
        while (m_sounds.Count > 0 &&m_curTotalFrame >= m_sounds.First.Value.frame)
        {
            cfg = m_sounds.First.Value;

            sfx =SoundMgr.instance.Play3DSound(cfg.soundId,m_root);
            if (sfx != null && cfg.stopIfEnd)
                m_sfxStopIfEnd.Add(sfx);

            m_sounds.RemoveFirst();
        }
#endif
    }
    #endregion

    public AniFxGroup GetGroup(string ani)
    {
        for (int i = 0; i < m_groups.Count; ++i)
        {
            if(m_groups[i].name == ani)
                return m_groups[i];
        }
        return null;
    }
    public void Play(string ani, WrapMode wrapMode, float fade = 0.2f, float speed = 1f)
    {
        m_defaultRateCfg.speed = speed;
        
        Play(ani, wrapMode, m_defaultRateCfg,fade);
    }

    
    public void Play(string ani, WrapMode wrapMode, AniRateCfg rateCfg,float fade = 0.2f)
    {
        Cache();

        //先做些音效的处理
#if !ART_DEBUG
        StopSound();
        m_sounds.Clear();
        AniSoundCfg.Get(m_modName,ani, ref m_sounds);
#endif
        ResetPause();//上一个动作如果卡帧中那么清空

        m_curRateCfg = rateCfg;
        m_curRateIdx = -1;
        m_curRateEndFrame = -1;
        

        //如果当前动作没有结束要先结束
        if (m_curGroup != null && !m_isEnd)
        {
            m_curGroup.End();
            m_isEnd = true;
        }

        //播放
        m_curSt = m_sts.Get(ani);
        if (m_curSt != null)
        {
            if (m_curRateCfg.clips.Count>0)
            {
                m_curRateIdx =0;
                m_curRateEndFrame = m_curRateCfg.clips[0].frame;
            }
            m_curSt.speed = GetRateSpeed();
            m_curSt.normalizedTime = 0;
            m_curSt.wrapMode = wrapMode;
            
            if (fade == 0 || (m_simpleRole != null && !m_simpleRole.m_needFade))
                m_ani.Play(ani);
            else
                m_ani.CrossFade(ani, fade);
        }
        else
        {
            m_ani.Stop();
            Debuger.LogError("{0}找不到动作{1}",this.transform.parent.name, ani);
        }

        //当前动作的处理
        m_curGroup = m_curSt == null ? null : GetGroup(ani);
        m_curTotalFrame = 0;
        if (m_curGroup != null)
        {
            CalcSt(m_curSt, out m_curFrame, out m_curLoop, out  m_isEnd);

            m_curGroup.Begin();
            m_curGroup.UpdateFrame(m_curFrame, m_curLoop, false, m_root);
            if (m_isEnd)
            {
                m_curGroup.End();
            }
        }

        //检查播放声音
        CheckSound();
    }


    public void ResetPause()
    {
        if (m_lastPauseTime == -1)
            return;

        //时间到，取消卡帧
        m_lastPauseTime = -1;
        m_duration = -1;
        if (m_curSt != null)
            m_curSt.speed = m_pauseSpeed;
    }
    
    //卡帧，注意切换到下一个动作的话卡帧就无效了
    public void AddPause(float duration)
    {
        if (m_lastPauseTime != -1)
        {
            m_duration += duration;
        }
        else
        {
            m_lastPauseTime =Time.time;
            m_duration = duration;
            if (m_curSt != null)
            {
                m_pauseSpeed = m_curSt.speed;
                m_curSt.speed =0;
            }
            else
                m_pauseSpeed= 1;
            

        }
    }

    public void DestroyFx()
    {
        for (int i = 0; i < m_groups.Count; ++i)
        {
            m_groups[i].Destroy(false);
            
        }
        StopSound();
    }
    
    public void StopSound()
    {
#if !ART_DEBUG
        for (int i=0;i<m_sfxStopIfEnd.Count;++i)
        {
            SoundMgr.instance.Stop(m_sfxStopIfEnd[i]);
        }
        m_sfxStopIfEnd.Clear();
#endif
    }
    public AnimationState GetSt(string name)
    {
        Cache();
        return m_sts.Get(name);
    }
}
