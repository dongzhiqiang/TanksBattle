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

public class EliteLevelsPart : RolePart
{
    #region Fields
    /// <summary>
    /// 关卡ID与关卡的映射
    /// </summary>
    private Dictionary<int, EliteLevel> m_eliteLevels = new Dictionary<int, EliteLevel>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.eliteLevels; } }
    public Dictionary<int, EliteLevel> EliteLevels { get { return m_eliteLevels; } }
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
  
        if( vo.eliteLevels != null )
        {
            m_eliteLevels = new Dictionary<int, EliteLevel>();
            foreach (EliteLevelVo eliteLevelVo in vo.eliteLevels)
            {
                EliteLevel eliteLevel = EliteLevel.Create(eliteLevelVo);
                AddOrUpdateEliteLevel(eliteLevel);
            }
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_eliteLevels != null)
            m_eliteLevels.Clear();
    }

    void AddOrUpdateEliteLevel(EliteLevel eliteLevel)
    {

        if (m_eliteLevels == null)
            return;

        m_eliteLevels[eliteLevel.levelId] = eliteLevel;

    }

    public void AddOrUpdateEliteLevel(EliteLevelVo eliteLevelVo)
    {
        if (m_eliteLevels == null)
            return;

        EliteLevel eliteLevel;
        if (m_eliteLevels.TryGetValue(eliteLevelVo.levelId, out eliteLevel))
        {
            eliteLevel.LoadFromVo(eliteLevelVo);
        }
        else
        {
            eliteLevel = EliteLevel.Create(eliteLevelVo);
            AddOrUpdateEliteLevel(eliteLevel);
        }
    }

    public EliteLevel GetEliteLevel(int eliteLevelId)
    {
        EliteLevel result;
        if (m_eliteLevels.TryGetValue(eliteLevelId, out result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    #endregion


    #region Private Methods
    
    #endregion
}