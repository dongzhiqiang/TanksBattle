#region Header
/**
 * 名称：物品部件
 
 * 日期：2015.9.21
 * 描述：背包和装备
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetFormationsPart : RolePart
{
    #region Fields
    /// <summary>
    /// 宠物阵型ID与宠物阵型的映射
    /// </summary>
    private Dictionary<int, PetFormation> m_petFormations = new Dictionary<int, PetFormation>();
    
    #endregion

    private static enPetFormation m_curPetFormationId = enPetFormation.normal;


    #region Properties
    public override enPart Type { get { return enPart.max; } }
    public Dictionary<int, PetFormation> PetFormations { get { return m_petFormations; } }
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
  
        if( vo.petFormations != null )
        {
            m_petFormations = new Dictionary<int, PetFormation>();
            foreach (PetFormationVo petFormationVo in vo.petFormations)
            {
                PetFormation petFormation = PetFormation.Create(petFormationVo, this);
                AddOrUpdatePetFormation(petFormation);
            }
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_petFormations != null)
            m_petFormations.Clear();
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {

    }

    public static void SetCurPetFormationId(enPetFormation curPetFormationId)
    {
        if (m_curPetFormationId == curPetFormationId)
        {
            return;
        }
        Role hero = RoleMgr.instance.Hero;
        if(hero!=null)
        {
            PetFormation oldPetFormation = hero.PetFormationsPart.GetPetFormation(m_curPetFormationId);
            m_curPetFormationId = curPetFormationId;
            PetFormation newPetFormation = hero.PetFormationsPart.GetPetFormation(m_curPetFormationId);
            // 重新计算旧阵型的主站位宠物属性
            oldPetFormation.FreshMainPetProps();
            // 重新计算新阵型的主站位宠物属性
            newPetFormation.FreshMainPetProps();
        }
        else
        {
            m_curPetFormationId = curPetFormationId;
        }

    }


    void AddOrUpdatePetFormation(PetFormation petFormation)
    {

        if (m_petFormations == null)
            return;

        m_petFormations[petFormation.formationId] = petFormation;

    }

    public void AddOrUpdatePetFormation(PetFormationVo petFormationVo)
    {
        if (m_petFormations == null)
            return;

        PetFormation petFormation;
        if (m_petFormations.TryGetValue(petFormationVo.formationId, out petFormation))
        {
            petFormation.LoadFromVo(petFormationVo);
        }
        else
        {
            petFormation = PetFormation.Create(petFormationVo, this);
            AddOrUpdatePetFormation(petFormation);
        }

        m_parent.Fire(MSG_ROLE.PET_FORMATION_CHANGE, petFormationVo.formationId);
    }

    /*这个函数只在进入副本的时候用，其他地方请直接获取对应系统的PetFormation*/
    public PetFormation GetCurPetFormation() 
    {
        return GetPetFormation(m_curPetFormationId); 
    }

    public PetFormation GetPetFormation(enPetFormation petFormationIdEn)
    {
        int petFormationId = (int)petFormationIdEn;
        PetFormation result;
        if (m_petFormations.TryGetValue(petFormationId, out result))
        {
            return result;
        }
        else
        {
            result = new PetFormation();
            result.formationId = petFormationId;
            result.formation = new List<string>();
            result.SetOwner(this);
            for (int i = 0; i < 6; i++) result.formation.Add("");
            return result;
        }
    }

    #endregion


    #region Private Methods
    
    #endregion
}