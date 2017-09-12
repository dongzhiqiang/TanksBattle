#region Header
/**
 * 名称：用于标记网络监听的特性
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NetModuleAttribute:Attribute
{
    public byte module;
    public NetModuleAttribute(byte module)
    {
        this.module = module;
    }

}

