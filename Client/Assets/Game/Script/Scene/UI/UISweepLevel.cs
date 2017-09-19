using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UISweepLevel : UIPanel, IPointerClickHandler
{

    #region Fields
    public StateGroup m_resultGrp;
    public ScrollRect m_resultScroll;
    public StateHandle m_sweepBtn;
    public StateHandle m_multiple;
    public TextEx m_multipleTxt;
    public TextEx m_staminaNum;
    public StateHandle m_staminaBuy;
    public RectTransform m_success;
    public float m_waitBeforeShowResult = 0.3f;
    public float m_stepScrollTime = 1.0f;

    private int m_staminaObId = 0;
    private int m_vipLvObId = 0;
    private string m_roomId = "";
    private bool m_playingAnim = false;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        //默认不多次扫荡
        m_multiple.SetState(0);

        m_sweepBtn.AddClick(OnStartSweep);
        m_staminaBuy.AddClick(OnBuyStamina);
        m_multiple.AddChangeState(OnCheckMultiple);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_roomId = (string)param;

        var myHero = RoleMgr.instance.Hero;
        m_staminaObId = myHero.AddPropChange(enProp.stamina, RefreshUIOnEvent);
        m_vipLvObId = myHero.AddPropChange(enProp.vipLv, RefreshUIOnEvent);
        m_success.gameObject.SetActive(false);
        m_playingAnim = false;
        RefreshUIOnEvent();
        ShowRewardPreview();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        EventMgr.Remove(m_staminaObId);
        EventMgr.Remove(m_vipLvObId);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    void OnStartSweep()
    {
        if (m_playingAnim)
        {
            UIMessage.Show("暂不可操作");
            return;
        }

        var myHero = RoleMgr.instance.Hero;
        var vipLv = myHero.GetInt(enProp.vipLv);
        var stars = myHero.LevelsPart.GetLevelInfoById(m_roomId).GetStars();

        var roomCfg = RoomCfg.GetRoomCfgByID(m_roomId);
        var vipCfg = VipCfg.Get(vipLv);
        var multiple = m_multiple.CurStateIdx == 0 ? 1 : Math.Min(vipCfg.sweepLvlTimes, roomCfg.maxChallengeNum);

        var sweepCfg = SweepLevelCfg.Get(m_multiple.CurStateIdx);

        if ((sweepCfg.condOp == 0 && (stars >= sweepCfg.stars || vipLv >= sweepCfg.vipLv))
                ||
            (sweepCfg.condOp != 0 && (stars >= sweepCfg.stars && vipLv >= sweepCfg.vipLv)))
        {
            NetMgr.instance.LevelHandler.SendSweep(m_roomId, m_multiple.CurStateIdx != 0);
        }
        else
        {
            //var opName = sweepCfg.condOp == 0 ? "或" : "且";
            //UIMessageBox.Open(string.Format("要扫荡{0}次，必须关卡{1}星以上{2}VIP{3}级以上", multiple, sweepCfg.stars, opName, sweepCfg.vipLv), () => { });
            UIMessageBox.Open(sweepCfg.tip, () => { });
        }
    }

    void OnBuyStamina()
    {

    }

    private void OnCheckMultiple(StateHandle handle, int idx)
    {
        RefreshUIOnEvent();
    }

    private void RefreshUIOnEvent()
    {
        var myHero = RoleMgr.instance.Hero;
        if (myHero == null)
            return;
        var vipLv = myHero.GetInt(enProp.vipLv);

        var roomCfg = RoomCfg.GetRoomCfgByID(m_roomId);
        var vipCfg = VipCfg.Get(vipLv);

        var multiple = Math.Min(vipCfg.sweepLvlTimes, roomCfg.maxChallengeNum);
        m_multipleTxt.text = string.Format("扫荡{0}次", multiple);
        
        var staminaCost = roomCfg.staminaCost;
        var curStamina = myHero.GetStamina();
        m_staminaNum.text = string.Format("{0}/{1}", curStamina, staminaCost);
    }

    private void ShowRewardPreview()
    {
        var hero = RoleMgr.instance.Hero;
        var itemPart = hero.ItemsPart;

        var roomCfg = RoomCfg.GetRoomCfgByID(m_roomId);
        var propRewards = new Dictionary<enProp, int>();
        var itemRewards = new Dictionary<int, int>();
        propRewards.Add(enProp.exp, roomCfg.expReward);

        for (int i = 0; i < roomCfg.rewardShow.Length; i++)
        {
            int itemId = roomCfg.rewardShow[i][0];
            int itemNum = roomCfg.rewardShow[i][1];
            var enumVal = itemPart.GetAbstractItemEnum(itemId);
            if (enumVal != enProp.max)
                propRewards.Add(enumVal, itemNum);
            else
                itemRewards.Add(itemId, itemNum);
        }
   
        m_resultGrp.SetCount(1);
        m_resultScroll.verticalNormalizedPosition = 1.0f;
        var uiItem = m_resultGrp.Get<UISweepLevelRewardItem>(0);
        uiItem.Init(true, null, propRewards, itemRewards);
    }

    private IEnumerator CoShowSweepResult(SweepLevelRes res)
    {
        m_playingAnim = true;

        RefreshUIOnEvent();
        m_resultGrp.SetCount(0);
        m_resultScroll.verticalNormalizedPosition = 1.0f;

        yield return new WaitForSeconds(m_waitBeforeShowResult);

        var hero = RoleMgr.instance.Hero;
        var itemPart = hero.ItemsPart;

        for (var i = 0; i < res.rewards.Count; ++i)
        {
            m_resultGrp.SetCount(i + 1);

            var dataItem = res.rewards[i];            
            var uiItem = m_resultGrp.Get<UISweepLevelRewardItem>(i);

            var propRewards = new Dictionary<enProp, int>();
            var itemRewards = new Dictionary<int, int>();
            var petExps = new Dictionary<string, int>();

            propRewards.Add(enProp.exp, dataItem.heroExp);
            foreach (var item in dataItem.items)
            {
                var itemId = StringUtil.ToInt(item.Key);
                var enumVal = itemPart.GetAbstractItemEnum(itemId);
                if (enumVal != enProp.max)
                    propRewards.Add(enumVal, item.Value);
                else
                    itemRewards.Add(itemId, item.Value);
            }

            uiItem.Init(false, string.Format("第{0}战", i + 1), propRewards, itemRewards);

            if (IsOpenEx && m_playingAnim)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_resultScroll.transform as RectTransform);

                uiItem.HideAllItems();

                var scrollStep = m_resultScroll.verticalNormalizedPosition / m_stepScrollTime;
                while (m_resultScroll.verticalNormalizedPosition > 0)
                {
                    if (!IsOpenEx || !m_playingAnim)
                    {
                        m_resultScroll.verticalNormalizedPosition = 0;
                        break;
                    }

                    m_resultScroll.verticalNormalizedPosition -= scrollStep * Time.unscaledDeltaTime;
                    yield return new WaitForSeconds(0.0f);
                }

                uiItem.PlayShowAnim();

                while (uiItem.IsPlayingAnim())
                {
                    if (!IsOpenEx || !m_playingAnim)
                    {
                        uiItem.CancelAnim();
                        break;
                    }

                    yield return new WaitForSeconds(0.0f);
                }
            }
        }

        //如果前面跳过了动画效果，这里就执行一下跳到最底部
        if (!IsOpenEx || !m_playingAnim)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_resultScroll.transform as RectTransform);
            m_resultScroll.verticalNormalizedPosition = 0;
        }

        m_success.gameObject.SetActive(true);

        m_playingAnim = false;
    }
    #endregion

    public void OnSweepResult(SweepLevelRes res)
    {
        UIMgr.instance.StartCoroutine(CoShowSweepResult(res));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_playingAnim = false;
    }
}
