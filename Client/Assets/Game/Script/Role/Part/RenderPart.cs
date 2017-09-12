#region Header
/**
 * 名称：渲染部件
 
 * 日期：2015.9.21
 * 描述：记录着角色所有的SkinRenderMesh，用于设置layer和处理材质叠加的冲突问题
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum enRoleCollider
{
    controllerRole,
    pathRole,
    deadRole,
    summonRole,
    max
}

public class RenderPart:RolePart
{
    #region Fields
    List<MaterialMgr> m_renders = new List<MaterialMgr>();
    SkinnedMeshRenderer m_mainRenderer;
    Transform m_weapon;
    Transform m_weapon1;
    Transform m_weapon2;
    Transform m_leftHand;
    Transform m_rightHand;
    Transform m_body;
    Transform m_externalWeaponsRight;
    Transform m_externalWeaponsLeft;


    #endregion


    #region Properties
    public override enPart Type { get { return enPart.render; } }
    public SkinnedMeshRenderer MainRenderer { get { return m_mainRenderer; } }
    #endregion


    #region Frame    
    //属于角色的部件在角色第一次创建的时候调用，属于模型的部件在模型第一次创建的时候调用
    public override void OnCreate(RoleModel model) {
        SkinnedMeshRenderer[] renders = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (renders != null)
        {
            for (int i = 0; i < renders.Length; ++i)
            {
                m_renders.Add(renders[i].AddComponentIfNoExist<MaterialMgr>());
            }
        }

        //模型上内置的武器
        m_weapon = model.Model.Find("weapon_mesh");
        m_weapon1 = model.Model.Find("weapon_mesh_01");
        m_weapon2 = model.Model.Find("weapon_mesh_02");

        //找到左右手
        m_leftHand = model.Model.Find("Bip01 Prop2");
        m_rightHand = model.Model.Find("Bip01 Prop1");

        m_body = model.Model.Find("body_mesh");
        m_mainRenderer = m_body == null ? null : m_body.GetComponent<SkinnedMeshRenderer>();
        
        //MeshCut(DebugUI.instance.meshCut);
    }

   //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        ResetLayer();
      
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {

    }

    public override void OnDestroy() {
        ClearExternalWeapon();
    }
    #endregion


    #region Private Methods
    
    #endregion
    public void ResetLayer()
    {
        SetLayer(this.Parent.Cfg.ColliderLayer);
        CheckMainMaterialQuality();
    }

    public void SetLayer(enGameLayer gamelayer)
    {
        LayerMgr.instance.SetLayer(this.RoleModel.gameObject, gamelayer);
    }

    public enGameLayer GetLayer()
    {
        return LayerMgr.instance.GetGameLayerByLayer(this.RoleModel.gameObject.layer);
    }

    void SetActive(Transform t,bool show)
    {
        if(t==null)return;
        if(show == t.gameObject.activeSelf)return;
        t.gameObject.SetActive(show);
        Renderer render = t.GetComponent<Renderer>();
        if(render!= null)
            render.enabled =show;
        else
        {
            render = t.GetComponentInChildren<Renderer>();
            if (render != null)
                render.enabled = show;
        }
    }

    void ClearExternalWeapon()
    {
        if (m_externalWeaponsRight != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(m_externalWeaponsRight.gameObject);
            m_externalWeaponsRight = null;
        }
        if (m_externalWeaponsLeft != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(m_externalWeaponsLeft.gameObject);
            m_externalWeaponsLeft = null;
        }
    }

    public void ClearWeapon()
    {
        ClearExternalWeapon();

        SetActive(m_weapon, false);
        SetActive(m_weapon1, false);
        SetActive(m_weapon2, false);
    }

    void SetExternalWeapon(string mod,Transform parent,ref Transform externalWeapon)
    {
        if (string.IsNullOrEmpty(mod))
            return;
        if (parent == null)
        {
            Debuger.LogError("找不到{0}应该放的位置,是不是模型上左手或者右手的命名变了", mod);
            return;
        }
        GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(mod,false);
        Transform t = go.transform;
        t.SetParent(parent);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        go.SetActive(true);
        externalWeapon = t;
    }
    public void ChangeWeapon(string modRight,string modLeft = null)
    {
        ClearExternalWeapon();

        if (string.IsNullOrEmpty(modRight)&&string.IsNullOrEmpty(modLeft))
        {
            SetActive(m_weapon,true);
            SetActive(m_weapon1, true);
            SetActive(m_weapon2, true);
        }
        else
        {
            SetExternalWeapon(modRight, m_rightHand, ref m_externalWeaponsRight);
            SetExternalWeapon(modLeft, m_leftHand, ref m_externalWeaponsLeft);
            
            SetActive(m_weapon, false);
            SetActive(m_weapon1, false);
            SetActive(m_weapon2, false);
        }
        m_parent.Fire(MSG_ROLE.WEAPON_RENDER_CHANGE);

    }

    //某些情况下要隐藏武器
    public void ShowWeapon(bool show)
    {
        if(m_externalWeaponsRight != null || m_externalWeaponsLeft !=null)
        {
            SetActive(m_externalWeaponsRight, show);
            SetActive(m_externalWeaponsLeft, show);
        }
        else
        {
            SetActive(m_weapon, show);
            SetActive(m_weapon1, show);
            SetActive(m_weapon2, show);
        }
        
        
        
        m_parent.Fire(MSG_ROLE.WEAPON_RENDER_CHANGE);
    }
    
    public void CheckMainMaterialQuality()
    {
        if (m_mainRenderer == null)
            return;

        if (!QualityMgr.instance.CheckMaterial(m_mainRenderer))
            return;
        //材质管理器需要重新cache下
        foreach (var mm in m_renders)
        {
            if(mm.Renderer== m_mainRenderer)
                mm.Cache(true);
        }
    }

    //public void MeshCut(bool cut)
    //{
    //    if (m_body == null)
    //        return;
    //    if(cut)
    //    {
    //        MeshSimplify ms = m_body.AddComponentIfNoExist<MeshSimplify>();
    //        ms.m_fVertexAmount = 0.3f;
    //        if (ms.HasData() == false )
    //        {
    //            ms.ComputeData(false, (string strTitle, string strProgressMessage, float fT)=> { });
    //        }
    //        ms.ComputeMesh(false, (string strTitle, string strProgressMessage, float fT) => { });
    //        ms.AssignSimplifiedMesh(false);
            
    //    }
    //    else
    //    {
    //        MeshSimplify ms = m_body.GetComponent<MeshSimplify>();
    //        if (ms == null)
    //            return;

    //        ms.RestoreOriginalMesh(false,false);
    //    }
    //}
}
