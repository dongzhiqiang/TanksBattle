using UnityEngine;
using System.Collections;

public class art_movieTalk : MonoBehaviour {

    public TextEx mTalkName;
    public TextEx mTalkContent;

    float startTime;

    string m_talkName;
    string m_talkContent;
    float m_time;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (UIMgr.instance == null)
        {
            if (Time.realtimeSinceStartup - startTime > m_time)
            {
                this.gameObject.SetActive(false);
                mTalkName.gameObject.SetActive(false);
                mTalkContent.gameObject.SetActive(false);
            }
        }
	}

    public void StartTalk(string talkName, string talkContent, float time)
    {
        mTalkName.gameObject.SetActive(true);
        mTalkContent.gameObject.SetActive(true);
        m_talkName = talkName;
        m_talkContent = talkContent;

        mTalkName.text = m_talkName;
        mTalkContent.text = m_talkContent;
        m_time = time;

        startTime = Time.realtimeSinceStartup;
    }

    public void UpdateTalk(string talkName, string talkContent, float time)
    {
        m_talkName = talkName;
        m_talkContent = talkContent;

        mTalkName.text = m_talkName;
        mTalkContent.text = m_talkContent;
        if (time >= m_time)
        {
            this.gameObject.SetActive(false);
        }
    }

}
