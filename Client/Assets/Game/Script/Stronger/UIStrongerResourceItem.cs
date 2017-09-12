using UnityEngine;
using System.Collections;

public class UIStrongerResourceItem : MonoBehaviour {

    public ImageEx icon;
    public TextEx name;
    public TextEx description;
    public StateGroup starGroup;
    public StateHandle GoBtn;
    StrongerDetailCfg cfg;

    public void Init(StrongerDetailCfg cfg)
    {
        this.cfg = cfg;
        name.text = cfg.name;
        icon.Set(cfg.icon);
        description.text = cfg.description;
        starGroup.SetCount(cfg.star);
        GoBtn.AddClick(OnGoBtn);
    }

    void OnGoBtn()
    {
        StrongerMgr.instance.GoStronger(cfg.type);
    }
}

