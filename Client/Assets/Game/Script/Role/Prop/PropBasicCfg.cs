#region Header
/**
 * 名称：属性常量表 
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion

using System;
using System.Collections.Generic;


public class PropBasicCfg
{
    public float damage=1f;
    public float damageCritical = 1;
    public float elementC = 1f;
    public float elementA = 1f;
    public float defRateC=1f;
    public float defRateB=1f;
    public float defRateA=1f;
    public float petPoint=1f;
    public float equipPoint = 1f;
    public float powerRate = 1f;
    							
    public static PropBasicCfg instance;
    

    public static void Init()
    {
        List<PropBasicCfg> m_cfgs = Csv.CsvUtil.Load<PropBasicCfg>("property/propBasic");
        instance = m_cfgs[0];
    }
}