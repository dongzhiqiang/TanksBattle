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

[AttributeUsage(AttributeTargets.Method)]
public sealed class NetHandlerAttribute : Attribute
{
    /// <summary>
    /// 命令
    /// </summary>
    public int  command;
    /// <summary>
    /// 表示第一个参数是错误码，消息体（如果有）就推后，如果没有错误码，如果返回的消息是回复类消息，就用公共代码来处理错误消息的提示
    /// </summary>
    public bool hasErrorCode;
    /// <summary>
    /// 表示第二个参数是错误消息，消息体（如果有）就推后，本参数仅hasErrorCode为true时用到
    /// </summary>
    public bool hasErrorMsg;
    public NetHandlerAttribute(int command, bool hasErrorCode = false, bool hasErrorMsg = true)
    {
        this.command     = command;
        this.hasErrorCode= hasErrorCode;
        this.hasErrorMsg = hasErrorMsg;
    }
}