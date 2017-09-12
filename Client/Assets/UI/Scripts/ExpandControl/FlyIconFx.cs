using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

class FlyIcon
{
    public GameObject icon;
    public float startTime;
    public float duration;
    public Vector3 startPos;
}

public class FlyIconFx : MonoBehaviour
{
    private List<FlyIcon> m_icons = new List<FlyIcon>();
    public GameObject m_targetPos;
    private List<FlyIcon> m_toRemove = new List<FlyIcon>();

    public void StartFly(ImageEx source, float duration)
    {
        if(duration<=0)
        {
            return;
        }
        FlyIcon flyIcon = new FlyIcon();
        flyIcon.icon = GameObject.Instantiate(source.gameObject);
        flyIcon.icon.transform.SetParent(source.transform.parent, false);
        flyIcon.icon.transform.localPosition = source.transform.localPosition;
        flyIcon.icon.transform.localScale = source.transform.localScale;
        flyIcon.icon.transform.SetParent(gameObject.transform,true);
        flyIcon.icon.GetComponent<ImageEx>().overrideSprite = source.overrideSprite;
        flyIcon.duration = duration;
        flyIcon.startTime = Time.time;
        flyIcon.startPos = flyIcon.icon.transform.localPosition;
        m_icons.Add(flyIcon);
    }

    public void StartFlyImage2(ImageEx source, ImageEx source2, float duration)
    {
        if (duration <= 0)
        {
            return;
        }
        FlyIcon flyIcon = new FlyIcon();
        flyIcon.icon = GameObject.Instantiate(source.gameObject);
        flyIcon.icon.transform.SetParent(source.transform.parent, false);
        flyIcon.icon.transform.localPosition = source.transform.localPosition;
        flyIcon.icon.transform.localScale = source.transform.localScale;
        flyIcon.icon.transform.SetParent(gameObject.transform, true);
        flyIcon.icon.GetComponent<ImageEx>().overrideSprite = source.overrideSprite;
        GameObject image2 = GameObject.Instantiate(source2.gameObject);
        image2.transform.SetParent(flyIcon.icon.transform, false);
        image2.transform.localPosition = Vector3.zero;
        image2.transform.localScale = Vector3.one;
        image2.GetComponent<ImageEx>().overrideSprite = source2.overrideSprite;
        flyIcon.duration = duration;
        flyIcon.startTime = Time.time;
        flyIcon.startPos = flyIcon.icon.transform.localPosition;
        m_icons.Add(flyIcon);
    }

    void Update()
    {
        foreach(FlyIcon flyIcon in m_icons)
        {
            float factor = (Time.time - flyIcon.startTime) / flyIcon.duration;
            if(factor >= 1)
            {
                factor = 1;
                m_toRemove.Add(flyIcon);
            }
            flyIcon.icon.gameObject.transform.localScale = Vector3.one * (1 - factor);
            flyIcon.icon.gameObject.transform.localPosition = flyIcon.startPos * (1 - factor) + m_targetPos.transform.localPosition * factor;
        }

        foreach(FlyIcon flyIcon in m_toRemove)
        {
            m_icons.Remove(flyIcon);
            Destroy(flyIcon.icon);
        }
        m_toRemove.Clear();
    }
}