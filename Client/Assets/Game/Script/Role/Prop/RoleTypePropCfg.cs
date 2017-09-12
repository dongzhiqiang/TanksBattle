#region Header
/**
 * 名称：属性值系数 
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class RoleTypePropCfg 
{
    public string id;
    public PropertyTable props;

    public static PropertyTable mstTypeProp;
    public static PropertyTable roleTypeProp;
    public static PropertyTable powerProp;
    public static PropertyTable powerRateProp;
    public static Dictionary<string, RoleTypePropCfg> m_cfgs = new Dictionary<string, RoleTypePropCfg>();
    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<string, RoleTypePropCfg>("property/roleTypeProp", "id", "props");
        mstTypeProp = m_cfgs["monster"].props;
        roleTypeProp = m_cfgs["role"].props;
        powerProp = m_cfgs["power"].props;
        powerRateProp = m_cfgs["powerRate"].props;
    }

}
