using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum Sound2DType
{
    bgm,  //背景音乐
    ui,       //ui
    other,  //其他
    other2 //其他
}

/// <summary>
/// 音效管理器
/// </summary>
public class SoundMgr : SingletonMonoBehaviour<SoundMgr>
{
    public class SoundRequest
    {
        public int m_id;
        public ResourceRequest m_req;
        public SoundRequest(SoundCfg cfg)
        {
            m_id = cfg.soundId;
            m_req = Resources.LoadAsync<AudioClip>(cfg.path);
        }
    }
    #region Fields
    //3d音效的prefab
    const string Fx_3d = "fx_sound_3d";
    //最大2的音效数量
    const int Max_2DSound = 4;
    //3D声音最大个数 初始4个，游戏过程中可能会修改最大个数，所以不用const
    int Max_3d_Num = 4;  
    //修正update检测频率
    float m_updateFrame;
    //播放中的声音
    LinkedList<SoundFx3D> m_playing3dFxs = new LinkedList<SoundFx3D>(); 
    //已经加载好的音效
    Dictionary<int, AudioClip> m_clips = new Dictionary<int, AudioClip>();
    //正在加载的音效
    Dictionary<int, SoundRequest> m_preLoadingClips = new Dictionary<int, SoundRequest>();
    //临时存储待删除的队列
    List<SoundRequest> m_removeList = new List<SoundRequest>();
    //临时存储等待放回池中的队列
    List<SoundFx3D> m_temFx3DRemoves = new List<SoundFx3D>();
    //除了背景音乐外的音效静音
    bool m_muteSound = false;
    //背景音乐静音
    bool m_muteBGM = false;
    //背景音乐音量
    float m_bgmVol=1;
    //音效音量，这里是除了背景音乐的所有音效，包括2D3D
    float m_soundVol = 1;
    //存储2D音效的列表
    List<AudioSource> m_2dSources = new List<AudioSource>();
#endregion
   
#region Properties
    public int RequestCount { get { return m_preLoadingClips.Count; } }
    public bool IsDone { get { return m_preLoadingClips.Count == 0; } }
    public bool muteSound { get { return m_muteSound; }set { m_muteSound = value; } }
    public bool muteBGM { get { return m_muteBGM; } }
    public float bgmVol { get { return m_bgmVol; } }
    public float soundVol { get { return m_soundVol; } }
    #endregion

    #region Mono Frame
    void Update()
    {
        m_updateFrame += TimeMgr.instance.realDelta;
        if (m_updateFrame >= 0.5f)
        {
            CheckPlaying();
            CheckRequest();
            m_updateFrame = 0;
        }
    }
#endregion

#region Private Methods

    //从已加载列表中获取AudioClip，没有的话再立刻加载
    AudioClip GetClip(SoundCfg soundCfg, bool checkPreload = true)
    {
        AudioClip clip = null;
        clip = m_clips.Get(soundCfg.soundId);
        if(clip == null)
        {
            if (checkPreload)
                Debug.LogError("音效：音效没有预加载，id为：" + soundCfg.soundId);
            clip = Resources.Load<AudioClip>( soundCfg.path);
            if (clip == null)
            {
                Debuger.LogError("音效：加载不成功{0}", soundCfg.soundId);
                return null;
            }
            m_clips.Add(soundCfg.soundId, clip);  //存进预加载里
        }
        return clip;
    }

    //检查声音播放完就放回池中
    void CheckPlaying()
    {
        SoundFx3D fx3d;
        LinkedListNode<SoundFx3D> cur = null;
        LinkedListNode<SoundFx3D> next = m_playing3dFxs.First;
        while (next != null)
        {
            cur = next;
            fx3d = cur.Value;
            next = cur.Next;
            if (fx3d.m_source.isPlaying == false)
                m_temFx3DRemoves.Add(fx3d);
        }
        
        if (m_temFx3DRemoves.Count != 0)
        {
            for (int i = 0, len = m_temFx3DRemoves.Count; i < len; i++)
            {
                Stop(m_temFx3DRemoves[i]);
            }
            m_temFx3DRemoves.Clear();
        }
    }
    //检查加载请求
    void CheckRequest()
    {
        AudioClip clip;
        foreach(SoundRequest req in m_preLoadingClips.Values)
        {
            if (!req.m_req.isDone)
                continue;

            clip = req.m_req.asset as AudioClip;
            if (clip == null)
            {
                Debuger.LogError("音效：预加载的时候传进来无效的音效id 加载不成功{0}", req.m_id);
                m_removeList.Add(req);  //加载不成功的也移除掉
                continue;
            }
            if (!m_clips.ContainsKey(req.m_id))
                m_clips.Add(req.m_id, clip);
            m_removeList.Add(req);//纪录记载好的索引
        }
        for (int i = 0, len = m_removeList.Count; i < len; i++)
            m_preLoadingClips.Remove(m_removeList[i].m_id);

        m_removeList.Clear();
    }
    //获取2d音效配置
    SoundCfg Get2DSoundCfg(int id)
    {
        CheckInit();
        SoundCfg cfg = SoundCfg.Get(id);
        if (cfg == null)
        {
            Debuger.LogError("音效：读取的音效配置为null id:{0}", id);
            return null;
        }
        return cfg;
    }

    AudioSource Get2DSound(Sound2DType type)
    {
        return m_2dSources[(int)type];
    }

#endregion
    public void Init()
    {
        if (m_2dSources.Count > 0)
        {
            Debuger.LogError("逻辑错误，soundMgr重复初始化");
            return;
        }
        for(int i = 0; i< Max_2DSound;++i)
        {
            m_2dSources.Add(this.gameObject.AddComponent<AudioSource>());
        }
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(Fx_3d, false);
    }

    public void CheckInit()
    {
        if (m_2dSources.Count != 0)
            return;
        Init();
    }

    //预加载音效
    public void PreLoad(int id)
    {
        if (m_clips.ContainsKey(id)|| m_preLoadingClips.ContainsKey(id))
            return;

        SoundCfg cfg = SoundCfg.Get(id);
        if (cfg == null)
            return;
        SoundRequest sReq = new SoundRequest( cfg);
        m_preLoadingClips.Add(id, sReq);   //记录请求id

    }

    //清空所有加载的音效
    public void Clear()
    {
        for(int i =0;i<Max_2DSound;++i)
        {
            AudioSource audio = m_2dSources[i];
            if (audio.isPlaying)
                audio.Stop();
            audio = null;
        }
        m_2dSources.Clear();
        m_clips.Clear();
    }

    public void RemovePreLoadClip(int id)
    {
        if (m_clips.ContainsKey(id))
            m_clips.Remove(id);
    }
    //播放2D音效
    public void Play2DSound(Sound2DType type, int id)
    {
        if (type== Sound2DType.bgm && m_muteBGM)  //静音不播放
            return;
        if (type != Sound2DType.bgm && m_muteSound)  //静音不播放
            return;
        SoundCfg cfg = Get2DSoundCfg(id);
        if (cfg != null)
        {
            AudioSource audio = Get2DSound(type);
            audio.clip = GetClip(cfg);
            audio.loop = cfg.needLoop;
            audio.Play();
        }
    }

    public void Play2DSoundAutoChannel(int id)
    {
        SoundCfg cfg = Get2DSoundCfg(id);
        if (cfg != null)
        {
            AudioSource audio = Get2DSound(Sound2DType.other);
            if(audio.isPlaying)
            {
                audio = Get2DSound(Sound2DType.other2);
            }
            if (audio.isPlaying)
            {
                audio = Get2DSound(Sound2DType.ui);
            }
            audio.clip = GetClip(cfg);
            audio.loop = cfg.needLoop;
            audio.Play();
        }
    }

    public void MuteBGM(bool mute)
    {
        AudioSource audio = Get2DSound(Sound2DType.bgm);
        if (audio != null && audio.clip != null)
        {
            audio.mute = mute;
            m_muteBGM = mute;
        }
    }

    public void Pause2DSound(Sound2DType type, bool pause)
    {
        AudioSource audio = Get2DSound(type);
        if (audio == null || audio.clip == null)
            return;
        if (pause)
            audio.Pause();
        else
            audio.UnPause();
    }
  
    public void Stop2DSound(Sound2DType type)
    {
        AudioSource audio = Get2DSound(type);
        if (audio != null && audio.clip != null)
            audio.Stop();
    }
    
    //播放3D声音
    public SoundFx3D Play3DSound(int id, Transform t)
    {
        return Play3DSound(id, t, Vector3.zero);
    }
    //播放3D声音
    public SoundFx3D Play3DSound(int id, Transform t, Vector3 offset)
    {
        CheckInit();
        SoundCfg soundCfg = SoundCfg.Get(id);
        if (soundCfg == null)
        {
            Debuger.LogError("音效：读取的音效配置为null id:{0}", id);
            return null;
        }
        if (m_playing3dFxs.Count+1 >= Max_3d_Num)//最大同时播放3D音效个数限制]
        {
            if (soundCfg.unMaxLimit == 0)
                return null;
            else  //像主角技能这些无限制的可以继续加，同时修正最大个数限制
                Max_3d_Num++;
        }

        GameObject go =GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(Fx_3d);
        Transform sT = go.transform;
        SoundFx3D info = go.GetComponent<SoundFx3D>();

        //设置AudioSource
        AudioSource source = info.m_source;
        source.loop = soundCfg.needLoop ;
        source.clip = GetClip(soundCfg);
        source.mute = m_muteSound;  //设置是否静音
        source.volume = m_soundVol;   //设置音量
        //source.minDistance = Sound3d_MinDistance;  //最小衰减系数  暂时不用
        //source.maxDistance = Sound3d_MaxDistance;  //最大衰减系数  暂时不用

        if (t == null)
            sT.position = offset;
        else
        {
            sT.SetParent(t, false);
            sT.localPosition = offset;
            sT.localEulerAngles = Vector3.zero;
        }
        source.Play();

        m_playing3dFxs.AddLast(info);//添加到播放中的列表
        return info;

    }
    //调整2D音效音量
    public void Vol2DSound(Sound2DType type, float vol)
    {
        AudioSource audio = Get2DSound(type);
        if (audio != null)  //判空一下
            audio.volume = vol;
    }

    //调整背景音乐音量
    public void SetBgmVolumn(float vol)
    {
        m_bgmVol = vol;

        bool isMute = m_bgmVol < 0.01f;
        if (isMute!= this.muteBGM)//应该关的时候关掉，以提升性能
            MuteBGM(isMute);

        Vol2DSound(Sound2DType.bgm, m_bgmVol);

    }
    //调整其他音效音量
    public void SetSoundVolumn(float vol)
    {
        m_soundVol = vol;

        bool isMute = m_soundVol < 0.01f;
        if (isMute != this.muteSound)//应该关的时候关掉，以提升性能
            this.muteSound =isMute;


        Vol2DSound(Sound2DType.ui, m_soundVol);
        Vol2DSound(Sound2DType.other, m_soundVol);
        Vol2DSound(Sound2DType.other2, m_soundVol);
        //3D的音效在每次Play的时候调整音量
    }

    //停止所有声音
    public void StopAllSounds()
    {
        if(m_2dSources.Count > 0)
        {
            for (int i = 0; i < Max_2DSound; ++i)
            {
                AudioSource audio = m_2dSources[i];
                if (audio != null)
                    audio.Stop();
            }
        }
     
        SoundFx3D info;
        while (m_playing3dFxs.Count > 0)
        {
            info = m_playing3dFxs.First.Value;
            Stop(info);
        }
    }

    public void Stop(SoundFx3D info)
    {
        LinkedListNode<SoundFx3D> node =m_playing3dFxs.Find(info);
        if (node == null)
            return;
        info.m_source.Stop();
        info.m_source.clip = null;
        m_playing3dFxs.Remove(node);

        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(info.gameObject);
    }

}
