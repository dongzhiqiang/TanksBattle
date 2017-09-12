using UnityEngine;
using System.Collections;
//公会总览界面
public class UICorpsOverView:MonoBehaviour
{
    //公会名
    public TextEx m_corpsName;
    //公会等级
    public TextEx m_level;
    //成长值
    public TextEx m_growValue;
    //成长值进度条
    public ImageEx m_growValueBar;
    //会长
    public TextEx m_president;
    //id
    public TextEx m_id;
    //人数
    public TextEx m_num;
    //个人贡献
    public TextEx m_contribution;
    //职位
    public TextEx m_pos;
    //宣言
    public TextEx m_declare;
    //日志按钮
    public StateHandle m_logBtn;
    //排行按钮
    public StateHandle m_rankBtn;
    //宣言修改按钮
    public StateHandle m_modifyBtn;
    //公会日常按钮
    public StateHandle m_dailyBtn;
    //公会建设按钮
    public StateHandle m_buildBtn;
    //公会boss按钮
    public StateHandle m_bossBtn;
    //公会商店按钮
    public StateHandle m_shopBtn; 
    
    public void OnInit()
    {
        //日志
        m_logBtn.AddClick(OnLogBtn);
        //排行
        m_rankBtn.AddClick(OnRankBtn);
        //修改宣言
        m_modifyBtn.AddClick(OnModifyBtn);
        //公会日常
        m_dailyBtn.AddClick(OnDailyBtn);
        //公会建设
        m_buildBtn.AddClick(OnBuildBtn);
        //公会boss
        m_bossBtn.AddClick(OnBossBtn);
        //公会商店
        m_shopBtn.AddClick(OnShopBtn);
    }

    void OnLogBtn()
    {
        UIMgr.instance.Open<UICorpsLog>();
    }

    void OnRankBtn()
    {

    }

    void OnModifyBtn()
    {
        CorpsInfo info = RoleMgr.instance.Hero.CorpsPart.corpsInfo;
        string m = info.props.declare;
        int limit = CorpsBaseCfg.Get().declareLimit;

        UIInputBox.Show("公会宣言", m, limit, (string input) =>
        {
            string badWords;
            if (string.IsNullOrEmpty(input))    //输入为空
            {
                UIMessage.ShowFlowTip("declare_is_null");
            }
            else if (BadWordsCfg.HasBadWords(input, out badWords))    //有屏蔽字
            {
                UIMessage.Show(string.Format("存在不合适的内容“{0}”，请修改", badWords));
            }
            else    //向服务端请求修改
            {
                NetMgr.instance.CorpsHandler.ModifyCorpsDeclare(info.props.corpsId, input, RoleMgr.instance.Hero.GetInt(enProp.heroId));
            }
            return false;
        }, true, "修改", "取消", "*宣言长度不能超过60个字母或30个汉字");
    }

    void OnDailyBtn()
    {

    }

    void OnBuildBtn()
    {
        UIMgr.instance.Open<UICorpsBuild>();
    }

    void OnBossBtn()
    {

    }

    void OnShopBtn()
    {
        UIMgr.instance.Open<UIShop>(enShopType.corpsShop);
    }

    //更新显示数据
    public void OnUpdateProp()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        CorpsProps props = part.corpsInfo.props;
        m_corpsName.text = props.name;
        m_level.text = "Lv." + props.level.ToString();
        int upValue;
        int topLevel = CorpsCfg.GetCorpsTopLevel();
        if (props.level < topLevel)
            upValue = CorpsCfg.Get(props.level + 1).upValue;
        else
            upValue = CorpsCfg.Get(topLevel).upValue;
        m_growValue.text = props.growValue + "/" + upValue;
        m_growValueBar.fillAmount = (float)props.growValue / upValue;
        m_president.text = props.president;
        m_id.text = props.corpsId.ToString();
    
        m_declare.text = props.declare;
      
    }
    //更新成员数量和自己的职位贡献信息
    public void OnUpdateMember()
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        CorpsProps props = part.corpsInfo.props;
        m_num.text = part.corpsInfo.members.Count + "/" + CorpsCfg.Get(props.level).maxMember;
        m_contribution.text = part.personalInfo.contribution.ToString();
        m_pos.text = CorpsPosFuncCfg.Get(part.personalInfo.pos).posName;
        m_modifyBtn.gameObject.SetActive(part.personalInfo.pos <= (int)CorpsPosEnum.Elder ? true : false);
    }
     
}
