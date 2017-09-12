using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetRecruitContext
{
    public string roleId = "";
    public int star = -1;       //负数表示从配置取
    public bool toPiece = false; //该宠物是否被分解成碎片？

    public UIPetRecruitContext(string roleId, int star = -1, bool toPiece = false)
    {
        this.roleId = roleId;
        this.star = star;
        this.toPiece = toPiece;
    }
}


public class UIPetRecruit: UIPanel
{
    #region SerializeFields
    public UI3DView m_view3d;
    public GameObject m_fx;
    public GameObject m_info;
    public TextEx m_name;
    public StateGroup m_stars;
    public GameObject m_fx2;
    public GameObject m_fx3;
    public TextEx m_toPieceTip;
    #endregion SerializeFields

    private UIPetRecruitContext m_context;
    private RoleCfg m_roleCfg;

    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_context = param as UIPetRecruitContext;
        if (m_context == null)
        {
            Debuger.LogError("招募宠物界面，参数为空或不为UIPetRecruitContext类型");
            Close();
            return;
        }

        m_view3d.m_stage.SetActive(false);
        m_info.SetActive(false);
        m_name.gameObject.SetActive(false);
        m_stars.gameObject.SetActive(false);
        m_toPieceTip.gameObject.SetActive(false);

        m_roleCfg = RoleCfg.Get(m_context.roleId);
        //先预载入模型，这样特效播放完之后可以马上显示
        GameObjectPool.GetPool(GameObjectPool.enPool.Role).PreLoad(m_roleCfg.mod, false);

        //特效
        m_fx.SetActive(false);
        m_fx.SetActive(true);
        m_fx2.SetActive(false);
        m_fx3.SetActive(false);
        TimeMgr.instance.AddTimer(1f, ShowRole);
        GetComponent<UIPanelBase>().m_btnClose.enabled = false;
    }

    public override void OnClosePanel() 
    {
        m_view3d.m_stage.SetActive(false);
    }


    void ShowRole()
    {
        m_view3d.m_stage.SetActive(true);
        m_view3d.SetModel(m_roleCfg.mod, m_roleCfg.uiModScale);

        m_fx2.SetActive(true);

        TimeMgr.instance.AddTimer(0.5f, () => { 
            m_fx3.SetActive(true);
            TimeMgr.instance.AddTimer(0.7f, () => { 
                m_info.SetActive(true);
                m_name.gameObject.SetActive(true);
                m_name.text = m_roleCfg.name;
                m_stars.gameObject.SetActive(true);
                var star = m_context.star >= 0 ? m_context.star : m_roleCfg.initStar;
                m_stars.SetCount(star);
                if (m_context.toPiece)
                {
                    m_toPieceTip.gameObject.SetActive(true);
                    m_toPieceTip.text = string.Format("已拥有该神侍，<color=#ffc000>{0}星神侍</color>转化成<color=#ffc000>{1}个神侍碎片</color>", star, m_roleCfg.pieceNumReturn);
                }

                GetComponent<UIPanelBase>().m_btnClose.enabled = true;
            });
        });
            
    }

}