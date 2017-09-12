using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIRewardItem : MonoBehaviour {
    public ImageEx icon;
    public ImageEx quality;
    public TextEx nums;
    public TextEx name;

    public void init(CheckInRewardCfg checkInRewardCfg)
    {
        ItemCfg itemCfg = ItemCfg.m_cfgs[checkInRewardCfg.itemId];

        icon.Set(itemCfg.icon);
        nums.text = checkInRewardCfg.itemNums.ToString();
        name.text = itemCfg.name;

    }

    public void init(int itemId,int num)
    {
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        icon.Set(itemCfg.icon);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        quality.Set(qualityCfg.backgroundSquare);
        nums.text = num.ToString();
        name.text = itemCfg.name;
        if(num>1)
        {
            nums.gameObject.SetActive(true);
        }
        else
        {
            nums.gameObject.SetActive(false);
        }

    }


}
