using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UILevelRewardItem : MonoBehaviour {
    public  TextEx description;
    public TextEx progress;
    public StateGroup itemsGroup;
    public GameObject hasGetReward;
    public StateHandle canGetReward;
    public StateHandle cantGetReward;
    private LevelRewardCfg levelRewardCfg;
    public void Init(LevelRewardCfg levelRewardCfg)
    {
        this.levelRewardCfg = levelRewardCfg;
        Role hero = RoleMgr.instance.Hero;
        OpActivityPart opActivityPart = hero.OpActivityPart;
        int currentLevel = hero.GetInt(enProp.level);

        description.text = levelRewardCfg.description;
        if (currentLevel > levelRewardCfg.level)
            currentLevel = levelRewardCfg.level;
        progress.text = "进度：" + currentLevel.ToString()  + "/" + levelRewardCfg.level;

        List<int> itemIds = levelRewardCfg.GetItemIdList();
        List<int> itemNums = levelRewardCfg.GetItemNumList();

        itemsGroup.SetCount(itemIds.Count);
        for(int i=0;i<itemsGroup.Count;++i)
        {
            UIItemIcon item = itemsGroup.Get<UIItemIcon>(i);
            item.Init(itemIds[i], itemNums[i]);
        }
        canGetReward.AddClick(OnCanGetReward);
        cantGetReward.AddClick(OnCantGetReward);



        SetLevelState(opActivityPart.CanGetLevelReward(levelRewardCfg.id));
    }

    void SetLevelState(enRewardState levelState)
    {
        switch(levelState)
        {
            case (enRewardState.canGetReward):
                canGetReward.gameObject.SetActive(true);
                cantGetReward.gameObject.SetActive(false);
                hasGetReward.SetActive(false);
                break;
            case (enRewardState.hasGetReward):
                canGetReward.gameObject.SetActive(false);
                cantGetReward.gameObject.SetActive(false);
                hasGetReward.SetActive(true);
                break;
            case (enRewardState.cantGetReward):
                canGetReward.gameObject.SetActive(false);
                cantGetReward.gameObject.SetActive(true);
                hasGetReward.SetActive(false);
                break;
        }
    }

    void OnCanGetReward()
    {
        NetMgr.instance.OpActivityHandler.SendGetLevelReward(levelRewardCfg.id);
    }

    void OnCantGetReward()
    {
        UIMessage.Show("达到" + levelRewardCfg.level + "级才可领取");
    }
}

