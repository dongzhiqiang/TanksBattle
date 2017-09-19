using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class UILotteryOpen : UIPanel
{

    #region Fields
    public TextEx m_diamondNum;
    public ImageEx m_buyCostIcon;
    public TextEx m_buyCostNum;
    public StateHandle m_buyAgain;
    public StateHandle m_buyAganText;
    public StateHandle m_closeMe;
    public StateGroup m_cardList;
    public float m_waitBeforePlay = 0.0f;
    public float m_waitMultiSimple = 0.3f;
    public float m_waitMultiComplex = 0.5f;

    private int m_type = 0;
    private int m_subType = 0;
    private DrawLotteryRes m_result = null;

    private int m_diamondObId = 0;
    private int m_itemChgObId = 0;

    private bool m_inPlayAnim = false;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_cardList.SetCount(0);
        m_buyAgain.AddClick(StartBuy);
        m_closeMe.AddClick(() => { Close(); });
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        var nums = param == null ? new int[] { LotteryBasicCfg.ADVANCED_TYPE_ID, LotteryBasicCfg.SUBTYPE_BUY_ONE } : (int[])param;
        m_type = nums[0];
        m_subType = nums[1];

        var myHero = RoleMgr.instance.Hero;
        m_diamondObId = myHero.AddPropChange(enProp.diamond, RefreshUIOnEvent);
        m_itemChgObId = myHero.Add(MSG_ROLE.ITEM_CHANGE, RefreshUIOnEvent);

        //先隐藏按钮
        HideButtons();
        RefreshUIOnEvent();        
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_type = 0;
        m_subType = 0;
        m_result = null;
        m_inPlayAnim = false;
        m_cardList.SetCount(0);
        EventMgr.Remove(m_itemChgObId);
        EventMgr.Remove(m_diamondObId);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private void RefreshUIOnEvent()
    {
        var type = m_result == null ? m_type : m_result.type;
        var subType = m_result == null ? m_subType : m_result.subType;

        ////////////////
        var myHero = RoleMgr.instance.Hero;
        var itemPart = myHero.ItemsPart;
        var lotteryCfg = LotteryBasicCfg.m_cfgs[type];
        ////////////////

        ////////////////
        var diamond = myHero.GetInt(enProp.diamond);
        m_diamondNum.text = diamond.ToString();
        ////////////////

        ////////////////
        if (m_subType == LotteryBasicCfg.SUBTYPE_BUY_ONE)
        {
            m_buyAganText.SetState(0);

            var opActPart = myHero.OpActivityPart;            
            var lastBuyFree = opActPart.GetLong(type == LotteryBasicCfg.ADVANCED_TYPE_ID ? enOpActProp.advLtyLastBuyFree : enOpActProp.topLtyLastBuyFree);
            var buyFreeCnt = opActPart.GetInt(type == LotteryBasicCfg.ADVANCED_TYPE_ID ? enOpActProp.advLtyBuyFreeCnt : enOpActProp.topLtyBuyFreeCnt);
            var cfgFreeBuyCnt = lotteryCfg.freeBuyCnt;
            var cfgFreeBuyCD = lotteryCfg.freeBuyCD;
            var buyItemId = lotteryCfg.buyOneWithItemCost[0];
            var buyCostNum = lotteryCfg.buyOneWithItemCost[1];

            //如果过了CD时间，那就是买了0次
            var curTime = TimeMgr.instance.GetTimestamp();
            if (curTime - lastBuyFree > cfgFreeBuyCD)
                buyFreeCnt = 0;

            //超过了免费次数？那就显示道具
            if (buyFreeCnt >= cfgFreeBuyCnt)
            {
                m_buyCostIcon.Set(ItemCfg.m_cfgs[buyItemId].icon);
                m_buyCostNum.text = buyCostNum.ToString();

                if (itemPart.GetItemNum(buyItemId) < buyCostNum)
                    m_buyCostNum.color = Color.white;//Color.red;//暂时不标红
                else
                    m_buyCostNum.color = Color.white;
            }
            //否则提示免费
            else
            {
                m_buyCostIcon.Set(ItemCfg.m_cfgs[buyItemId].icon);
                m_buyCostNum.text = "免费";
                m_buyCostNum.color = Color.white;
            }
        }
        else        
        {
            m_buyAganText.SetState(1);

            var ticketItemId = lotteryCfg.buyTenWithTicketCost[0];
            var ticketCostNum = lotteryCfg.buyTenWithTicketCost[1];
            var ticketItemNum = itemPart.GetItemNum(ticketItemId);
            if (ticketItemNum < ticketCostNum)
            {
                var buyItemId = lotteryCfg.buyTenWithItemCost[0];
                var buyCostNum = lotteryCfg.buyTenWithItemCost[1];

                m_buyCostIcon.Set(ItemCfg.m_cfgs[buyItemId].icon);
                m_buyCostNum.text = buyCostNum.ToString();

                if (itemPart.GetItemNum(buyItemId) < buyCostNum)
                    m_buyCostNum.color = Color.white;//Color.red;//暂时不标红
                else
                    m_buyCostNum.color = Color.white;
            }
            else
            {
                m_buyCostIcon.Set(ItemCfg.m_cfgs[ticketItemId].icon);
                m_buyCostNum.text = ticketCostNum.ToString();
                m_buyCostNum.color = Color.white;
            }
        }        
        ////////////////
    }

    private void StartBuy()
    {
        if (m_result == null || m_inPlayAnim)
        {
            UIMessage.Show("暂时不可操作");
            return;
        }

        UIMgr.instance.Get<UILottery>().CheckAndBuy(m_result.type, m_result.subType);        
    }

    public void ShowResultOnList(DrawLotteryRes result)
    {
        m_result = result;

        //type可能变了，刷新一下界面
        RefreshUIOnEvent();

        var randIds = m_result.randIds;
        //打乱randIds
        randIds.Shuffle();

        m_cardList.SetCount(randIds.Count);
        var firstUIItem = m_cardList.Get<UILotteryOpenItem>(0);
        for (var i = 0; i < randIds.Count; ++i)
        {
            var randId = randIds[i];
            var randCfg = LotteryRandPool.Get(randId);
            var uiItem = m_cardList.Get<UILotteryOpenItem>(i);
            uiItem.transform.localPosition = new Vector3(uiItem.transform.localPosition.x, uiItem.transform.localPosition.y, firstUIItem.transform.localPosition.z);
            switch (randCfg.objectType)
            {
                case 1:
                    {
                        uiItem.Init(StringUtil.ToInt(randCfg.objectId), randCfg.count);
                    }
                    break;
                case 2:
                    {
                        uiItem.Init(randCfg.objectId);
                    }
                    break;
            }
        }

        UIMgr.instance.StartCoroutine(CoPlayAnimation());
    }

    private void HideButtons()
    {
        m_buyCostIcon.gameObject.SetActive(false);
        m_buyAgain.gameObject.SetActive(false);
        m_closeMe.gameObject.SetActive(false);
    }

    private void ShowButtons()
    {
        m_buyCostIcon.gameObject.SetActive(true);
        m_buyAgain.gameObject.SetActive(true);
        m_closeMe.gameObject.SetActive(true);
    }

    private IEnumerator CoPlayAnimation()
    {
        m_inPlayAnim = true;
        HideButtons();

        yield return UIMgr.instance.StartCoroutine(CoPlayAnimation2());

        ShowButtons();
        m_inPlayAnim = false;
    }

    private IEnumerator CoPlayAnimation2()
    {
        //等一会再播放动画
        yield return new WaitForSeconds(m_waitBeforePlay);

        if (m_result == null)
            yield break;

        if (!this.IsOpenEx)
            yield break;

        var lotteryCfg = LotteryBasicCfg.m_cfgs[m_result.type];
        var getItems = m_result.subType == LotteryBasicCfg.SUBTYPE_BUY_ONE ? lotteryCfg.buyOneGet : lotteryCfg.buyTenGet;
        var strList = new List<string>();
        foreach (var e in getItems)
        {
            strList.Add(string.Format("{0}个{1}", e[1], ItemCfg.Get(e[0]).name));
        }
        UIMessage.Show("您获得" + string.Join("，", strList.ToArray()));

        var myHero = RoleMgr.instance.Hero;
        var randIds = m_result.randIds;
        var pieceRandIds = m_result.pieceRandIds;

        for (var j = 0; j < 2; ++j)
        {
            if (!this.IsOpenEx)
                yield break;

            for (var i = 0; i < randIds.Count; ++i)
            {
                if (!this.IsOpenEx)
                    yield break;

                var randId = randIds[i];
                var randCfg = LotteryRandPool.Get(randId);
                if (j == 0 && randCfg.turnType == 1
                    ||
                    j == 1 && randCfg.turnType != 1)
                {
                    var uiItem = m_cardList.Get<UILotteryOpenItem>(i);
                    if (uiItem == null)
                        continue;

                    UIMgr.instance.StartCoroutine(CoPlayAnimation3(uiItem, randCfg.turnType));

                    yield return new WaitForSeconds(GetAnimationWaitTime(uiItem, randCfg.turnType));
                }
            }
        }        

        m_inPlayAnim = false;

        TeachMgr.instance.OnDirectTeachEvent("lottery", "open_anim_end");
    }

    private float GetAnimationWaitTime(UILotteryOpenItem uiItem, int turnType)
    {
        switch (turnType)
        {
            case 1:
                return uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_SIMPLE_ROLL) * m_waitMultiSimple;
            case 2:
                return uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_PURPLE_ROLL) * m_waitMultiComplex;
            case 3:
                return uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_GOLDEN_ROLL) * m_waitMultiComplex;
        }
        return 0;
    }

    private IEnumerator CoPlayAnimation3(UILotteryOpenItem uiItem, int turnType)
    { 
        switch (turnType)
        {
            case 1:
                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_SIMPLE_ROLL);
                yield return new WaitForSeconds(uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_SIMPLE_ROLL));

                if (!this.IsOpenEx)
                    yield break;

                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_SIMPLE_FLAT);
                break;
            case 2:
                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_PURPLE_ROLL);
                yield return new WaitForSeconds(uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_PURPLE_ROLL));

                if (!this.IsOpenEx)
                    yield break;

                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_PURPLE_FLAT);
                break;
            case 3:
                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_GOLDEN_ROLL);
                yield return new WaitForSeconds(uiItem.GetAnimLen(UILotteryOpenItem.ANI_TYPE_GOLDEN_ROLL));

                if (!this.IsOpenEx)
                    yield break;

                uiItem.PlayAnim(UILotteryOpenItem.ANI_TYPE_GOLDEN_FLAT);
                break;
        }
    }
    #endregion
}
