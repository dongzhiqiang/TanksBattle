using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIStronger : UIPanel
{

    #region SerializeFields
    public StateGroup btnGroup;
    public ScrollRect abilityScroll;
    public ScrollRect resourceScroll;
    public StateGroup abilityGroup;
    public StateGroup resourceGroup;
    public GameObject arrow;
    public GameObject emptyMsg;
    int index = 0;
    bool isAddListener = false;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        btnGroup.AddSel(OnStrongerBtn);
        resourceScroll.onValueChanged.AddListener(OnResourceScrollChanged);
        abilityScroll.onValueChanged.AddListener(OnAbilityScrollChanged);
        UIMainCity.AddClick(enSystem.strong, () =>
        {
            UIMgr.instance.Open<UIStronger>();
        });

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        List<StrongerBasicCfg> strongerBasicCfgs = StrongerBasicCfg.m_cfgs;
        btnGroup.SetCount(strongerBasicCfgs.Count);
        for (int i = 0; i < btnGroup.Count; ++i)
        {
            UIStrongerBtnItem item = btnGroup.Get<UIStrongerBtnItem>(i);
            item.Init(strongerBasicCfgs[i]);
        }
        btnGroup.SetSel(0);
        if (!isAddListener)
        {
            isAddListener = true;         
            AddAllListener();
            UIMainCity.AddOpen(() =>
            {
                AddAllListener();
            });
        }

    }
    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion
    void OnResourceScrollChanged(Vector2 v)
    {
        arrow.SetActive(resourceGroup.Count > 4 && resourceScroll.verticalNormalizedPosition > 0.01f);
    }

    void OnAbilityScrollChanged(Vector2 v)
    {
        arrow.SetActive(abilityGroup.Count > 4 && abilityScroll.verticalNormalizedPosition > 0.01f);
    }

    void OnStrongerBtn(StateHandle s, int idx)
    {
        index = idx;
        ScrollRect scroll = RefreshCurrentItem() ? abilityScroll : resourceScroll;
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(scroll, 0); });
    }
    void AddAllListener()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.AddPropChange(enProp.powerTotal, () =>
        {
            if (UIMgr.instance.Get<UIStronger>().IsOpen)
            {
                RefreshCurrentItem();
            }
        });
    }
    #region Private Methods



        #endregion

    public bool  RefreshCurrentItem()
    {
        UIStrongerBtnItem btnItem = btnGroup.Get<UIStrongerBtnItem>(index);        
        List<StrongerDetailCfg> detailCfgs = StrongerDetailCfg.GetStrongerDetailListByStrongerId(btnItem.cfg.id);
        emptyMsg.SetActive(detailCfgs.Count == 0);
        bool isAbility;
        if (btnItem.cfg.type==1)
        {
            isAbility = true;
            abilityScroll.gameObject.SetActive(true);
            resourceScroll.gameObject.SetActive(false);            
            abilityGroup.SetCount(detailCfgs.Count);
            for (int i = 0; i < detailCfgs.Count; ++i)
            {
                UIStrongerAbilityItem abilityItem = abilityGroup.Get<UIStrongerAbilityItem>(i);
                abilityItem.Init(detailCfgs[i]);
            }
            arrow.SetActive(abilityGroup.Count > 4 && abilityScroll.verticalNormalizedPosition > 0.01f);
        }
        else
        {
            isAbility = false;
            abilityScroll.gameObject.SetActive(false);
            resourceScroll.gameObject.SetActive(true);
            resourceGroup.SetCount(detailCfgs.Count);
            for (int i = 0; i < detailCfgs.Count; ++i)
            {
                UIStrongerResourceItem resourceItem = resourceGroup.Get<UIStrongerResourceItem>(i);
                resourceItem.Init(detailCfgs[i]);
            }
            arrow.SetActive(resourceGroup.Count > 4 && resourceScroll.verticalNormalizedPosition > 0.01f);
        }               
        return isAbility;
    }

}



