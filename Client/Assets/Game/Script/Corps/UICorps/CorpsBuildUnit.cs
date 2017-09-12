using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CorpsBuildUnit : MonoBehaviour {

    //建筑名称
    public ImageEx m_buildName;
    //图片
    public ImageEx m_img;
    //消耗类型
    public ImageEx m_costType;
    //消耗
    public TextEx m_cost;
    //收获公会建设
    public TextEx m_add1;
    //收获个人贡献
    public TextEx m_add2;
    //建设按钮
    public StateHandle m_buildBtn;
    public TextEx m_btnLabel;
    //建设状态
    public StateHandle m_state;

    //是否已初始化
    bool m_isInit;
    //记录建设索引
    int m_index;
    
    public void SetUnitData(int index)
    {
        if (!m_isInit)
            Init();
        m_index = index;
        CorpsBuildConfig cfg = CorpsBuildCfg.Get(index);
        m_buildName.Set(cfg.nameImg);
        m_img.Set("ui_gonghui_weihui_0" + index);
        string[] s = cfg.cost.Split('|');
        m_costType.Set(s[0] == "1" ? "ui_tongyong_icon_jinbi" : "ui_tongyong_icon_zuanshi");
        m_cost.text = s[1];
        m_add1.text = "+" + cfg.corpsConstr;
        m_add2.text = "+" + cfg.contri;

        //今日是否已建设状态
        Role role = RoleMgr.instance.Hero;
        CorpsPart part = role.CorpsPart;
        m_state.SetState(part.ownBuildState[index - 1]);

        //vip开启的
        if (index == 3)
        {
            //判断vip等级，若达不到则按钮显示vipx开启，否则显示建设
            if(role.GetInt(enProp.vipLv) < cfg.openVipLv)
            {
                m_buildBtn.GetComponent<ImageEx>().SetGrey(true);
                m_buildBtn.GetComponent<StateHandle>().enabled = false;
                m_btnLabel.text = string.Format("VIP{0}开启", cfg.openVipLv);
            }
            else
            {
                m_buildBtn.GetComponent<ImageEx>().SetGrey(false);
                m_buildBtn.GetComponent<StateHandle>().enabled = true;
                m_btnLabel.text = "建设";
            }
        }
    }

    public void Init()
    {
        m_buildBtn.AddClick(OnBtnClick);
        m_isInit = true;
    }

    private void OnBtnClick()
    {
        //判断一下当前捐献是否会超过每日捐献上限
        Role role = RoleMgr.instance.Hero;
        CorpsInfo corpsInfo = role.CorpsPart.corpsInfo;
        CorpsBuildConfig cfg = CorpsBuildCfg.Get(m_index);
        string[] s = cfg.cost.Split('|');
        if (int.Parse(s[0]) == 1 && role.GetInt(enProp.gold) < int.Parse(s[1]))  //金币不足
        {
            UIMessage.Show(LanguageCfg.Get("gold_low"));
            return;
        }
        if (int.Parse(s[0]) == 2 && role.GetInt(enProp.diamond) < int.Parse(s[1]))  //钻石不足
        {
            UIMessage.Show(LanguageCfg.Get("diamond_low"));
            return;
        }
        if (corpsInfo.props.buildNum >= CorpsCfg.Get(corpsInfo.props.level).maxMember)
        {
            UIMessageBox.Open(LanguageCfg.Get("give_desc_over_nums"), () =>
            {
                NetMgr.instance.CorpsHandler.ReqBuildCorps(role.GetInt(enProp.corpsId), role.GetInt(enProp.heroId), m_index);
            }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("continue"), LanguageCfg.Get("cancle"));
        }
        else
            NetMgr.instance.CorpsHandler.ReqBuildCorps(role.GetInt(enProp.corpsId), role.GetInt(enProp.heroId), m_index);
    }
}
