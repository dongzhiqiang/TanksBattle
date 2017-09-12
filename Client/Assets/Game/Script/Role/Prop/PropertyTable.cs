#region Header
/**
 * 名称: 属性表
 
 * 日期：2015.10.10
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PropertyTable 
{
    #region Fields
    public static Property Zero = new Property((float)0);
    public static Property One= new Property((float)1f);
    private Property[] m_props = new Property[(int)enProp.max];
    
    bool m_isRead=false;//只读
    #endregion

    #region Properties
    public bool IsRead { get { return m_isRead; } set { m_isRead = value; } }
    public Property[] Props { get { return m_props; } }
    #endregion

    #region get和set
    public int GetInt(enProp prop){
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("找不到这个属性,还没有初始化过:{0}", prop);
            return 0;
        }

        return p.Int;
    }

    public int AddInt(enProp prop, int add,bool newIfNo=false)
    {
        if (CheckRead()) return 0;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            if (newIfNo)
            {
                p = new Property((int)0);
                m_props[(int)prop] = p;
            }
            else
            {
                Debuger.LogError("不能add。找不到这个属性", prop);
                return 0;
            }
        }

        p.Int+=add;
        return p.Int;
    }
    public void SetInt(enProp prop,int v)
    {
        if(CheckRead())return;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("调试属性:{0} 类型:int", prop);
            p = new Property(v);
            m_props[(int)prop] = p;
        }
        else
            p.Int = v;
    }

    public long GetLong(enProp prop)
    {
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("找不到这个属性,还没有初始化过", prop);
            return 0;
        }

        return p.Long;
    }

    public void SetLong(enProp prop, long v)
    {
        if (CheckRead()) return;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("调试属性:{0} 类型:long", prop);
            p = new Property(v);
            m_props[(int)prop] = p;
        }
        else
            p.Long = v;
    }

    public long AddLong(enProp prop, long add, bool newIfNo = false)
    {
        if (CheckRead()) return 0;
        Property p = m_props[(int)prop];
        if (p == null)
        {   
            if (newIfNo)
            {
                p = new Property((long)0);
                m_props[(int)prop] = p;
            }
            else
            {
                Debuger.LogError("不能add。找不到这个属性", prop);
            }
            return 0;
        }

        p.Long+= add;
        return p.Long;
    }

    public float GetFloat(enProp prop)
    {
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("找不到这个属性,还没有初始化过:{0}", prop);
            return 0;
        }

        return p.Float;
    }

    public void SetFloat(enProp prop, float v)
    {
        if (CheckRead()) return;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //if (prop  == enProp.hp)
               // Debuger.LogError("调试属性:{0} 类型:float", prop);
            p = new Property(v);
            m_props[(int)prop] = p;
        }
        else
            p.Float = v;
    }

    public float AddFloat(enProp prop, float add, bool newIfNo = false)
    {
        if (CheckRead()) return 0;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            if (newIfNo)
            {
                p = new Property((float)0f);
                m_props[(int)prop] = p;
            }
            else
            {
                Debuger.LogError("不能add。找不到这个属性", prop);
                return 0;
            }
        }

        p.Float += add;
        return p.Float;
    }

    public string GetString(enProp prop)
    {
        Property p = m_props[(int)prop];
        if (p == null)
        {
           // Debuger.LogError("找不到这个属性,还没有初始化过", prop);
            return "";
        }

        return p.String;
    }

    public void SetString(enProp prop, string v)
    {
        if (CheckRead()) return;
        Property p = m_props[(int)prop];
        if (p == null)
        {
            //Debuger.LogError("调试属性:{0} 类型:string", prop);
            p = new Property(v);
            m_props[(int)prop] = p;
            
        }
        else
            p.String = v;
    }

    public void SetValue(enProp prop, Property v)
    {
        if (CheckRead()) return;
        m_props[(int)prop] = v;
    }
    #endregion

    #region 表计算,只能用于战斗属性
    public static void Set(float f, PropertyTable target)
    {
        Property p2;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p2 = target.m_props[i];
            
            if (p2 == null)
                target.m_props[i] = new Property(f);
            else
                p2.Float =f;

        }
    }
    public static void Copy(PropertyTable source,PropertyTable target)
    {
        Property p1,p2;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];
            p2 = target.m_props[i];

            if(p1 == null)
               p1 = Zero; 
            
            if (p2 == null)
                target.m_props[i] = new Property(p1);
            else
                p2.SetValue(p1);

        }
    }

    public static void Mul(float f, PropertyTable source, PropertyTable target)
    {
        Property p1, p2;
        float v;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];
            p2 = target.m_props[i];

            v = p1 == null ? 1 : p1.Float;
            
            if (p2 == null)
                target.m_props[i] = new Property(f * v);
            else
                p2.Float = f * v;

        }
    }

    public static void Mul(PropertyTable source, PropertyTable source2, PropertyTable target)
    {
        Property p1, p11,p2;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];
            p11 = source2.m_props[i];
            p2 = target.m_props[i];

            if (p1 == null)
                p1 = One;

            if (p11 == null)
                p11 = One; 

            
            if (p2 == null)
                target.m_props[i] = new Property(p1.Float*p11.Float);
            else
                p2.Float = p1.Float * p11.Float;

        }
    }

    public static void Add(float f, PropertyTable source, PropertyTable target)
    {
        Property p1, p2;
        float v;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];
            p2 = target.m_props[i];

            v = p1 == null ? 1 : p1.Float;

            if (p2 == null)
                target.m_props[i] = new Property(f + v);
            else
                p2.Float = f +v;

        }
    }

    public static void Add(PropertyTable source, PropertyTable source2, PropertyTable target)
    {
        Property p1, p11, p2;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];
            p11 = source2.m_props[i];
            p2 = target.m_props[i];

            if (p1 == null)
                p1 = Zero;

            if (p11 == null)
                p11 = Zero;


            if (p2 == null)
                target.m_props[i] = new Property(p1.Float + p11.Float);
            else
                p2.Float = p1.Float + p11.Float;

        }
    }

    public static float Sum(PropertyTable source)
    {
        float sum = 0;
        Property p1;
        for (int i = (int)enProp.minFightProp; i < (int)enProp.maxFightProp; ++i)
        {
            p1 = source.m_props[i];

            if (p1 == null)
                p1 = Zero;

            sum += p1.Float;

        }
        return sum;
    }


    #endregion

    bool CheckRead()
    {
        if(m_isRead)Debuger.LogError("只读的属性表，不能设置");
        return m_isRead;
    }

    public void Clear()
    {
        if (CheckRead())
            return;

        for (var i = 0; i < m_props.Length; ++i)
        {
            m_props[i] = null;
        }
    }

}
