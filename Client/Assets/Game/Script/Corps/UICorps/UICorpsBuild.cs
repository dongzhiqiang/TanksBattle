using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICorpsBuild : UIPanel
{
    //公会名
    public TextEx m_corpsName;
    //公会等级
    public TextEx m_level;
    //成长值
    public TextEx m_growValue;
    //成长值进度条
    public ImageEx m_growValueBar;
    //个人贡献
    public TextEx m_contribution;
    //今日建设人数
    public TextEx m_buildNum;
    //建筑
    public CorpsBuildUnit[] m_units;
    //自己的金币
    public TextEx m_gold;
    //自己的钻石
    public TextEx m_diamond;

    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //更多的箭头
    public ImageEx m_moreArrow;
    //显示区域
    public RectTransform m_gridRect;

    public override void OnInitPanel()
    {
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    public override void OnOpenPanel(object param)
    {
        NetMgr.instance.CorpsHandler.ReqCorpsBuildData(RoleMgr.instance.Hero.GetInt(enProp.corpsId));

        SoundMgr.instance.Play3DSound(108, this.transform);
    }
    
    public override void OnClosePanel()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }
    public void UpdatePanel()
    {
        Role role = RoleMgr.instance.Hero;
        m_gold.text = role.GetInt(enProp.gold).ToString();
        m_diamond.text = role.GetInt(enProp.diamond).ToString();
        CorpsPart part = role.CorpsPart;
        CorpsProps props = part.corpsInfo.props;
        m_corpsName.text = props.name;
        m_level.text = "Lv." + props.level.ToString();
     
        int topLevel = CorpsCfg.GetCorpsTopLevel();
        CorpsConfig cfg = null;
        if (props.level < topLevel)
            cfg = CorpsCfg.Get(props.level + 1);
        else
            cfg = CorpsCfg.Get(topLevel);
        int upValue = cfg.upValue;
        m_growValue.text = props.growValue + "/" + upValue;
        m_growValueBar.fillAmount = (float)props.growValue / upValue;
        m_contribution.text = part.personalInfo.contribution.ToString();
        m_buildNum.text = props.buildNum + "/" + CorpsCfg.Get(props.level).maxMember;

        //建设记录
        UpdateLog(part);
        //建设
        for (int i = 0; i < 3; ++i)
        {
            CorpsBuildUnit unit = m_units[i];
            unit.SetUnitData(i + 1);
        }
    }
    //更新记录
    public void UpdateLog(CorpsPart part)
    {
        int count = part.corpsInfo.buildLogs.Count;
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(m_gridRect.sizeDelta.y > 170 ? true : false);
        for (int i = 0; i < count; ++i)
        {
            int buildId = part.corpsInfo.buildLogs[i].buildId;
            CorpsBuildConfig bCfg = CorpsBuildCfg.Get(buildId);
            TextEx t = m_GridGroup.Get<TextEx>(i);
            t.text = string.Format("<color=#ffec7f>{0}</color>对公会{1}，公会建设度<color=#49d74a>+{2}</color>", part.corpsInfo.buildLogs[i].name, bCfg.name, bCfg.corpsConstr);
        }
    }

    void OnScrollChanged(Vector2 v)
    {
        m_moreArrow.gameObject.SetActive(m_gridRect.sizeDelta.y > 170 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }

}
