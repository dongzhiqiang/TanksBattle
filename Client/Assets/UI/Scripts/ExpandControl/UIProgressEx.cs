using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIProgressEx : MonoBehaviour
{
    public int mOldStage = 0;
    public int mOldScore = 15;
    public int mAdd = 60;
    public List<int> mStages = new List<int>();
    public ImageEx mProgress;
    public TextEx mAddScore;
    public TextEx mScorePercent;
    public TextEx mCurStage;//当前阶段，从1开始算
    public string mAddScoreFormat = "";
    public string mCurStageFormat = "";

    Action mOnEnd;
    Action<int, int> mOnOverlay;


    float m_value;
    public float Value
    {
        get { return m_value; }
        set
        {
            m_value = value;
            int curAdd = (int)(mAdd * value);
            int score = 0;

            if (mStages.Count == 0)
                return;

            int nextStage = 0;
            bool isFull = false;

            StageUtil.CalcStageByTotal(false, mStages, mOldStage, mOldScore, curAdd, out nextStage, out score, out isFull);
         
            if (mProgress != null)
            {
                if (mProgress.type == ImageEx.Type.Filled)
                {
                    int num = 0;
                    if (nextStage > 0)
                        num = (mStages[nextStage] - mStages[nextStage - 1]);
                    else
                        num = mStages[nextStage];

                    if (num == 0)
                        num = 1;

                    mProgress.fillAmount = (float)(score) / num;
                    
                    EditorUtil.SetDirty(mProgress);
                }
                    
                else
                    Debug.LogError("UITweenExValue 设置的UISprite mProgress类型只支持filled");
            }

            if (mAddScore != null)
            {
                if (string.IsNullOrEmpty(mAddScoreFormat))
                    mAddScore.text = curAdd.ToString();
                else
                {
                    if (mAddScoreFormat.IndexOf("{0}") != -1)
                        mAddScore.text = string.Format(mAddScoreFormat, curAdd);
                    else
                        mAddScore.text = mAddScoreFormat;
                }
            }

            if (mScorePercent != null)
            {
                mScorePercent.text = string.Format("{0}/{1}", score, mStages[nextStage]);
            }

            if (mCurStage != null)
            {
                if (string.IsNullOrEmpty(mCurStageFormat))
                    mCurStage.text = nextStage.ToString();
                else
                {
                    if (mCurStageFormat.IndexOf("{0}") != -1)
                        mCurStage.text = string.Format(mCurStageFormat, nextStage - 1);
                    else
                        mCurStage.text = mCurStageFormat;
                }
            }
        }
    }

    //结束监听
    public void AddEnd(Action a)
    {
        mOnEnd += a;
    }


    //叠加监听
    public void AddOverlay(Action<int, int> a)
    {
        mOldStage = int.MinValue;
        mOnOverlay += a;
    }

    //叠加监听
    public void SetOverlay(Action<int, int> a)
    {
        mOldStage = int.MinValue;
        mOnOverlay = a;
    }

}
