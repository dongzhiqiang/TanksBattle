using UnityEngine;
using System.Collections;

public class UIQte : UIPanel
{
    #region Fields
    public const int BUFFID_REMOVE_BIG_QTE = 602;
    #endregion

    #region Fields
    public GameObject mLeft;
    public GameObject mRight;
    public GameObject mBottom;

    public StateHandle mLeftBtn;
    public StateHandle mRightBtn;
    public StateHandle mBottomBtn;

    public GameObject mLeftWinFx;
    public GameObject mLeftLoseFx;
    public GameObject mRightWinFx;
    public GameObject mRightLoseFx;
    #endregion

    #region Properties

    public System.Action OnLeftClickCallback;
    public System.Action OnRightClickCallback;
    public System.Action OnBottomClickCallback;

    #endregion

    #region Frame
    //首次初始化的时候调用
    public override void OnInitPanel()
    {
        mLeftBtn.AddClick(OnLeftClick);
        mRightBtn.AddClick(OnRightClick);
        mBottomBtn.AddClick(OnBottomClick);
        EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.BUFF_ADD, OnAddBuf);
    }

    //显示
    public override void OnOpenPanel(object param)
    {
        mLeft.SetActive(false);
        mRight.SetActive(false);
        mBottom.SetActive(false);
    }


    #endregion

    public int ShowRandomBtn()
    {
        mBottom.SetActive(false);
        mLeft.SetActive(false);
        mRight.SetActive(false);
        int i = Random.Range(1, 3);
        if (i == 1)
        {
            mLeft.SetActive(true);
            mLeftBtn.gameObject.SetActive(true);
            TeachMgr.instance.SetNextStepUIObjParam(mLeftBtn.transform as RectTransform);
        }
        else
        {
            mRight.SetActive(true);
            mRightBtn.gameObject.SetActive(true);
            TeachMgr.instance.SetNextStepUIObjParam(mRightBtn.transform as RectTransform);
        }
        TeachMgr.instance.OnDirectTeachEvent("combat", "bigQTEBtnShow");
        return i;
    }
    public void ShowBottomBtn()
    {
        mLeft.SetActive(false);
        mRight.SetActive(false);

        mBottom.SetActive(true);
        mBottomBtn.gameObject.SetActive(true);

        TeachMgr.instance.SetNextStepUIObjParam(mBottomBtn.transform as RectTransform);
        TeachMgr.instance.OnDirectTeachEvent("combat", "bigQTEBtnShow");
    }

    public void ShowFail()
    {
        if (mLeft.activeSelf)
        {
            mLeftBtn.gameObject.SetActive(false);
            mLeftLoseFx.gameObject.SetActive(true);
        }

        if (mRight.activeSelf)
        {
            mRightBtn.gameObject.SetActive(false);
            mRightLoseFx.gameObject.SetActive(true);
        }

        StartCoroutine(CloseWnd());
    }

    public void ShowWin()
    {
        if (mLeft.activeSelf)
        {
            mLeftBtn.gameObject.SetActive(false);
            mLeftWinFx.gameObject.SetActive(true);
        }

        if (mRight.activeSelf)
        {
            mRightBtn.gameObject.SetActive(false);
            mRightWinFx.gameObject.SetActive(true);
        }
        StartCoroutine(CloseWnd());
    }


    #region Private Methods
    void OnLeftClick()
    {
        if (OnLeftClickCallback != null)
            OnLeftClickCallback();
    }

    void OnRightClick()
    {
        if (OnRightClickCallback != null)
            OnRightClickCallback();
    }

    void OnBottomClick()
    {
        if (OnBottomClickCallback != null)
            OnBottomClickCallback();
    }

    IEnumerator CloseWnd()
    {
        yield return new WaitForSeconds(0.5f);
        mLeftWinFx.gameObject.SetActive(false);
        mLeftLoseFx.gameObject.SetActive(false);
        mRightWinFx.gameObject.SetActive(false);
        mRightLoseFx.gameObject.SetActive(false);

        mLeft.SetActive(false);
        mRight.SetActive(false);
        mBottom.SetActive(false);

        OnLeftClickCallback = null;
        OnRightClickCallback = null;
        OnBottomClickCallback = null;

        UIMgr.instance.Close<UIQte>();
        yield return 0;
    }

    void OnAddBuf(object param1, object param2, object param3)
    {
        BuffCfg cfg = (BuffCfg)param1;
        if (cfg.id == BUFFID_REMOVE_BIG_QTE)
        {
            TeachMgr.instance.OnDirectTeachEvent("combat","bigQTE");
        }
    }
    #endregion

}