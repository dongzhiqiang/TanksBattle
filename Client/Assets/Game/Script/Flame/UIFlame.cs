using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIFlame : UIPanel
{
    #region SerializeFields
    public StateGroup m_flames;
    public Text m_flameName;
    public Text m_flameLevel;
    public Text m_flameNextAttr;
    public ImageEx m_expBar;
    public Text m_exp;
    public StateHandle m_upgrade;
    public StateGroup m_attributes;
    public UIFx m_fxPan;
    public UI3DView m_pan;
    public StateHandle m_panState;
    public GameObject m_fxUpgrade;
    public Text m_levelMax;
    public Text m_gold;
    public StateHandle m_addGold;
    #endregion
    private int m_frameId;
    //初始化时调用
    public override void OnInitPanel()
    {
        m_upgrade.AddClick(OnUpgrade);
        m_addGold.AddClick(OnAddGoldClick);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        List<int> flames = new List<int>();
        foreach(FlameCfg cfg in FlameCfg.m_cfgs.Values)
        {
            flames.Add(cfg.id);
        }
        flames.Sort();
        m_flames.SetCount(flames.Count);
        for(int i=0; i<flames.Count; i++)
        {
            m_flames.Get<UIFlameIcon>(i).Init(this, flames[i]);
        }
        SelectFlame(flames[0]);
        m_gold.text = RoleMgr.instance.Hero.ItemsPart.GetGold().ToString();
        GetComponent<ShowUpController>().Prepare();
    }

    public void Reflesh()
    {
        List<int> flames = new List<int>();
        foreach (FlameCfg cfg in FlameCfg.m_cfgs.Values)
        {
            flames.Add(cfg.id);
        }
        flames.Sort();
        for (int i = 0; i < flames.Count; i++)
        {
            m_flames.Get<UIFlameIcon>(i).Init(this, flames[i]);
        }
        SelectFlame(m_frameId);
        m_gold.text = RoleMgr.instance.Hero.ItemsPart.GetGold().ToString();
    }

    void OnAddGoldClick()
    {
        UIMessage.Show("该功能未实现，敬请期待!");
    }

    void OnUpgrade()
    {
        int level = 0;
        Flame flame = RoleMgr.instance.Hero.FlamesPart.GetFlame(m_frameId);
        if (flame != null)
        {
            level = flame.Level;
        }
        FlameLevelCfg levelCfg = FlameLevelCfg.Get(m_frameId, level + 1);
        if (levelCfg==null)
        {
            return;
        }
        UIMgr.instance.Open<UIFlameMaterial>(m_frameId);
    }

    public override void OnOpenPanelEnd()
    {
        for (int i = 0; i < m_flames.Count; i++)
        {
            m_flames.Get<UIFlameIcon>(i).UpdateSelect(m_frameId);
        }
        GetComponent<ShowUpController>().Start();
    }

    public void SelectFlame(int flameId)
    {
        m_frameId = flameId;
        for (int i = 0; i < m_flames.Count; i++)
        {
            m_flames.Get<UIFlameIcon>(i).UpdateSelect(flameId);
        }
        Flame flame = RoleMgr.instance.Hero.FlamesPart.GetFlame(flameId);
        FlameCfg flameCfg = FlameCfg.m_cfgs[flameId];
        m_flameName.text = flameCfg.name;
        int level = 0;
        int exp = 0;
        if(flame != null )
        {
            level = flame.Level;
            exp = flame.Exp;
        }
        m_flameLevel.text = "Lv." + level;
        FlameLevelCfg levelCfg = FlameLevelCfg.Get(flameId, level + 1);
        if(levelCfg == null )
        {
            m_exp.text = "已满级";
            m_expBar.fillAmount = 1f;
            m_levelMax.gameObject.SetActive(true);
            m_upgrade.gameObject.SetActive(false);
        }
        else
        {
            m_exp.text = exp + "/" + levelCfg.exp;
            float rate = exp / (float)levelCfg.exp;
            if (rate > 1) rate = 1;
            m_expBar.fillAmount = rate;
            m_levelMax.gameObject.SetActive(false);
            m_upgrade.gameObject.SetActive(true);
        }
        FlameLevelCfg thisLevelCfg = FlameLevelCfg.Get(flameId, level);
        PropValueCfg valueCfg = null;
        string fxName = null;
        if (thisLevelCfg != null)
        {
            valueCfg = PropValueCfg.Get(thisLevelCfg.attributeId);
            fxName = thisLevelCfg.fx;
        }
        else
        {
            fxName = levelCfg.fx;
        }
        m_fxPan.SetFx(fxName);
        m_panState.SetState(2);//level == 0 ? 0 : (level - 1) / 10);
        m_attributes.SetCount(flameCfg.attrLimitCxts.Count);
        for(int i=0; i<flameCfg.attrLimitCxts.Count; i++)
        {
            float attrValue = 0;
            if(valueCfg!=null)
            {
                attrValue = valueCfg.props.GetFloat(flameCfg.attrLimitCxts[i].prop);
            }
            float maxValue = flameCfg.attrLimitCxts[i].value.Get();
            m_attributes.Get<UIFlameAttributeItem>(i).Init(PropTypeCfg.Get(flameCfg.attrLimitCxts[i].prop).name, attrValue, maxValue);
        }
        PropValueCfg nextValueCfg = valueCfg;
        if (levelCfg != null)
        {
            nextValueCfg = PropValueCfg.Get(levelCfg.attributeId);
        }

        string attrName = "";
        string attrValueStr = "";
        if(Flame.GetAddAttr(flameId, level+1, out attrName, out attrValueStr))
        {
            m_flameNextAttr.text = "<color=#49D74AFF>" + attrName + "+" + attrValueStr + "</color>";
        }
        else
        {
            m_flameNextAttr.text = "";
        }
        m_pan.SetModel(flameCfg.mod);
    }

    public void PlayUpgrade()
    {
        m_fxPan.Clear();
        m_fxUpgrade.SetActive(true);
        TimeMgr.instance.AddTimer(1f, () =>
        {
            Reflesh();
        });
    }
}