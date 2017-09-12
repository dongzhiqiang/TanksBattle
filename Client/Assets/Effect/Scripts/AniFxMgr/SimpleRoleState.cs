#region Header
/**
 * 名称：SimpleRoleState
 
 * 日期：2015.12.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SimpleRoleState 
{
    protected SimpleRole m_parent;
    public SimpleRoleState(SimpleRole r)
    {
        m_parent=r;
    }

    //进入这个状态的时候
    public virtual void OnEnter(SimpleRole.enState lastState, object param)
    {
    
    }

    //用于输入检测、位移和结束判断
    public virtual void OnUpdate(){
    
    }
   
    

}
