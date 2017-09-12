#region Header
/**
 * 名称：用于标记一个基类类被json序列化的时候是不是支持多态的特性
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonCanInheritAttribute:Attribute
{
    
    public JsonCanInheritAttribute()
    {
        
    }

}

