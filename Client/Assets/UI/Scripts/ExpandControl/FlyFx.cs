using UnityEngine;
using System;
using System.Collections;

//让一个ui对象从一个点创建出来，并飞到目标点
public class FlyFx: MonoBehaviour
{

    public static Vector3 Invalid_Pos = Vector3.forward * 10000;

    public class Cxt
    {
        public Vector3 posFrom;
        public Vector3 posDown;
        public float fromWaitTime;//开始位置上停留的时间
        
        public float stage_time_1;
        public float stage_time_2;
        
        public float stage_speed_1;
        public float stage_speed_2;

        public float stage_speed_y1;
        public float stage_speed_y2;

        public Action onEnd;
        
        public AnimationCurve stage_curve_1;
        public AnimationCurve stage_curve_2;

        public Vector3 dir;
#if !ART_DEBUG
        public Role roleTo;
#endif
        
    }

    Cxt mCxt;
    float mBeginTime;
    Transform mTran;
    bool mIsEnd = true;
    int stage = 1;
    Vector3 posTo;
    Quaternion mDir = Quaternion.identity;//方向，这里不能用transform的方向


    public void Init(Cxt cxt)
    {
        

        this.gameObject.SetActive(false);//如果不先隐藏，粒子的拖尾会有残影
        mTran = this.transform;
        mCxt = cxt;
        mBeginTime = Time.unscaledTime;
        SetEnd(false);

        //设置初始方向
        mTran.eulerAngles = mCxt.dir;

        //计算初始距离
        Vector3  posFrom = mCxt.posFrom;

        mTran.position = mCxt.posFrom;

        stage = 1;
        
        LateUpdate();
        this.gameObject.SetActive(true);
    }


    public void SetEnd(bool end)
    {
        mIsEnd = end;
        if(mIsEnd && mCxt!=null)
        { 
            mBeginTime = 0;
            if (mCxt.onEnd != null)
                mCxt.onEnd();
            mCxt = null;
            FxDestroy.DoDestroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (!mIsEnd)
            SetEnd(true);
    }
    void OnDisable()
    {
        if (!mIsEnd)
            SetEnd(true);
    }

    void LateUpdate()
    {
        if (mIsEnd)
        {
            if (gameObject.activeSelf)
                FxDestroy.DoDestroy(gameObject);
            return;
        }

        Vector3 pos;
        Vector3 posNow = mTran.position;

#if !ART_DEBUG
        if (mCxt.roleTo == null)
        {
            SetEnd(true);
            return;
        }
        if (RoleMgr.instance.Hero != null && RoleMgr.instance.Hero.TranPart != null)
            posTo = RoleMgr.instance.Hero.TranPart.GetYOff(0.5f);
        else
        {
            SetEnd(true);
            return;
        }
        if (!UIMgr.instance.Get<UILevel>().gameObject.activeSelf)
        {
            SetEnd(true);
            return;
        }

#endif
        

        //停留
        if (Time.unscaledTime - mBeginTime < mCxt.fromWaitTime && stage == 1)
        {
            pos = mCxt.posFrom;
            if (pos == Invalid_Pos)
            {
                SetEnd(true);
                return;
            }
            //mTran.position = pos + new Vector3(mCxt.fromOffsetX, mCxt.fromOffsetHigh, mCxt.fromOffsetY);
            mTran.position = pos;
            return;
        }

        //飞向目标
        pos = posTo;
        if (pos == Invalid_Pos)
        {
            SetEnd(true);
            return;
        }


        float disHero = Vector3.Distance(posTo, posNow);
        if (disHero < 0.5f && stage == 3)//达到目标
        {
            mTran.position = pos;
            SetEnd(true);
            return;
        }


        //路程比率
        //float factor = 1f-Mathf.Clamp01(dis / mBeginDis);
        
        ////转向，前进
        //if(mCxt.turnCurve != null)
        //    mDir = Quaternion.Slerp(mDir, Quaternion.LookRotation(pos - posNow), Time.deltaTime * mCxt.turnSpeed * mCxt.turnCurve.Evaluate(factor));
        //else
        //    mDir = Quaternion.Slerp(mDir, Quaternion.LookRotation(pos - posNow), Time.deltaTime * mCxt.turnSpeed);

        //if(mCxt.speedCurve != null)
        //    mTran.position += mDir * Vector3.forward * move * mCxt.speedCurve.Evaluate(factor);
        //else
        //    mTran.position += mDir *Vector3.forward* move;

        //第一阶段 落地
        if (stage == 1)
        {
            float factor = (Time.unscaledTime - mBeginTime )/ mCxt.stage_time_1;
            mTran.position += mTran.forward * mCxt.stage_speed_1* Time.deltaTime;

            //y轴抛物线
            mTran.position = new Vector3(mTran.position.x, mTran.position.y + mCxt.stage_speed_y1 * mCxt.stage_curve_1.Evaluate(Mathf.Clamp01(factor)) * Time.deltaTime, mTran.position.z);

            if (mTran.position.y < mCxt.posDown.y)//达到目标
            {
                stage = 3;
                mBeginTime = Time.unscaledTime;
            }

        }  
       
        //第三阶段 飞向玩家
        if (stage == 3)
        {
#if !ART_DEBUG
            if (mCxt.roleTo != null)
            {
                Vector3 dir = mCxt.roleTo.TranPart.GetYOff(0.5f) - mTran.position;
                mTran.forward = dir;
                mTran.position += mTran.forward * mCxt.stage_speed_2 * Time.deltaTime;

                float factor = (Time.unscaledTime - mBeginTime) / mCxt.stage_time_2;
                mTran.position = new Vector3(mTran.position.x, mTran.position.y + (mCxt.stage_speed_y2 * mCxt.stage_curve_2.Evaluate(Mathf.Clamp01(factor)) * Time.deltaTime), mTran.position.z);
            }
            else
            {
                SetEnd(true);
            }
#endif
        }

    }

}
