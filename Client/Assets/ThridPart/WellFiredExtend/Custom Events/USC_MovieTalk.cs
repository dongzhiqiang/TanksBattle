using UnityEngine;
using System.Collections;
using WellFired;

[USequencerFriendlyName("Movie Talk")]
[USequencerEvent("Custom/Movie Talk")]
public class USC_MovieTalk : USEventBase
{
    public string talkName;
    public string talkContent;

    public float showTime = 2.0f;

    art_movieTalk artTalk = null;

    public override void FireEvent()
    {
        if (UIMgr.instance != null)
        {
#if !ART_DEBUG
            if (StoryMgr.instance == null)
                return;

            MovieTalkCxt cxt = new MovieTalkCxt();
            cxt.name = talkName;
            cxt.content = talkContent;
            cxt.time = showTime;
            StoryMgr.instance.ShowMovieTalk(cxt);
#endif
        }
        else
        {
            if (artTalk == null)
            {
                GameObject prefab = Resources.Load<GameObject>("UIMovie");

                if (prefab == null)
                    return;

                GameObject ob = Object.Instantiate(prefab);
                artTalk = ob.GetComponent<art_movieTalk>();
                if (artTalk == null)
                    Debug.LogError("过场对话预置体中没绑定脚本 art_MovieTalk");
            }

            artTalk.gameObject.SetActive(true);
            artTalk.StartTalk(talkName, talkContent, showTime);
        }

    }

    public override void UndoEvent()
    {
        Time.timeScale = 1;
    }

    public override void EndEvent()
    {
        if (UIMgr.instance == null && artTalk != null)
            artTalk.gameObject.SetActive(false);
        UndoEvent();
    }

    public override void StopEvent()
    {
        if (UIMgr.instance == null && artTalk != null)
            artTalk.gameObject.SetActive(false);
        UndoEvent();
    }



    public override void ProcessEvent(float deltaTime)
    {
        if (UIMgr.instance == null && artTalk != null)
        {
            artTalk.UpdateTalk(talkName, talkContent, showTime);
        }
    }
}
