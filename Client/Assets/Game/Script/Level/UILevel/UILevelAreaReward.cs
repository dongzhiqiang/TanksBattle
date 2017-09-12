using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UILevelAreaReward : UILevelArea
{
    #region Fields
    public ImageEx m_soulBox;
    public ImageEx m_soulAniBox;
    public TextEx m_soulVal;    
    public ImageEx m_goldBox;
    public ImageEx m_goldAniBox;
    public TextEx m_goldVal;
    public ImageEx m_itemBox;
    public ImageEx m_itemAniBox;
    public TextEx m_itemVal;
    public Transform hp;
    public Transform mp;
    #endregion

    #region Properties

    public override enLevelArea Type
    {
        get { return enLevelArea.reward; }
    }

    public override bool IsOpenOnStart
    {
        get { return false; }
    }
    #endregion

    #region Frame
    protected override void OnInitPage()
    {
        //ResetUI();
    }

    protected override void OnOpenArea(bool reopen)
    {
        m_soulAniBox.gameObject.SetActive(false);
        m_goldAniBox.gameObject.SetActive(false);
        m_itemAniBox.gameObject.SetActive(false);
    }

    protected override void OnUpdateArea()
    {
        
    }

    protected override void OnCloseArea()
    {
        //ResetUI();    
    }

    protected override void OnRoleBorn()
    {
        
    }
    #endregion

    #region Private Methods
    void onPlayItemEnd()
    {
        if (m_itemAniBox.transform.parent.gameObject.activeInHierarchy)
        {
            ItemValPlusOne();
            m_itemAniBox.gameObject.SetActive(true);
        }
    }
    void onPlayGoldEnd(int addNum)
    {
        if (m_goldAniBox.transform.parent.gameObject.activeInHierarchy)
        {
            GoldValPlus(addNum);
            m_goldAniBox.gameObject.SetActive(true);
        }
    }
    void onPlayRedSoulEnd()
    {
        if (m_soulAniBox.transform.parent.gameObject.activeInHierarchy)
        {
            SoulValPlusOne();
            m_soulAniBox.gameObject.SetActive(true);
        }
    }

    #endregion
    public void ResetUI()
    {
        SetShowFlag(true, true, true);
        SetGoldVal(0);
        SetItemVal(0);
        SetSoulVal(0);
    }
    public void SetShowFlag(bool showSoul, bool showGold, bool showItem)
    {
        m_soulBox.gameObject.SetActive(showSoul);
        m_goldBox.gameObject.SetActive(showGold);
        m_itemBox.gameObject.SetActive(showItem);        
    }
    public void SetGoldVal(int val)
    {
        m_goldVal.text = val.ToString();
    }
    public void SetItemVal(int val)
    {
        m_itemVal.text = val.ToString();
    }
    public void ItemValPlusOne()
    {
        int itemVal = int.Parse(m_itemVal.text);
        itemVal++;
        SetItemVal(itemVal);
    }

    public void GoldValPlusOne()
    {
        int goldVal = int.Parse(m_goldVal.text);
        goldVal++;
        SetGoldVal(goldVal);
    }

    public void GoldValPlus(int addNum)
    {
        int goldVal = int.Parse(m_goldVal.text);
        goldVal+=addNum;
        SetGoldVal(goldVal);
    }
    public void SetSoulVal(int val)
    {
        m_soulVal.text = val.ToString();
    }
    public void SoulValPlusOne()
    {
        int soulVal = int.Parse(m_soulVal.text);
        soulVal++;
        SetSoulVal(soulVal);
    }
    //飞物品(掉落来源，掉落数量，结束函数)
    public void PlayItemFly(Role roleFrom, int addNum, System.Action onEnd = null)
    {
        if (UIMgr.instance.Get<UILevel>().IsOpen)
        {
            m_parent.Get<UILevelAreaFlyFx>().PlayItemFly(roleFrom, m_itemBox.transform, addNum, () =>
            {
                onPlayItemEnd();
                if (--addNum <= 0 && onEnd != null)
                    onEnd();
            });
        }
    }

   //飞金币
    public void PlayGoldFly(Role roleFrom, int addNum, int showGoldNum = 1, System.Action onEnd = null)
    {
        if (UIMgr.instance.Get<UILevel>().IsOpen)
        {
            //至少一个
            showGoldNum = Math.Max(1, showGoldNum);
            var addNumPerShow = addNum / showGoldNum;
            var addNumLastShow = addNum - (showGoldNum - 1) * addNumPerShow;
            m_parent.Get<UILevelAreaFlyFx>().PlayGoldFly(roleFrom, m_goldBox.transform, showGoldNum, () =>
            {
                if (--showGoldNum <= 0)
                {
                    onPlayGoldEnd(addNumLastShow);
                    if (onEnd != null)
                        onEnd();
                }
                else
                {
                    onPlayGoldEnd(addNumPerShow);
                }
            });
        }
    }
      
    //飞红魂
    public void PlayRedSoulFly(Role roleFrom, int addNum, System.Action<int, object> onEnd = null, object param1 = null)
    {
        if (UIMgr.instance.Get<UILevel>().IsOpen)
        {
            m_parent.Get<UILevelAreaFlyFx>().PlayRedSoulFly(roleFrom, m_soulBox.transform, addNum, () =>
        {
            onPlayRedSoulEnd();
            if (--addNum <= 0 && onEnd != null)
                onEnd(addNum, param1);
        });
        }
    }

    //飞绿魂
    public void PlayGreenSoulFly(Role roleFrom, int addNum, System.Action<int, object> onEnd = null, object param1 = null)
    {
        if (UIMgr.instance.Get<UILevel>().IsOpen)
        {
            m_parent.Get<UILevelAreaFlyFx>().PlayGreenSoulFly(roleFrom, hp, addNum, () =>
        {
            if (--addNum <= 0 && onEnd != null)
                onEnd(addNum, param1);
        });
        }
    }

    //飞蓝魂
    public void PlayBlueSoulFly(Role roleFrom, int addNum, System.Action<int, object> onEnd = null, object param1 = null)
    {
        if (UIMgr.instance.Get<UILevel>().IsOpen)
        {
            m_parent.Get<UILevelAreaFlyFx>().PlayBlueSoulFly(roleFrom, mp, addNum, () =>
        {
            if (--addNum <= 0 && onEnd != null)
                onEnd(addNum, param1);
        });
        }
    }
  
}
