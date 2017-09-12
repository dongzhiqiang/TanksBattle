using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIStrongerBtnItem : MonoBehaviour {

    public TextEx btnName;
    public StrongerBasicCfg cfg;
	public void Init(StrongerBasicCfg cfg)
    {
        this.cfg = cfg;
        btnName.text = cfg.name;
    }
}
