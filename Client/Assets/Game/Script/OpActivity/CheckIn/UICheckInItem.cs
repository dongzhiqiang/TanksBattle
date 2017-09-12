using UnityEngine;
using System.Collections;

public class UICheckInItem : MonoBehaviour {

    public GameObject kuang;
    public GameObject zhezhao;
    public GameObject gou;
    public GameObject vip;
    public GameObject fx;
    public TextEx itemNumsText;
    public ImageEx image;
    public ImageEx bg;
    private int itemId;
    private int itemNums;
    private int vipLevel;
    public  CheckInRewardCfg checkInRewardCfg;
    public ItemCfg itemCfg;
    public enum CheckInItemState { HasCheckedIn,CanCheckIn,NotCheckIn};
    public CheckInItemState state;


    public void HasCheckedInInit(CheckInRewardCfg checkInRewardCfg)
    {
        kuang.SetActive(false);
        zhezhao.SetActive(true);
        gou.SetActive(true);
        fx.SetActive(false);
        this.checkInRewardCfg = checkInRewardCfg;        
        LoadItem();
        state = CheckInItemState.HasCheckedIn;

    }
    public void CanCheckInInit(CheckInRewardCfg checkInRewardCfg)
    {
        kuang.SetActive(true);
        zhezhao.SetActive(false);
        gou.SetActive(false);
        fx.SetActive(true);
        this.checkInRewardCfg = checkInRewardCfg;
        LoadItem();
        state = CheckInItemState.CanCheckIn;
    }
    public void NotCheckInInit(CheckInRewardCfg checkInRewardCfg)
    {
        kuang.SetActive(false);
        zhezhao.SetActive(false);
        gou.SetActive(false);
        fx.SetActive(false);
        this.checkInRewardCfg = checkInRewardCfg;
        LoadItem();
        state = CheckInItemState.NotCheckIn;
    }

    void LoadItem()
    {
        itemId = checkInRewardCfg.itemId;
        itemNums = checkInRewardCfg.itemNums;
        vipLevel = checkInRewardCfg.vipLevel;
        itemCfg = ItemCfg.m_cfgs[itemId];
        //显示vip双倍      
        if (vipLevel != 0)
        {
            vip.GetComponentInChildren<TextEx>().text = "V"+vipLevel;
        }
        else
        {
            vip.SetActive(false);
        }

        //显示物品图标      
        string itemIconName = itemCfg.icon;
        image.GetComponent<ImageEx>().Set(itemIconName);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        bg.Set(qualityCfg.backgroundSquare);


        //显示物品数量       
        if (itemNums > 1)
        {
            itemNumsText.gameObject.SetActive(true);
            itemNumsText.GetComponent<TextEx>().text = itemNums.ToString();
        }
        else
        {
            itemNumsText.gameObject.SetActive(false);
        }
    }

}

