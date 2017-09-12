using UnityEngine;
using System.Collections;

public class UIMovie : UIPanel
{
    #region Fields
    public TextEx mTalkName;
    public TextEx mTalkContent;
    public GameObject mTalk;
    public StateHandle mSkip;

    float startTime;
    MovieTalkCxt m_Cxt;

    public static string HeroName = "[主角]";

    #endregion

    #region Frame
    public override void OnInitPanel()
    {
        mTalk.gameObject.SetActive(false);

        mSkip.AddClick(OnSkip);
    }

    public override void OnOpenPanel(object param)
    {
        mTalk.gameObject.SetActive(false);
        StartCoroutine(HideHightCamera());
    }

    public override void OnUpdatePanel()
    {
        if (m_Cxt != null)
        {
            if ((TimeMgr.instance.realTime - startTime) > m_Cxt.time)
            {
                mTalk.gameObject.SetActive(false);

                m_Cxt = null;
            }
        }

        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            OnSkip();
        }
    }

    #endregion

    #region Public
    public void UpdateTalk(MovieTalkCxt cxt)
    {
        mTalk.gameObject.SetActive(true);

        m_Cxt = cxt;

        mTalkName.text = m_Cxt.name;
        mTalkContent.text = m_Cxt.content;
        if (RoleMgr.instance.Hero != null)
        {
            mTalkName.text = m_Cxt.name.Replace(HeroName, RoleMgr.instance.Hero.GetString(enProp.name));
            mTalkContent.text = m_Cxt.content.Replace(HeroName, RoleMgr.instance.Hero.GetString(enProp.name));
        }
        startTime = TimeMgr.instance.realTime;
    }

    #endregion

    #region Private
    void OnSkip()
    {
        StoryMgr.instance.SkipStory();
    }

    IEnumerator HideHightCamera()
    {
        yield return 1;

        UIMgr.instance.UICameraHight.gameObject.SetActive(false);

        yield return 0;
    }
    #endregion
}
