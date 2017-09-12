#region Header
/**
 * 名称：副本固定属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PetBattleAssistRateCfg 
{
    public int id;
    public PropertyTable props;

    public static Dictionary<int, PetBattleAssistRateCfg> m_cfgs = new Dictionary<int, PetBattleAssistRateCfg>();
    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<int, PetBattleAssistRateCfg>("pet/petBattleAssistRate", "id", "props");
    }
}
