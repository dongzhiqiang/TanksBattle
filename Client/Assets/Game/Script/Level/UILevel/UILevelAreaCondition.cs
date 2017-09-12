using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelAreaCondition : UILevelArea
{

    #region Fields
    public List<ImageEx> mStars;
    public List<TextEx> mTips;
    public StateHandle mOpenBtn;

    bool isOperate = false;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.condition; } }
    public override bool IsOpenOnStart { get { return false; } }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        isOperate = false;
        mOpenBtn.AddClick(OnOpenBtn);
    }

    void OnOpenBtn()
    {
        isOperate = true;
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {

    }

    protected override void OnUpdateArea()
    {
        List<SceneTrigger> triList = SceneEventMgr.instance.conditionTriggerList;
        if (triList != null && triList.Count <= 0)
        {
            if (UIMgr.instance.Get<UILevel>().IsOpen)
                UIMgr.instance.Get<UILevel>().Close<UILevelAreaCondition>();
        }
        else
        {
            for (int i = 0; i < triList.Count; i++)
            {
                mTips[i].text = triList[i].GetDesc();
                mStars[i].gameObject.SetActive(triList[i].bReach());
            }
        }

    }

    //关闭
    protected override void OnCloseArea()
    {

    }

    protected override void OnRoleBorn()
    {
        mOpenBtn.SetState(1);
        Room.instance.StartCoroutine(hideTips());
    }

    #endregion

    #region Private Methods

    IEnumerator hideTips()
    {
        yield return new WaitForSeconds(5);
        if (!isOperate)
            mOpenBtn.SetState(0);
    }
    #endregion


}