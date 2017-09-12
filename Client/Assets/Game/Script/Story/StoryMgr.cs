using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DynamicShadowProjector;
using WellFired;

public class MovieTalkCxt
{
    public string name;
    public string content;
    public float time;
}

public class StoryMgr : Singleton<StoryMgr>
{
    #region Fields
    bool m_isPlaying = false;
    bool m_isPlayMovieOnly = false;
    bool m_isMovieLoopEnd = true;
    int m_curIdx = -1;
    bool m_canSpeed = true;
    StoryCfg m_curCfg;          //当前正在运行的单个剧情配置
    string m_curStoryId = "";   //当前播放的剧情ID
    static List<StoryCfg> m_cfgList = new List<StoryCfg>();

    //单独保存下过场配置 需要单独播放
    static List<StoryMovieCfg> m_movieCfgList = new List<StoryMovieCfg>();

    List<UIPanel> closePanelCache = new List<UIPanel>();
    #endregion

    public bool CanSpeed { get { return m_canSpeed; } }
    public bool IsPlaying { get { return m_isPlaying; } }
    public bool IsPlayingMovie { get { return m_isPlayMovieOnly; } }

    public IEnumerator LoadMovie()
    {
        //加载当前关卡所有剧情里配置的过场动画
        SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;
        string storyId;
        List<string> loadPrefab = new List<string>();
        m_movieCfgList.Clear();
        for (int i = 0; i < sceneData.mStoryIdList.Count; i++)
        {
            storyId = sceneData.mStoryIdList[i];
            StorySaveCfg storySave = StorySaveCfg.GetCfg(storyId);
            if (storySave == null)
            {
                Debuger.Log("未找到剧情 " + storyId);
                yield break;
            }
            List<StoryCfg> cfgList = storySave.storyList;
            foreach (StoryCfg cfg in cfgList)
            {
                if (cfg.type == StoryType.STORY_MOVIE)
                {
                    StoryMovieCfg movieCfg = cfg as StoryMovieCfg;

                    //加载音效
                    if (movieCfg.soundId != 0)
                        SoundMgr.instance.PreLoad(movieCfg.soundId);

                    m_movieCfgList.Add(movieCfg);
                    
                    if (loadPrefab.Contains(movieCfg.prefab))
                    {
                        //Debuger.LogError("剧情里配置了重复的剧情内容" + movieCfg.movieId);
                        continue;
                    }

                    GameObject prefab = Resources.Load<GameObject>(movieCfg.prefab + "/" + movieCfg.prefab);

                    if (prefab == null)
                    {
                        Debug.LogError("没有找到剧情" + movieCfg.prefab);
                    }
                    else
                    {
                        GameObject ob = Object.Instantiate(prefab);
                        ob.gameObject.SetActive(false);
                        ob.transform.SetParent(Room.instance.transform);

                        //看品质开启关闭相机bloom和角色高光
                        QualityMgr.instance.CheckGameObject(ob);


                        CharacterController[] characterControllers = ob.GetComponentsInChildren<CharacterController>(true);
                        foreach (CharacterController cc in characterControllers)
                            cc.enabled = false;

                        loadPrefab.Add(movieCfg.prefab);
                    }

                }

                if (cfg.type == StoryType.STORY_TALK)
                {
                    StoryTalkCfg talkCfg = cfg as StoryTalkCfg;
                    if (talkCfg.soundId != 0)
                        SoundMgr.instance.PreLoad(talkCfg.soundId);
                }
                yield return 1;
            }

        }

        yield return 0;
    }

    //播放剧情
    public void PlayStory(string storyId)
    {
       
        if (m_isPlaying)
        {
            Debuger.LogError("当前有未播放完的剧情ID {0}, 传入了剧情ID {1}", m_curStoryId, storyId);
            return;
        }
        
        StorySaveCfg saveCfg = StorySaveCfg.GetCfg(storyId);
        if (saveCfg == null)
        {
            Debuger.LogError("未找到剧情id对应配置 id : {0}", storyId);
            return;
        }
        PoolMgr.instance.GCCollect();//垃圾回收下
        m_curStoryId = storyId;
        m_cfgList = saveCfg.storyList;
        m_isPlaying = true;
        m_curIdx = -1;
        m_canSpeed = saveCfg.canSpeed;
        TimeMgr.instance.AddPause();

        StoryCfg cfg = GetNext();
        if (cfg != null)
            Play(cfg);
    }

    public void PlayMovie(string movieId, bool canSkip = false)
    {
        StoryMovieCfg movieCfg = null;
        foreach(StoryMovieCfg cfg in m_movieCfgList)
        {
            if (cfg.movieId == movieId)
                movieCfg = cfg;
        }
        if (movieCfg == null)
        {
            Debuger.LogError("没有找到预加载的过场 " + movieId);
            return;
        }
        m_curCfg = movieCfg;
        TimeMgr.instance.AddPause();
        m_isPlayMovieOnly = true;
        PlayMovie(movieCfg, canSkip);

        if (canSkip && UIMgr.instance.Get<UIMovie>().IsOpen)
            UIMgr.instance.Close<UIMovie>();
    }

    public void StopCurMovie()
    {
        if (m_curCfg != null && m_curCfg.type == StoryType.STORY_MOVIE)
        {
            string movieId = (m_curCfg as StoryMovieCfg).movieId;
            StopMovie(movieId);
        }
    }

    public void StopMovie(string movieId)
    {
        StoryMovieCfg movieCfg = null;
        foreach (StoryMovieCfg cfg in m_movieCfgList)
        {
            if (cfg.movieId == movieId)
                movieCfg = cfg;
        }

        if (movieCfg == null)
        {
            Debuger.LogError("没有找到过场 " + movieId);
            return;
        }

        GameObject movieGo = null;
        movieGo = Room.instance.transform.Find(string.Format("{0}(Clone)/{1}", movieCfg.prefab, movieCfg.movieId)).gameObject;
        if (movieGo == null)
            return;

        m_isMovieLoopEnd = false;
        movieGo.gameObject.SetActive(true);
        USSequencer us = movieGo.GetComponentInChildren<USSequencer>();
        if (us != null)
            us.SetPlaybackTime(1000);   //用us.Stop()接口有问题 这里直接设置一个大的时间来结束
    }

    void Play(StoryCfg cfg)
    {
        if (cfg == null)
        {
            EndStory();
            return;
        }

        if (!IsPlaying)
            return;

        switch (cfg.type)
        {
            case StoryType.STORY_TALK:
                Room.instance.StartCoroutine(CoPlayTalk((StoryTalkCfg)cfg));
                break;
            case StoryType.STORY_MOVIE:
                PlayMovie((StoryMovieCfg)cfg);
                break;
            case StoryType.STORY_POP:
                break;
        }
    }

    public void EndStory()
    {
        
        if (!m_isPlaying)
        {
            return;
        }

        if (UIMgr.instance.Get<UIMovie>().IsOpen)
            UIMgr.instance.Close<UIMovie>();

        TimeMgr.instance.SubPause();
        m_isPlaying = false;
        TimeMgr.instance.AddTimeScale(1, -1);
        PoolMgr.instance.GCCollect();//垃圾回收下
    }

    #region PlayTalk
    IEnumerator CoPlayTalk(StoryTalkCfg cfg)
    {
        //等一帧关闭UILevel 因为之前可能有暂停逻辑的出场正在播放 暂停逻辑会关掉暂停界面 如果关掉暂停界面在关UILevel时就不能记录 打开时也就没了暂停界面
        yield return 1;
        UIMgr.instance.Close<UILevel>();
        UITalk talkUI = UIMgr.instance.Open<UITalk>(cfg);

        if (cfg.soundId != 0)
            SoundMgr.instance.Play2DSound(Sound2DType.other, cfg.soundId);

        while (!talkUI.IsEnd())
            yield return 0;

        if (UIMgr.instance.Get<UITalk>().IsOpen)
            UIMgr.instance.Close<UITalk>();

        UIMgr.instance.Open<UILevel>(true);

        if (m_curCfg != null)
            Play(m_curCfg);

        yield return 0;
    }

    #endregion

    #region PlayPop
    IEnumerator CoPlayPop(StoryPopCfg cfg)
    {
        StoryCfg nextCfg = GetNext();
        if (nextCfg == null)
            yield return 0;

        if (nextCfg.type != StoryType.STORY_POP)
            Play(nextCfg);

        yield return 0;
    }

    #endregion

    #region PlayMovie
    void PlayMovie(StoryMovieCfg cfg, bool canSkip = false)
    {
        
        if (!m_movieCfgList.Contains(cfg))
        {
            if (IsPlaying)
            {
                Debug.LogError("预置体没有绑定USSequencer");
                StoryCfg c = GetNext();
                if (c != null)
                    Play(c);
                return;
            }
        }

        PoolMgr.instance.GCCollect();//垃圾回收下
        Room.instance.transform.Find(string.Format("{0}(Clone)", cfg.prefab)).gameObject.SetActive(true);

        GameObject movieGo = null;
        Transform tf = Room.instance.transform.Find(string.Format("{0}(Clone)/{1}", cfg.prefab, cfg.movieId));
        if (tf == null)
        {
            Debug.LogError("没有找到预加载的过场动画" + cfg.movieId);
            return;
        }
        movieGo = tf.gameObject;

        //把要播放的显示 
        USSequencer[] usArray = tf.parent.GetComponentsInChildren<USSequencer>();
        string prefab = cfg.prefab + "(Clone)";
        for (int i = 0; i < usArray.Length; i++)
        {
            if (usArray[i].gameObject.name == prefab || usArray[i].gameObject.name == cfg.movieId)
                usArray[i].gameObject.SetActive(true);
            else
                usArray[i].gameObject.SetActive(false);
        }
        
        //隐藏相机
        CameraMgr.instance.CurCamera.gameObject.SetActive(false);

        //隐藏角色
        RoleMgr.instance.ShowAllRole(false);

        //隐藏特效
        FxDestroy.Clear();

        movieGo.gameObject.SetActive(true);
        USSequencer us = movieGo.GetComponentInChildren<USSequencer>();
        if (us == null)
        {
            Debug.LogError("没有绑定USSequencer" + cfg.movieId);
            return;
        }


        //将打开的界面先关闭并存下来
        closePanelCache.Clear();
        if (UIMgr.instance.Get<UILevel>().gameObject.activeSelf)
            closePanelCache.Add(UIMgr.instance.Get<UILevel>());
        if (UIMgr.instance.Get<UIMainCity>().gameObject.activeSelf)
            closePanelCache.Add(UIMgr.instance.Get<UIMainCity>());
        foreach (UIPanel panel in closePanelCache)
            panel.Close();

        //过场背景音 =0 不改变背景音 <0 停掉背景音 >0 切换背景音
        if (cfg.soundId != 0)
        {
            SoundMgr.instance.Pause2DSound(Sound2DType.bgm, true);
            SoundMgr.instance.Play2DSound(Sound2DType.other, cfg.soundId);
        }

        if (cfg.soundId < 0)
            SoundMgr.instance.Pause2DSound(Sound2DType.bgm, true);

        if (!canSkip)
        UIMgr.instance.Open<UIMovie>();

        m_isMovieLoopEnd = cfg.isLoop;

        us.PlaybackFinished = null;
        us.PlaybackStopped = null;
        us.Stop();
        us.gameObject.SetActive(true);
        us.IsLopping = cfg.isLoop;
        us.IsPingPonging = false;
        us.PlaybackFinished = OnPlayMovieEnd;
        us.PlaybackStopped = OnPlayMovieEnd;
        us.Play();
        us.SetPlaybackTime(0.033f);

        return;
    }

    void OnPlayMovieEnd(USSequencer sequencer)
    {
        
        //在循环播放 不做处理
        if (m_isMovieLoopEnd)
            return;
        PoolMgr.instance.GCCollect();//垃圾回收下

        //先把音效释放 不然隐藏时音效又回收会出问题
        AniFxMgr[] fxMgrs = sequencer.transform.parent.GetComponentsInChildren<AniFxMgr>(true);
        foreach (AniFxMgr fx in fxMgrs)
            fx.DestroyFx();

        sequencer.gameObject.SetActive(false);
        sequencer.transform.parent.gameObject.SetActive(false);

        //先不销毁 有些地方会重复播放剧情
        //GameObject.Destroy(sequencer.transform.parent.gameObject);
        //m_objDict.Remove(m_curStoryId);

        if (UIMgr.instance.Get<UIMovie>().IsOpen)
            UIMgr.instance.Close<UIMovie>();

        //开启之前隐藏的界面
        foreach (UIPanel panel in closePanelCache)
            panel.Open(true);
        closePanelCache.Clear();
        //恢复背景音乐
        SoundMgr.instance.Pause2DSound(Sound2DType.bgm, false);

        //可能回调回来之后已经点了调过剧情了
        if (m_isPlaying)
        {
            StoryCfg nextCfg = GetNext();
            if (nextCfg != null)
                Play(nextCfg);
        }

        if (m_isPlayMovieOnly)
        {
            TimeMgr.instance.SubPause();
            m_isPlayMovieOnly = false;
        }

        //延迟打开相机 否则会在主角出生前有一帧没有相机照的对象
        UIMgr.instance.UICameraHight.gameObject.SetActive(true);
        if (CameraMgr.instance.CurCamera != null)
            CameraMgr.instance.CurCamera.gameObject.SetActive(true);

        //隐藏角色
        RoleMgr.instance.ShowAllRole(true);
        TimeMgr.instance.AddTimeScale(1, -1);
    }
    
    #endregion

    public void ShowMovieTalk(MovieTalkCxt cxt)
    {
        if (UIMgr.instance == null)
            return;

        UIMovie uiMovie = UIMgr.instance.Get<UIMovie>();
        if (uiMovie == null)
            return;

        if (uiMovie.gameObject.activeSelf)
            uiMovie.UpdateTalk(cxt);
        else
        {
            uiMovie.Open(null);
            uiMovie.UpdateTalk(cxt);
        }
    }

    public void SkipStory()
    {
        EndStory();

        if (m_curCfg.type == StoryType.STORY_MOVIE)
        {
            StoryMovieCfg cfg = m_curCfg as StoryMovieCfg;
            GameObject movieGo = null;
            movieGo = Room.instance.transform.Find(string.Format("{0}(Clone)/{1}", cfg.prefab, cfg.movieId)).gameObject;
            if (movieGo == null)
                return;

            movieGo.gameObject.SetActive(true);
            USSequencer us = movieGo.GetComponentInChildren<USSequencer>();
            OnPlayMovieEnd(us);
            //if (us != null)
            //    us.SetPlaybackTime(1000);   //用us.Stop()接口有问题 这里直接设置一个大的时间来结束
        }
        else if (m_curCfg.type == StoryType.STORY_TALK)
        {
        }

    }

    public StoryCfg GetNext()
    {
        m_curIdx++;
        if (m_curIdx >= m_cfgList.Count)
        {
            EndStory();
            m_curCfg = null;
            return null;
        }
        m_isPlaying = true;
        m_curCfg = m_cfgList[m_curIdx];
        return m_curCfg;
    }

    public void OnExit()
    {
        if (m_isPlaying)
            EndStory();

        GameObject movieGo = null;

        foreach (StoryMovieCfg cfg in m_movieCfgList)
        {
            movieGo = Room.instance.transform.Find(string.Format("{0}(Clone)", cfg.prefab)).gameObject;
            if (movieGo != null)
                GameObject.Destroy(movieGo);
        }

        m_movieCfgList.Clear();
    }
}
