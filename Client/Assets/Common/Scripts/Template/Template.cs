#region Header
/**
 * 名称：类模板
 
 * 日期：201x.xx.xx
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Template
{
    #region Fields
    int m_field=0;
    #endregion


    #region Properties
    public int FieldRead { get { return m_field; } }

    public int Field {
        get { return m_field; }
        set { m_field = value; }
    }
    #endregion


    #region Constructors
    //static Template (){
    //}

    public Template () {
    
    }
    #endregion


    #region Static Methods
    public static void StaticFun ()
    {

    }
    #endregion


    #region Private Methods
    void PrivateFun ()
    {
        
    }
    #endregion

    public void PublicFun()
    {

    }

    

}
