using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UICombatBegin : UIPanel
{
    public class CombatBeginParam
    {
        public string roleIdLeft;
        public string roleIdRight;

        public string roleNameLeft;
        public string roleNameRight;

        public int rolePowerLeft;
        public int rolePowerRight;
        
    }

    public UI3DView2 m_role3DView;

    public TextEx m_roleNameLeft;
    public TextEx m_roleNameRight;

    public TextEx m_rolePowerLeft;
    public TextEx m_rolePowerRight;

    public float m_vsFxDuration = 2.5f;

    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        TimeMgr.instance.AddTimer(m_vsFxDuration, () =>
        {
            this.Close();
        });

        var beginParam = (CombatBeginParam)param;

        RoleCfg roleCfgLeft = RoleCfg.Get(beginParam.roleIdLeft);
        if (roleCfgLeft != null)
        {
            m_role3DView.SetLeftModel(roleCfgLeft.mod, roleCfgLeft.uiModScale, AniFxMgr.Ani_DaiJi);
        }            
        else
        {
            m_role3DView.SetLeftModel(null);
        }            
        RoleCfg roleCfgRight = RoleCfg.Get(beginParam.roleIdRight);
        if (roleCfgRight != null)
        {
            m_role3DView.SetRightModel(roleCfgRight.mod, roleCfgRight.uiModScale, AniFxMgr.Ani_DaiJi);
        }
        else
        {
            m_role3DView.SetRightModel(null);
        }

        m_roleNameLeft.text = beginParam.roleNameLeft;
        m_roleNameRight.text = beginParam.roleNameRight;

        m_rolePowerLeft.text = beginParam.rolePowerLeft.ToString();
        m_rolePowerRight.text = beginParam.rolePowerRight.ToString();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
}