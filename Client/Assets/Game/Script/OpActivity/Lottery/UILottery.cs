using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UILottery : UIPanel
{
    #region Fields
    public const float TIMER_INV_SHORT = 1.0f;
    public const float TIMER_INV_LONG = 60.0f;
    #endregion

    #region Fields
    public TextEx m_diamondNum;

    public TextEx m_advanceTipText;
    public ImageEx m_advanceChipIcon;
    public TextEx m_advanceChipNum;
    public TextEx m_advanceBuyOneFreeDesc;
    public RectTransform m_advanceBuyOneCost;
    public ImageEx m_advanceBuyOneCostIcon;
    public TextEx m_advanceBuyOneCostNum;
    public TextEx m_advanceBuyOneFreeNow;
    public TextEx m_advanceBuyTenTicketName;
    public ImageEx m_advanceBuyTenTicketIcon;
    public TextEx m_advanceBuyTenTicketTotalNum;
    public RectTransform m_advanceBuyTenTicketCost;
    public ImageEx m_advanceBuyTenTicketCostIcon;
    public TextEx m_advanceBuyTenTicketCostNum;
    public RectTransform m_advanceBuyTenItemCost;
    public ImageEx m_advanceBuyTenItemCostIcon;
    public TextEx m_advanceBuyTenItemCostNum;
    public ImageEx m_tipIconAdvanceBuyOne;
    public StateHandle m_btnAdvanceBuyOne;
    public StateHandle m_btnAdvanceBuyTen;
    public StateHandle m_btnAdvancePreview;
    public StateHandle m_btnAdvanceExchange;

    public TextEx m_topLevelTipText;
    public ImageEx m_topLevelChipIcon;
    public TextEx m_topLevelChipNum;
    public TextEx m_topLevelBuyOneFreeDesc;
    public RectTransform m_topLevelBuyOneCost;
    public ImageEx m_topLevelBuyOneCostIcon;
    public TextEx m_topLevelBuyOneCostNum;
    public TextEx m_topLevelBuyOneFreeNow;
    public TextEx m_topLevelBuyTenTicketName;
    public ImageEx m_topLevelBuyTenTicketIcon;
    public TextEx m_topLevelBuyTenTicketTotalNum;
    public RectTransform m_topLevelBuyTenTicketCost;
    public ImageEx m_topLevelBuyTenTicketCostIcon;
    public TextEx m_topLevelBuyTenTicketCostNum;
    public RectTransform m_topLevelBuyTenItemCost;
    public ImageEx m_topLevelBuyTenItemCostIcon;
    public TextEx m_topLevelBuyTenItemCostNum;
    public ImageEx m_tipIconTopLevelBuyOne;
    public StateHandle m_btnTopLevelBuyOne;
    public StateHandle m_btnTopLevelBuyTen;
    public StateHandle m_btnTopLevelPreview;
    public StateHandle m_btnTopLevelExchange;

    public Transform m_boxMod1;
    public Transform m_boxMod2;
    public RectTransform m_boxOpenFx;
    public float m_fxDuration;

    private int m_diamondObId = 0;
    private int m_itemChgObId = 0;
    private TimeMgr.Timer m_timerIdShort = null;
    private TimeMgr.Timer m_timerIdLong = null;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_btnAdvanceBuyOne.AddClickEx(OnPreBuy);
        m_btnAdvanceBuyTen.AddClickEx(OnPreBuy);
        m_btnTopLevelBuyOne.AddClickEx(OnPreBuy);
        m_btnTopLevelBuyTen.AddClickEx(OnPreBuy);
        m_btnAdvancePreview.AddClickEx(OnPreview);
        m_btnTopLevelPreview.AddClickEx(OnPreview);

        m_btnAdvanceExchange.AddClick(()=> {
            UIMgr.instance.Open<UIShop>(enShopType.lotteryShop);
        });

        m_btnTopLevelExchange.AddClick(() =>
        {
            UIMgr.instance.Open<UIShop>(enShopType.lotteryTopShop);
        });

        //主角创建后创建定时器定时检测是否有免费次数
        EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.HERO_CREATED, () =>
        {
            if (m_timerIdLong == null)
            {
                m_timerIdLong = TimeMgr.instance.AddTimer(TIMER_INV_LONG, OnTimerCheckCD, 0, -1);
            }
        });
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        var myHero = RoleMgr.instance.Hero;
        m_diamondObId = myHero.AddPropChange(enProp.diamond, RefreshUIOnEvent);
        m_itemChgObId = myHero.Add(MSG_ROLE.ITEM_CHANGE, RefreshUIOnEvent);
        m_timerIdShort = TimeMgr.instance.AddTimer(TIMER_INV_SHORT, OnTimerCheckCD, 0, -1);

        InitUI();
        RefreshUIOnEvent();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        if (m_timerIdShort != null)
        {
            m_timerIdShort.Release();
            m_timerIdShort = null;
        }
        EventMgr.Remove(m_itemChgObId);
        EventMgr.Remove(m_diamondObId);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private void InitUI()
    {
        var cfgAdvanced = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.ADVANCED_TYPE_ID];
        var cfgTopLevel = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.TOPLEVEL_TYPE_ID];

        m_advanceTipText.text = cfgAdvanced.tipText;
        m_advanceChipIcon.Set(ItemCfg.m_cfgs[cfgAdvanced.chipItemId].icon);
        m_advanceBuyOneCostIcon.Set(ItemCfg.m_cfgs[cfgAdvanced.buyOneWithItemCost[0]].icon);
        m_advanceBuyOneCostNum.text = cfgAdvanced.buyOneWithItemCost[1].ToString();
        var itemCfg1_1 = ItemCfg.m_cfgs[cfgAdvanced.buyTenWithTicketCost[0]];
        var itemCfg1_2 = ItemCfg.m_cfgs[cfgAdvanced.buyTenWithItemCost[0]];
        m_advanceBuyTenTicketName.text = itemCfg1_1.name;
        m_advanceBuyTenTicketIcon.Set(itemCfg1_1.icon);
        m_advanceBuyTenTicketCostIcon.Set(itemCfg1_1.icon);
        m_advanceBuyTenTicketCostNum.text = cfgAdvanced.buyTenWithTicketCost[1].ToString();
        m_advanceBuyTenItemCostIcon.Set(itemCfg1_2.icon);
        m_advanceBuyTenItemCostNum.text = cfgAdvanced.buyTenWithItemCost[1].ToString();

        m_topLevelTipText.text = cfgTopLevel.tipText;
        m_topLevelChipIcon.Set(ItemCfg.m_cfgs[cfgTopLevel.chipItemId].icon);
        m_topLevelBuyOneCostIcon.Set(ItemCfg.m_cfgs[cfgTopLevel.buyOneWithItemCost[0]].icon);
        m_topLevelBuyOneCostNum.text = cfgTopLevel.buyOneWithItemCost[1].ToString();
        var itemCfg2_1 = ItemCfg.m_cfgs[cfgTopLevel.buyTenWithTicketCost[0]];
        var itemCfg2_2 = ItemCfg.m_cfgs[cfgTopLevel.buyTenWithItemCost[0]];
        m_topLevelBuyTenTicketName.text = itemCfg2_1.name;
        m_topLevelBuyTenTicketIcon.Set(itemCfg2_1.icon);
        m_topLevelBuyTenTicketCostIcon.Set(itemCfg2_1.icon);
        m_topLevelBuyTenTicketCostNum.text = cfgTopLevel.buyTenWithTicketCost[1].ToString();
        m_topLevelBuyTenItemCostIcon.Set(itemCfg2_2.icon);
        m_topLevelBuyTenItemCostNum.text = cfgTopLevel.buyTenWithItemCost[1].ToString();
    }

    private void RefreshUIOnEvent()
    {
        ////////////////
        var myHero = RoleMgr.instance.Hero;
        var itemPart = myHero.ItemsPart;
        var cfgAdvanced = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.ADVANCED_TYPE_ID];
        var cfgTopLevel = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.TOPLEVEL_TYPE_ID];
        ////////////////

        ////////////////
        var diamond = myHero.GetInt(enProp.diamond);
        m_diamondNum.text = diamond.ToString();
        ////////////////

        ////////////////
        m_advanceChipNum.text = itemPart.GetItemNum(cfgAdvanced.chipItemId).ToString();

        var advBuyOneItemId = cfgAdvanced.buyOneWithItemCost[0];
        var advBuyOneCostNum = cfgAdvanced.buyOneWithItemCost[1];
        if (itemPart.GetItemNum(advBuyOneItemId) < advBuyOneCostNum)
            m_advanceBuyOneCostNum.color = Color.white;//Color.red;//暂时不标红
        else
            m_advanceBuyOneCostNum.color = Color.white;

        var advTicketItemId = cfgAdvanced.buyTenWithTicketCost[0];
        var advTicketCostNum = cfgAdvanced.buyTenWithTicketCost[1];
        var advTicketItemNum = itemPart.GetItemNum(advTicketItemId);
        m_advanceBuyTenTicketTotalNum.text = advTicketItemNum.ToString();
        if (advTicketItemNum < advTicketCostNum)
        {
            m_advanceBuyTenTicketCost.gameObject.SetActive(false);
            m_advanceBuyTenItemCost.gameObject.SetActive(true);
        }            
        else
        {
            m_advanceBuyTenTicketCost.gameObject.SetActive(true);
            m_advanceBuyTenItemCost.gameObject.SetActive(false);
        }

        var advBuyTenItemId = cfgAdvanced.buyTenWithItemCost[0];
        var advBuyTenCostNum = cfgAdvanced.buyTenWithItemCost[1];
        if (itemPart.GetItemNum(advBuyTenItemId) < advBuyTenCostNum)
            m_advanceBuyTenItemCostNum.color = Color.white;//Color.red;//暂时不标红
        else
            m_advanceBuyTenItemCostNum.color = Color.white;
        ////////////////

        ////////////////
        m_topLevelChipNum.text = itemPart.GetItemNum(cfgTopLevel.chipItemId).ToString();

        var topBuyOneItemId = cfgTopLevel.buyOneWithItemCost[0];
        var topBuyOneCostNum = cfgTopLevel.buyOneWithItemCost[1];
        if (itemPart.GetItemNum(topBuyOneItemId) < topBuyOneCostNum)
            m_topLevelBuyOneCostNum.color = Color.white;//Color.red;
        else
            m_topLevelBuyOneCostNum.color = Color.white;

        var topTicketItemId = cfgTopLevel.buyTenWithTicketCost[0];
        var topTicketCostNum = cfgTopLevel.buyTenWithTicketCost[1];
        var topTicketItemNum = itemPart.GetItemNum(topTicketItemId);
        m_topLevelBuyTenTicketTotalNum.text = topTicketItemNum.ToString();
        if (topTicketItemNum < topTicketCostNum)
        {
            m_topLevelBuyTenTicketCost.gameObject.SetActive(false);
            m_topLevelBuyTenItemCost.gameObject.SetActive(true);
        }
        else
        {
            m_topLevelBuyTenTicketCost.gameObject.SetActive(true);
            m_topLevelBuyTenItemCost.gameObject.SetActive(false);
        }

        var topBuyTenItemId = cfgTopLevel.buyTenWithItemCost[0];
        var topBuyTenCostNum = cfgTopLevel.buyTenWithItemCost[1];
        if (itemPart.GetItemNum(topBuyTenItemId) < topBuyTenCostNum)
            m_topLevelBuyTenItemCostNum.color = Color.white;//Color.red;//暂时不标红
        else
            m_topLevelBuyTenItemCostNum.color = Color.white;
        ////////////////
    }

    private void OnTimerCheckCD()
    {
        var myHero = RoleMgr.instance.Hero;
        if (myHero == null)
            return;

        var part = myHero.OpActivityPart;
        var cfgAdvanced = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.ADVANCED_TYPE_ID];
        var cfgTopLevel = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.TOPLEVEL_TYPE_ID];
        var curTime = TimeMgr.instance.GetTimestamp();
        var needTip = false;

        var advLtyLastBuyFree = part.GetLong(enOpActProp.advLtyLastBuyFree);
        var advLtyBuyFreeCnt = part.GetInt(enOpActProp.advLtyBuyFreeCnt);
        var cfgAdvLtyFreeCnt = cfgAdvanced.freeBuyCnt;
        var cfgAdvLtyFreeCD = cfgAdvanced.freeBuyCD;

        //如果过了CD时间，那就是买了0次
        if (curTime - advLtyLastBuyFree > cfgAdvLtyFreeCD)
            advLtyBuyFreeCnt = 0;
        
        //超过了免费次数？那就进入CD
        if (advLtyBuyFreeCnt >= cfgAdvLtyFreeCnt)
        {
            //窗口显示才更新UI
            if (this.IsTop)
            {
                m_tipIconAdvanceBuyOne.gameObject.SetActive(false);
                var leftTime = cfgAdvLtyFreeCD - (curTime - advLtyLastBuyFree);
                var timeSpan = new TimeSpan(leftTime * System.TimeSpan.TicksPerSecond);
                m_advanceBuyOneFreeDesc.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds) + "后免费";
                m_advanceBuyOneFreeNow.gameObject.SetActive(false);
                m_advanceBuyOneCost.gameObject.SetActive(true);
            }            
        }
        else
        {
            needTip = true;

            //窗口显示才更新UI
            if (this.IsTop)
            {
                m_tipIconAdvanceBuyOne.gameObject.SetActive(true);
                m_advanceBuyOneFreeDesc.text = "免费次数：" + (cfgAdvLtyFreeCnt - advLtyBuyFreeCnt);
                m_advanceBuyOneFreeNow.gameObject.SetActive(true);
                m_advanceBuyOneCost.gameObject.SetActive(false);
            }
        }

        var topLtyLastBuyFree = part.GetLong(enOpActProp.topLtyLastBuyFree);
        var topLtyBuyFreeCnt = part.GetInt(enOpActProp.topLtyBuyFreeCnt);
        var cfgTopLtyFreeCnt = cfgTopLevel.freeBuyCnt;
        var cfgTopLtyFreeCD = cfgTopLevel.freeBuyCD;

        //如果过了CD时间，那就是买了0次
        if (curTime - topLtyLastBuyFree > cfgTopLtyFreeCD)
            topLtyBuyFreeCnt = 0;

        //超过了免费次数？那就进入CD
        if (topLtyBuyFreeCnt >= cfgTopLtyFreeCnt)
        {
            //窗口显示才更新UI
            if (this.IsTop)
            {
                m_tipIconTopLevelBuyOne.gameObject.SetActive(false);
                var leftTime = cfgTopLtyFreeCD - (curTime - topLtyLastBuyFree);
                var timeSpan = new TimeSpan(leftTime * System.TimeSpan.TicksPerSecond);
                m_topLevelBuyOneFreeDesc.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds) + "后免费";
                m_topLevelBuyOneFreeNow.gameObject.SetActive(false);
                m_topLevelBuyOneCost.gameObject.SetActive(true);
            }
        }
        else
        {
            needTip = true;

            //窗口显示才更新UI
            if (this.IsTop)
            {
                m_tipIconTopLevelBuyOne.gameObject.SetActive(true);
                m_topLevelBuyOneFreeDesc.text = "免费次数：" + (cfgTopLtyFreeCnt - topLtyBuyFreeCnt);
                m_topLevelBuyOneFreeNow.gameObject.SetActive(true);
                m_topLevelBuyOneCost.gameObject.SetActive(false);
            }
        }

        SystemMgr.instance.SetTip(enSystem.lottery, needTip);
    }

    private bool CanBuyNow(int type, int subType)
    {
        var myHero = RoleMgr.instance.Hero;
        var itemPart = myHero.ItemsPart;
        var opActPart = myHero.OpActivityPart;
        var cfgAdvanced = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.ADVANCED_TYPE_ID];
        var cfgTopLevel = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.TOPLEVEL_TYPE_ID];
        var curTime = TimeMgr.instance.GetTimestamp();

        if (type == LotteryBasicCfg.ADVANCED_TYPE_ID && subType == LotteryBasicCfg.SUBTYPE_BUY_ONE)
        {
            //检查是否开启了图标
            string errMsg = "";
            if (SystemMgr.instance.IsGrey(enSystem.lotteryAdv, out errMsg))
            {
                UIMessage.Show(errMsg);
                return false;
            }

            //检查是否有免费次数，如果没有就检查是否道具足够，如果不够，那就不能购买

            var advLtyLastBuyFree = opActPart.GetLong(enOpActProp.advLtyLastBuyFree);
            var advLtyBuyFreeCnt = opActPart.GetInt(enOpActProp.advLtyBuyFreeCnt);
            var cfgAdvLtyFreeCnt = cfgAdvanced.freeBuyCnt;
            var cfgAdvLtyFreeCD = cfgAdvanced.freeBuyCD;
            //如果过了CD时间，那就是买了0次
            if (curTime - advLtyLastBuyFree > cfgAdvLtyFreeCD)
                advLtyBuyFreeCnt = 0;
            //超过了免费次数？那就判断道具是否够
            if (advLtyBuyFreeCnt >= cfgAdvLtyFreeCnt)
            {
                var advBuyOneItemId = cfgAdvanced.buyOneWithItemCost[0];
                var advBuyOneCostNum = cfgAdvanced.buyOneWithItemCost[1];
                if (itemPart.GetItemNum(advBuyOneItemId) < advBuyOneCostNum)
                {
                    UIMessageBox.Open(LanguageCfg.Get("buy_no_diamond_desc"), () =>
                    {
                        //TODO 打开充值窗口
                    }, () => { });
                    return false;
                }
            }
        }
        else if (type == LotteryBasicCfg.ADVANCED_TYPE_ID && subType == LotteryBasicCfg.SUBTYPE_BUY_TEN)
        {
            //检查是否开启了图标
            string errMsg = "";
            if (SystemMgr.instance.IsGrey(enSystem.lotteryAdv, out errMsg))
            {
                UIMessage.Show(errMsg);
                return false;
            }

            //检查是否优先道具足够，如果不够就检查是否常规道具足够，如果不够，那就不能购买

            var advTicketItemId = cfgAdvanced.buyTenWithTicketCost[0];
            var advTicketCostNum = cfgAdvanced.buyTenWithTicketCost[1];
            var advTicketItemNum = itemPart.GetItemNum(advTicketItemId);

            //优先道具不够？看常规道具
            if (advTicketItemNum < advTicketCostNum)
            {
                var advBuyTenItemId = cfgAdvanced.buyTenWithItemCost[0];
                var advBuyTenCostNum = cfgAdvanced.buyTenWithItemCost[1];
                if (itemPart.GetItemNum(advBuyTenItemId) < advBuyTenCostNum)
                {
                    UIMessageBox.Open(LanguageCfg.Get("buy_no_diamond_desc"), () =>
                    {
                        //TODO 打开充值窗口
                    }, () => { });
                    return false;
                }
            }
        }
        else if (type == LotteryBasicCfg.TOPLEVEL_TYPE_ID && subType == LotteryBasicCfg.SUBTYPE_BUY_ONE)
        {
            //检查是否开启了图标
            string errMsg = "";
            if (SystemMgr.instance.IsGrey(enSystem.lotteryTop, out errMsg))
            {
                UIMessage.Show(errMsg);
                return false;
            }

            //检查是否有免费次数，如果没有就检查是否道具足够，如果不够，那就不能购买

            var topLtyLastBuyFree = opActPart.GetLong(enOpActProp.topLtyLastBuyFree);
            var topLtyBuyFreeCnt = opActPart.GetInt(enOpActProp.topLtyBuyFreeCnt);
            var cfgTopLtyFreeCnt = cfgTopLevel.freeBuyCnt;
            var cfgTopLtyFreeCD = cfgTopLevel.freeBuyCD;
            //如果过了CD时间，那就是买了0次
            if (curTime - topLtyLastBuyFree > cfgTopLtyFreeCD)
                topLtyBuyFreeCnt = 0;
            //超过了免费次数？那就判断道具是否够
            if (topLtyBuyFreeCnt >= cfgTopLtyFreeCnt)
            {
                var topBuyOneItemId = cfgTopLevel.buyOneWithItemCost[0];
                var topBuyOneCostNum = cfgTopLevel.buyOneWithItemCost[1];
                if (itemPart.GetItemNum(topBuyOneItemId) < topBuyOneCostNum)
                {
                    UIMessageBox.Open(LanguageCfg.Get("buy_no_diamond_desc"), () =>
                    {
                        //TODO 打开充值窗口
                    }, () => { });
                    return false;
                }
            }
        }
        else if (type == LotteryBasicCfg.TOPLEVEL_TYPE_ID && subType == LotteryBasicCfg.SUBTYPE_BUY_TEN)
        {
            //检查是否开启了图标
            string errMsg = "";
            if (SystemMgr.instance.IsGrey(enSystem.lotteryTop, out errMsg))
            {
                UIMessage.Show(errMsg);
                return false;
            }

            //检查是否优先道具足够，如果不够就检查是否常规道具足够，如果不够，那就不能购买

            var topTicketItemId = cfgTopLevel.buyTenWithTicketCost[0];
            var topTicketCostNum = cfgTopLevel.buyTenWithTicketCost[1];
            var topTicketItemNum = itemPart.GetItemNum(topTicketItemId);

            //优先道具不够？看常规则道具
            if (topTicketItemNum < topTicketCostNum)
            {
                var topBuyTenItemId = cfgTopLevel.buyTenWithItemCost[0];
                var topBuyTenCostNum = cfgTopLevel.buyTenWithItemCost[1];
                if (itemPart.GetItemNum(topBuyTenItemId) < topBuyTenCostNum)
                {
                    UIMessageBox.Open(LanguageCfg.Get("buy_no_diamond_desc"), () =>
                    {
                        //TODO 打开充值窗口
                    }, () => { });
                    return false;
                }
            }
        }

        return true;
    }

    private void OnPreBuy(StateHandle handle)
    {
        var type = 0;
        var subType = 0;

        if (handle == m_btnAdvanceBuyOne)
        {
            type = LotteryBasicCfg.ADVANCED_TYPE_ID;
            subType = LotteryBasicCfg.SUBTYPE_BUY_ONE;
        }
        else if (handle == m_btnAdvanceBuyTen)
        {
            type = LotteryBasicCfg.ADVANCED_TYPE_ID;
            subType = LotteryBasicCfg.SUBTYPE_BUY_TEN;
        }
        else if (handle == m_btnTopLevelBuyOne)
        {
            type = LotteryBasicCfg.TOPLEVEL_TYPE_ID;
            subType = LotteryBasicCfg.SUBTYPE_BUY_ONE;
        }
        else if (handle == m_btnTopLevelBuyTen)
        {
            type = LotteryBasicCfg.TOPLEVEL_TYPE_ID;
            subType = LotteryBasicCfg.SUBTYPE_BUY_TEN;
        }

        if (!CanBuyNow(type, subType))
            return;

        UIMgr.instance.StartCoroutine(CoPlayOpenFx(type, subType));
    }

    private IEnumerator CoPlayOpenFx(int type, int subType)
    {
        m_boxMod1.gameObject.SetActive(false);
        m_boxMod2.gameObject.SetActive(false);
        m_boxOpenFx.gameObject.SetActive(true);

        yield return new WaitForSeconds(m_fxDuration);

        m_boxMod1.gameObject.SetActive(true);
        m_boxMod2.gameObject.SetActive(true);

        UIMgr.instance.Open<UILotteryOpen>(new int[] { type, subType });
        NetMgr.instance.OpActivityHandler.SendDrawLotteryReq(type, subType);
    }

    private void OnPreview(StateHandle handle)
    {
        var cfgAdvanced = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.ADVANCED_TYPE_ID];
        var cfgTopLevel = LotteryBasicCfg.m_cfgs[LotteryBasicCfg.TOPLEVEL_TYPE_ID];
        UIMgr.instance.Open<UILotteryPreview>(handle == m_btnAdvancePreview ? cfgAdvanced : cfgTopLevel);
    }

    public void CheckAndBuy(int type, int subType)
    {
        if (!CanBuyNow(type, subType))
            return;

        NetMgr.instance.OpActivityHandler.SendDrawLotteryReq(type, subType);
    }
    #endregion
}
