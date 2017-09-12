using UnityEngine;
using System.Collections;

public class UIWeaponTalentItem : MonoBehaviour {

    public ImageEx icon;
    public TextEx name;
    public StateHandle btn;
    WeaponSkillTalent talent=null;

    public void Init(WeaponSkillTalent talent)
    {
        this.talent = talent;
        HeroTalentCfg cfg =talent.Cfg;
        icon.Set(cfg.icon);
        name.text = cfg.name;

        btn.AddClick(OnClick);
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIWeaponTalentUp>(talent);
    }
}
