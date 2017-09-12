using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class GrowAct
{
    public abstract void Start(GrowFxMgr mgr, float time);
    public virtual void Stop(GrowFxMgr mgr) { }
    public virtual float GetActTime() { return 0f; }
}

public class GrowActBatch
{
    public List<GrowAct> m_acts = new List<GrowAct>();
    private float m_time;
    public void Start(GrowFxMgr mgr)
    {
        foreach(GrowAct act in m_acts)
        {
            act.Start(mgr, m_time);
        }
    }

    public float GetTime()
    {
        float time = m_time;
        foreach (GrowAct act in m_acts)
        {
            float actTime = act.GetActTime();
            if(actTime>time)
            {
                time = actTime;
            }
        }
        return time;
    }

    public void Stop(GrowFxMgr mgr)
    {
        foreach (GrowAct act in m_acts)
        {
            act.Stop(mgr);
        }
    }

    public GrowActBatch(List<GrowAct> acts, float time)
    {
        m_acts = acts;
        m_time = time;
    }
}

public class GrowActAttrTextShake : GrowAct
{
    public override void Start(GrowFxMgr mgr, float time)
    {
        foreach(SimpleHandle handle in mgr.m_attrAnis)
        {
            handle.m_handle.Start();
        }
    }


}

public class GrowActFxPlay : GrowAct
{
    private GameObject m_fx;
    private bool m_stopRemove;
    public override void Start(GrowFxMgr mgr, float time)
    {
        m_fx.SetActive(false);
        m_fx.SetActive(true);
    }

    public GrowActFxPlay(GameObject fx, bool stopRemove=false)
    {
        m_fx = fx;
        m_stopRemove = stopRemove;
    }

    public override void Stop(GrowFxMgr mgr)
    {
        if (m_stopRemove)
        m_fx.SetActive(false);
    }
}

public class GrowActFx : GrowAct
{
    private string m_fx;
    private Vector3 m_pos = Vector3.zero;
    public override void Start(GrowFxMgr mgr, float time)
    {
        UIFxPanel.ShowFx(m_fx, m_pos);
    }

    public GrowActFx(string fx)
    {
        m_fx = fx;
    }

    public GrowActFx(string fx, Vector3 pos)
    {
        m_fx = fx;
        m_pos = pos;
    }
}

public class GrowActFunc : GrowAct
{
    private Action m_func;
    public override void Start(GrowFxMgr mgr, float time)
    {
        m_func();
    }

    public GrowActFunc(Action func)
    {
        m_func = func;
    }

}

public class GrowActGrowUI : GrowAct
{
    private bool m_isRouse;
    public override void Start(GrowFxMgr mgr, float time)
    {
        UIMgr.instance.Open<UIEquipGrow>(new UIEquipGrowParam(m_isRouse, mgr.m_oldEquipData, mgr.m_newEquipData));
    }

    public GrowActGrowUI(bool isRouse)
    {
        m_isRouse = isRouse;
    }
}

/*
public class GrowActEquipChangeFloat : GrowAct
{
    static PropertyTable prop1 = new PropertyTable();
    static PropertyTable prop2 = new PropertyTable();
    public override void Start(GrowFxMgr mgr, float time)
    {
        List<string> attrList = new List<string>();
        mgr.m_oldEquipData.GetBaseProp(prop1);
        mgr.m_newEquipData.GetBaseProp(prop2);
        for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
        {
            float difVal = prop2.GetFloat(i) - prop1.GetFloat(i);
            if(difVal>Mathf.Epsilon)
            {
                PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
                string addStr = propTypeCfg.name + "+";
                if (propTypeCfg.format == enPropFormat.FloatRate)
                {
                    addStr += String.Format("{0:F}", difVal * 100) + "%";
                }
                else
                {
                    addStr += Mathf.RoundToInt(difVal);
                }
                attrList.Add(addStr);
            }
        }
        UITxtFloatPanel.ShowFloatTexts(attrList);

    }
}
 * */

public class GrowActFlyIcon : GrowAct
{
    public override void Start(GrowFxMgr mgr, float time)
    {
        foreach (ImageEx iconSource in mgr.m_flyIconSources)
        {
            mgr.m_flyIconFx.StartFly(iconSource, time);
        }
    }


}

public class GrowActPlaySound : GrowAct
{
    private int m_soundId;
    public override void Start(GrowFxMgr mgr, float time)
    {
        SoundMgr.instance.Play2DSound(Sound2DType.ui, m_soundId);
    }

    public GrowActPlaySound(int soundId)
    {
        m_soundId = soundId;
    }
} 

public class GrowActStarPlay : GrowAct
{

    public override void Start(GrowFxMgr mgr, float time)
    {
        mgr.m_stars[mgr.m_newStar-1].SetActive(false);
        mgr.m_stars[mgr.m_newStar-1].SetActive(true);
    }

    public GrowActStarPlay()
    {

    }

}

public class GrowActPowerUp : GrowAct
{
    private bool m_showAttr;
    private float m_defaultTime;
    private bool m_showSuccess;
    public override void Start(GrowFxMgr mgr, float time)
    {
        m_showSuccess = UIPowerUp.ShowPowerUp(m_showAttr);
    }

    public GrowActPowerUp(bool showAttr, float defaultTime)
    {
        m_showAttr = showAttr;
        m_defaultTime = defaultTime;
    }

    public override float GetActTime()
    {
        if(m_showSuccess)
        {
            return m_defaultTime;
        }
        else
        {
            return 0;
        }
    }
}



public class GrowFxMgr
{
    public List<SimpleHandle> m_attrAnis = new List<SimpleHandle>();
    public Equip m_oldEquipData;
    public Equip m_newEquipData;
    public List<ImageEx> m_flyIconSources = new List<ImageEx>();
    public FlyIconFx m_flyIconFx;
    public List<GameObject> m_stars;
    public int m_newStar;

    public List<GrowActBatch> m_actList = new List<GrowActBatch>();
    public Action m_callback;
    private int m_actIndex;
    private TimeMgr.Timer m_timer;

    public void Start()
    {
        Stop();
        m_actIndex = 0;
        StartBatch();
    }

    void Stop()
    {
        if( m_timer != null)
        {
            TimeMgr.instance.RemoveTimer(m_timer);
        }
    }

    void StartBatch()
    {
        if (m_actIndex>0)
        {
            m_actList[m_actIndex - 1].Stop(this);
        }
        if(m_actIndex >= m_actList.Count)
        {
            if (m_callback!=null)
            {
                m_callback();
            }
            m_timer = null;
            return;
        }
        m_actList[m_actIndex].Start(this);
        m_actIndex++;
        /*
        if (m_actIndex >= m_actList.Count)
        {
            if (m_callback!= null)
            {
                m_callback();
            }
            m_timer = null;
            return;
        }*/
        m_timer = TimeMgr.instance.AddTimer(m_actList[m_actIndex - 1].GetTime(), StartBatch);
    }
}