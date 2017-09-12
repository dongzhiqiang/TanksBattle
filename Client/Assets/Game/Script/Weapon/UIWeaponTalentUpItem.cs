using UnityEngine;
using System.Collections;

public class UIWeaponTalentUpItem : MonoBehaviour {

    public ImageEx icon;
    public TextEx name;
    public TextEx lvText;
    public TextEx desc;
    

    public void Init(HeroTalentCfg cfg,int lv,bool isMax)
    {
        if(!isMax)
        {
            icon.Set(cfg.icon);
            name.text = string.Format("{0}", cfg.name);
            desc.text = LvValue.ParseText(cfg.desc, lv);
            this.lvText.text = string.Format("Lv.{0}", lv);
        }
        else
        {
            icon.Set(null);
            name.text = "等级已满";
            desc.text = "";
            this.lvText.text = "";
        }

        
    }
}
