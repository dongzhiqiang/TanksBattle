using UnityEngine;
using System.Collections;

public class UIOpActivityItem : MonoBehaviour {
    public ImageEx icon;
    public TextEx name;
    public GameObject tip;
    public string opActivityName;
    public int id;

    public void init(OpActivitiySortCfg opActivitySortCfg)
    {
        name.text = opActivitySortCfg.opActivityName;
        opActivityName= opActivitySortCfg.opActivityName;
        id = opActivitySortCfg.id;
        icon.Set(opActivitySortCfg.icon);
        UpdateTip();
    }
    public void UpdateTip()
    {
        Role hero = RoleMgr.instance.Hero;
        OpActivityPart opActivityPart = hero.OpActivityPart;
        switch (id)
        {
            case (0)://每日签到
                {
                    if(!TimeMgr.instance.IsToday(opActivityPart.GetInt(enOpActProp.lastCheckIn)))
                    {
                        tip.SetActive(true);
                    }
                    else
                    {
                        tip.SetActive(false);
                    }
                    break;
                }
            case (1):
                {
                    if(opActivityPart.CanGetLevelReward())
                    {
                        tip.SetActive(true);
                    }
                    else
                    {
                        tip.SetActive(false);
                    }
                    break;
                }
        }
    }
    public void SetTip(bool set)
    {
        tip.SetActive(set);
    }
}
