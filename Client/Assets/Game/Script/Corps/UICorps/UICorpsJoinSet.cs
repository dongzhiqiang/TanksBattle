using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UICorpsJoinSet : UIPanel {
    //取消按钮
    public StateHandle m_cancelBtn;
    //修改按钮
    public StateHandle m_modifyBtn;
    //切换页按钮们
    public StateHandle m_left1;
    public StateHandle m_right1;
    public StateHandle m_left2;
    public StateHandle m_right2;
    //入会设置类别
    public TextEx m_joinTypeTxt;
    //状态切换
  //  public StateHandle m_handle1;
    public StateHandle m_handle2;
    //等级输入
    public InputField m_input;

    int index1;
    int index2;
    //公户系统开放等级
    int openLevel;
    //角色最大开放等级 
    const int MAX_ROLE_LEVEL = 100;


    public override void OnInitPanel()
    {
        m_cancelBtn.AddClick(() =>
        {
            Close();
        });
        m_modifyBtn.AddClick(() =>
        {
            int corpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
            int heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
            NetMgr.instance.CorpsHandler.ReqCorpsSet(corpsId, heroId, 1, index1, index2);
        });
        m_left1.AddClick(() =>
        {
            //if (index1 == 0)
            //    index1 = 1;
            index1 = index1 == 0 ? 1 : 0;
            CheckType();
        });
        m_right1.AddClick(() =>
        {
            index1 = index1 == 0 ? 1 : 0;
            CheckType();
        });
        m_left2.AddClick(() =>
        {
            if (index2 <= openLevel)
                return;
            index2--;
            CheckLevel();
        });
        m_right2.AddClick(() =>
        {
            if (index2 >= MAX_ROLE_LEVEL)  //角色最大开放等级
                return;
            index2++;
            CheckLevel();
        });

        m_input.onEndEdit.AddListener(OnInputChange);
    }
    

    public override void OnOpenPanel(object param)
    {
     //   NetMgr.instance.CorpsHandler.ReqGetAllCorps();
      
        openLevel = CorpsBaseCfg.Get().openLevel;
        UpdateSetting();
    }
    //更新等级设置
    public void UpdateSetting()
    {
        CorpsProps corpsProps = RoleMgr.instance.Hero.CorpsPart.corpsInfo.props;
        index1 = corpsProps.joinSet > 0 ? 1 : 0;
        index2 = corpsProps.joinSetLevel;
        CheckType();
        CheckLevel();
    }

    public override void OnClosePanel()
    {
    }

    #region PrivateMethod
    //检查申请设置
    void CheckType()
    {
        if (index1 == 0)
            m_joinTypeTxt.text = "无需申请";
        else
            m_joinTypeTxt.text = "需要申请";
  //      m_handle1.SetState(index1);
    }
    //检查等级区间
    void CheckLevel()
    {
        if (index2 == openLevel)
            m_handle2.SetState(0);
        else if (index2 > openLevel && index2 < MAX_ROLE_LEVEL)
            m_handle2.SetState(1);
        else
            m_handle2.SetState(2);
        m_input.text = index2.ToString();
    }
    //监听输入数值变化
    void OnInputChange(string arg)
    {
        if (int.Parse(arg) > MAX_ROLE_LEVEL)
            m_input.text = MAX_ROLE_LEVEL.ToString();
        if (int.Parse(arg) < openLevel)
            m_input.text = openLevel.ToString();
        index2 = int.Parse(m_input.text);
    } 
    #endregion
}
