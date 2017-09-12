using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelAreaArenaItem : MonoBehaviour {

    public GameObject heroFrame;
    public GameObject petFrame;
    public GameObject cha;
    public ImageEx icon;
    public ImageEx progress;
    public GameObject progressGo;
    public StateHandle btn;
    Role role;
    List<int> obs = new List<int>();


    public void Init(Role role)
    {               

        if(btn!=null)
        {
            btn.AddClick(OnClick);
        }
        this.role = role;
        if(role.GetInt(enProp.heroId)==0)
        {
            heroFrame.SetActive(false);
            petFrame.SetActive(true);
        }
        else
        {
            heroFrame.SetActive(true);
            petFrame.SetActive(false);
        }
        icon.Set(role.Cfg.icon);
        icon.SetGrey(false);
        progressGo.SetActive(true);
        progress.fillAmount = 1;
        cha.SetActive(false);

        var ob = role.AddPropChange(enProp.hp, OnHp);
        obs.Add(ob);
        ob = role.AddPropChange(enProp.hpMax, OnHp);
        obs.Add(ob);

        role.Add(MSG_ROLE.DEAD, OnDead);
    }

    public void Clear()
    {
        foreach (var ob in obs)
        {
            EventMgr.Remove(ob);
        }
        progress.fillAmount = 1;
        cha.SetActive(false);
    }
    void OnHp()
    {
        if (role == null)
            return;

        int hp = role.GetInt(enProp.hp);
        float hpMax = Mathf.Max(1.0f, role.GetFloat(enProp.hpMax));
        progress.fillAmount = Mathf.Clamp01((float)hp / hpMax);
    }

    void OnDead()
    {
        cha.SetActive(true);
        icon.SetGrey(true);
        progressGo.SetActive(false);
    }

    void OnClick()
    {
        if (role != null && role.State == Role.enState.alive)
            CameraMgr.instance.SetFollow(role.transform);
    }
}


