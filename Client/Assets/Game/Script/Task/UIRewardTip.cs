using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Candlelight.UI;
public class UIRewardTip : UIPanel
{
    #region SerializeFields
    public StateGroup itemsGroup;
    public Text mTitle;
    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        List<RewardItem> itemList = param as List<RewardItem>;
        if (itemList == null)
            return;
        itemsGroup.SetCount(itemList.Count);
        for(int i=0;i< itemList.Count;++i)
        {
            UIRewardTipItem taskTipItem = itemsGroup.Get<UIRewardTipItem>(i);
            taskTipItem.init(itemList[i].itemId, itemList[i].itemNum); 
        }
    }

    public void SetTitle(string title)
    {
        mTitle.text = title;
    }
}
