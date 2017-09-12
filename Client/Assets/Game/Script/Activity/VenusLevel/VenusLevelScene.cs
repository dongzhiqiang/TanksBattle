using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//体力活动场景
public class VenusLevelScene : LevelBase
{
    bool m_playingStartMovie = false;
    bool m_playingShakeMovie = false;
    Dictionary<int, GameObject> m_evalEffects = new Dictionary<int,GameObject>();
    GameObject m_water;
    int m_evaluate = (int)enLevelEval.b;
    //UILevel m_uiLevel;
    //UILevelAreaReward m_reward;

    //能不能加非战斗状态
    public override bool CanAddUnaliveBuff { get { return true; } }

    //场景切换完成
    public override void OnLoadFinish()
    {
        m_evalEffects[2] = GameObject.Find("fx_tili_water01");
        m_evalEffects[3] = GameObject.Find("fx_tili_water02");
        m_evalEffects[4] = GameObject.Find("fx_tili_water03");
        m_evalEffects[5] = GameObject.Find("fx_tili_water04");
        m_water = GameObject.Find("fx_tili_water");
        foreach(GameObject effect in m_evalEffects.Values)
        {
            if (effect != null)
                effect.SetActive(false);
        }
        m_water.SetActive(false);

        m_evaluate = (int)enLevelEval.b; ;
        m_playingShakeMovie = false;
        m_playingStartMovie = true;
        StoryMgr.instance.PlayMovie("tili_huodong");
        
    }

    public override void OnUpdate() 
    {
        if (!StoryMgr.instance.IsPlayingMovie && m_playingStartMovie) // 动画播完
        {
            m_playingStartMovie = false;
            UIMgr.instance.Open<UIGainStamina>(this);
            UIMgr.instance.Get<UIGainStamina>().StartPlay();
            //m_uiLevel = UIMgr.instance.Open<UILevel>();
            //m_reward = m_uiLevel.Open<UILevelAreaReward>(false);
            //m_reward.SetShowFlag(false, false, false);
            //StoryMgr.instance.PlayMovie("03", true);
            //StoryMgr.instance.PlayMovie("tili_jiemian", true);
            m_playingShakeMovie = true;
        }
        if (!StoryMgr.instance.IsPlayingMovie && m_playingShakeMovie) // 动画播完
        {
            //StoryMgr.instance.PlayMovie("tili_jiemian", true);
            //m_playingShakeMovie = false;
            PlayCameraShake(m_evaluate);
        }
    }

    public void PlayCameraShake(int evaluate)
    {
        /*修改实现方式..
        if (StoryMgr.instance.IsPlayingMovie && m_playingShakeMovie)
        {
            //StoryMgr.instance.StopMovie(playingMovie);
            //return;
            StoryMgr.instance.StopCurMovie();
        }*/
        if (StoryMgr.instance.IsPlayingMovie)
        {
            return;
        }
        if(evaluate >= (int)enLevelEval.ss)
        {
            StoryMgr.instance.PlayMovie("03", true);
            //m_playingShakeMovie = true;
        }
        else if (evaluate >= (int)enLevelEval.s)
        {
            StoryMgr.instance.PlayMovie("02", true);
            //m_playingShakeMovie = true;
        }
        else if (evaluate >= (int)enLevelEval.a)
        {
            StoryMgr.instance.PlayMovie("01", true);
            //m_playingShakeMovie = true;
        }
        
    }

    public void PlayEffect(int evaluate)
    {
        if(evaluate > 5)
        {
            evaluate = 5;
        }

        if (m_evalEffects.ContainsKey(evaluate) && m_evalEffects[evaluate] != null)
        {
            m_evalEffects[evaluate].SetActive(false);
            m_evalEffects[evaluate].SetActive(true);
        }

        m_evaluate = evaluate;
        PlayCameraShake(evaluate);
    }

    public void Result(int evaluate, float percentage)
    {
        m_playingShakeMovie = false;
        StoryMgr.instance.StopCurMovie();
        m_water.SetActive(true);
        TimeMgr.instance.AddTimer(3, () =>
        {
            NetMgr.instance.ActivityHandler.SendEndVenusLevel(evaluate, percentage);
        });
    }

    public override void OnExit()
    {
        //UIMgr.instance.Get<UIGainStamina>().Close();
    }

}
