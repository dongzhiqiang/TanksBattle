using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class Flame
{
    #region Fields
    int m_flameId;
    int m_level;
    int m_exp;
    #endregion

    #region Properties
    public int FlameId { get { return m_flameId; } set { m_flameId = value; } }
    public int Level { get { return m_level; } set { m_level = value; } }
    public int Exp { get { return m_exp; } set { m_exp = value; } }
    #endregion

    private static PropertyTable m_temp = new PropertyTable();
    private static PropertyTable m_temp2 = new PropertyTable();

    public static Flame Create(FlameVo vo)
    {
        Flame flame;
        flame = new Flame();
        flame.LoadFromVo(vo);
        return flame;
    }

    virtual public void LoadFromVo(FlameVo vo)
    {
        m_flameId = vo.flameId;
        m_level = vo.level;
        m_exp = vo.exp;
    }

    public static bool GetAddAttr(int flameId, int level, out string attrName, out string attrValue)
    {
        attrName = "";
        attrValue = "";
        FlameLevelCfg levelCfg = FlameLevelCfg.Get(flameId, level);
        PropValueCfg valueCfg = null;
        if (levelCfg == null)
        {
            return false;
        }
        valueCfg = PropValueCfg.Get(levelCfg.attributeId);
        PropertyTable.Copy(valueCfg.props, m_temp);
        FlameLevelCfg lastLevelCfg = FlameLevelCfg.Get(flameId, level-1);
        if (lastLevelCfg!=null)
        {
            PropValueCfg lastValueCfg = PropValueCfg.Get(lastLevelCfg.attributeId);
            PropertyTable.Copy(lastValueCfg.props, m_temp2);
            PropertyTable.Mul(-1f, m_temp2, m_temp2);
            PropertyTable.Add(m_temp, m_temp2, m_temp);
        }
        for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
        {
            float value = m_temp.GetFloat(i);
            if (value < Mathf.Epsilon)
            {
                continue;
            }
            PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
            attrName = propTypeCfg.name;
            if (propTypeCfg.format == enPropFormat.FloatRate)
            {
                attrValue = string.Format("{0:P4}", value);
            }
            else
            {
                attrValue = "" + Mathf.RoundToInt(value);
            }
            return true;
        }
        return false;
    }
}