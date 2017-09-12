using UnityEngine;
using System.Collections;

public class UIStrongerAbilityItem : MonoBehaviour {
    public ImageEx icon;
    public TextEx name;
    public TextEx description;
    public ImageEx progress;
    public TextEx progressText;
    public StateHandle GoBtn;
    StrongerDetailCfg cfg;

    public void Init(StrongerDetailCfg cfg)
    {
        this.cfg = cfg;
        name.text = cfg.name;
        icon.Set(cfg.icon);
        int progressNum = StrongerMgr.instance.GetStrongerProgress(cfg.type);
        progress.fillAmount = (float)progressNum / 100f;
        progressText.text = progressNum + "%";
        description.text = StrongerProgressCfg.GetTextByProgress(progressNum);
        GoBtn.AddClick(OnGoBtn);
    }

    void OnGoBtn()
    {
        StrongerMgr.instance.GoStronger(cfg.type);
    }
}
