using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIHeroInfo : UIPanel
{
    public UI3DView m_role3DView;
    public ImageEx m_heroHeadImg;
    public TextEx m_roleName;
    public TextEx m_heroName;
    public TextEx m_corpsName;
    public TextEx m_maxLevel;
    public TextEx m_arenaRank;
    public UIArtFont m_powerVal;
    public ImageEx m_petHeadImg1;
    public ImageEx m_petHeadImg2;
    public TextEx m_petName1;
    public TextEx m_petName2;
    public TextEx m_treasureInfo;
    public TextEx m_corpsPowerAdd;
    public StateHandle m_btnAddFriend;
    public StateHandle m_btnViewCorps;
    public StateHandle m_btnViewPet1;
    public StateHandle m_btnViewPet2;

    private Role m_targetRole;
    private bool m_needDestroy = false;

    public override void OnInitPanel()
    {
        m_btnAddFriend.AddClick(OnAddFriend);
        m_btnViewCorps.AddClick(OnViewCorps);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        if (param is Role)
        {
            m_targetRole = (Role)param;
            m_needDestroy = false;
        }            
        else if (param is FullRoleInfoVo)
        {
            FullRoleInfoVo roleVo = (FullRoleInfoVo)param;
            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            cxt.OnClear();
            cxt.roleId = roleVo.props["roleId"].String;
            
            try
            {
                m_targetRole = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
                m_needDestroy = true;
            }
            catch (Exception ex)
            {
                return;
            }            
        }
        else
        {
            return;
        }

        var roleCfg = RoleCfg.Get(m_targetRole.GetString(enProp.roleId));
        m_role3DView.SetModel(roleCfg.mod, roleCfg.uiModScale, true);
        m_heroHeadImg.Set(RoleCfg.GetHeadIcon(roleCfg.id));
        m_roleName.text = roleCfg.name;
        m_heroName.text = m_targetRole.GetString(enProp.name);
        m_corpsName.text = m_targetRole.GetString(enProp.corpsName).ToString();
        //m_maxLevel.text = "";
        //m_arenaRank.text = "";
        m_powerVal.SetNum(m_targetRole.GetInt(enProp.powerTotal).ToString());

        //m_treasureInfo.text = "";
        //m_corpsPowerAdd.text = "";        
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        if (m_targetRole != null && m_needDestroy)
        {
            RoleMgr.instance.DestroyRole(m_targetRole, false);
            m_targetRole = null;
        }
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    void OnAddFriend()
    {
        NetMgr.instance.SocialHandler.AddFriend(m_targetRole.GetString(enProp.name));
    }

    void OnViewCorps()
    {
        var corpsId = m_targetRole.GetInt(enProp.corpsId);
        if (corpsId != 0)
            NetMgr.instance.CorpsHandler.ReqOtherCorps(corpsId);
        else
            UIMessage.Show("没有公会，不能查看");
    }
    
}
