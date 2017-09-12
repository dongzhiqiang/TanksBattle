using UnityEngine;
using System.Collections;

public class QteEventOperate : QteEvent
{

    bool m_isReach = false;
    UIQte m_uiQte;
    int curNum = 0;

    public bool IsReach { get { return m_isReach; } set { m_isReach = value; } }

    public enQteStateType stageType = enQteStateType.QteClickRandom;
    public int totalNum = 100;
    public int addNum = 10;
    public override void Init()
    {
        m_isReach = false;

    }
    public override void Start()
    {
        if (!Application.isPlaying)
            return;


        m_uiQte = UIMgr.instance.Open<UIQte>();
        m_uiQte.OnLeftClickCallback += OnRandomClick;
        m_uiQte.OnRightClickCallback += OnRandomClick;
        m_uiQte.OnBottomClickCallback += OnContinueClick;

        int side = 0;   //左边还是右边 1是左 2是右
        if (stageType == enQteStateType.QteClickRandom)
            side = m_uiQte.ShowRandomBtn();
        else if (stageType == enQteStateType.QteClickContinue)
            m_uiQte.ShowBottomBtn();

        curNum = 0;

        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.QTE_OPERATE, stageType, side);

    }
    public override void Update(float time)
    {
        if (!Application.isPlaying)
            return;

    }
    public override void Stop()
    {
        CurQte.IsWin = IsReach;

        if (!Application.isPlaying)
            return;

        if (!IsReach)
        {
            if (m_uiQte != null && m_uiQte.IsOpen)
                m_uiQte.ShowFail();
        }
    }

    void OnRandomClick()
    {
        if (stageType == enQteStateType.QteClickRandom)
        {
            IsReach = true;
            m_uiQte.ShowWin();
        }
    }

    void OnContinueClick()
    {
        if (stageType == enQteStateType.QteClickContinue)
        {
            curNum += addNum;
            if (curNum >= totalNum)
            {
                m_uiQte.Close();
                IsReach = true;
            }
        }
    }
}
