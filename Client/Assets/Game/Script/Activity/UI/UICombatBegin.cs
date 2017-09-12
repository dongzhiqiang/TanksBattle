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

        public string pet1RoleIdLeft;
        public string pet1RoleIdRight;

        public string pet2RoleIdLeft;
        public string pet2RoleIdRight;
    }

    public UI3DView2 m_role3DView;

    public TextEx m_roleNameLeft;
    public TextEx m_roleNameRight;

    public TextEx m_rolePowerLeft;
    public TextEx m_rolePowerRight;

    public ImageEx m_pet1HeadLeft;
    public ImageEx m_pet1HeadRight;

    public ImageEx m_pet2HeadLeft;
    public ImageEx m_pet2HeadRight;

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


        RoleCfg pet1LeftCfg = beginParam.pet1RoleIdLeft == "" ? null : RoleCfg.Get(beginParam.pet1RoleIdLeft);
        if (pet1LeftCfg != null)
        {
            m_pet1HeadLeft.gameObject.SetActive(true);
            m_pet1HeadLeft.Set(RoleCfg.GetHeadIcon(beginParam.pet1RoleIdLeft));
        }
        else
        {
            m_pet1HeadLeft.gameObject.SetActive(false);
        }
        RoleCfg pet1RightCfg = beginParam.pet1RoleIdRight == "" ? null : RoleCfg.Get(beginParam.pet1RoleIdRight);
        if (pet1RightCfg != null)
        {
            m_pet1HeadRight.gameObject.SetActive(true);
            m_pet1HeadRight.Set(RoleCfg.GetHeadIcon(beginParam.pet1RoleIdRight));
        }
        else
        {
            m_pet1HeadRight.gameObject.SetActive(false);
        }

        RoleCfg pet2LeftCfg = beginParam.pet2RoleIdLeft == "" ? null : RoleCfg.Get(beginParam.pet2RoleIdLeft);
        if (pet2LeftCfg != null)
        {
            m_pet2HeadLeft.gameObject.SetActive(true);
            m_pet2HeadLeft.Set(RoleCfg.GetHeadIcon(beginParam.pet2RoleIdLeft));
        }
        else
        {
            m_pet2HeadLeft.gameObject.SetActive(false);
        }
        RoleCfg pet2RightCfg = beginParam.pet2RoleIdRight == "" ? null : RoleCfg.Get(beginParam.pet2RoleIdRight);
        if (pet2RightCfg != null)
        {
            m_pet2HeadRight.gameObject.SetActive(true);
            m_pet2HeadRight.Set(RoleCfg.GetHeadIcon(beginParam.pet2RoleIdRight));
        }
        else
        {
            m_pet2HeadRight.gameObject.SetActive(false);
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
}